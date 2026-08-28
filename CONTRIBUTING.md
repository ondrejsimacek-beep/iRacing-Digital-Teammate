# Contributing

Thanks for helping improve iRacing Digital Teammate.

## Before opening a change

1. Open an issue for bugs or proposed features.
2. Keep changes focused and avoid unrelated formatting rewrites.
3. Do not add third-party branding or artwork without documented permission.
4. Never commit personal paths, generated `dist/` files, or application settings.

## Local verification

Run the build from Windows PowerShell:

```powershell
.\build.ps1
```

The build must complete without warnings or errors. Launch the resulting executable
from `dist/` and verify the modified workflow without starting unrelated applications.

## Versioning

Only maintainers update `version.txt`. User-facing changes must be added to
`CHANGELOG.md`. Release tags must exactly match `v` followed by the value in
`version.txt`.
