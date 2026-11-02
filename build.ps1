$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$dotnet = Join-Path $env:LOCALAPPDATA 'Microsoft\dotnet\dotnet.exe'
if (-not (Test-Path $dotnet)) {
    $dotnet = 'dotnet'
}

$publish = Join-Path $root 'publish'
if (Test-Path $publish) {
    Remove-Item $publish -Recurse -Force
}

& $dotnet publish (Join-Path $root 'src\AzanTimes\AzanTimes.csproj') `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -o $publish `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -p:DebugType=None `
    -p:DebugSymbols=false
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$cities = Get-Content (Join-Path $root 'src\AzanTimes.Core\Data\cities.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$lines = @($cities | Sort-Object name | ForEach-Object { "$($_.id)|$($_.name)" })
$utf8 = New-Object System.Text.UTF8Encoding $true
[System.IO.File]::WriteAllLines((Join-Path $root 'installer\cities.txt'), $lines, $utf8)

$iss = Join-Path $root 'installer\AzanTimes.iss'
$issText = [System.IO.File]::ReadAllText($iss)
$utf8Bom = New-Object System.Text.UTF8Encoding $true
[System.IO.File]::WriteAllText($iss, $issText, $utf8Bom)

$iscc = Join-Path $env:LOCALAPPDATA 'Programs\Inno Setup 6\ISCC.exe'
if (-not (Test-Path $iscc)) {
    throw "ISCC.exe not found at $iscc"
}

& $iscc $iss
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Output ("SETUP " + (Join-Path $root 'dist\AzanTimes-Setup.exe'))
