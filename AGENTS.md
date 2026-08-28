# iRacing Digital Teammate project guidance

## Product identity

- Product: **iRacing Digital Teammate — Teammate for your software** by Digital Downforce Sim Racing (DDS).
- Keep a graphite and near-black foundation with restrained violet and blue accents inspired by the DDS banner. Avoid large saturated purple surfaces.
- Keep the supplied DDS logo and banner as the authoritative brand assets.
- Keep sidebar elements functional: the status panel reports session, stack readiness, and Auto Mode; do not add decorative navigation that suggests unavailable pages.
- The primary user language is Czech; repository documentation and public release notes are English unless requested otherwise.

## Core behavior and safety

- Auto Mode starts enabled companion apps only when an actual `iRacingSim*` session process appears, not when only the iRacing UI opens.
- After a confirmed session end, stop only processes that Teammate started for that session. Never close instances that were already running.
- Preserve support for launchers that hand execution to replacement or child processes, especially Electron/Squirrel apps such as irDashies.
- Use exact process-name queries. Do not reintroduce broad process enumeration or antivirus exclusions.
- Use the current user's standard Startup folder shortcut for Start with Windows. Do not restore registry Run-key persistence.
- VRS Setup Downloader is a separate DDS project and must never be bundled or listed here. VRS Telemetry Logger is an allowed external companion app.

## Supported local priorities

- Garage61, irDashies, and Edge Overlays are important integrations.
- CONSPIT Launcher (`ConspitLink2.0.exe`) and SimConnect Manager (`SimConnectManager.exe`) are user-specific priorities and should remain automatically detected.
- Keep per-app USE/HIDE/BROWSE controls and allow hidden cards to be restored.

## Architecture and build

- This is a dependency-free Windows Forms application targeting the installed .NET Framework compiler; there is intentionally no NuGet dependency or project file.
- `version.txt` is the single release-version source. `build.ps1` generates assembly version metadata and embeds the GitHub update repository; `build-installer.ps1` creates the per-user Inno Setup installer.
- Build locally with:
  `powershell -ExecutionPolicy Bypass -File .\build.ps1 -UpdateRepository ondrejsimacek-beep/iRacing-Digital-Teammate`
- The installer must create a Start menu entry, register the app in Windows Installed apps, and provide an uninstaller without requiring administrator rights.
- Generated files belong in `dist/` or `obj/` and must remain ignored by Git.
- Public repository: `https://github.com/ondrejsimacek-beep/iRacing-Digital-Teammate`.

## Verification and releases

- After process-lifecycle changes, test a launcher that exits after spawning a differently named target and verify `StopTracked` closes the handed-off process.
- Run a screenshot smoke test and inspect the resulting UI after layout or catalog changes.
- Check `git diff --check`, confirm no local absolute paths or VRS Setup Downloader references leaked into tracked files, and verify no new Microsoft Defender detection after the final build.
- For a release: work on a non-protected feature branch, update `version.txt` and `CHANGELOG.md`, build with the repository embedded, commit and push the branch, open a pull request, wait for the required `build` check, merge it, update local `main`, create and push annotated tag `vX.Y.Z`, wait for the release workflow, and verify both the installer and portable ZIP against their published SHA-256 files.
- Do not claim the executable is digitally signed until a real code-signing certificate is configured.
