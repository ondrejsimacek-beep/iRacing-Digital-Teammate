$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$output = Join-Path $root 'obj\lifecycle-smoke'
$compiler = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
New-Item -ItemType Directory -Path $output -Force | Out-Null

$fake = Join-Path $output 'FakeCrewChief.exe'
$external = Join-Path $output 'FakeExternal.exe'
$handoff = Join-Path $output 'FakeHandoffLauncher.exe'
$smoke = Join-Path $output 'ProcessLifecycleSmoke.exe'
Get-Process FakeCrewChief,FakeExternal,FakeHandoffLauncher -ErrorAction SilentlyContinue |
    Where-Object { $_.Path -in @($fake, $external, $handoff) } |
    Stop-Process -Force
& $compiler /nologo /target:exe ('/out:' + $fake) (Join-Path $PSScriptRoot 'FakeCrewChief.cs')
if ($LASTEXITCODE -ne 0) { throw 'Fake Crew Chief build failed.' }
& $compiler /nologo /target:exe ('/out:' + $external) (Join-Path $PSScriptRoot 'FakeCrewChief.cs')
if ($LASTEXITCODE -ne 0) { throw 'Fake external app build failed.' }
& $compiler /nologo /target:exe ('/out:' + $handoff) (Join-Path $PSScriptRoot 'FakeHandoffLauncher.cs')
if ($LASTEXITCODE -ne 0) { throw 'Fake handoff launcher build failed.' }
& $compiler /nologo /target:exe ('/out:' + $smoke) `
    /reference:System.dll /reference:System.Core.dll /reference:System.Management.dll `
    /reference:System.Web.Extensions.dll /reference:System.Xml.dll `
    (Join-Path $root 'LauncherCore.cs') (Join-Path $PSScriptRoot 'ProcessLifecycleSmoke.cs')
if ($LASTEXITCODE -ne 0) { throw 'Process lifecycle smoke build failed.' }
& $smoke $fake $external $handoff
if ($LASTEXITCODE -ne 0) { throw 'Process lifecycle smoke failed.' }
