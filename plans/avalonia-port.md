# Plan: Nocturne for Avalonia

Status: phases 0-6 are done and merged into master (2026-11-19, PR #4): tokens, NocturneWindow, the
standard controls, StatusBar, the own controls, Avalonia's pickers, a plain DataGrid theme, the demo
pages, the gallery and the Admin and Portal sample apps. What is built, how it works and its gotchas
are in CLAUDE.md ("Avalonia port"). Left: NocturneGrid (phase 7), then docs and release (phase 8).
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

- An Avalonia section in `docs/` with the gallery pictures; `release.yml` for own tags
  (e.g. `avalonia-v0.1.0`) and the NuGet package `Nocturne.Avalonia`.
- Freeze the endless animations in the gallery pictures (ProgressRing, the indeterminate ProgressBar),
  as WPF does at 700 ms, so a picture only changes when the look does. Avalonia's animation clock
  (`Animatable.Clock`) is internal in version 12, so it needs another way than in WPF.

## Not in the first version

- ToolBar, RichTextBox and ListView/GridView do not exist in Avalonia (DataGrid covers ListView).

## Not checked yet

- On screen: the opaque ground over Remote Desktop, and the window on macOS and Linux (only the
  headless runs on Linux in CI). Most phase 4-6 controls have only been checked headless.
