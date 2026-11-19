# Plan: Nocturne for Avalonia

Status: phases 0-3 done (2026-11-19, branch `feature/avalonia`); phase 4 next. Avalonia 12.1.6.
Left from phase 3 for later: document tabs (TabHelper close/add/badge), StatusBar (phase 4),
Avalonia's DatePicker/TimePicker/ColorPicker/AutoCompleteBox (phase 5, with the own controls), and
scroll bars that fade in while the content scrolls (they fade in on hover only).

Decided by the user (2026-11-19): 1. cross-platform; 2. same repository; 3. Avalonia's own
controls, styled by the theme, plus our own classes for what Avalonia lacks; 4. NocturneGrid waits
for a later version.

This is not a straight copy. The design values (colours, radii, sizes, 120/150 ms transitions),
the icons, the resource keys and the logic of the custom controls carry over almost unchanged.
Every template has to be rewritten: Avalonia uses `ControlTheme` and selectors with pseudo-classes
(`:pointerover`, `:pressed`, `:checked`) instead of triggers, and its template parts have other
names. The WPF library is about 15,500 lines (styles for ~60 control types, 25 custom controls);
NocturneGrid alone is ~3,500 lines on top of WPF's DataGrid and is the heaviest part.

## Decisions before starting

1. **Windows only or cross-platform?**
   Recommended: target `net10.0` and keep the Windows parts (backdrop, Remote Desktop detection,
   the 7-shade `AccentPalette` from the registry) behind `OperatingSystem.IsWindows()`. macOS and
   Linux get the opaque ground (or Acrylic on macOS). Segoe UI is missing there, so
   `Nocturne.Fonts.Family` becomes a fallback list. Still no fonts in the repo.
2. **Same repository?**
   Recommended: yes. `src/Nocturne.Avalonia`, `src/Nocturne.Avalonia.Demo` and Avalonia samples in
   the same solution, sharing `icons/`, `tools/` and `docs/`. Own tags (e.g. `avalonia-v0.1.0`) so
   the two packages are versioned separately.
3. **Avalonia's own controls or ours?**
   Avalonia already has NumericUpDown, TimePicker, ColorPicker, DropDownButton, SplitButton,
   AutoCompleteBox, ToggleSwitch and SplitView. Recommended: style those instead of porting ours,
   and build only what Avalonia lacks: Tile, Badged, InfoBar, ProgressRing, BusyIndicator, Rating,
   RangeSlider, Accordion, HamburgerMenu, SearchComboBox, NocturneDialog, Flyout. The cost: the
   API is not 1:1 with the WPF package.
4. **NocturneGrid in the first version?**
   Recommended: no, a later version. It carries the most risk; the rest can ship without it.

## Phases

### 0. Skeleton and tools
- Projects, CI build, XAML namespace `urn:nocturne-avalonia` with prefix `nc`.
- `--snapshot` and `--render` on `Avalonia.Headless` (Skia, no window, also on Linux in CI), so
  pictures can be checked from day one. Try whether popups can be captured headless.
- An Avalonia variant of the `/render-control` skill.

### 1. Tokens and foundation
- `Brushes`, `Fonts`, `Icons` with the same `Nocturne.` keys.
- `tools/generate_icons.py` also writes an Avalonia `NocturneIconKind.g.cs` (`--check` covers both).
- `NocturneIcon`, `StrokeIcon`, `{nc:Icon}` / `{nc:IconGeometry}`.
- `NocturneTheme.SetAccent` with all palettes; `System` follows live through
  `PlatformSettings.ColorValuesChanged` (plus the registry palette on Windows).
- A CI check that compares the colour values with `src/Nocturne.Wpf/Styles/Brushes.xaml`, so the
  two themes cannot drift apart.

### 2. NocturneWindow
- `ExtendClientAreaToDecorationsHint`, `TransparencyLevelHint` (Acrylic default, Mica), tint 0.5.
- Opaque ground over Remote Desktop unless `UseBackdropInRemoteSession`; following the session live
  still needs a Win32 hook (WM_WTSSESSION_CHANGE) through the platform handle.
- `CornerStyle`, `TitleBarStyle` (Seamless, Separated, Accent), `LeftWindowCommands`.
- `LightTexture` on `WriteableBitmap`. Keep the dithering: Skia gradients band over RDP too.

### 3. Standard controls (one file per group, as now)
- Buttons, Toggles, Text (validation through `DataValidationErrors`), ScrollViewer / ScrollBar
  (Avalonia has `AllowAutoHide` and scroll chaining built in, so most of `ScrollViewerHelper`
  goes away), Lists, Menu / ContextMenu (text field menus), Containers (TabControl with the gliding
  indicator through Transitions instead of `CompositionTarget.Rendering`), Range, Calendar /
  CalendarDatePicker, ToolTip.
- 120/150 ms become `Transitions`; `DropShadowEffect` becomes `BoxShadow`.

### 4. Gaps in Avalonia
- StatusBar and GroupBox: styles on existing panels / `HeaderedContentControl`.
- ToolBar, RichTextBox, ListView/GridView do not exist: leave them out of the first version
  (DataGrid covers ListView/GridView).

### 5. Custom controls
- Port the logic, rewrite the templates. Flyout and NocturneDialog move to the window's
  `OverlayLayer`.

### 6. Demo and samples
- The demo pages, the gallery (one sample per control) and the Admin and Portal sample apps.

### 7. NocturneGrid (later version)
- On `Avalonia.Controls.DataGrid`: columns, filters, grouping through `DataGridCollectionView`,
  the check-box column and the thumb-drag preview.

### 8. Docs and release
- An Avalonia section in `docs/`, `release.yml` for the new tags, NuGet package `Nocturne.Avalonia`.

## Gotchas
- WPF gotchas that go away: StaticResource / BasedOn across files, TabItem leaking its font,
  Calendar styles not reaching the day buttons, the wheel helper.
- New ones to watch: `ControlTheme` and `Style` are different things; template parts must have
  the exact `PART_…` names Avalonia expects; popups are separate top-levels.
- CLAUDE.md gets an Avalonia section once work starts.
