param([int]$Port = 5103)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$project = Join-Path $root 'TIME-ID_OFC/Time-ID_backend/TIME-ID_OFC.csproj'
$runDirectory = Join-Path $root ('artifacts/geolocation-http-' + [guid]::NewGuid())
$output = Join-Path $runDirectory 'build'
$data = Join-Path $runDirectory 'timeid.json'
$baseUrl = "http://localhost:$Port"
$script:server = $null
$script:checks = 0
$script:starts = 0

function Check($condition, $label) {
    if (!$condition) { throw $label }
    $script:checks++
    Write-Host "OK: $label"
}
function Request($method, $path, $body, $session, $expected) {
    $options = @{ Uri = "$baseUrl$path"; Method = $method; WebSession = $session; UseBasicParsing = $true; TimeoutSec = 10 }
    if ($null -ne $body) {
        $options.ContentType = 'application/json; charset=utf-8'
        $options.Body = if ($body -is [string]) { $body } else { ConvertTo-Json -InputObject $body -Depth 10 -Compress }
    }
    $response = $null
    try { $response = Invoke-WebRequest @options; $status = [int]$response.StatusCode }
    catch {
        if ($null -eq $_.Exception.Response) { throw }
        $status = [int]$_.Exception.Response.StatusCode
    }
    Check ($status -eq $expected) "$method $path : esperado $expected, recebido $status"
    if ($null -ne $response -and $response.Content) { return ($response.Content | ConvertFrom-Json) }
}
function Start-Server {
    $script:starts++
    $stdout = Join-Path $runDirectory "server-$script:starts.out.log"
    $stderr = Join-Path $runDirectory "server-$script:starts.err.log"
    $script:server = Start-Process dotnet -ArgumentList @(
        ('"' + (Join-Path $output 'TIME-ID_OFC.dll') + '"'), '--urls', $baseUrl,
        '--contentRoot', ('"' + $runDirectory + '"'), '--TIMEID_DATA_PATH', ('"' + $data + '"')
    ) -WorkingDirectory $runDirectory -WindowStyle Hidden -PassThru -RedirectStandardOutput $stdout -RedirectStandardError $stderr
    for ($i = 0; $i -lt 100; $i++) {
        if ($script:server.HasExited) { throw "Servidor encerrou. Logs: $runDirectory" }
        try { $null = Invoke-WebRequest "$baseUrl/api" -UseBasicParsing -TimeoutSec 1; return } catch { }
        Start-Sleep -Milliseconds 200
    }
    throw "Servidor não iniciou. Logs: $runDirectory"
}
function Stop-Server {
    if ($null -ne $script:server -and !$script:server.HasExited) { Stop-Process -Id $script:server.Id; $script:server.WaitForExit() }
}
function Login($email) {
    $session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
    $null = Request POST '/api/auth/login' @{ email = $email; password = 'TimeId@123' } $session 200
    return $session
}
function LocationBody {
    return @{ name = 'Escritório'; latitude = 0; longitude = 0; radiusMeters = 100; companyWide = $true; departmentIds = @(); employeeIds = @(); active = $true }
}
function Position($longitude = 0, $accuracy = 5) {
    return @{ latitude = 0; longitude = $longitude; accuracyMeters = $accuracy; capturedAt = [DateTimeOffset]::UtcNow.ToString('O') }
}

