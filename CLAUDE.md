# CLAUDE.md

Frosted Glass for WPF: a dark frosted-glass theme for .NET 8+, built from the user's "Frosted Glass"
design canvas (claude.ai artifact "Frosted Glass – WPF kontrolbibliotek", boards Main and Tokens).
Structured like the user's JetPack theme (`C:\Git\JetpackTheme`): one library, one merged
dictionary, MahApps-style resource keys with a `Frosted.` prefix.

## Rules

- **Everything in the repo is English**: code, comments, XAML strings, demo text, READMEs, commit
  messages. The user chats in Danish; replies can be Danish.
- Keep the design's values (colours, radii, sizes, 120/150 ms transitions). The Tokens board is the
  spec; `Styles/Brushes.xaml` holds the colours.
- Window defaults chosen by the user after trying them on screen (they differ from the design's
  Mica / 55 % / 80 %): `Backdrop` Acrylic, `TintOpacity` 0.5.
- No grain/noise texture. The design had one; the user tried many versions (sandblasted, hammered,
  1:1 pixels) and had it removed: it added nothing and turned to mud over RDP compression.
- Accents: `FrostedTheme.SetAccent` (`Controls/FrostedTheme.cs`) writes the accent brushes,
  `Text.OnAccent`, primary button, `Popup`, `Window.Tint` and `Window.Fallback` for green (default),
  orange, blue, yellow (dark text on it), red or `System` into the app's resources. `System` reads
  Windows' 7-shade `AccentPalette` from HKCU\...\Explorer\Accent (no Windows SDK target needed),
  derives the ground from its hue and picks white or dark `OnAccent` by luminance; FrostedWindow's
  WndProc (WM_SETTINGCHANGE, WM_DWMCOLORIZATIONCOLORCHANGED) makes it follow live. New
  accent-coloured brushes must be added to the palettes too. `--snapshot` writes `accent-<name>.png`.
- The light from the top-left corner is drawn dithered (`Controls/LightTexture.cs`, `PART_Light`),
  pixel for pixel and redrawn 150 ms after a resize. As a plain gradient (8 % to 0) it has ~20
  steps that show as diagonal bands, worst over RDP. `--snapshot`
  writes `light.png` (gradient left, dithered right; stretch the contrast to compare).
- Everything is MIT. No third-party images
  or fonts in the repo. The font is Segoe UI (the design used Figtree; apps can override
  `Frosted.Fonts.Family`).
- This is the basic control set. Grow it step by step; do not add a large set of extra controls
  without the user asking.

## Layout

```
src/Frosted.Wpf/              library (net8.0-windows, NuGet package Frosted.Wpf)
  Styles/Frosted.xaml         the one dictionary apps merge; merges everything below
  Styles/Brushes.xaml, Fonts.xaml, Icons.xaml
  Styles/Controls.*.xaml      one file per control group
  Themes/Generic.xaml         merges Frosted.xaml (implicit styles don't reach MainWindow : FrostedWindow)
  Controls/                   FrostedWindow (hosts Flyouts and dialogs), FrostedTheme (accents), LightTexture,
                              StrokeIcon, TabPanelBorder, NativeMethods; own controls (behaviour ported
                              from JetPack, look new): DropDownButton, SplitButton, Tile (Buttons.xaml),
                              NumericUpDown (Text), ProgressRing, RangeSlider, Rating (Range), InfoBar,
                              Badged, BusyIndicator, Accordion (Containers), HamburgerMenu (Lists),
                              TimePicker, ColorPicker, AutoSuggestBox, SearchComboBox (Pickers.xaml),
                              FrostedDialog, Flyout (Overlays.xaml), FrostedGrid with FrostedGridColumn,
                              FrostedGridCheckBoxColumn and its Grid* helpers (DataGrid.xaml; JetPackGrid renamed)
  Helpers/                    ControlHelper, PasswordBoxHelper, TabHelper, ScrollViewerHelper, FocusHelper
                              (attached properties); Converters (tree indent, splitter direction)
src/Frosted.Wpf.Demo/         demo app; pages in Views/, Snapshot.cs for --snapshot
```

