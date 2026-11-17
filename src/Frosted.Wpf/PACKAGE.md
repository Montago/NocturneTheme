# Frosted Glass for WPF

A dark, frosted-glass theme for WPF on .NET 8 and later. The Windows 11 backdrop (Mica, Mica Alt or
Acrylic) shows through a tinted, grainy window; cards and controls are light glass surfaces on top.
One resource dictionary styles the standard controls; `FrostedWindow` gives the frame.

## Getting started

```bash
dotnet add package Frosted.Wpf
```

Merge the theme in `App.xaml`:

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="pack://application:,,,/Frosted.Wpf;component/Styles/Frosted.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

Make the main window a `FrostedWindow`:

```xml
<fg:FrostedWindow x:Class="MyApp.MainWindow"
                  xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                  xmlns:fg="urn:frosted-wpf"
                  Title="My app" Backdrop="Mica">
    <HeaderedContentControl Style="{StaticResource Frosted.Styles.Card}" Header="SETTINGS">
        <Button Style="{StaticResource Frosted.Styles.Button.Primary}" Content="Save" />
    </HeaderedContentControl>
</fg:FrostedWindow>
```

```csharp
public partial class MainWindow : Frosted.Wpf.Controls.FrostedWindow { ... }
```

## What is in it

- **FrostedWindow**: `Backdrop` (Mica, MicaAlt, Acrylic, None), `TintOpacity`, `NoiseOpacity`,
  44px title bar with its own caption buttons, `RightWindowCommands`. Without Windows 11 22H2 or with
  transparency effects off it paints an opaque ground of the same colour.
- **Styles** for Button (default, `.Primary`, `.Subtle`, `.Icon`), RepeatButton, ToggleButton,
  TextBox (`.Search`, `.MultiLine`), PasswordBox (reveal button), Label, CheckBox (three-state),
  RadioButton, toggle switch (`Frosted.Styles.ToggleSwitch`), ComboBox (editable too), ListBox and
  side navigation, Slider, ProgressBar (indeterminate), ScrollViewer, GroupBox, Expander, TabControl
  (segmented and `.Document` tabs with close buttons, badges and "+"), DataGrid, ContextMenu, Menu,
  ToolTip, Hyperlink, Separator.
- **Glass cards** (`Frosted.Styles.Card`), tags, status pills, badges, icon tiles, typography styles.
- **StrokeIcon** and a small set of line icons (`Frosted.Icons.*`).
- Attached helpers: `ControlHelper` (`Placeholder`, `Icon`, `HasError`, `CornerRadius`),
  `PasswordBoxHelper.CanReveal`, `TabHelper` (`ShowCloseButton`, `Badge`, `ShowAddButton`).

Brushes are `Frosted.Brushes.*`; reference them with `DynamicResource`.
