# Nocturne for WPF

A dark theme for WPF (.NET 8+): the Windows 11 backdrop (Acrylic by default, or Mica / Mica Alt)
under a dark tint and a soft light from the top-left corner, with translucent cards and controls on
top. The accent is blue; `NocturneTheme.SetAccent` switches it to green, orange, yellow, red or the
Windows accent colour.

![The controls page of the demo](docs/screenshots/controls.png)

**Documentation:** [docs/](docs/index.md), one page per group of controls with a picture and XAML for
each (published with GitHub Pages from the `docs` folder). The package's own readme is
[src/Nocturne.Wpf/PACKAGE.md](src/Nocturne.Wpf/PACKAGE.md).

## Layout

```
src/Nocturne.Wpf/          the library (NuGet package Nocturne.Wpf)
src/Nocturne.Wpf.Demo/     the demo app: Controls, More controls, Custom controls, Data grid and Layers & colours
                           pages, and the gallery (Views/GalleryPage.xaml) the docs pictures come from
docs/                      the documentation (Just the Docs), screenshots in docs/screenshots
tools/                     update_docs.py (pictures, resource keys, checks), generate_icon.ps1
```

## Build and run

```bash
dotnet build NocturneTheme.slnx
src/Nocturne.Wpf.Demo/bin/Debug/net8.0-windows/Nocturne.Wpf.Demo.exe
```

`Nocturne.Wpf.Demo.exe --snapshot <folder>` renders the pages, the popups and one picture per control
to PNG and exits. `python tools/update_docs.py` does that and copies the pictures into `docs/`.

## Licence

MIT, see [LICENSE](LICENSE).
