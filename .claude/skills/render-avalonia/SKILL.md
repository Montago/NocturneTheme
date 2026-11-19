---
name: render-avalonia
description: Take a quick picture of Nocturne.Avalonia (the Avalonia port) - one control from a small XAML snippet, or the whole demo window - off screen with Avalonia's headless platform. Use to look at or check a visual change to an Avalonia ControlTheme, style or brush, and to zoom in on details. For the WPF library use render-control instead.
---

# Render Nocturne.Avalonia

The Avalonia demo (`src/Nocturne.Avalonia.Demo`) has three commands that draw off screen on Avalonia's
headless platform with Skia (`Headless.cs`). They open no window, need no screen and take about a
second; they work the same on Windows, macOS and Linux (CI runs `--snapshot` on Linux).

- `--render <snippet.axaml> <out.png>`: one XAML element on the window ground with 16px around it
  (`RenderCommand.cs`).
- `--render-gallery <Tag,…|all> <folder>`: chosen samples of `Views/GalleryPage.axaml` (one per control,
  Tag = file name) to `<folder>/<Tag>.png`, with popups marked `demo:Gallery.Open` drawn open and the
  picture cropped to the sample and its popups. All 59 take about six seconds.
- `--snapshot <folder>` (`Snapshot.cs`, a few seconds): `controls.png`, `controls-bottom.png`, `more.png`,
  `more-bottom.png`, `icons.png`, every gallery sample in `controls/`,
  `tokens.png`, `popup-combobox.png` and `popup-menu.png` (drawn open), `accent-<name>.png` and
  `titlebar-separated.png` / `titlebar-accent.png`, the custom controls page (`custom.png`,
  `custom-bottom.png`, `popup-timepicker.png`, `popup-datepicker.png`, `popup-colorpicker.png`,
  `popup-searchcombobox.png`, `dialog.png`, `flyout.png`, `hamburger-compact.png`); then `checks.txt`
  (PASS/FAIL per behaviour check, driven with the headless mouse and keyboard; the command exits with 1
  when one fails).

A DatePicker or TimePicker has no open property: click its `PART_FlyoutButton` (as `Snapshot.cs`
does), or draw the drop-down inline in a snippet with `<TimePickerPresenter Width="216" />` /
`<DatePickerPresenter Width="316" />`.

Popups are drawn into the window (`OverlayPopups`), so an open popup shows in the picture.

## 1. Build (only when the library or the demo changed)

Build to a folder in your scratchpad directory, never to `bin/` (Visual Studio may have it locked):

```bash
dotnet build src/Nocturne.Avalonia.Demo -o <scratch>/av/out -v q -nologo
```

A snippet is read when the command runs, so changing only the snippet needs no rebuild.

## 2. Write a snippet

One root element. Do not add `xmlns`: the Avalonia namespace, `x:` and `nc:` (`urn:nocturne-avalonia`)
are declared for you, and the theme's resources (`{DynamicResource Nocturne.*}`) are there.

```xml
<StackPanel Spacing="8">
    <TextBlock Text="Title" FontSize="{DynamicResource Nocturne.Font.Size.Title}" />
    <Border Width="96" Height="56" CornerRadius="10" Background="{DynamicResource Nocturne.Brushes.Accent}" />
</StackPanel>
```

States that can be set in XAML (`IsEnabled="False"`, `IsChecked="True"`, `Classes="clearButton"`,
`nc:ControlHelper.HasError="True"`) show; hover and pressed cannot be forced from a snippet. A menu's
surface can be drawn inline with `<MenuFlyoutPresenter>` and `MenuItem`s in it.

## 3. Render and look

```bash
<scratch>/av/out/Nocturne.Avalonia.Demo.exe --render <scratch>/av/micro/x.axaml <scratch>/av/micro/x.png --scale 3
```

| Option | Effect |
| --- | --- |
| `--scale 3` | more pixels per unit (both commands). 3–4 for strokes, radii and alignment; 1 for how it really looks |

Read the PNG to look at it. On an error the command exits with 1 and writes `<out>.error.txt`
(the XAML parser's message, with line and position in the snippet after the namespaces were added).

Keep the pictures in the scratchpad, out of the repo.
