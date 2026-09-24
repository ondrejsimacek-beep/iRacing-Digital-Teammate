$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$output = Join-Path $root 'obj\lifecycle-smoke'
$compiler = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
New-Item -ItemType Directory -Path $output -Force | Out-Null

$fake = Join-Path $output 'FakeCrewChief.exe'
$smoke = Join-Path $output 'ProcessLifecycleSmoke.exe'
& $compiler /nologo /target:exe ('/out:' + $fake) (Join-Path $PSScriptRoot 'FakeCrewChief.cs')
if ($LASTEXITCODE -ne 0) { throw 'Fake Crew Chief build failed.' }
& $compiler /nologo /target:exe ('/out:' + $smoke) `
    /reference:System.dll /reference:System.Core.dll /reference:System.Management.dll `
    /reference:System.Web.Extensions.dll /reference:System.Xml.dll `
    (Join-Path $root 'LauncherCore.cs') (Join-Path $PSScriptRoot 'ProcessLifecycleSmoke.cs')
if ($LASTEXITCODE -ne 0) { throw 'Process lifecycle smoke build failed.' }
& $smoke $fake
if ($LASTEXITCODE -ne 0) { throw 'Process lifecycle smoke failed.' }
