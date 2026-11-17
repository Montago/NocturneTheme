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
- Everything is MIT. The grain is drawn in code (`Controls/NoiseTexture.cs`); no third-party images
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
  Controls/                   FrostedWindow, StrokeIcon, TabPanelBorder, NoiseTexture, NativeMethods
  Helpers/                    ControlHelper, PasswordBoxHelper, TabHelper (attached properties)
src/Frosted.Wpf.Demo/         demo app; pages in Views/, Snapshot.cs for --snapshot
```

XAML namespace `urn:frosted-wpf`, prefix `fg`. Brushes are referenced with `DynamicResource`.

## Build and verify

```bash
dotnet build FrostedTheme.slnx
src/Frosted.Wpf.Demo/bin/Debug/net8.0-windows/Frosted.Wpf.Demo.exe --snapshot <scratch folder>
```

- `--snapshot` renders both pages (top and scrolled to the bottom) and the combo box, context menu
  and tool tip popups to PNG, then exits. RenderTargetBitmap cannot see Mica, so it switches the
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
- Effects (DropShadowEffect) sit on separate background layers, never on text.
- FrostedWindow extends the DWM frame over the whole window (GlassFrameThickness -1) while a backdrop
  is active and removes WS_SYSMENU so Windows does not draw its own caption buttons (same approach
  as JetPackWindow).
