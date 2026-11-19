# Nocturne for Avalonia

The Nocturne dark theme of [Nocturne.Wpf](https://www.nuget.org/packages/Nocturne.Wpf) for Avalonia 12
on .NET 10 and later, on Windows, macOS and Linux. A tinted window with a soft light from the top-left
corner (over the Windows 11 backdrop where there is one), and cards and controls as light, translucent
surfaces on top. The theme stands alone: it styles Avalonia's own controls and adds the ones a desktop
app usually needs.

Documentation, with a picture and XAML for every control:
https://montago.github.io/NocturneTheme/avalonia.html

## Getting started

```bash
dotnet add package Nocturne.Avalonia
```

Add the theme to `App.axaml` instead of the Fluent or Simple theme, and ask for the dark variant:

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:nc="urn:nocturne-avalonia"
             x:Class="MyApp.App"
             RequestedThemeVariant="Dark">
    <Application.Styles>
        <nc:NocturneTheme />
    </Application.Styles>
</Application>
```

Make the main window a `NocturneWindow` and put the controls on cards:

```xml
<nc:NocturneWindow xmlns="https://github.com/avaloniaui"
                   xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                   xmlns:nc="urn:nocturne-avalonia"
                   x:Class="MyApp.MainWindow"
                   Title="My app" Width="1100" Height="720">
    <HeaderedContentControl Margin="24" Theme="{StaticResource Nocturne.Styles.Card}" Header="SETTINGS">
        <Button Theme="{StaticResource Nocturne.Styles.Button.Primary}" Content="Save" />
    </HeaderedContentControl>
</nc:NocturneWindow>
```

```csharp
public partial class MainWindow : Nocturne.Avalonia.Controls.NocturneWindow
{
    public MainWindow() => InitializeComponent();
}
```

## What is in it

- **NocturneWindow**: `Backdrop` (Acrylic by default, Mica, MicaAlt, None), `TintOpacity` (0.5), a 44px
  title bar with its own caption buttons and Windows' snap layouts, `LeftWindowCommands` and
  `RightWindowCommands`, `TitleBarStyle` (Seamless, Separated, Accent), `CornerStyle`, in-window dialogs
  (`await window.ShowMessageAsync(...)`) and `Flyouts`. macOS keeps its traffic lights; where there is no
  backdrop (Windows 10, transparency effects off, Remote Desktop, most Linux desktops) it paints an
  opaque ground of the same colour.
- **Avalonia's controls**, styled: Button (`Nocturne.Styles.Button.Primary`, `.Danger`, `.Subtle`, `.Icon`),
  ToggleButton, RepeatButton, HyperlinkButton, DropDownButton, SplitButton, TextBox (search, multi-line,
  password with an eye button, clear button), NumericUpDown, ComboBox, AutoCompleteBox,
  CalendarDatePicker, DatePicker, TimePicker, CheckBox, RadioButton, ToggleSwitch, Slider, ProgressBar,
  ListBox (and side navigation), TreeView, DataGrid (row headers, groups), Calendar, ScrollViewer, TabControl
  (segments and `.Document` tabs with close buttons, badges and "+"), Expander, GroupBox, Menu,
  ContextMenu, ToolTip, GridSplitter. Text fields get a right-click menu with icons, validation errors show
  in a callout under the field, and scroll bars fade out while the pointer is away.
- **Own controls** (`nc:`): `Tile`, `Badged`, `ColorPicker`, `SearchComboBox` (a combo box you can type
  in to filter thousands of items), `RangeSlider`, `Rating`, `ProgressRing`, `BusyIndicator`, `InfoBar`,
  `Accordion`, `HamburgerMenu`, `StatusBar`, `NocturneDialog` and `Flyout` (a panel that slides in
  from a window edge).
- **Accents**: blue by default; `NocturneTheme.SetAccent(NocturneAccent.Green)` (or Orange, Yellow, Teal,
  or `System`, the accent colour of the operating system) switches them at run time.
- **Building blocks**: cards, tags, pills, badges, icon tiles, a type scale, the theme's line icons
  (`nc:StrokeIcon`) and Material Design Icons (`<nc:NocturneIcon Kind="Bell" />`, `{nc:Icon Bell}`).
- **The font**: Inter, embedded, so the theme looks the same on every platform.

## License

MIT, except the Material Design Icons that `NocturneIcon` draws (Apache License 2.0, by Pictogrammers)
and the font Inter (SIL Open Font License 1.1); their licences are in the package (`icons/`, `fonts/`)
and `NOTICE.md` says where they come from.
