# Plan: Nocturne for Avalonia

Status: phases 0-6 are done and merged into master (2026-11-19, PR #4): tokens, NocturneWindow, the
standard controls, StatusBar, the own controls, Avalonia's pickers, a plain DataGrid theme, the demo
pages, the gallery and the Admin and Portal sample apps. Phase 8 (docs, package README, `avalonia-v*`
release tags, frozen animations in the pictures) is done, and Nocturne.Avalonia 0.1.0 is released
(2026-11-19, tag `avalonia-v0.1.0`, on nuget.org). Phase 7 (NocturneGrid) is merged into master
(2026-11-19, PR #5), not released yet. What is built, how it works and its gotchas are in
CLAUDE.md ("Avalonia port").
Avalonia 12.1.6, Avalonia.Controls.DataGrid 12.1.2.

Decided by the user (2026-11-19): cross-platform (`net10.0`, Windows-only parts behind
`OperatingSystem.IsWindows()`); same repository; Avalonia's own controls styled by the theme, our own
classes only for what Avalonia lacks; NocturneGrid in a later version. NocturneGrid and the DataGrid
theme stay in Nocturne.Avalonia (no package of their own).

## 7. NocturneGrid

Done (PR #5):
- `NocturneGrid` on Avalonia's DataGrid, through a `DataGridCollectionView`: the word search with the hits
  marked in `NocturneGridColumn` cells, a filter in every column header (values to tick with their counts, the
  date tree, typed values, wildcards and ranges; the rules ported from WPF), three-state sorting,
  `GroupMemberPath` (Avalonia's group headers, with counts), paging with a pager, `NocturneGridCheckBoxColumn`,
  the counts, `Filtered` and `SelectedItemChanged`. Same property names as in WPF.
- The demo's Data grid page (3,000 cars), the gallery samples `nocturnegrid`, `-filter`, `-groups`, `-paging`,
  `--snapshot` pictures (`grid.png`, `grid-right.png`, `grid-grouped.png`, `popup-filter.png`,
  `popup-filter-dates.png`) and six behaviour checks; the samples use it (Admin: Employees, Timesheets;
  Portal: Orders).

Not ported (WPF-only work-arounds): the glyph-run cell text (`EnableLightweightTextRendering`), the cell style
scoping, the group rows of its own (Avalonia groups natively), the alternating rows within groups.

Left:
- The thumb-drag preview: check on screen whether dragging the scroll bar thumb over the 3,000 rows keeps up
  in Avalonia; port the preview only if it lags.
- Docs for the next Avalonia release: NocturneGrid on `docs/avalonia-controls.md` (the three classes are not
  mentioned yet; `update_docs.py` warns), `PACKAGE.md` and the new gallery pictures.

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
- A few page pictures (the accent title bar, a popup) can still differ by a shade between runs: the
  scroll bars fade on real-time DispatcherTimers, which the virtual clock does not drive.

## Not in the first version

- ToolBar, RichTextBox and ListView/GridView do not exist in Avalonia (DataGrid covers ListView).

## Not checked yet

- On screen: the opaque ground over Remote Desktop, and the window on macOS and Linux (only the
  headless runs on Linux in CI). Most phase 4-6 controls have only been checked headless.
