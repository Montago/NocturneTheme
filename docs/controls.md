---
title: Controls
nav_order: 4
has_children: true
---

# Controls

Every standard WPF control gets the Nocturne look as soon as `Nocturne.xaml` is merged. The pages
below show each one, the named variants, and the controls Nocturne.Wpf adds (`nc:` namespace).

| Page | Controls |
| --- | --- |
| [Window and dialogs](window.md) | `NocturneWindow`, title bar commands, `NocturneDialog`, `ShowMessageAsync`, `Flyout` |
| [Buttons](buttons.md) | `Button` and its variants, `ToggleButton`, `RepeatButton`, `Hyperlink`, `DropDownButton`, `SplitButton`, `Tile`, `Badged` |
| [Text and input](inputs.md) | `TextBox` (search, multi-line, clear button), `PasswordBox`, `RichTextBox`, `Label`, `ComboBox`, `NumericUpDown`, `DatePicker`, `TimePicker`, `ColorPicker`, `AutoSuggestBox`, `SearchComboBox`, error look |
| [Selection](selection.md) | `CheckBox`, `RadioButton`, toggle switch, `Slider`, `RangeSlider`, `Rating` |
| [Lists and data](lists.md) | `ListBox`, `ListView`/`GridView`, `TreeView`, `DataGrid`, `NocturneGrid`, `Calendar`, `ScrollViewer` |
| [Navigation and layout](navigation.md) | `TabControl` (segments and document tabs), `HamburgerMenu`, `Expander`, `Accordion`, `GroupBox`, `Menu`, `ContextMenu`, `ToolBar`, `StatusBar`, `GridSplitter`, `ToolTip` |
| [Progress and status](status.md) | `ProgressBar`, `ProgressRing`, `BusyIndicator`, `InfoBar` |
| [Typography and surfaces](typography.md) | Text styles, cards, tags, pills, badges, icon tiles, `StrokeIcon` and the icons |

The picture under each heading is rendered from the demo's gallery
(`src/Nocturne.Wpf.Demo/Views/GalleryPage.xaml`), on a card over the window's fallback ground.
