param([int]$Port = 5081)
$ErrorActionPreference = 'Stop'
Set-Location (Split-Path $PSScriptRoot -Parent)
if (!(Test-Path -LiteralPath 'output/final-demo.json')) {
    throw 'Tạo database demo trước: dotnet run --project tests/TroLyQuyChe.Checks -c Release -- --prepare-final-demo'
}
$demoProfile = Get-Content output/final-demo.json -Raw | ConvertFrom-Json
if (!$demoProfile.Database.StartsWith('TroLyQuyCheDemo_Final_')) { throw 'Sai hồ sơ database demo.' }
$previousConnection = $env:ConnectionStrings__Regulations
try {
    $env:ConnectionStrings__Regulations = $demoProfile.ConnectionString
    dotnet run --project src/TroLyQuyChe.Web -c Release --no-build --no-launch-profile --urls "http://localhost:$Port"
} finally {
    if ($null -eq $previousConnection) { Remove-Item Env:ConnectionStrings__Regulations -ErrorAction SilentlyContinue }
    else { $env:ConnectionStrings__Regulations = $previousConnection }
}
