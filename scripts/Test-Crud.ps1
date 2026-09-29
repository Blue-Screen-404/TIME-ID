param([int]$Port = 5101)

$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
$project = Join-Path $root "TIME-ID_OFC/Time-ID_backend/TIME-ID_OFC.csproj"
$testOutput = Join-Path $root "artifacts/crud-tests"
$dll = Join-Path $testOutput "TIME-ID_OFC.dll"
$baseUrl = "http://localhost:$Port"
$testData = Join-Path $root ("artifacts/test-crud-" + [guid]::NewGuid() + ".json")
$script:serverProcess = $null
$logPrefix = Join-Path ([System.IO.Path]::GetTempPath()) ("timeid-login-" + [guid]::NewGuid())
$script:runNumber = 0

function Start-TestServer {
    $script:runNumber++
    $stdout = "$logPrefix-$script:runNumber.out.log"
    $stderr = "$logPrefix-$script:runNumber.err.log"
    $script:serverProcess = Start-Process dotnet -ArgumentList @(
        ('"' + $dll + '"'), "--urls", $baseUrl, "--TIMEID_DATA_PATH", ('"' + $testData + '"')
    ) -PassThru -WindowStyle Hidden -WorkingDirectory (Split-Path $project -Parent) -RedirectStandardOutput $stdout -RedirectStandardError $stderr
    for ($attempt = 0; $attempt -lt 120; $attempt++) {
        if ($script:serverProcess.HasExited) {
            throw "Backend encerrado. Consulte $stdout e $stderr."
        }
        try {
            $response = Invoke-WebRequest "$baseUrl/api" -UseBasicParsing -TimeoutSec 1
            if ($response.StatusCode -eq 200) { return }
        } catch { }
        Start-Sleep -Milliseconds 250
    }
    throw "Backend nao iniciou. Consulte $stdout e $stderr."
}

function Stop-TestServer {
    if ($null -ne $script:serverProcess -and !$script:serverProcess.HasExited) {
        Stop-Process -Id $script:serverProcess.Id
        $script:serverProcess.WaitForExit()
    }
}

function Assert-Request($label, $method, $path, $body, $session, $expected) {
    $parameters = @{
        Uri = "$baseUrl$path"; Method = $method; UseBasicParsing = $true
        WebSession = $session; TimeoutSec = 10
    }
    if ($null -ne $body) {
        $parameters.ContentType = "application/json; charset=utf-8"
        $parameters.Body = $body
    }
    $response = $null
    try {
        $response = Invoke-WebRequest @parameters
        $status = [int]$response.StatusCode
    } catch {
        if ($null -eq $_.Exception.Response) { throw }
        $status = [int]$_.Exception.Response.StatusCode
    }
    if ($status -ne $expected) { throw "$label : esperado $expected, recebido $status" }
    Write-Host "OK: $label ($status)"
    return $response
}


function Call-Api($label, $method, $path, $body, $session, $status = 200) {
    $json = if ($null -eq $body) { $null } else { $body | ConvertTo-Json -Depth 20 -Compress }
    $response = Assert-Request $label $method $path $json $session $status
    if ($response -and $response.Content) { return ($response.Content | ConvertFrom-Json) }
}
function Check($condition, $message) { if (!$condition) { throw $message } }

