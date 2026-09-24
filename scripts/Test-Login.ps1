param([int]$Port = 5099)

$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
$project = Join-Path $root "TIME-ID_OFC/Time-ID_backend/TIME-ID_OFC.csproj"
$dll = Join-Path $root "TIME-ID_OFC/Time-ID_backend/bin/Debug/net8.0/TIME-ID_OFC.dll"
$baseUrl = "http://localhost:$Port"
$script:serverProcess = $null
$logPrefix = Join-Path ([System.IO.Path]::GetTempPath()) ("timeid-login-" + [guid]::NewGuid())
$script:runNumber = 0

function Start-TestServer {
    $script:runNumber++
    $stdout = "$logPrefix-$script:runNumber.out.log"
    $stderr = "$logPrefix-$script:runNumber.err.log"
    $script:serverProcess = Start-Process dotnet -ArgumentList @(
        ('"' + $dll + '"'), "--urls", $baseUrl
    ) -PassThru -WindowStyle Hidden -WorkingDirectory $root -RedirectStandardOutput $stdout -RedirectStandardError $stderr
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
        $parameters.ContentType = "application/json"
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

Push-Location $root
try {
    & dotnet build $project --no-incremental -warnaserror -v minimal
    if ($LASTEXITCODE -ne 0) { throw "Falha na compilacao." }

    # Garante que os testes nao usem uma API preexistente nessa porta.
    $listener = [System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Loopback, $Port)
    try { $listener.Start() } finally { $listener.Stop() }
    Start-TestServer

    $session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
    $valid = '{"email":"teste@timeid.local","password":"TimeId@123"}'
    $null = Assert-Request "Sem sessao" GET "/api/auth/me" $null $session 401
    $null = Assert-Request "Campos ausentes" POST "/api/auth/login" '{}' $session 400
    $null = Assert-Request "Email invalido" POST "/api/auth/login" '{"email":"invalido","password":"abc"}' $session 400
    $null = Assert-Request "JSON invalido" POST "/api/auth/login" '{' $session 400
    $null = Assert-Request "Senha incorreta" POST "/api/auth/login" '{"email":"teste@timeid.local","password":"errada"}' $session 401
    $null = Assert-Request "Email desconhecido" POST "/api/auth/login" '{"email":"outro@timeid.local","password":"TimeId@123"}' $session 401
    $null = Assert-Request "Falha nao autentica" GET "/api/auth/me" $null $session 401
    $response = Assert-Request "Login" POST "/api/auth/login" $valid $session 200
    $user = $response.Content | ConvertFrom-Json
    if ($user.email -ne "teste@timeid.local" -or ($user.PSObject.Properties.Name -contains "passwordHash") -or ($user.PSObject.Properties.Name -contains "password")) {
        throw "Resposta de login incorreta."
    }
    $cookie = $session.Cookies.GetCookies([uri]$baseUrl)["TimeId.Session"]
    if ($null -eq $cookie -or !$cookie.HttpOnly -or $response.Headers["Set-Cookie"] -notmatch "samesite=strict") {
        throw "Cookie de sessao ausente ou sem atributos esperados."
    }
    $null = Assert-Request "Rota protegida" GET "/api/auth/me" $null $session 200
    $null = Assert-Request "Logout" POST "/api/auth/logout" $null $session 204
    $null = Assert-Request "Sessao encerrada" GET "/api/auth/me" $null $session 401
    $null = Assert-Request "Email sem diferenciar caixa" POST "/api/auth/login" '{"email":"TESTE@TIMEID.LOCAL","password":"TimeId@123"}' $session 200
    $null = Assert-Request "Espacos no email" POST "/api/auth/login" '{"email":" teste@timeid.local ","password":"TimeId@123"}' $session 200
    $null = Assert-Request "Cadastro removido" GET "/api/employees" $null $session 404
    Stop-TestServer
    Start-TestServer
    $null = Assert-Request "Cookie anterior ao reinicio" GET "/api/auth/me" $null $session 401
    $null = Assert-Request "Login apos reinicio" POST "/api/auth/login" $valid $session 200
    Write-Host "Todos os testes passaram."
} finally {
    Stop-TestServer
    Pop-Location
}
