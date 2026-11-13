param([int]$Port = 5080)
$ErrorActionPreference = 'Stop'
Set-Location (Split-Path $PSScriptRoot -Parent)
$appConfig = Get-Content src/TroLyQuyChe.Web/appsettings.json -Raw | ConvertFrom-Json
$previousConnection = $env:ConnectionStrings__Regulations
try {
    $env:ConnectionStrings__Regulations = $appConfig.ConnectionStrings.Regulations
    dotnet run --project src/TroLyQuyChe.Web -c Release --no-build --no-launch-profile --urls "http://localhost:$Port"
} finally {
    if ($null -eq $previousConnection) { Remove-Item Env:ConnectionStrings__Regulations -ErrorAction SilentlyContinue }
    else { $env:ConnectionStrings__Regulations = $previousConnection }
}
