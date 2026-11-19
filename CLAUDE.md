# CLAUDE.md

Nocturne for WPF: a dark theme for .NET 10+, built from the user's design canvas (claude.ai artifact
"Nocturne – WPF kontrolbibliotek", boards Main and Tokens). It was called Frosted Glass until the
glass look was dropped.
Structured like the user's JetPack theme (`C:\Git\JetpackTheme`): one library, one merged
dictionary, MahApps-style resource keys with a `Nocturne.` prefix.

## Rules

- **Everything in the repo is English**: code, comments, XAML strings, demo text, READMEs, commit
  messages. The user chats in Danish; replies can be Danish.
- Keep the design's values (colours, radii, sizes, 120/150 ms transitions). The Tokens board is the
  spec; `Styles/Brushes.xaml` holds the colours.
- Window defaults chosen by the user after trying them on screen (they differ from the design's
  Mica / 55 % / 80 %): `Backdrop` Acrylic, `TintOpacity` 0.5. Over Remote Desktop (WPF renders in
  software there) the window paints its opaque ground instead unless `UseBackdropInRemoteSession` is
  set; it follows the session live (WM_WTSSESSION_CHANGE). The user often works over RDP, so on screen
  they see the opaque ground unless the demo's "Also over Remote Desktop" box is ticked.
- No grain/noise texture. The design had one; the user tried many versions (sandblasted, hammered,
  1:1 pixels) and had it removed: it added nothing and turned to mud over RDP compression.
