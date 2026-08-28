# Changelog

All notable changes to iRacing Teammate are documented here.

The project uses semantic versioning. GitHub release tags use the `vX.Y.Z` format.

## [2.0.0] - 2026-08-28

### Changed

- Rebranded the launcher from its previous identity to Digital Downforce Sim Racing (DDS).
- Reworked the full interface around the DDS violet, magenta, blue, cyan, and silver palette.
- Replaced the previous artwork, application icon, tray icon, company metadata, and public documentation with the supplied DDS logo and banner.
- Moved settings to `%APPDATA%\DDS\iRacing Teammate` while automatically migrating existing user configuration.

## [1.2.3] - 2026-08-26

### Added

- Automatic detection and lifecycle support for Edge Overlays.

## [1.2.2] - 2026-08-20

### Added

- Notification-area mode with restore and explicit exit actions.

### Changed

- Start with Windows now launches Teammate minimized to the notification area.
- Closing or minimizing the main window keeps Auto Mode running in the tray.

## [1.2.1] - 2026-08-19

### Added

- Automatic detection and lifecycle support for CONSPIT Launcher and SimConnect Manager.

### Fixed

- Track replacement and child processes started by self-updating launchers, so
  Electron/Squirrel applications such as irDashies close with the iRacing session.

## [1.2.0] - 2026-08-19

### Added

- Start with Windows toggle for the current user.
- Auto Mode tied to the real iRacing simulator session lifecycle.
- Automatic companion-app cleanup after a three-second session-end confirmation.
- GitHub Releases update checker.
- Automatic repository embedding during GitHub Actions builds.
- GitHub build and release workflows.
- Support for irDashies, GO Fast, iRSidekick, VRS Telemetry Logger, Kapps,
  Joel Real Timing, OpenKneeboard, and Marvin's AIRA.
- Hide/show controls for individual application cards.

### Changed

- iRacing detection now reads its installed location and scans fixed-drive game folders.
- Garage61 detection supports its current roaming installation directory.
- Brand artwork is embedded in the executable.

## [1.0.0] - 2026-08-19

- Initial Windows launcher with sequential race-stack launch, safe tracked-process
  shutdown, application discovery, persistent configuration, and livery-inspired UI.
