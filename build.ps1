param(
    [switch]$Release,
    [string]$UpdateRepository = $env:GITHUB_REPOSITORY
)

$ErrorActionPreference = 'Stop'
$projectDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$outputDir = Join-Path $projectDir 'dist'
$objectDir = Join-Path $projectDir 'obj'
$compiler = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$versionFile = Join-Path $projectDir 'version.txt'

if (-not (Test-Path -LiteralPath $compiler)) {
    throw "C# compiler not found: $compiler"
}

New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
New-Item -ItemType Directory -Path $objectDir -Force | Out-Null

$version = (Get-Content -LiteralPath $versionFile -Raw).Trim()
if ($version -notmatch '^\d+\.\d+\.\d+$') {
    throw "version.txt must contain a semantic version such as 1.2.0"
}

$generatedVersionPath = Join-Path $objectDir 'GeneratedVersion.cs'
$generatedVersionSource = @"
using System.Reflection;
[assembly: AssemblyVersion("$version.0")]
[assembly: AssemblyFileVersion("$version.0")]
"@
[System.IO.File]::WriteAllText($generatedVersionPath, $generatedVersionSource, [System.Text.Encoding]::UTF8)

$updateRepositoryPath = Join-Path $objectDir 'update-repository.txt'
$repositoryValue = if ([string]::IsNullOrWhiteSpace($UpdateRepository)) { '' } else { $UpdateRepository.Trim() }
[System.IO.File]::WriteAllText($updateRepositoryPath, $repositoryValue, [System.Text.Encoding]::UTF8)

$iconPath = Join-Path $objectDir 'dds-teammate.ico'
$logoPath = Join-Path $projectDir 'dds-logo.png'
$bannerPath = Join-Path $projectDir 'dds-banner.png'
if (-not (Test-Path -LiteralPath $logoPath)) {
    throw "DDS logo asset not found: $logoPath"
}
if (-not (Test-Path -LiteralPath $bannerPath)) {
    throw "DDS banner asset not found: $bannerPath"
}
Add-Type -AssemblyName System.Drawing
$bitmap = New-Object System.Drawing.Bitmap 64, 64
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$graphics.Clear([System.Drawing.Color]::FromArgb(13, 15, 23))
$logoImage = [System.Drawing.Image]::FromFile($logoPath)
$destination = New-Object System.Drawing.Rectangle 4, 8, 56, 44
$source = New-Object System.Drawing.Rectangle 55, 300, 340, 265
$graphics.DrawImage($logoImage, $destination, $source, [System.Drawing.GraphicsUnit]::Pixel)
$icon = [System.Drawing.Icon]::FromHandle($bitmap.GetHicon())
$iconStream = [System.IO.File]::Create($iconPath)
$icon.Save($iconStream)
$iconStream.Dispose()
$icon.Dispose()
$logoImage.Dispose()
$graphics.Dispose()
$bitmap.Dispose()

$sources = @(
    (Join-Path $projectDir 'AssemblyInfo.cs'),
    $generatedVersionPath,
    (Join-Path $projectDir 'Program.cs'),
    (Join-Path $projectDir 'LauncherCore.cs'),
    (Join-Path $projectDir 'LauncherForm.cs')
)
$outputExe = Join-Path $outputDir 'iRacing Digital Teammate.exe'
$outputArgument = '/out:' + $outputExe
$logoResourceArgument = '/resource:' + $logoPath + ',DdsLogo'
$bannerResourceArgument = '/resource:' + $bannerPath + ',DdsBanner'
$updateRepositoryArgument = '/resource:' + $updateRepositoryPath + ',UpdateRepository'

& $compiler /nologo /target:winexe /optimize+ /platform:anycpu `
    ('/win32icon:' + $iconPath) `
    $logoResourceArgument `
    $bannerResourceArgument `
    $updateRepositoryArgument `
    /reference:System.dll `
    /reference:System.Core.dll `
    /reference:System.Drawing.dll `
    /reference:System.Web.Extensions.dll `
    /reference:System.Windows.Forms.dll `
    $outputArgument `
    $sources

if ($LASTEXITCODE -ne 0) {
    throw "Build failed with exit code $LASTEXITCODE"
}

Copy-Item -LiteralPath (Join-Path $projectDir 'README.md') -Destination $outputDir -Force
Write-Output $outputExe
Write-Output ("Version: " + $version)
Write-Output ("Update repository: " + $(if ($repositoryValue) { $repositoryValue } else { '<not configured>' }))
