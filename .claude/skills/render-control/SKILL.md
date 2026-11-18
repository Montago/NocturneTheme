---
name: render-control
description: Take a quick picture of one Nocturne control (or a few) instead of running the whole demo --snapshot. Use to look at or check a visual change to a control, a template or a style, to zoom in on details (alignment, strokes, sub-pixel offsets), to see an animation at chosen moments, or to compare accents. Renders a small XAML snippet or chosen gallery samples to PNG in about a second.
---

# Render one control

The demo app has two light commands next to `--snapshot` (`src/Nocturne.Wpf.Demo/RenderCommand.cs`).
They open no demo window and take about a second:

- `--render <snippet.xaml> <out.png>`: one XAML element on the window ground with 16px around it.
- `--render-gallery <Tag[,Tag…]> <folder>`: those samples of `Views/GalleryPage.xaml`, drawn as
  `--snapshot` draws them, to `<folder>/<Tag>.png`. The Tag is the sample's file name (`progressring`,
  `button`, `combobox`…). An unknown Tag puts the list of Tags in the error file.

Use `--snapshot` only when a change touches the window, the pages or many controls at once.

## 1. Build (only when the library or the demo changed)

Build to a folder in your scratchpad directory, never to `bin/` (Visual Studio may have it locked):

```bash
dotnet build src/Nocturne.Wpf.Demo -o <scratch>/out -v q -nologo
```

A snippet is read when the command runs, so changing only the snippet needs no rebuild.

## 2. Write a snippet

One root element. Do not add `xmlns`: `nc:` (the theme), `demo:` (`demo:Gallery.Open`) and `x:` are
declared for you, and the theme's resources (`{DynamicResource Nocturne.*}`) are there.

```xml
<StackPanel Orientation="Horizontal">
    <nc:ProgressRing Width="16" Height="16" Margin="0,0,12,0" />
    <nc:ProgressRing Width="64" Height="64" />
</StackPanel>
```

- To show states, set them in the XAML: `IsEnabled="False"`, `IsChecked="True"`, `Tag`, validation.
  Hover, pressed and keyboard focus cannot be forced; check those triggers in the template by reading
  it, or ask the user to look in the running demo.
- `demo:Gallery.Open="True"` on a ComboBox, DatePicker, TimePicker, ColorPicker, SearchComboBox,
  AutoSuggestBox, MenuItem, SplitButton, DropDownButton, or an element with a ContextMenu or a
  ToolTip draws its popup open next to it.

## 3. Render and look

```bash
<scratch>/out/Nocturne.Wpf.Demo.exe --render <scratch>/micro/ring.xaml <scratch>/micro/ring.png --scale 3
```

Options (both commands):

| Option | Effect |
| --- | --- |
| `--scale 4` | more pixels per WPF unit. Use 3–4 for strokes, radii and alignment; 1 for how it really looks |
| `--time 0,250,500` | where the endless animations (ProgressRing, indeterminate ProgressBar) stop, in ms (700 by default). With several values `--render` writes one picture each, `ring-250.png` (`--render-gallery` uses the first) |
| `--accent Green` | Blue (default), Green, Orange, Yellow, Teal, System |
| `--card` | `--render` only: put the snippet on a card, as the gallery samples are |

Exit code 0 means the picture is there. On 1, read `<out.png>.error.txt` (for `--render-gallery`:
`<folder>/error.txt`). That is usually a XAML parse error. Its line number matches the
snippet's, but positions on line 1 are shifted by the wrapper. Then open the PNG with Read and look at it.

## Keep in mind

- Pictures go to the scratchpad and stay out of the repo. Docs screenshots change only for a
  release (`/update-docs`).
- RenderTargetBitmap draws no Mica/Acrylic. The ground is the opaque `Window.Fallback` colour, as in
  `--snapshot`.
- After a change to a shared style, also render the gallery samples that use it, e.g.
  `--render-gallery button,splitbutton,dropdownbutton`. Before you finish a visual change, the full
  `--snapshot` and the sample apps are still the final check (see CLAUDE.md).
