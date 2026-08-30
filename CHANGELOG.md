# Changelog

All notable changes to iRacing Digital Teammate are documented here.

The project uses semantic versioning. GitHub release tags use the `vX.Y.Z` format.

## [2.3.0] - 2026-08-30

### Added

- Added a silent automatic update check after startup and while Teammate remains active,
  limited to once every 24 hours.
- Added a notification-area alert and menu action when a new version is available.
- Kept update downloads and installation behind explicit user confirmation.

## [2.2.1] - 2026-08-30

### Fixed

- Added a restricted one-time Task Scheduler bridge for the administrator-only CONSPIT Launcher.
- CONSPIT can now start without a UAC prompt on every iRacing session and is stopped through the same elevated task when the session ends.
- Limited the elevated bridge to `ConspitLink2.0.exe` installed under Program Files.

## [2.2.0] - 2026-08-28

### Added

- Added background update downloads from GitHub Releases.
- Added SHA-256 verification before an update installer can run.
- Added silent in-place installation followed by an automatic minimized restart in the notification area.

### Changed

- Updated repository references for the iRacing Digital Teammate product name.

## [2.1.1] - 2026-08-28

### Changed

- Replaced the decorative sidebar navigation with a live status panel for the iRacing session, configured race stack, and Auto Mode.
- Simplified the sidebar hierarchy while preserving the restrained DDS visual treatment.

## [2.1.0] - 2026-08-28

### Added

- Added a per-user Windows installer that creates Start menu entries, registers the app in Installed apps, and provides a standard uninstaller.
- Kept a portable ZIP release for users who do not want to install the application.

### Changed

- Renamed the product to iRacing Digital Teammate.
- Softened the interface to a graphite and near-black base with restrained violet and blue accents inspired by the DDS banner.
- Moved settings to `%APPDATA%\DDS\iRacing Digital Teammate` with automatic migration from previous releases.

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
