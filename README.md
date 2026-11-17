# Nocturne for WPF

A dark theme for WPF (.NET 8+): the Windows 11
backdrop (Acrylic by default, or Mica / Mica Alt) under a dark tint and a soft light from the top-left
corner, with translucent cards and controls on top. The accent is meadow green; `NocturneTheme.SetAccent`
switches it to orange, blue, yellow, red or the Windows accent colour.

This is the first, basic control set. See [src/Nocturne.Wpf/PACKAGE.md](src/Nocturne.Wpf/PACKAGE.md)
for how to use the package.

## Layout

```
src/Nocturne.Wpf/          the library (NuGet package Nocturne.Wpf)
src/Nocturne.Wpf.Demo/     the demo app: Controls, More controls, Custom controls, Data grid and Layers & colours pages
```

## Build and run

```bash
dotnet build NocturneTheme.slnx
src/Nocturne.Wpf.Demo/bin/Debug/net8.0-windows/Nocturne.Wpf.Demo.exe
```

`Nocturne.Wpf.Demo.exe --snapshot <folder>` renders both pages and the popups to PNG and exits.

## Licence

MIT, see [LICENSE](LICENSE).