XAML namespace `urn:frosted-wpf`, prefix `fg`. Brushes are referenced with `DynamicResource`.

## Build and verify

```bash
dotnet build FrostedTheme.slnx
src/Frosted.Wpf.Demo/bin/Debug/net8.0-windows/Frosted.Wpf.Demo.exe --snapshot <scratch folder>
```

- `--snapshot` renders the five pages (top, halfway down a long page, and the bottom), the popups
  (combo box, context menu, tool tip, date picker, drop-down button, time picker, colour picker,
  search combo box, two FrostedGrid filters), an open dialog, the open flyout, the folded
  HamburgerMenu and the grid grouped by make to PNG,
  and writes `wheel-check.txt` (PASS when a wheel turn over the settings ListBox scrolls the page),
  then exits. RenderTargetBitmap cannot see Mica, so it switches the
  backdrop to None (opaque fallback ground). Look at the PNGs after every visual change; keep them
  out of the repo.
- The user often runs the demo from Visual Studio, which locks `bin\...`. Do not kill it; build to
  another folder: `dotnet build src/Frosted.Wpf.Demo -o <scratch>/out`.
- On-screen captures (`CopyFromScreen`) fail while the RDP window is minimised ("The handle is
  invalid"); use `--snapshot` then.

## WPF gotchas (same as JetPack)

- StaticResource / BasedOn across the merged files fails at runtime: cross-file references are
  `DynamicResource`, and a `BasedOn` style lives in the same file as its base.
- Attached properties in binding *paths* need `fg:` (`urn:frosted-wpf`); setters, `TemplateBinding`
  and triggers are fine with the `clr-namespace` prefixes `c:` / `h:`.
- TabItem header fonts and colours sit on the header presenter (`TextElement.*`), never on the
  TabItem, or the page content inherits them.
- TextBox and PasswordBox apply `Padding` to their text themselves; the placeholder uses
  `Margin="{TemplateBinding Padding}"` plus the 2px text inset.
- A ScrollViewer in a template takes its scroll bar visibility from the templated parent's attached
  `ScrollViewer.*` properties; popups (ContextMenu, MenuItem) set them to Auto, or an empty bar shows.
- WPF's ScrollViewer swallows every wheel turn, even with nothing to scroll. The Frosted ScrollViewer
  style turns on `ScrollViewerHelper.ChainMouseWheel`: the innermost scroll viewer under the mouse
  passes a turn it cannot use to the nearest outer one that can (visual parents only, so popups
  never scroll the page).
- Scroll bars are 18px wide with a thumb that fills that width (the drawn bar is 6px, 10px under
  the mouse), so a bar along the window edge stays clear of the 6px resize border.
- Effects (DropShadowEffect) sit on separate background layers, never on text.
- ToolBar gives its children the styles under `ToolBar.*StyleKey`, not the implicit ones. Each lives
  in the file of its base style (Buttons, Lists, Text, Toggles), since `BasedOn` cannot cross files.
- In `ControlTemplate.Triggers`, a DataTrigger's `RelativeSource Self` is the templated control;
  `RelativeSource TemplatedParent` is one level further up.
- Calendar binds its day buttons' `Style` to `CalendarDayButtonStyle`; set it (and
  `CalendarButtonStyle`) explicitly, an implicit style alone may not reach them.
- DataGrid rows choose their template (with or without a row header) from the inherited
  `DataGridHelper.HeadersVisibility` in triggers, not a binding to the ancestor DataGrid: FrostedGrid
  refreshes and recycles rows, and an ancestor binding can give up for good. FrostedGrid's code needs
  the row template's `DGR_Border`, the ScrollViewer template's `PART_ScrubLayer` and
  `GridColumnHeadersPresenter` (both in the shared `Frosted.Templates.DataGrid.ScrollViewer`).
- A FlowDocument does not take the RichTextBox's font (justified Georgia); the RichTextBox style
  gives FlowDocument the theme font in its resources.
- FrostedWindow extends the DWM frame over the whole window (GlassFrameThickness -1) while a backdrop
  is active and removes WS_SYSMENU so Windows does not draw its own caption buttons (same approach
  as JetPackWindow).
