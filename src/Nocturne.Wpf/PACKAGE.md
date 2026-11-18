# Nocturne for WPF

A dark theme for WPF on .NET 8 and later. The Windows 11 backdrop (Acrylic, Mica or
Mica Alt) shows through a tinted window; cards and controls are light translucent surfaces on top.
One resource dictionary styles the standard controls; `NocturneWindow` gives the frame.

Documentation, with a picture and XAML for every control:
https://montago.github.io/NocturneTheme/

## Getting started

```bash
dotnet add package Nocturne.Wpf
```

Merge the theme in `App.xaml`:

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="pack://application:,,,/Nocturne.Wpf;component/Styles/Nocturne.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

Make the main window a `NocturneWindow`:

```xml
<nc:NocturneWindow x:Class="MyApp.MainWindow"
                  xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                  xmlns:nc="urn:nocturne-wpf"
                  Title="My app">
    <HeaderedContentControl Style="{StaticResource Nocturne.Styles.Card}" Header="SETTINGS">
        <Button Style="{StaticResource Nocturne.Styles.Button.Primary}" Content="Save" />
    </HeaderedContentControl>
</nc:NocturneWindow>
```

```csharp
public partial class MainWindow : Nocturne.Wpf.Controls.NocturneWindow { ... }
```

## What is in it

- **NocturneWindow**: `Backdrop` (Acrylic by default, Mica, MicaAlt, None), `TintOpacity` (0.5),
  44px title bar with its own caption buttons, `RightWindowCommands`. Without Windows 11 22H2 or with
  transparency effects off it paints an opaque ground of the same colour.
- **Styles** for Button (default, `.Primary`, `.Subtle`, `.Icon`), RepeatButton, ToggleButton,
  TextBox (`.Search`, `.MultiLine`), PasswordBox (reveal button), Label, CheckBox (three-state),
  RadioButton, toggle switch (`Nocturne.Styles.ToggleSwitch`), ComboBox (editable too), ListBox and
  side navigation, ListView (with GridView column headers), TreeView, Slider, ProgressBar
  (indeterminate), ScrollViewer, GroupBox, Expander, TabControl (segmented and `.Document` tabs with
  close buttons, badges and "+"), DataGrid (row headers too), Calendar, DatePicker, RichTextBox,
  ToolBar and ToolBarTray, StatusBar, GridSplitter, ContextMenu, Menu, ToolTip, Hyperlink, Separator,
  ResizeGrip.
- **Own controls**: `DropDownButton` and `SplitButton` (each also `.Primary`), `NumericUpDown`,
  `ProgressRing`, `InfoBar` (Info, Success, Warning, Error), `NocturneDialog` (shown in the window with
  `await window.ShowMessageAsync(...)` or `ShowDialogAsync`) and `Flyout` (in `NocturneWindow.Flyouts`,
  slides in from an edge), `TimePicker`, `ColorPicker`, `AutoSuggestBox`, `SearchComboBox` (a combo box
  you can type in to filter thousands of items), `RangeSlider`, `Rating`, `Tile` (also `.Accent`),
  `Badged`, `BusyIndicator`, `Accordion` and `HamburgerMenu` (a navigation pane that folds to its icons,
  with groups of sub-items). `ControlHelper.ClearButton` puts an × in a TextBox.
- **NocturneGrid**: a DataGrid for large lists. Search across all columns (every word must match;
  hits drawn in the accent), a filter in every column header (tick values with their counts or type
  them, `*` wildcards, ranges such as `>50000` or `2020-01-01..2020-06-30`, a tree of years, months and
  days for dates), grouping (`GroupMemberPath`), three-state sorting, paging (`PageSize`), and
  virtualized rows and columns with text drawn straight from the font. Columns: `NocturneGridColumn`
  and `NocturneGridCheckBoxColumn`.
- **Accent colours**: blue by default; `NocturneTheme.SetAccent(NocturneAccent.Green)` (or Orange,
  Yellow, Red) switches the accent, the primary button and the window tint at run time. `NocturneAccent.System` uses
  the accent colour chosen in Windows and follows it when the user changes it.
- **Cards** (`Nocturne.Styles.Card`), tags, status pills, badges, icon tiles, typography styles.
- **StrokeIcon** and a small set of line icons (`Nocturne.Icons.*`).
- Attached helpers: `ControlHelper` (`Placeholder`, `Icon`, `HasError`, `CornerRadius`),
  `PasswordBoxHelper.CanReveal`, `TabHelper` (`ShowCloseButton`, `Badge`, `ShowAddButton`),
  `FocusHelper` (a keyboard-focus ring a template draws itself, as the TreeViewItem does).

Brushes are `Nocturne.Brushes.*`; reference them with `DynamicResource`.
