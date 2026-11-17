# Frosted Glass for WPF

A dark, frosted-glass theme for WPF (.NET 8+), built from the "Frosted Glass" design: the Windows 11
backdrop (Mica, Mica Alt or Acrylic) under a dark green tint, a fine grain and a soft light from the
top-left corner, with glass cards and controls on top.

This is the first, basic control set. See [src/Frosted.Wpf/PACKAGE.md](src/Frosted.Wpf/PACKAGE.md)
for how to use the package.

## Layout

```
src/Frosted.Wpf/          the library (NuGet package Frosted.Wpf)
src/Frosted.Wpf.Demo/     the demo app: Controls page and Layers & colours page
```

## Build and run

```bash
dotnet build FrostedTheme.slnx
src/Frosted.Wpf.Demo/bin/Debug/net8.0-windows/Frosted.Wpf.Demo.exe
```

`Frosted.Wpf.Demo.exe --snapshot <folder>` renders both pages and the popups to PNG and exits.

## Licence

MIT, see [LICENSE](LICENSE).