- Accents: `NocturneTheme.SetAccent` (`Controls/NocturneTheme.cs`) writes the accent brushes,
  `Text.OnAccent`, primary button, `Popup`, `Window.Tint` and `Window.Fallback` for blue (the
  default, chosen by the user; `Brushes.xaml` holds its values), green (the design's colour),
  orange, yellow (dark text on it), teal or `System` into the app's resources. `System` reads
  Windows' 7-shade `AccentPalette` from HKCU\...\Explorer\Accent (no Windows SDK target needed),
  derives the ground from its hue and picks white or dark `OnAccent` by luminance; NocturneWindow's
  WndProc (WM_SETTINGCHANGE, WM_DWMCOLORIZATIONCOLORCHANGED) makes it follow live. New
  accent-coloured brushes must be added to the palettes too. `--snapshot` writes `accent-<name>.png`.
- The light from the top-left corner is drawn dithered (`Controls/LightTexture.cs`, `PART_Light`),
  pixel for pixel and redrawn 150 ms after a resize. As a plain gradient (8 % to 0) it has ~20
  steps that show as diagonal bands, worst over RDP. `--snapshot`
  writes `light.png` (gradient left, dithered right; stretch the contrast to compare).
- Everything is MIT. No third-party images
  or fonts in the repo. The font is Segoe UI (the design used Figtree; apps can override
  `Nocturne.Fonts.Family`).
- The licence exception (approved by the user, as in JetPack): `NocturneIcon` draws Material Design
  Icons (Apache 2.0), kept unchanged in `icons/mdi/` with their licence in `icons/` and a note in
  `NOTICE.md`; the package licence is `MIT AND Apache-2.0`. Add icons only from that set: the MDI
  name in `icons/icons.json`, then `python tools/generate_icons.py --fetch` (CI runs `--check`).
  They are filled; the theme's own `Nocturne.Icons.*` line icons (`StrokeIcon`) stay for the
  templates. Shortcuts: `{nc:Icon Bell}` (a new NocturneIcon, for `Icon`/`Content`; not in Style
  setters, one element cannot have two parents) and `{nc:IconGeometry Bell}` (the geometry).
- This is the basic control set. Grow it step by step; do not add a large set of extra controls
  without the user asking.
- **Docs and screenshots are updated only for a release.** Code changes leave `docs/` (pages,
  `resource-keys.md`, screenshots), `README.md` and `PACKAGE.md` alone; take snapshots into a scratch
  folder to check the look. Before tagging, `/update-docs` (`.claude/skills/update-docs`) brings
  everything up to date with the changes since the previous docs update. A new control gets a sample
  in `Views/GalleryPage.xaml` and a section on its docs page.

## Layout

```
src/Nocturne.Wpf/              library (net10.0-windows, NuGet package Nocturne.Wpf)
  Styles/Nocturne.xaml         the one dictionary apps merge; merges everything below
  Styles/Brushes.xaml, Fonts.xaml, Icons.xaml
  Styles/Controls.*.xaml      one file per control group
  Themes/Generic.xaml         merges Nocturne.xaml (implicit styles don't reach MainWindow : NocturneWindow)
  Controls/                   NocturneWindow (hosts Flyouts and dialogs), NocturneTheme (accents), LightTexture,
                              StrokeIcon, NocturneIcon (+ Icon/IconGeometry extensions; NocturneIconKind.g.cs
                              generated), TabPanelBorder, TabSelectionIndicator, NativeMethods; own controls (behaviour ported
                              from JetPack, look new): DropDownButton, SplitButton, Tile (Buttons.xaml),
                              NumericUpDown (Text), ProgressRing, RangeSlider, Rating (Range), InfoBar,
                              Badged, BusyIndicator, Accordion (Containers), HamburgerMenu (Lists),
                              TimePicker, ColorPicker, AutoSuggestBox, SearchComboBox (Pickers.xaml),
                              NocturneDialog, Flyout (Overlays.xaml), NocturneGrid with NocturneGridColumn,
                              NocturneGridCheckBoxColumn and its Grid* helpers (DataGrid.xaml; JetPackGrid renamed)
  Helpers/                    ControlHelper, PasswordBoxHelper, TabHelper, ScrollViewerHelper, FocusHelper,
                              ContextMenuHelper (attached properties); Converters (tree indent, splitter direction)
src/Nocturne.Wpf.Demo/         demo app; pages in Views/, Snapshot.cs for --snapshot, RenderCommand.cs for
                              --render / --render-gallery (one control, no demo window); Views/GalleryPage.xaml
                              has one Demo.Sample per control (Tag = file name), GallerySnapshot.cs renders
                              each to controls/<Tag>.png with popups marked demo:Gallery.Open drawn open
samples/                      two sample apps built with the theme, in the solution: Nocturne.Wpf.Samples.Admin
                              (HamburgerMenu; dashboard, employees, timesheets, projects, settings flyout,
                              delete dialog) and .Portal (ListBox.Navigation side bar; home, products, orders,
                              customers, about). Fixed data in SampleData.cs, one UserControl per page in Views/;
                              Shared/ has WindowSnapshot.cs (--snapshot: admin-*.png / portal-*.png) and
                              SampleIcons.xaml (Sample.Icons.*, icons the theme lacks)
docs/                         GitHub Pages site (Just the Docs, colour scheme in _sass); pages per control
                              group, screenshots/ (pages) and screenshots/controls/ (gallery)
tools/update_docs.py          builds the demo and the samples, runs --snapshot, copies the pictures to docs/
                              (samples to screenshots/samples/), regenerates
                              docs/resource-keys.md (generate_resource_keys.py) and runs the checks
                              (StaticResource order, images, every control documented); --check: checks only
icons/                        icons.json (the MDI names in the set), mdi/*.svg, LICENSE (Apache 2.0)
tools/generate_icons.py       icons.json + icons/mdi -> Controls/NocturneIconKind.g.cs; --fetch, --check
tools/generate_icon.ps1       draws src/Nocturne.Wpf/icon.png, the package icon (System.Drawing)
.github/workflows/            ci.yml (build and pack on every push and PR), release.yml (version tags)
```

XAML namespace `urn:nocturne-wpf`, prefix `nc`. Brushes are referenced with `DynamicResource`.

## Build and verify

```bash
dotnet build NocturneTheme.slnx
src/Nocturne.Wpf.Demo/bin/Debug/net10.0-windows/Nocturne.Wpf.Demo.exe --snapshot <scratch folder>
```

- `--snapshot` renders the six pages (controls, more, custom, grid, tokens, icons: top, halfway down a long page, and the bottom), the popups
  (combo box, context menu, tool tip, date picker, drop-down button, time picker, colour picker,
  search combo box, two NocturneGrid filters), an open dialog, the open flyout, the folded
  HamburgerMenu, the grid grouped by make and the Separated and Accent title bars to PNG, one picture per gallery sample into `controls/`,
  and writes `wheel-check.txt` (PASS when a wheel turn over the settings ListBox scrolls the page),
  then exits. RenderTargetBitmap cannot see Mica, so it switches the
  backdrop to None (opaque fallback ground). Look at the PNGs after every visual change; keep them
  out of the repo.
- For one control, use `/render-control` (`.claude/skills/render-control`): `--render <snippet.xaml> <out.png>`
  or `--render-gallery <Tag,…> <folder>` (`RenderCommand.cs`) draw a XAML snippet or chosen gallery
  samples in about a second, with `--scale`, `--time` (animation moments) and `--accent`. Look at
  details with those; run the full `--snapshot` once a visual change is done.
- The sample apps take `--snapshot <folder>` too (one picture per page, off screen at 1280×800).
  A change to a control or style shows in them as well, so check them after visual changes.
- The user often runs the demo from Visual Studio, which locks `bin\...`. Do not kill it; build to
  another folder: `dotnet build src/Nocturne.Wpf.Demo -o <scratch>/out`.
- On-screen captures (`CopyFromScreen`) fail while the RDP window is minimised ("The handle is
  invalid"); use `--snapshot` then.

## Avalonia port (Nocturne.Avalonia)

The plan, its decisions and its phases are in `plans/avalonia-port.md`; keep its Status line current.
Decided by the user: cross-platform (`net10.0`, Windows-only parts behind `OperatingSystem.IsWindows()`),
same repo, Avalonia's own controls where they exist (NumericUpDown, TimePicker, ColorPicker, …) styled
by the theme, our own classes only for what Avalonia lacks, NocturneGrid in a later version.

```
src/Avalonia.props             the Avalonia version, imported by every Avalonia project
src/Nocturne.Avalonia/         library (net10.0, package Nocturne.Avalonia, xmlns urn:nocturne-avalonia, prefix nc)
  NocturneTheme.axaml(.cs)     the Styles class apps add to Application.Styles (<nc:NocturneTheme />); merges Styles/*;
                               NocturneTheme.Accents.cs: SetAccent, System accent (Windows' AccentPalette, else Avalonia's)
  Styles/                      Brushes, Icons (generated), Fonts, Controls.*.axaml (ControlThemes keyed {x:Type …});
                               Controls.Base.axaml holds the hosts every standalone theme needs (PopupRoot,
                               OverlayPopupHost, AdornerLayer with the focus ring, FlyoutPresenter, ItemsControl, …)
  Controls/                    NocturneWindow, LightTexture, NativeMethods (Windows only), NocturneIcon (+ Icon/IconGeometry
                               extensions; NocturneIconKind.g.cs generated), StrokeIcon, TabSelectionIndicator,
                               NocturneAccent.g.cs (generated)
  Helpers/                     ControlHelper (attached properties), NocturneConverters
src/Nocturne.Avalonia.Demo/    demo (Views/ControlsPage, TokensPage); Headless.cs, Snapshot.cs (--snapshot), RenderCommand.cs (--render)
tools/generate_avalonia_tokens.py   Brushes.xaml, Icons.xaml and the accent palettes of NocturneTheme.cs (WPF) ->
                               Styles/Brushes.axaml, Styles/Icons.axaml, Controls/NocturneAccent.g.cs; --check in CI
```

- The theme stands alone (no Fluent or Simple theme under it), so every control needs a ControlTheme
  from us or it draws nothing. Avalonia's own Simple theme (same version) is the reference for each
  control's template parts (`PART_*`) and pseudo-classes.
- The WPF library stays the source of the design values: change a colour, line icon or palette there
  and run `python tools/generate_avalonia_tokens.py` (CI runs `--check`); `generate_icons.py` writes both
  icon enums.
- `--snapshot` / `--render` run on the headless platform with Skia and system fonts: no screen, a few
  seconds, the same on Linux (CI's `avalonia-linux` job uploads its snapshots). `/render-avalonia`
  (`.claude/skills/render-avalonia`) describes them. Popups are drawn into the window there
  (OverlayPopups); `NocturneWindow.AlwaysDrawTitleBar` draws the title bar, which headless cannot extend into.
- NocturneWindow extends the client area and marks its title bar and caption buttons with Avalonia 12's
  `WindowDecorationProperties.ElementRole` (TitleBar, MinimizeButton, MaximizeButton, CloseButton; User for
  commands inside the bar): Windows drags, snaps and shows the snap layouts itself. macOS keeps its traffic
  lights (`:macos` hides our buttons). Remote Desktop and the transparency switch are polled every 2 s (no
  public WndProc hook). Checked on screen on Windows 11: Acrylic, hover on all three buttons, snap layouts,
  maximised.
- Avalonia's packages send build statistics; the empty `AvaloniaStats` target in
  `Directory.Build.targets` turns that off. Keep it.
- `nuget.config` limits package sources to nuget.org (the user's global config lists a local
  `C:\Git\Nuget` that does not exist and breaks restore).

### Avalonia gotchas

- `.axaml` files use `x:Double` (not `sys:Double`); relative gradient points need `%` (`0%,0%`; `0,1` is pixels).
- A `BasedOn` theme gives its setters to the template's children too when its `TargetType` matches them:
  a shared base with `TargetType="TemplatedControl"` painted the field ground on a TextBox's scroll viewer.
  Keep base themes as narrow as the control (the field themes repeat their setters instead).
- A selector may hold one `/template/` (no `^ /template/ Thumb /template/ Border`): reach into a nested
  template through the inner control's own properties (the scroll thumb's bar follows its `Padding`).
- Attached properties cannot be selected on (`[(h|ControlHelper.X)=True]` does not parse):
  `ControlHelper.ClearButton` / `HasError` set the classes `clearButton` / `hasError`, which the themes
  select; elsewhere bind visibility with a MultiBinding.
- The window templates need a `VisualLayerManager` named `PART_VisualLayerManager`, or overlay popups
  find no layer. NocturneWindow's wraps the title bar too, so its commands can open popups.
- `DataValidationErrors` takes the field's `Background` and `Padding`; its presenter must not bind them.
- ToggleSwitch moves `PART_MovingKnobs` from 0 to the width of `PART_SwitchKnob` (keep that canvas as
  wide as the travel, 20).
- Transitions: `Transitions` on the part (120/150 ms); rotate chevrons with `RenderTransform="rotate(…)"`
  and a `TransformOperationsTransition`. Headless captures run 30 frames (0.5 s) so they finish.

## Releases

- Repo: github.com/Montago/NocturneTheme, default branch `master`.
- A tag `v1.2.3` runs `release.yml`: it packs Nocturne.Wpf with that version (the tag overrides
  `<Version>` in the csproj) and creates a GitHub release with the `.nupkg` and `.snupkg` attached.
  Tag with `git tag -a v1.2.3 -m "Nocturne.Wpf 1.2.3"` and push the tag; tag and push only when the
  user asks.
- nuget.org (Trusted Publishing) is on since 0.1.2: the release workflow pushes to nuget.org because
  the repository variable `NUGET_PUBLISH` is `true` and the secret `NUGET_USER` is set (the user set
  both, and the nuget.org policy "Nocturne Publishing" for Montago/NocturneTheme, release.yml). Never
  add API keys or secrets to the repo. A failed push can be retried by re-running the release run.

## WPF gotchas (same as JetPack)

- StaticResource / BasedOn across the merged files fails at runtime: cross-file references are
  `DynamicResource`, and a `BasedOn` style lives in the same file as its base.
- Attached properties in binding *paths* need `nc:` (`urn:nocturne-wpf`); setters, `TemplateBinding`
  and triggers are fine with the `clr-namespace` prefixes `c:` / `h:`.
- TabItem header fonts and colours sit on the header presenter (`TextElement.*`), never on the
  TabItem, or the page content inherits them.
- TextBox and PasswordBox apply `Padding` to their text themselves; the placeholder uses
  `Margin="{TemplateBinding Padding}"` plus the 2px text inset.
- A ScrollViewer in a template takes its scroll bar visibility from the templated parent's attached
  `ScrollViewer.*` properties; popups (ContextMenu, MenuItem) set them to Auto, or an empty bar shows.
- WPF's ScrollViewer swallows every wheel turn, even with nothing to scroll. The Nocturne ScrollViewer
  style turns on `ScrollViewerHelper.ChainMouseWheel`: the innermost scroll viewer under the mouse
  passes a turn it cannot use to the nearest outer one that can (visual parents only, so popups
  never scroll the page).
- Scroll bars are 18px wide with a thumb that fills that width (the drawn bar is 6px, 10px under
  the mouse), so a bar along the window edge stays clear of the 6px resize border.
  `ScrollViewerHelper.AutoHide` (on in the ScrollViewer styles) fades the bars out while the mouse
  is away and in on hover or scrolling; their room stays. Snapshots show no bars for that reason.
- The segmented TabControl draws the selected pill and stripe with `TabSelectionIndicator` (code,
  `CompositionTarget.Rendering`), which glides them to a new tab; the TabItem template hides its own
  while the inherited `TabSelectionIndicator.DrawsSelection` is true. Like the theme's other
  transitions it ignores Windows' animation effects setting, which is off in most RDP sessions.
- Fields use `Nocturne.Templates.ValidationError` (Text.xaml): the field draws its red frame on
  `Validation.HasError`, and a popup callout under it shows the first error on focus or hover.
  `demo:Gallery.Open` opens it for a picture. `Nocturne.Templates.NoErrorAdorner` leaves it out.
- Switches stay a style (`Nocturne.Styles.ToggleSwitch`) on WPF's ToggleButton / CheckBox: the user
  wants WPF's own controls rather than new classes that only rename them.
- Effects (DropShadowEffect) sit on separate background layers, never on text.
- ToolBar gives its children the styles under `ToolBar.*StyleKey`, not the implicit ones. Each lives
  in the file of its base style (Buttons, Lists, Text, Toggles), since `BasedOn` cannot cross files.
- In `ControlTemplate.Triggers`, a DataTrigger's `RelativeSource Self` is the templated control;
  `RelativeSource TemplatedParent` is one level further up.
- Calendar binds its day buttons' `Style` to `CalendarDayButtonStyle`; set it (and
  `CalendarButtonStyle`) explicitly, an implicit style alone may not reach them.
  `CalendarDayButton.IsToday` is set whatever `Calendar.IsTodayHighlighted` says; the day template
  checks both (the gallery turns the ring off so its pictures do not change with the date).
- DataGrid rows choose their template (with or without a row header) from the inherited
  `DataGridHelper.HeadersVisibility` in triggers, not a binding to the ancestor DataGrid: NocturneGrid
  refreshes and recycles rows, and an ancestor binding can give up for good. NocturneGrid's code needs
  the row template's `DGR_Border`, the ScrollViewer template's `PART_ScrubLayer` and
  `GridColumnHeadersPresenter` (both in the shared `Nocturne.Templates.DataGrid.ScrollViewer`).
- A FlowDocument does not take the RichTextBox's font (justified Georgia); the RichTextBox style
  gives FlowDocument the theme font in its resources.
- NocturneWindow's title bar: `LeftWindowCommands` (before the icon, with a divider), `TitleBarStyle`
  (Seamless, Separated, Accent: a strip `TitleBarBg` behind it; the Accent brushes are in the accent
  palettes) and `CornerStyle` (DWMWA_WINDOW_CORNER_PREFERENCE: Round, RoundSmall, Square).
- Text fields get `Nocturne.ContextMenu.Text` (Menu.xaml, `x:Shared="False"`; PasswordBox gets
  `.Password`, Paste only). A MenuItem in a ContextMenu runs its command on itself and does not
  re-ask CanExecute when a `CommandTarget` binding arrives later, so `ContextMenuHelper.TargetsPlacement`
  points the items at `PlacementTarget` on every opening. Read-only fields hide the editing items
  (DataTrigger on `PlacementTarget.IsReadOnly`); with `SpellCheck.IsEnabled` the field keeps WPF's own
  menu, which lists the suggestions.
- A grouped NocturneGrid alternates its rows within each group through the attached
  `NocturneGrid.GroupRowBackground`, which the DataGridRow style binds as its `Background`. Never set a
  row's `Background` locally: a local value hides the style's hover and selection triggers.
- Flyout ignores Windows' animation setting like the other transitions, moves keyboard focus to its first
  field on opening and gives it back on closing. TimePicker and ColorPicker take the keyboard as a
  ComboBox does (Alt+Down / F4 open, arrows move the highlight only, Enter takes, Escape closes).
- On-screen tests drive the real mouse and keyboard: check that the demo is the foreground window and
  under the pointer before every key or click (it was not once, and the keys went into another app).
- NocturneWindow extends the DWM frame over the whole window (GlassFrameThickness -1) while a backdrop
  is active and removes WS_SYSMENU so Windows does not draw its own caption buttons (same approach
  as JetPackWindow).
