param([int]$Port = 5104)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$project = Join-Path $root 'TIME-ID_OFC/Time-ID_backend/TIME-ID_OFC.csproj'
$manual = Join-Path $root $(if ($Port -eq 5104) { 'artifacts/geolocation-manual' } else { "artifacts/geolocation-manual-$Port" })
$output = Join-Path $manual 'build'
$baseUrl = "http://localhost:$Port"
New-Item -ItemType Directory -Force $manual | Out-Null
$listener = [System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Loopback, $Port)
try { $listener.Start() } catch { throw "Porta $Port ocupada. Encerre a instância anterior ou escolha -Port com outra porta." } finally { $listener.Stop() }
& dotnet build $project --no-restore -v minimal -o $output
if ($LASTEXITCODE -ne 0) { throw 'Falha na compilação.' }
$process = Start-Process dotnet -ArgumentList @(
    ('"' + (Join-Path $output 'TIME-ID_OFC.dll') + '"'), '--urls', $baseUrl,
    '--contentRoot', ('"' + $manual + '"'), '--TIMEID_DATA_PATH', ('"' + (Join-Path $manual 'timeid.json') + '"')
    '--webroot', ('"' + (Join-Path $root 'TIME-ID_OFC/Time-ID_backend/wwwroot') + '"')
) -WindowStyle Hidden -WorkingDirectory $manual -PassThru -RedirectStandardOutput (Join-Path $manual 'server.out.log') -RedirectStandardError (Join-Path $manual 'server.err.log')
$ready = $false
try {
    for ($i = 0; $i -lt 100; $i++) {
        if ($process.HasExited) { throw "Servidor encerrou. Consulte os logs em $manual." }
        try { $null = Invoke-RestMethod "$baseUrl/api" -TimeoutSec 1; $ready = $true; break } catch { }
        Start-Sleep -Milliseconds 200
    }
    if (!$ready) { throw 'Servidor não respondeu.' }
    $loginPage = Invoke-WebRequest "$baseUrl/login" -UseBasicParsing -TimeoutSec 10
    if ($loginPage.StatusCode -ne 200 -or $loginPage.Content -notmatch 'id="root"') {
        throw 'A API iniciou, mas a interface não foi encontrada. Confira wwwroot e a compilação do frontend.'
    }
    $session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
    $null = Invoke-RestMethod "$baseUrl/api/auth/login" -Method Post -ContentType 'application/json' -Body '{"email":"teste@timeid.local","password":"TimeId@123"}' -WebSession $session
    function Api($path, $body) {
        Invoke-RestMethod "$baseUrl$path" -Method Post -ContentType 'application/json; charset=utf-8' -WebSession $session -Body (ConvertTo-Json -InputObject $body -Depth 8)
    }
    $state = Invoke-RestMethod "$baseUrl/api/workspace" -WebSession $session
    $team = @($state.departments | Where-Object name -eq 'Teste manual geolocalização') | Select-Object -First 1
    if (!$team) { $team = Api '/api/workspace/departments' @{ name = 'Teste manual geolocalização' } }
    $staff = @($state.employees | Where-Object registration -eq 'GEO-MANUAL') | Select-Object -First 1
    if (!$staff) {
        $staff = Api '/api/workspace/employees' @{
            name = 'Colaborador teste geolocalização'; registration = 'GEO-MANUAL'; cpf = '52998224725'; email = 'geo-manual@timeid.local'
            birthDate = '1990-01-01'; hireDate = '2020-01-01'; departmentId = $team.id; position = 'Analista'
            workMode = 'Remoto'; weeklyHours = 40; active = $true
        }
    }
    $locations = Invoke-RestMethod "$baseUrl/api/work-locations" -WebSession $session
    $location = @($locations | Where-Object name -eq 'Local fictício para teste manual') | Select-Object -First 1
    if (!$location) {
        $location = Api '/api/work-locations' @{ name = 'Local fictício para teste manual'; latitude = 0; longitude = 0; radiusMeters = 100; companyWide = $false; employeeIds = @($staff.id); departmentIds = @(); active = $true }
    }
    $environment = @{
        id = [guid]::NewGuid().ToString(); name = 'TIMEID - teste manual local'
        values = @(
            @{ key = 'baseUrl'; value = $baseUrl; enabled = $true },
            @{ key = 'employeeId'; value = $staff.id; enabled = $true },
            @{ key = 'locationId'; value = $location.id; enabled = $true },
            @{ key = 'punchId'; value = ''; enabled = $true }
        ); _postman_variable_scope = 'environment'
    }
    $environment | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $manual 'Manual.postman_environment.json')
    Write-Host "Pronto: $baseUrl | PID: $($process.Id)"
    Write-Host "Abra no navegador deste computador: $baseUrl/geolocalizacao"
    Write-Host 'Administrador: teste@timeid.local | Senha: TimeId@123'
    Write-Host "Colaborador: $($staff.id)"
    Write-Host 'A interface está pronta para teste; não é necessário Postman.'
    Write-Host "Para encerrar: Stop-Process -Id $($process.Id)"
    Write-Host 'As próximas inicializações preservam os testes anteriores. Nenhuma batida foi criada pela preparação.'
}
catch {
    if (!$process.HasExited) { Stop-Process -Id $process.Id }
    throw
}
