---
title: Home
nav_order: 1
permalink: /
---

# Nocturne documentation

Nocturne is a dark theme for .NET 10 and later: a deep tinted ground with a soft light from the
top-left corner, and cards and controls on top as light, translucent surfaces. It comes as three
packages that share one set of colours, accents, sizes and icons, so a desktop app, a cross-platform
app and a web portal can look the same.

| | [Nocturne for WPF](wpf.md) | [Nocturne for Avalonia](avalonia.md) | [Nocturne for the web](web.md) |
| --- | --- | --- | --- |
| Package | [Nocturne.Wpf](https://www.nuget.org/packages/Nocturne.Wpf) | [Nocturne.Avalonia](https://www.nuget.org/packages/Nocturne.Avalonia) | [Nocturne.Web](https://www.nuget.org/packages/Nocturne.Web) |
| For | WPF desktop apps on Windows | Avalonia 12 apps on Windows, macOS and Linux | ASP.NET Core sites; the CSS and JavaScript work on any site |
| The window | `NocturneWindow` over the Windows 11 backdrop (Acrylic, Mica) | `NocturneWindow` over the backdrop on Windows, an opaque ground elsewhere | The page paints the opaque ground; nothing of the desktop shows through |
| What you get | Styles for every standard WPF control, plus dialogs, `Flyout`, `HamburgerMenu`, `NocturneGrid` and more | ControlThemes for Avalonia's controls, plus the controls Avalonia lacks and `NocturneGrid` | CSS classes and a script for the same controls, tag helpers with the WPF names (`<nc-combo-box>`) and an SVG icon sprite |
| Start with | [Getting started with WPF](getting-started.md) | [Getting started with Avalonia](avalonia.md#getting-started) | [Getting started on the web](web/getting-started.md) |

![Nocturne for WPF](screenshots/controls.png)

## What they share

- **Colours and accents.** Nocturne.Wpf's `Styles/Brushes.xaml` and accent palettes are the source:
  Avalonia's brushes and the web theme's CSS custom properties are generated from them, so
  `Nocturne.Brushes.Accent` in XAML and `var(--nc-accent)` in CSS are the same colour. All three switch
  between blue (the default), green, orange, yellow and the system's accent colour at run time.
- **Names.** A control has the same name everywhere: `ComboBox` in XAML, `<nc-combo-box>` as a tag
  helper and `.nc-combo-box` as a CSS class. Style keys become themes in Avalonia and modifiers on the
  web: `Nocturne.Styles.Button.Primary` is `Theme="{StaticResource Nocturne.Styles.Button.Primary}"`,
  `button-style="Primary"` and `.nc-button--primary`.
- **Icons.** `nc:NocturneIcon` in WPF and Avalonia and `<nc-icon>` on the web draw the same Material
  Design Icons by the same names (`Bell`, `AccountMultiple`); the theme's own line icons are
  `nc:StrokeIcon` and `<nc-stroke-icon>`.
- **NocturneGrid**, the data grid for large lists with a word search that marks its hits, a value
  filter in every header, sorting, grouping and paging, is in all three; on the web it can also
  filter and page on the server.

## License

MIT, except the Material Design Icons (Apache License 2.0, by Pictogrammers) and the Inter font
(SIL Open Font License 1.1) that all three carry. See
[NOTICE](https://github.com/Montago/NocturneTheme/blob/master/NOTICE.md).