Push-Location $root
try {
    New-Item -ItemType Directory -Path $runDirectory -Force | Out-Null
    & dotnet build $project --no-restore -v minimal -o $output
    if ($LASTEXITCODE -ne 0) { throw 'Falha na compilação.' }
    $listener = [System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Loopback, $Port)
    try { $listener.Start() } finally { $listener.Stop() }
    Start-Server
    $anonymous = New-Object Microsoft.PowerShell.Commands.WebRequestSession
    $null = Request GET '/api/work-locations' $null $anonymous 401
    $null = Request POST '/api/work-locations' (LocationBody) $anonymous 401
    $null = Request POST '/api/workspace/punches' @{} $anonymous 401
    $null = Request POST "/api/workspace/punches/$([guid]::NewGuid())/location-review" @{} $anonymous 401
    $admin = Login 'teste@timeid.local'
    $user = Login 'usuario@timeid.local'
    $team = Request POST '/api/workspace/departments' @{ name = 'Departamento geolocalização' } $admin 200
    $staffs = @()
    $cpfs = @('52998224725', '11144477735', '12345678909')
    for ($i = 0; $i -lt 3; $i++) {
        $email = if ($i -eq 0) { 'usuario@timeid.local' } else { "geoteste$i@timeid.local" }
        $staffs += Request POST '/api/workspace/employees' @{
            name = "Colaborador Geo $i"; registration = "GEO-$i"; cpf = $cpfs[$i]; email = $email
            birthDate = '1990-01-01'; hireDate = '2020-01-01'; departmentId = $team.id
            position = 'Analista'; workMode = 'Remoto'; weeklyHours = 40; active = $true
        } $admin 200
    }
    $own = $staffs[0].id; $other = $staffs[1].id; $legacy = $staffs[2].id
    $null = Request POST '/api/work-locations' (LocationBody) $user 403
    $null = Request GET '/api/work-locations' $null $user 403
    $null = Request POST '/api/work-locations' '{' $admin 400
    $null = Request POST '/api/work-locations' @{ name = 'Sem coordenadas'; companyWide = $true } $admin 400
    $invalid = LocationBody; $invalid.latitude = 91
    $null = Request POST '/api/work-locations' $invalid $admin 400
    $invalid = LocationBody; $invalid.radiusMeters = 0
    $null = Request POST '/api/work-locations' $invalid $admin 400
    $invalid = LocationBody; $invalid.companyId = [guid]::NewGuid().ToString()
    $null = Request POST '/api/work-locations' $invalid $admin 400
    $invalid = LocationBody; $invalid.companyWide = $false; $invalid.employeeIds = @([guid]::NewGuid().ToString())
    $null = Request POST '/api/work-locations' $invalid $admin 400
    $null = Request POST '/api/work-locations' @{ name = 'Sem abrangência'; latitude = 0; longitude = 0; radiusMeters = 100 } $admin 400
    $null = Request POST "/api/work-locations/validate/$other" (Position) $user 403
    $location = Request POST '/api/work-locations' (LocationBody) $admin 201
    $id = $location.id
    $null = Request PUT "/api/work-locations/$id" (LocationBody) $user 403
    $null = Request POST "/api/work-locations/$id/deactivate" $null $user 403
    $result = Request POST "/api/work-locations/validate/$own" (Position) $user 200
    Check ($result.status -eq 'DentroDaArea') 'Simulação dentro da área'
    $result = Request POST "/api/work-locations/validate/$own" (Position 0.001 20) $user 200
    Check ($result.status -eq 'Inconclusivo') 'Precisão na borda gera resultado inconclusivo'
    $result = Request POST "/api/work-locations/validate/$own" (Position 1) $user 200
    Check ($result.status -eq 'ForaDaArea') 'Simulação fora da área'
    $invalid = Position; $invalid.accuracyMeters = -1
    $null = Request POST "/api/work-locations/validate/$own" $invalid $user 400
    $invalid = Position; $invalid.latitude = $null
    $null = Request POST "/api/work-locations/validate/$own" $invalid $user 400
    $body = LocationBody; $body.companyWide = $false; $body.departmentIds = @($team.id)
    $null = Request PUT "/api/work-locations/$id" $body $admin 200
    $result = Request POST "/api/work-locations/validate/$own" (Position) $user 200
    Check ($result.status -eq 'DentroDaArea') 'Permissão por departamento via HTTP'
    $body.departmentIds = @(); $body.employeeIds = @($own)
    $null = Request PUT "/api/work-locations/$id" $body $admin 200
    $result = Request POST "/api/work-locations/validate/$other" (Position) $admin 200
    Check ($result.status -eq 'SemRegra') 'Permissão individual não se estende a terceiros'
    $null = Request POST '/api/workspace/punches' @{ employeeId = $other; location = (Position) } $user 403
    $outside = @{ employeeId = $own; location = (Position 1) }
    $null = Request POST '/api/workspace/punches' $outside $user 400
    $null = Request POST '/api/workspace/punches' @{ employeeId = $own } $user 400
    $stale = Position; $stale.capturedAt = [DateTimeOffset]::UtcNow.AddMinutes(-6).ToString('O')
    $null = Request POST '/api/workspace/punches' @{ employeeId = $own; location = $stale } $user 400
    $invalid = Position; $invalid.capturedAt = 'invalido'
    $null = Request POST '/api/workspace/punches' @{ employeeId = $own; location = $invalid } $user 400
    $invalid = Position; $invalid.Remove('capturedAt')
    $null = Request POST '/api/workspace/punches' @{ employeeId = $own; location = $invalid } $user 400
    $state = Request GET '/api/workspace' $null $admin 200
    Check (@($state.punches).Count -eq 0) 'Solicitações rejeitadas não gravam pontos parciais'
    $before = [DateTimeOffset]::UtcNow
    $validPunch = Request POST '/api/workspace/punches' @{ employeeId = $own; location = (Position) } $user 200
    Check ($validPunch.geolocation.status -eq 'DentroDaArea' -and $validPunch.geolocation.reviewStatus -eq 'NaoNecessaria') 'Batida validada sem análise'
    Check ([DateTimeOffset]$validPunch.at -ge $before -and [DateTimeOffset]$validPunch.at -le [DateTimeOffset]::UtcNow) 'Horário oficial definido pelo servidor'
    $legacyPunch = Request POST '/api/workspace/punches' @{ employeeId = $legacy } $admin 200
    Check ($legacyPunch.geolocation.status -eq 'SemRegra') 'Sem locais aplicáveis mantém compatibilidade'
    $body = LocationBody
    $null = Request PUT "/api/work-locations/$id" $body $admin 200
    $pending = Request POST '/api/workspace/punches' @{ employeeId = $other; location = (Position 1); locationJustification = 'Atendimento externo autorizado' } $admin 200
    Check ($pending.geolocation.status -eq 'ForaDaArea' -and $pending.geolocation.reviewStatus -eq 'Pendente') 'Justificativa registra batida pendente'
    $reviewPath = "/api/workspace/punches/$($pending.id)/location-review"
    $review = @{ decision = 'Aceita'; reason = 'Atendimento externo confirmado' }
    $null = Request POST $reviewPath $review $user 403
    $null = Request POST $reviewPath @{ decision = 'Invalida'; reason = 'Motivo informado' } $admin 400
    $null = Request POST $reviewPath @{ decision = 'Aceita'; reason = 'abc' } $admin 400
    $accepted = Request POST $reviewPath $review $admin 200
    Check ($accepted.geolocation.reviewStatus -eq 'Aceita' -and $accepted.geolocation.reviewedBy -and $accepted.geolocation.reviewedAt) 'Análise registra decisão, autor e horário'
    Check ($accepted.at -eq $pending.at -and $accepted.geolocation.status -eq 'ForaDaArea') 'Análise preserva horário e classificação original'
    $null = Request POST $reviewPath $review $admin 400
    $null = Request POST "/api/workspace/punches/$($validPunch.id)/location-review" $review $admin 400
    $state = Request GET '/api/workspace' $null $user 200
    Check (@($state.punches).Count -eq 1 -and $state.punches[0].id -eq $validPunch.id) 'Usuário não consulta geolocalização de terceiros'
    $body.name = 'Escritório alterado'; $body.radiusMeters = 200
    $null = Request PUT "/api/work-locations/$id" $body $admin 200
    $null = Request POST "/api/work-locations/$id/deactivate" $null $admin 200
    $state = Request GET '/api/workspace' $null $admin 200
    $saved = @($state.punches | Where-Object id -eq $pending.id)[0]
    Check ($saved.geolocation.workLocationName -eq 'Escritório' -and $saved.geolocation.radiusMeters -eq 100) 'Alterar local não muda snapshot da batida'
    $result = Request POST "/api/work-locations/validate/$own" (Position) $user 200
    Check ($result.status -eq 'SemRegra') 'Local desativado não autoriza'
    Stop-Server
    Start-Server
    $null = Request GET '/api/work-locations' $null $admin 401
    $admin = Login 'teste@timeid.local'
    $state = Request GET '/api/workspace' $null $admin 200
    $saved = @($state.punches | Where-Object id -eq $pending.id)[0]
    Check ($saved.geolocation.reviewStatus -eq 'Aceita' -and $saved.geolocation.justification -eq 'Atendimento externo autorizado') 'Batida e análise persistem após reinício'
    $locations = @(Request GET '/api/work-locations' $null $admin 200)
    Check ($locations.Count -eq 1 -and !$locations[0].active) 'Local desativado persiste após reinício'
    Check (@($state.audit | Where-Object { $_.action -eq 'Analisou localização' }).Count -eq 1) 'Análise registrada na auditoria'
    $null = Request GET '/api/geolocation/settings' $null $user 401
    $user = Login 'usuario@timeid.local'
    $null = Request GET '/api/geolocation/settings' $null $user 403
    $null = Request PUT '/api/geolocation/settings' @{ retentionDays = 0 } $admin 400
    $null = Request PUT '/api/geolocation/settings' @{ maxCaptureAgeSeconds = 0 } $admin 400
    $null = Request PUT '/api/geolocation/settings' @{ retentionDays = -1 } $admin 400
    $null = Request PUT '/api/geolocation/settings' @{ managersCanReview = $true } $user 403
    $settings = Request GET '/api/geolocation/settings' $null $admin 200
    Check ($null -eq $settings.retentionDays -and !$settings.managersCanReview) 'Retenção e delegação desativadas por padrão'
    $provider = Request GET '/api/geolocation/provider-status' $null $admin 200
    Check (!$provider.configured -and !$provider.enabled) 'Sem provedor configurado não envia coordenadas'
    $null = Request POST "/api/geolocation/punches/$($validPunch.id)/retry-geocoding" $null $admin 409
    $managerStaff = Request POST '/api/workspace/employees' @{
        name = 'Gestor geolocalização'; registration = 'GEO-GESTOR'; cpf = '39053344705'; email = 'gestor@timeid.local'
        birthDate = '1990-01-01'; hireDate = '2020-01-01'; departmentId = $team.id; position = 'Gestor'
        workMode = 'Remoto'; weeklyHours = 40; active = $true; createAccess = $true; initialPassword = 'TimeId@123'; accessRole = 'Gestor'
    } $admin 200
    $null = Request PUT "/api/workspace/departments/$($team.id)" @{ name = 'Departamento geolocalização'; leaderId = $managerStaff.id } $admin 200
    $manager = Login 'gestor@timeid.local'
    $null = Request GET '/api/work-locations' $null $manager 403
    $null = Request GET '/api/geolocation/reviews' $null $manager 403
    $null = Request PUT '/api/geolocation/settings' @{ managersCanManageLocations = $true; managersCanReview = $true; maxCaptureAgeSeconds = 120; requireJustification = $false } $admin 200
    $restricted = LocationBody; $restricted.companyWide = $false; $restricted.departmentIds = @($team.id)
    $managerLocation = Request POST '/api/work-locations' $restricted $manager 201
    $null = Request POST '/api/work-locations' (LocationBody) $manager 403
    $null = Request POST "/api/work-locations/$id/deactivate" $null $manager 403
    $scoped = @(Request GET '/api/work-locations' $null $manager 200)
    Check ($scoped.Count -eq 1 -and $scoped[0].id -eq $managerLocation.id) 'Gestor consulta apenas locais dentro da sua abrangência'
    $unjustified = Request POST '/api/workspace/punches' @{ employeeId = $managerStaff.id; location = (Position 1) } $admin 200
    Check ($unjustified.geolocation.reviewStatus -eq 'Pendente') 'Justificativa opcional mantém divergência pendente'
    $queue = Request GET '/api/geolocation/reviews' $null $manager 200
    Check (@($queue.items | Where-Object id -eq $unjustified.id).Count -eq 1) 'Gestor consulta fila dos departamentos liderados'
    $null = Request POST "/api/workspace/punches/$($unjustified.id)/location-review" @{ decision = 'Recusada'; reason = 'Local não autorizado' } $manager 200
    $null = Request GET '/api/geolocation/provider-status' $null $manager 403
    $null = Request POST "/api/geolocation/punches/$($validPunch.id)/retry-geocoding" $null $user 403
    Write-Host "Concluído: $script:checks verificações passaram. Dados e logs isolados: $runDirectory"
}
finally { Stop-Server; Pop-Location }
