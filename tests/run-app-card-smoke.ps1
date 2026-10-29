$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$output = Join-Path $root 'obj\ui-smoke'
$compiler = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
New-Item -ItemType Directory -Path $output -Force | Out-Null
$fake = Join-Path $output 'FakeIconApp.exe'
$smoke = Join-Path $root 'dist\AppCardSmoke.exe'
& $compiler /nologo /target:exe ('/out:' + $fake) (Join-Path $PSScriptRoot 'FakeCrewChief.cs')
if ($LASTEXITCODE -ne 0) { throw 'Icon fixture build failed.' }
& $compiler /nologo /target:exe ('/out:' + $smoke) `
    /reference:System.dll /reference:System.Core.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll `
    ('/reference:' + (Join-Path $root 'dist\iRacing Digital Teammate.exe')) (Join-Path $PSScriptRoot 'AppCardSmoke.cs')
if ($LASTEXITCODE -ne 0) { throw 'App card smoke build failed.' }
& $smoke $fake (Join-Path $output 'cards.png')
if ($LASTEXITCODE -ne 0) { throw 'App card smoke failed.' }
