param(
    [string]$CompilerPath
)

$ErrorActionPreference = 'Stop'
$projectDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$version = (Get-Content -LiteralPath (Join-Path $projectDir 'version.txt') -Raw).Trim()
$appPath = Join-Path $projectDir 'dist\iRacing Digital Teammate.exe'
$scriptPath = Join-Path $projectDir 'installer.iss'

if (-not (Test-Path -LiteralPath $appPath)) {
    throw 'Build the application before building the installer.'
}

if ([string]::IsNullOrWhiteSpace($CompilerPath)) {
    $knownPaths = @(
        (Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6\ISCC.exe'),
        (Join-Path $env:ProgramFiles 'Inno Setup 6\ISCC.exe'),
        (Join-Path $env:LOCALAPPDATA 'Programs\Inno Setup 6\ISCC.exe')
    )
    $CompilerPath = $knownPaths | Where-Object { $_ -and (Test-Path -LiteralPath $_) } | Select-Object -First 1
}

if ([string]::IsNullOrWhiteSpace($CompilerPath) -or -not (Test-Path -LiteralPath $CompilerPath)) {
    throw 'Inno Setup 6 compiler was not found. Install JRSoftware.InnoSetup or pass -CompilerPath.'
}

& $CompilerPath ('/DMyAppVersion=' + $version) $scriptPath
if ($LASTEXITCODE -ne 0) {
    throw "Installer build failed with exit code $LASTEXITCODE"
}

$installerPath = Join-Path $projectDir ("dist\iRacing-Digital-Teammate-Setup-v$version.exe")
if (-not (Test-Path -LiteralPath $installerPath)) {
    throw "Installer output was not created: $installerPath"
}

Write-Output $installerPath