function Get-Totp($secret, [long]$seconds = -1) {
    $alphabet='ABCDEFGHIJKLMNOPQRSTUVWXYZ234567'; $bits=''
    foreach ($character in $secret.ToCharArray()) { $bits += [Convert]::ToString($alphabet.IndexOf($character),2).PadLeft(5,'0') }
    $key = New-Object byte[] ([int]($bits.Length / 8))
    for ($i=0; $i -lt $key.Length; $i++) { $key[$i]=[Convert]::ToByte($bits.Substring($i*8,8),2) }
    if ($seconds -lt 0) { $seconds=[DateTimeOffset]::UtcNow.ToUnixTimeSeconds() }
    $counter=[BitConverter]::GetBytes([long][Math]::Floor($seconds / 30)); [Array]::Reverse($counter)
    $hmac=New-Object System.Security.Cryptography.HMACSHA1; $hmac.Key=$key
    try { $hash=$hmac.ComputeHash($counter) } finally { $hmac.Dispose() }
    $offset=$hash[19] -band 15
    $number=(([int]$hash[$offset] -band 127) -shl 24) -bor ([int]$hash[$offset+1] -shl 16) -bor ([int]$hash[$offset+2] -shl 8) -bor [int]$hash[$offset+3]
    return ($number % 1000000).ToString('D6')
}
Check ((Get-Totp 'GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ' 59) -eq '287082') 'Vetor TOTP incorreto.'
Push-Location $root
try {
    & dotnet build $project --no-incremental -warnaserror -v minimal -o $testOutput
    if ($LASTEXITCODE -ne 0) { throw "Build falhou." }
    $listener = [System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Loopback, $Port)
    try { $listener.Start() } finally { $listener.Stop() }
    Start-TestServer
    $admin = New-Object Microsoft.PowerShell.Commands.WebRequestSession
    $regular = New-Object Microsoft.PowerShell.Commands.WebRequestSession
    $anon = New-Object Microsoft.PowerShell.Commands.WebRequestSession
    $null = Call-Api "Dados protegidos" GET /api/workspace $null $anon 401
    $null = Call-Api "Login admin" POST /api/auth/login @{ email='teste@timeid.local'; password='TimeId@123' } $admin
    $null = Call-Api "Login comum" POST /api/auth/login @{ email='usuario@timeid.local'; password='TimeId@123' } $regular
    $team = Call-Api "Criar departamento" POST /api/workspace/departments @{name='Tecnologia';description='Equipe de TI'} $admin
    $null = Call-Api "Duplicidade de departamento" POST /api/workspace/departments @{name='Tecnologia'} $admin 400
    $null = Call-Api "Usuario nao gerencia departamentos" POST /api/workspace/departments @{name='Negado'} $regular 403
    $person = @{name='Pessoa Teste';registration='001';cpf='52998224725';email='usuario@timeid.local';phone='21999990000';birthDate='1990-01-01';hireDate='2020-01-01';departmentId=$team.id;position='Analista';weeklyHours=40;address='Teresopolis';postalCode='25900-000';street='Rua de Teste';number='120';complement='Bloco B';neighborhood='Centro';city='Teresopolis';state='RJ';duties='Apoiar a equipe';rg='123';active=$true;createAccess=$false;accessRole='Usuário'}
    $staff = Call-Api "Criar funcionario e vincular conta" POST /api/workspace/employees $person $admin
    $null = Call-Api "Duplicidade de funcionario" POST /api/workspace/employees $person $admin 400
    $null = Call-Api "Departamento ocupado nao pode ser excluido" DELETE "/api/workspace/departments/$($team.id)" $null $admin 400
    $person.name='Pessoa Editada'
    $staff = Call-Api "Editar funcionario" PUT "/api/workspace/employees/$($staff.id)" $person $admin
    $state = Call-Api "Ler funcionario editado" GET /api/workspace $null $admin
    Check ($state.employees[0].name -eq 'Pessoa Editada') 'Edicao nao persistiu.'
    $personal = Call-Api "Usuario ve propria ficha" GET /api/workspace $null $regular
    Check ($personal.employeeId -eq $staff.id) 'Conta nao vinculada.'
    $team = Call-Api "Definir lider" PUT "/api/workspace/departments/$($team.id)" @{name='TI';description='Tecnologia';leaderId=$staff.id} $admin
    $person2 = $person.Clone();$person2.name='Segunda Pessoa';$person2.registration='002';$person2.cpf='11144477735';$person2.email='segundo@timeid.local';$person2.createAccess=$true;$person2.initialPassword='NovaSenha@123'
    $second = Call-Api "Funcionario com nova conta" POST /api/workspace/employees $person2 $admin
    $newSession = New-Object Microsoft.PowerShell.Commands.WebRequestSession
    $null = Call-Api "Nova conta pode entrar" POST /api/auth/login @{email='segundo@timeid.local';password='NovaSenha@123'} $newSession
    $personal = Call-Api "Privacidade entre funcionarios" GET /api/workspace $null $regular
    Check (@($personal.employees).Count -eq 1) 'Dados de outro funcionario expostos.'
    $null = Call-Api "Usuario nao edita funcionarios" PUT "/api/workspace/employees/$($second.id)" $person2 $regular 403
    $null = Call-Api "Usuario nao registra ponto alheio" POST /api/workspace/punches @{employeeId=$second.id} $regular 403
    $entry = Call-Api "Registrar entrada" POST /api/workspace/punches @{employeeId=$staff.id} $regular
    $null = Call-Api "Bloquear duplo clique" POST /api/workspace/punches @{employeeId=$staff.id} $regular 400
    $null = Call-Api "Usuario nao corrige ponto" PUT "/api/workspace/punches/$($entry.id)" @{at=$entry.at;reason='Tentativa'} $regular 403
    $adjusted = ([DateTimeOffset]::UtcNow.AddMinutes(-5)).ToString('o')
    $entry = Call-Api "Corrigir entrada com justificativa" PUT "/api/workspace/punches/$($entry.id)" @{at=$adjusted;reason='Ajuste no teste'} $admin
    $interval = Call-Api "Registrar intervalo" POST /api/workspace/punches @{employeeId=$staff.id} $regular
    Check ($interval.kind -eq 'Início do intervalo') 'Sequencia incorreta.'
    $null = Call-Api "Nao excluir registro intermediario" DELETE "/api/workspace/punches/$($entry.id)?reason=Teste%20de%20exclusao" $null $admin 400
    $null = Call-Api "Funcionario com historico nao pode ser excluido" DELETE "/api/workspace/employees/$($staff.id)" $null $admin 400
    $null = Call-Api "Excluir ultima marcacao" DELETE "/api/workspace/punches/$($interval.id)?reason=Registro%20de%20teste" $null $admin
    $overLimit=@{employeeId=$staff.id;start='2027-02-01';end='2027-02-15';status='Solicitada';notes='Limite'}
    $null=Call-Api "Bloquear ferias de 15 dias do usuario" POST /api/workspace/vacations $overLimit $regular 400
    $null=Call-Api "Bloquear ferias de 15 dias do admin" POST /api/workspace/vacations $overLimit $admin 400
    $overLimit.end='2027-02-14'
    $boundaryLeave=Call-Api "Permitir exatamente 14 dias" POST /api/workspace/vacations $overLimit $regular
    $overLimit.end='2027-02-15'
    $null=Call-Api "Bloquear edicao para 15 dias" PUT "/api/workspace/vacations/$($boundaryLeave.id)" $overLimit $regular 400
    $overLimit.end=$overLimit.start
    $boundaryLeave=Call-Api "Permitir um dia de ferias" PUT "/api/workspace/vacations/$($boundaryLeave.id)" $overLimit $regular
    $null=Call-Api "Remover ferias do teste de limite" DELETE "/api/workspace/vacations/$($boundaryLeave.id)" $null $regular
    $leaveInput=@{employeeId=$staff.id;start='2027-01-10';end='2027-01-20';status='Solicitada';notes='Ferias teste'}
    $leave = Call-Api "Solicitar ferias" POST /api/workspace/vacations $leaveInput $regular
    $null = Call-Api "Bloquear ferias sobrepostas" POST /api/workspace/vacations $leaveInput $regular 400
    $leaveInput.status='Aprovada'
    $null = Call-Api "Usuario nao aprova ferias" PUT "/api/workspace/vacations/$($leave.id)" $leaveInput $regular 400
    $leave = Call-Api "Admin aprova ferias" PUT "/api/workspace/vacations/$($leave.id)" $leaveInput $admin
    $null = Call-Api "Usuario nao exclui ferias aprovadas" DELETE "/api/workspace/vacations/$($leave.id)" $null $regular 400
    $holiday = Call-Api "Criar feriado" POST /api/workspace/holidays @{name='Feriado teste';date='2027-01-01';scope='Empresa'} $admin
    $holiday = Call-Api "Editar feriado" PUT "/api/workspace/holidays/$($holiday.id)" @{name='Ano novo';date='2027-01-01';scope='Nacional'} $admin
    $null = Call-Api "Usuario nao cria feriados" POST /api/workspace/holidays @{name='Negado';date='2027-01-01'} $regular 403
    $null = Call-Api "Salvar empresa" PUT /api/workspace/settings @{name='Empresa Teste';email='empresa@teste.local';cnpj='';phone='';address='Teresopolis'} $admin
    $null = Call-Api "Salvar preferencias" PUT /api/workspace/preferences @{theme='escuro';notifications=$false} $regular
    $report = Call-Api "Relatorio de funcionarios" GET '/api/workspace/reports?type=employees&start=2019-01-01&end=2028-01-01' $null $admin
    Check (@($report.rows).Count -eq 2) 'Relatorio nao corresponde aos dados.'
    $custom = Call-Api "Relatorio personalizado" GET '/api/workspace/reports?type=custom&start=2019-01-01&end=2028-01-01&fields=1,3' $null $admin
    Check ($custom.columns.Count -eq 2 -and $custom.rows[0].Count -eq 2) 'Colunas personalizadas incorretas.'
    $null = Call-Api "Usuario nao exporta dados da empresa" GET '/api/workspace/reports?type=employees&start=2019-01-01&end=2028-01-01' $null $regular 403
    $null = Call-Api "Periodo invalido" GET '/api/workspace/reports?type=punches&start=2028-01-01&end=2020-01-01' $null $admin 400
    $csv=Assert-Request "Exportar CSV" GET '/api/workspace/reports?type=employees&start=2019-01-01&end=2028-01-01&csv=true' $null $admin 200
    Check ($csv.Content -match 'Pessoa Editada') 'CSV vazio.'
    $backup=Call-Api "Backup completo" GET /api/workspace/backup $null $admin
    Check ($backup.employees.Count -eq 2 -and $backup.accounts.Count -eq 3) 'Backup incompleto.'
    $null = Call-Api "Backup restrito" GET /api/workspace/backup $null $regular 403
    Stop-TestServer
    Start-TestServer
    $admin=New-Object Microsoft.PowerShell.Commands.WebRequestSession
    $null=Call-Api "Login apos reiniciar" POST /api/auth/login @{email='teste@timeid.local';password='TimeId@123'} $admin
    $state=Call-Api "Dados persistem apos reiniciar" GET /api/workspace $null $admin
    Check ($state.employees.Count -eq 2 -and $state.settings.name -eq 'Empresa Teste' -and $state.punches.Count -eq 1 -and $state.vacations.Count -eq 1 -and $state.holidays.Count -eq 1) 'Persistencia incompleta.'
    $savedEmployee=$state.employees | Where-Object id -eq $staff.id
    Check ($savedEmployee.postalCode -eq '25900000' -and $savedEmployee.street -eq 'Rua de Teste' -and $savedEmployee.number -eq '120' -and $savedEmployee.complement -eq 'Bloco B' -and $savedEmployee.neighborhood -eq 'Centro' -and $savedEmployee.city -eq 'Teresopolis' -and $savedEmployee.state -eq 'RJ' -and $savedEmployee.duties -eq 'Apoiar a equipe') 'Endereco estruturado ou atividades nao persistidos.'
    $null=Call-Api "Excluir ferias" DELETE "/api/workspace/vacations/$($leave.id)" $null $admin
    $null=Call-Api "Excluir feriado" DELETE "/api/workspace/holidays/$($holiday.id)" $null $admin
    $null=Call-Api "Excluir ponto com justificativa" DELETE "/api/workspace/punches/$($entry.id)?reason=Limpeza%20do%20teste" $null $admin
    $null=Call-Api "Excluir funcionario sem historico" DELETE "/api/workspace/employees/$($second.id)" $null $admin
    $person.active=$false
    $null=Call-Api "Inativar funcionario" PUT "/api/workspace/employees/$($staff.id)" $person $admin
    $regular=New-Object Microsoft.PowerShell.Commands.WebRequestSession
    $null=Call-Api "Conta inativa nao entra" POST /api/auth/login @{email='usuario@timeid.local';password='TimeId@123'} $regular 401
    $null=Call-Api "Excluir funcionario" DELETE "/api/workspace/employees/$($staff.id)" $null $admin
    $null=Call-Api "Excluir departamento vazio" DELETE "/api/workspace/departments/$($team.id)" $null $admin
    $null=Call-Api "Senha atual incorreta" PUT /api/workspace/password @{currentPassword='errada';newPassword='SenhaNova@123'} $admin 400
    $null=Call-Api "Alterar senha" PUT /api/workspace/password @{currentPassword='TimeId@123';newPassword='SenhaNova@123'} $admin
    $null=Call-Api "Troca de senha invalida sessao" GET /api/auth/me $null $admin 401
    $null=Call-Api "Nova senha funciona" POST /api/auth/login @{email='teste@timeid.local';password='SenhaNova@123'} $admin
    $setup=Call-Api "Preparar dois fatores" POST /api/workspace/two-factor/setup @{password='SenhaNova@123'} $admin
    $null=Call-Api "Codigo 2FA invalido" POST /api/workspace/two-factor/enable @{password='SenhaNova@123';code='invalido'} $admin 400
    $codes=Call-Api "Ativar dois fatores" POST /api/workspace/two-factor/enable @{password='SenhaNova@123';code=(Get-Totp $setup.secret)} $admin
    Check ($codes.recoveryCodes.Count -eq 8) 'Codigos de recuperacao ausentes.'
    $mfa=New-Object Microsoft.PowerShell.Commands.WebRequestSession
    $null=Call-Api "2FA exige codigo no login" POST /api/auth/login @{email='teste@timeid.local';password='SenhaNova@123'} $mfa 401
    $null=Call-Api "Login com autenticador" POST /api/auth/login @{email='teste@timeid.local';password='SenhaNova@123';code=(Get-Totp $setup.secret)} $mfa
    $null=Call-Api "Codigo de recuperacao funciona" POST /api/auth/login @{email='teste@timeid.local';password='SenhaNova@123';code=$codes.recoveryCodes[0]} $mfa
    $null=Call-Api "Recuperacao tem uso unico" POST /api/auth/login @{email='teste@timeid.local';password='SenhaNova@123';code=$codes.recoveryCodes[0]} $mfa 401
    $null=Call-Api "Desativar 2FA com senha e codigo" POST /api/workspace/two-factor/disable @{password='SenhaNova@123';code=(Get-Totp $setup.secret)} $mfa
    Write-Host 'Todos os testes CRUD passaram.'
} finally { Stop-TestServer; Pop-Location }
