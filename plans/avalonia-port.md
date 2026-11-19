# Plan: Nocturne for Avalonia

Status: phases 0-6 are done and merged into master (2026-11-19, PR #4): tokens, NocturneWindow, the
standard controls, StatusBar, the own controls, Avalonia's pickers, a plain DataGrid theme, the demo
pages, the gallery and the Admin and Portal sample apps. Phase 8 (docs and release) is done on
master (2026-11-19): docs, the package README, `avalonia-v*` release tags and frozen animations in the
pictures, and the nuget.org policy allows the new package; left is the first tag. What is built, how
it works and its gotchas are in CLAUDE.md ("Avalonia port"). Left after that: NocturneGrid (phase 7).
Avalonia 12.1.6, Avalonia.Controls.DataGrid 12.1.2.

Decided by the user (2026-11-19): cross-platform (`net10.0`, Windows-only parts behind
`OperatingSystem.IsWindows()`); same repository; Avalonia's own controls styled by the theme, our own
classes only for what Avalonia lacks; NocturneGrid in a later version.

## 7. NocturneGrid

- On `Avalonia.Controls.DataGrid` and the theme's DataGrid styles: search with hits marked in the
  cells, a filter in each column header, grouping through `DataGridCollectionView`, the pager, the
  check-box column and the thumb-drag preview. The WPF version is ~3,500 lines on WPF's DataGrid and
  the heaviest part of the port.
- Then the WPF demo's Grid page and the gallery's NocturneGrid samples (`nocturnegrid`, `-filter`,
  `-groups`, `-paging`), and the sample apps switch from DataGrid with their own search filter to it
  (Admin: Employees and Timesheets; Portal: Orders).
- Open: the DataGrid theme now sits in Nocturne.Avalonia, which references the DataGrid package.
  It could move to a package of its own (as Avalonia ships it) together with NocturneGrid.

## 8. Docs and release

Done:
- `docs/avalonia.md` (getting started, window, dialogs, flyout, accents, fonts, how it differs from WPF),
  `docs/avalonia-controls.md` (every gallery sample) and `docs/avalonia-samples.md`, pictures in
  `docs/screenshots/avalonia/`; `tools/update_docs.py --only avalonia` renders and checks them.
  README, the docs home page and the site description point at them.
- `src/Nocturne.Avalonia/PACKAGE.md` (the nuget.org page), the package icon and release notes link.
- `release.yml`: a tag `avalonia-v1.2.3` packs Nocturne.Avalonia (GitHub release not marked latest).
- Headless animation time is virtual (`Headless.AnimationClock`), and the gallery restarts the
  ProgressRing and indeterminate ProgressBar so they stand at 700 ms as in WPF. The Avalonia demo runs
  in en-US (month and day names in the pictures were Danish).

Left:
- Tag and push `avalonia-v0.1.0` when the user asks.
- A few page pictures (the accent title bar, a popup) can still differ by a shade between runs: the
  scroll bars fade on real-time DispatcherTimers, which the virtual clock does not drive.

## Not in the first version

- ToolBar, RichTextBox and ListView/GridView do not exist in Avalonia (DataGrid covers ListView).

## Not checked yet

- On screen: the opaque ground over Remote Desktop, and the window on macOS and Linux (only the
  headless runs on Linux in CI). Most phase 4-6 controls have only been checked headless.
