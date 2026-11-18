---
title: Resource keys
parent: Reference
nav_order: 1
---

# Resource keys

Every brush, colour, font size, icon and named style the theme defines, generated from the XAML in
`src/Nocturne.Wpf/Styles` by `tools/generate_resource_keys.py`. Reference brushes with
`{DynamicResource ...}` so they follow accent changes.

To change a value app-wide, redefine the key in `App.xaml` after merging `Nocturne.xaml`, for example
`<SolidColorBrush x:Key="Nocturne.Brushes.Error" Color="#FFFF6F66" />`. The accent brushes are
rewritten by `NocturneTheme.SetAccent`, so change those with an accent instead ([Theme and accents](theming.md)).

## Brushes and colours

Values shown are the default blue accent. `NocturneTheme.SetAccent` writes new values for the accent brushes, `Text.OnAccent`, the primary button, `Popup`, `Window.Tint` and `Window.Fallback`.

| Key | Type | Value |
| --- | --- | --- |
| `Nocturne.Colors.Accent` | Color | `#FF1F78D1` |
| `Nocturne.Colors.Accent.Light` | Color | `#FF4DA3FF` |
| `Nocturne.Brushes.Accent` | SolidColorBrush | `#FF1F78D1` |
| `Nocturne.Brushes.Accent.Light` | SolidColorBrush | `#FF4DA3FF` |
| `Nocturne.Brushes.Accent.Text` | SolidColorBrush | `#FF9CCBFF` |
| `Nocturne.Brushes.Accent.Text.Hover` | SolidColorBrush | `#FFC9E2FF` |
| `Nocturne.Brushes.Text` | SolidColorBrush | `#FFF2F6F3` |
| `Nocturne.Brushes.Text.Secondary` | SolidColorBrush | `#C7EBF2ED` |
| `Nocturne.Brushes.Text.Label` | SolidColorBrush | `#D6EEF4F0` |
| `Nocturne.Brushes.Text.Hint` | SolidColorBrush | `#BDEBF2ED` |
| `Nocturne.Brushes.Text.Placeholder` | SolidColorBrush | `#9EEBF2ED` |
| `Nocturne.Brushes.Text.Selected` | SolidColorBrush | `#FFFFFFFF` |
| `Nocturne.Brushes.Text.OnAccent` | SolidColorBrush | `#FFFFFFFF` |
| `Nocturne.Brushes.Surface1` | SolidColorBrush | `#0EFFFFFF` |
| `Nocturne.Brushes.Surface1.Border` | SolidColorBrush | `#1CFFFFFF` |
| `Nocturne.Brushes.Surface1.Highlight` | SolidColorBrush | `#12FFFFFF` |
| `Nocturne.Brushes.Surface2` | SolidColorBrush | `#21FFFFFF` |
| `Nocturne.Brushes.Surface.Hover` | SolidColorBrush | `#12FFFFFF` |
| `Nocturne.Brushes.Surface.Pressed` | SolidColorBrush | `#0AFFFFFF` |
| `Nocturne.Brushes.Surface.Sunken` | SolidColorBrush | `#1A000000` |
| `Nocturne.Brushes.Stroke` | SolidColorBrush | `#24FFFFFF` |
| `Nocturne.Brushes.Stroke.Subtle` | SolidColorBrush | `#12FFFFFF` |
| `Nocturne.Brushes.Divider` | SolidColorBrush | `#1FFFFFFF` |
| `Nocturne.Brushes.Focus` | SolidColorBrush | `#FFFFFFFF` |
| `Nocturne.Brushes.Button.Primary` | LinearGradientBrush | `#FF1A6CC0` 0 → `#FF155FAD` 1 |
| `Nocturne.Brushes.Button.Primary.Hover` | LinearGradientBrush | `#FF1D76CF` 0 → `#FF1867B9` 1 |
| `Nocturne.Brushes.Button.Primary.Pressed` | SolidColorBrush | `#FF134F8C` |
| `Nocturne.Brushes.Button.Primary.Border` | SolidColorBrush | `#33FFFFFF` |
| `Nocturne.Brushes.Button.Primary.Highlight` | SolidColorBrush | `#33FFFFFF` |
| `Nocturne.Brushes.Button.Danger` | LinearGradientBrush | `#FF9E2121` 0 → `#FF861B1B` 1 |
| `Nocturne.Brushes.Button.Danger.Hover` | LinearGradientBrush | `#FFAB2626` 0 → `#FF922020` 1 |
| `Nocturne.Brushes.Button.Danger.Pressed` | SolidColorBrush | `#FF701616` |
| `Nocturne.Brushes.Button.Secondary` | SolidColorBrush | `#14FFFFFF` |
| `Nocturne.Brushes.Button.Secondary.Hover` | SolidColorBrush | `#24FFFFFF` |
| `Nocturne.Brushes.Button.Secondary.Pressed` | SolidColorBrush | `#0DFFFFFF` |
| `Nocturne.Brushes.Button.Secondary.Border` | SolidColorBrush | `#26FFFFFF` |
| `Nocturne.Brushes.Button.Subtle.Hover` | SolidColorBrush | `#14FFFFFF` |
| `Nocturne.Brushes.Button.Pressed.Text` | SolidColorBrush | `#CCF2F6F3` |
| `Nocturne.Brushes.Field` | SolidColorBrush | `#33000000` |
| `Nocturne.Brushes.Field.Hover` | SolidColorBrush | `#42000000` |
| `Nocturne.Brushes.Field.Focus` | SolidColorBrush | `#4D000000` |
| `Nocturne.Brushes.Field.Border` | SolidColorBrush | `#24FFFFFF` |
| `Nocturne.Brushes.Field.Border.Bottom` | SolidColorBrush | `#57FFFFFF` |
| `Nocturne.Brushes.Field.Border.Focus` | SolidColorBrush | `#2EFFFFFF` |
| `Nocturne.Brushes.Toggle.Fill` | SolidColorBrush | `#2E000000` |
| `Nocturne.Brushes.Toggle.Border` | SolidColorBrush | `#73FFFFFF` |
| `Nocturne.Brushes.Toggle.Border.Hover` | SolidColorBrush | `#B3FFFFFF` |
| `Nocturne.Brushes.Switch.Border` | SolidColorBrush | `#8CFFFFFF` |
| `Nocturne.Brushes.Switch.Knob` | SolidColorBrush | `#D9FFFFFF` |
| `Nocturne.Brushes.Slider.Track` | SolidColorBrush | `#3DFFFFFF` |
| `Nocturne.Brushes.Slider.Thumb.Border` | SolidColorBrush | `#FFEAF2EC` |
| `Nocturne.Brushes.Progress.Track` | SolidColorBrush | `#29FFFFFF` |
| `Nocturne.Brushes.ScrollBar.Track` | SolidColorBrush | `#0FFFFFFF` |
| `Nocturne.Brushes.ScrollBar.Thumb` | SolidColorBrush | `#66FFFFFF` |
| `Nocturne.Brushes.ScrollBar.Thumb.Hover` | SolidColorBrush | `#8CFFFFFF` |
| `Nocturne.Brushes.Item.Hover` | SolidColorBrush | `#12FFFFFF` |
| `Nocturne.Brushes.Item.Pressed` | SolidColorBrush | `#0AFFFFFF` |
| `Nocturne.Brushes.Item.Selected` | SolidColorBrush | `#21FFFFFF` |
| `Nocturne.Brushes.Row.Hover` | SolidColorBrush | `#0FFFFFFF` |
| `Nocturne.Brushes.Row.Selected` | SolidColorBrush | `#1AFFFFFF` |
| `Nocturne.Brushes.Row.Selected.Hover` | SolidColorBrush | `#1FFFFFFF` |
| `Nocturne.Brushes.DataGrid.SearchHit` | SolidColorBrush | `#14FFFFFF` |
| `Nocturne.Brushes.Tile` | SolidColorBrush | `#0FFFFFFF` |
| `Nocturne.Brushes.Tile.Border` | SolidColorBrush | `#29FFFFFF` |
| `Nocturne.Brushes.Tile.Border.Selected` | SolidColorBrush | `#47FFFFFF` |
| `Nocturne.Brushes.Tile.Icon` | SolidColorBrush | `#FFEAF2EC` |
| `Nocturne.Brushes.TabStrip` | SolidColorBrush | `#29000000` |
| `Nocturne.Brushes.TabStrip.Border` | SolidColorBrush | `#14FFFFFF` |
| `Nocturne.Brushes.Tab.Text` | SolidColorBrush | `#C7EEF4F0` |
| `Nocturne.Brushes.DocumentTab.Hover` | SolidColorBrush | `#0FFFFFFF` |
| `Nocturne.Brushes.DocumentTab.Selected` | SolidColorBrush | `#1AFFFFFF` |
| `Nocturne.Brushes.Expander.Header` | SolidColorBrush | `#0FFFFFFF` |
| `Nocturne.Brushes.Expander.Header.Hover` | SolidColorBrush | `#1AFFFFFF` |
| `Nocturne.Brushes.Popup` | SolidColorBrush | `#F71F2633` |
| `Nocturne.Brushes.Popup.Border` | SolidColorBrush | `#24FFFFFF` |
| `Nocturne.Brushes.Option.Hover` | SolidColorBrush | `#17FFFFFF` |
| `Nocturne.Brushes.Option.Selected` | SolidColorBrush | `#1FFFFFFF` |
| `Nocturne.Brushes.Shortcut` | SolidColorBrush | `#B8EBF2ED` |
| `Nocturne.Brushes.Error` | SolidColorBrush | `#FFFF8F85` |
| `Nocturne.Brushes.Error.Text` | SolidColorBrush | `#FFFFA59C` |
| `Nocturne.Brushes.Error.Background` | SolidColorBrush | `#26FF786E` |
| `Nocturne.Brushes.Warning` | SolidColorBrush | `#FFFFD27A` |
| `Nocturne.Brushes.Warning.Background` | SolidColorBrush | `#26FFC440` |
| `Nocturne.Brushes.Success` | SolidColorBrush | `#FF8EF0B8` |
| `Nocturne.Brushes.Success.Background` | SolidColorBrush | `#293FD27F` |
| `Nocturne.Brushes.Window.Fallback` | SolidColorBrush | `#FF1E2633` |
| `Nocturne.Brushes.Window.Tint` | SolidColorBrush | `#FF10161F` |
| `Nocturne.Brushes.Window.Light` | LinearGradientBrush | `#14FFFFFF` 0 → `#00FFFFFF` 0.38 |
| `Nocturne.Brushes.TitleBar.Separated` | SolidColorBrush | `#38000000` |
| `Nocturne.Brushes.TitleBar.Separated.Border` | SolidColorBrush | `#14FFFFFF` |
| `Nocturne.Brushes.TitleBar.Accent` | LinearGradientBrush | `#4D1F78D1` 0 → `#381F78D1` 1 |
| `Nocturne.Brushes.TitleBar.Accent.Border` | SolidColorBrush | `#384DA3FF` |
| `Nocturne.Brushes.Caption.Hover` | SolidColorBrush | `#17FFFFFF` |
| `Nocturne.Brushes.Caption.Pressed` | SolidColorBrush | `#0DFFFFFF` |
| `Nocturne.Brushes.Close.Hover` | SolidColorBrush | `#FFC42B1C` |
| `Nocturne.Brushes.Close.Pressed` | SolidColorBrush | `#FFA82618` |
| `Nocturne.Brushes.Overlay` | SolidColorBrush | `#73000000` |

## Fonts

The font family and the type scale. Override `Nocturne.Fonts.Family` to use another font.

| Key | Type | Value |
| --- | --- | --- |
| `Nocturne.Fonts.Family` | FontFamily | `Segoe UI` |
| `Nocturne.Fonts.Family.Mono` | FontFamily | `Cascadia Mono, Consolas` |
| `Nocturne.Font.Size.Title` | Double | `30` |
| `Nocturne.Font.Size.Subtitle` | Double | `20` |
| `Nocturne.Font.Size.ListTitle` | Double | `16` |
| `Nocturne.Font.Size.Default` | Double | `14` |
| `Nocturne.Font.Size.Caption` | Double | `13` |
| `Nocturne.Font.Size.Small` | Double | `12` |
| `Nocturne.Font.Size.Tag` | Double | `11` |

## Icons

Line icons (24×24 geometries) for `nc:StrokeIcon` and `ControlHelper.Icon`, as in [Typography and surfaces](typography.md#icons).

`Nocturne.Icons.Check`, `Nocturne.Icons.ChevronDown`, `Nocturne.Icons.ChevronUp`, `Nocturne.Icons.ChevronRight`, `Nocturne.Icons.ChevronLeft`, `Nocturne.Icons.Close`, `Nocturne.Icons.Plus`, `Nocturne.Icons.Search`, `Nocturne.Icons.Refresh`, `Nocturne.Icons.Eye`, `Nocturne.Icons.Copy`, `Nocturne.Icons.Cut`, `Nocturne.Icons.Paste`, `Nocturne.Icons.Undo`, `Nocturne.Icons.SelectAll`, `Nocturne.Icons.Edit`, `Nocturne.Icons.Delete`, `Nocturne.Icons.Sync`, `Nocturne.Icons.Shield`, `Nocturne.Icons.Globe`, `Nocturne.Icons.Database`, `Nocturne.Icons.Grid`, `Nocturne.Icons.Layers`, `Nocturne.Icons.Text`, `Nocturne.Icons.Clock`, `Nocturne.Icons.Calendar`, `Nocturne.Icons.Folder`, `Nocturne.Icons.Info`, `Nocturne.Icons.CheckCircle`, `Nocturne.Icons.Warning`, `Nocturne.Icons.Error`, `Nocturne.Icons.Components`, `Nocturne.Icons.Filter`, `Nocturne.Icons.PageFirst`, `Nocturne.Icons.PageLast`, `Nocturne.Icons.Menu`, `Nocturne.Icons.File`, `Nocturne.Icons.Caption.Minimize`, `Nocturne.Icons.Caption.Maximize`, `Nocturne.Icons.Caption.Restore`, `Nocturne.Icons.Caption.Close`

## Named styles

Every standard control also gets an implicit style (no key needed). These keyed styles are the
variants and building blocks; use them with `Style="{StaticResource ...}"` or as `BasedOn`.

| Key | Target type | File |
| --- | --- | --- |
| `Nocturne.Styles.Button` | `ButtonBase` | Controls.Buttons.xaml |
| `Nocturne.Styles.Button.Primary` | `ButtonBase` | Controls.Buttons.xaml |
| `Nocturne.Styles.Button.Danger` | `ButtonBase` | Controls.Buttons.xaml |
| `Nocturne.Styles.Button.Subtle` | `ButtonBase` | Controls.Buttons.xaml |
| `Nocturne.Styles.Button.Icon` | `ButtonBase` | Controls.Buttons.xaml |
| `Nocturne.Styles.Button.Icon.Subtle` | `ButtonBase` | Controls.Buttons.xaml |
| `Nocturne.Styles.DropDownButton.Primary` | `c:DropDownButton` | Controls.Buttons.xaml |
| `Nocturne.Styles.SplitButton.Arrow` | `ToggleButton` | Controls.Buttons.xaml |
| `Nocturne.Styles.SplitButton.Primary` | `c:SplitButton` | Controls.Buttons.xaml |
| `Nocturne.Styles.Tile` | `c:Tile` | Controls.Buttons.xaml |
| `Nocturne.Styles.Tile.Accent` | `c:Tile` | Controls.Buttons.xaml |
| `Nocturne.Styles.CalendarDayButton` | `CalendarDayButton` | Controls.Calendar.xaml |
| `Nocturne.Styles.CalendarButton` | `CalendarButton` | Controls.Calendar.xaml |
| `Nocturne.Styles.Calendar.NavButton` | `Button` | Controls.Calendar.xaml |
| `Nocturne.Styles.Calendar.HeaderButton` | `Button` | Controls.Calendar.xaml |
| `Nocturne.Styles.CalendarItem` | `CalendarItem` | Controls.Calendar.xaml |
| `Nocturne.Styles.Calendar.Popup` | `Calendar` | Controls.Calendar.xaml |
| `Nocturne.Styles.DatePicker.Button` | `Button` | Controls.Calendar.xaml |
| `Nocturne.Styles.Expander.Header` | `ToggleButton` | Controls.Containers.xaml |
| `Nocturne.Styles.TabItem.CloseButton` | `Button` | Controls.Containers.xaml |
| `Nocturne.Styles.TabItem.Document` | `TabItem` | Controls.Containers.xaml |
| `Nocturne.Styles.TabItem.AddButton` | `Button` | Controls.Containers.xaml |
| `Nocturne.Styles.TabControl.Document` | `TabControl` | Controls.Containers.xaml |
| `Nocturne.Styles.GridSplitter.Preview` | `Control` | Controls.Containers.xaml |
| `Nocturne.Styles.Expander.Accordion` | `Expander` | Controls.Containers.xaml |
| `Nocturne.Styles.DataGrid.HeaderGripper` | `Thumb` | Controls.DataGrid.xaml |
| `Nocturne.Styles.DataGridColumnHeader` | `DataGridColumnHeader` | Controls.DataGrid.xaml |
| `Nocturne.Styles.DataGrid.SelectAll` | `Button` | Controls.DataGrid.xaml |
| `Nocturne.Styles.NocturneGrid.FilterButton` | `ToggleButton` | Controls.DataGrid.xaml |
| `Nocturne.Styles.NocturneGrid.ColumnHeader` | `DataGridColumnHeader` | Controls.DataGrid.xaml |
| `Nocturne.Styles.NocturneGrid.GroupHeader` | `ToggleButton` | Controls.DataGrid.xaml |
| `Nocturne.Styles.NocturneGrid.PagerButton` | `Button` | Controls.DataGrid.xaml |
| `Nocturne.Styles.NocturneGrid` | `c:NocturneGrid` | Controls.DataGrid.xaml |
| `Nocturne.Styles.ListBoxItem.Navigation` | `ListBoxItem` | Controls.Lists.xaml |
| `Nocturne.Styles.ListBox.Navigation` | `ListBox` | Controls.Lists.xaml |
| `Nocturne.Styles.ComboBox.Toggle` | `ToggleButton` | Controls.Lists.xaml |
| `Nocturne.Styles.ComboBox.EditableTextBox` | `TextBox` | Controls.Lists.xaml |
| `Nocturne.Styles.ScrollViewer.GridView` | `ScrollViewer` | Controls.Lists.xaml |
| `Nocturne.Styles.TreeViewItem.Expander` | `ToggleButton` | Controls.Lists.xaml |
| `Nocturne.Styles.NocturneGrid.DateFilterItem` | `TreeViewItem` | Controls.Lists.xaml |
| `Nocturne.Styles.HamburgerMenu.Toggle` | `ToggleButton` | Controls.Lists.xaml |
| `Nocturne.Styles.MenuItem.TextEdit` | `MenuItem` | Controls.Menu.xaml |
| `Nocturne.Styles.Separator.TextEdit` | `Separator` | Controls.Menu.xaml |
| `Nocturne.Styles.ToolBar.Thumb` | `Thumb` | Controls.Menu.xaml |
| `Nocturne.Styles.ToolBar.Overflow` | `ToggleButton` | Controls.Menu.xaml |
| `Nocturne.Styles.ListBoxItem.Popup` | `ListBoxItem` | Controls.Pickers.xaml |
| `Nocturne.Styles.ListBox.Popup` | `ListBox` | Controls.Pickers.xaml |
| `Nocturne.Styles.ToggleButton.Field` | `ToggleButton` | Controls.Pickers.xaml |
| `Nocturne.Styles.Picker.Base` | `Control` | Controls.Pickers.xaml |
| `Nocturne.Styles.ColorPicker.Swatch` | `ListBoxItem` | Controls.Pickers.xaml |
| `Nocturne.Styles.ScrollBar.PageButton` | `RepeatButton` | Controls.Range.xaml |
| `Nocturne.Styles.ScrollBar.Thumb` | `Thumb` | Controls.Range.xaml |
| `Nocturne.Styles.ScrollBar.Thumb.Horizontal` | `Thumb` | Controls.Range.xaml |
| `Nocturne.Styles.Slider.Fill` | `RepeatButton` | Controls.Range.xaml |
| `Nocturne.Styles.Slider.Rest` | `RepeatButton` | Controls.Range.xaml |
| `Nocturne.Styles.Slider.Thumb` | `Thumb` | Controls.Range.xaml |
| `Nocturne.Styles.FocusVisual.Round` | `` | Controls.Range.xaml |
| `Nocturne.Styles.FocusVisual` | `` | Controls.Shared.xaml |
| `Nocturne.Styles.FocusVisual.Inset` | `` | Controls.Shared.xaml |
| `Nocturne.Styles.TextBlock.Title` | `TextBlock` | Controls.Shared.xaml |
| `Nocturne.Styles.TextBlock.Subtitle` | `TextBlock` | Controls.Shared.xaml |
| `Nocturne.Styles.TextBlock.Eyebrow` | `TextBlock` | Controls.Shared.xaml |
| `Nocturne.Styles.TextBlock.ListTitle` | `TextBlock` | Controls.Shared.xaml |
| `Nocturne.Styles.TextBlock.Body` | `TextBlock` | Controls.Shared.xaml |
| `Nocturne.Styles.TextBlock.Caption` | `TextBlock` | Controls.Shared.xaml |
| `Nocturne.Styles.TextBlock.Label` | `TextBlock` | Controls.Shared.xaml |
| `Nocturne.Styles.TextBlock.Hint` | `TextBlock` | Controls.Shared.xaml |
| `Nocturne.Styles.TextBlock.Error` | `TextBlock` | Controls.Shared.xaml |
| `Nocturne.Styles.TextBlock.Mono` | `TextBlock` | Controls.Shared.xaml |
| `Nocturne.Styles.Border.Card` | `Border` | Controls.Shared.xaml |
| `Nocturne.Styles.Card` | `HeaderedContentControl` | Controls.Shared.xaml |
| `Nocturne.Styles.Tag` | `ContentControl` | Controls.Shared.xaml |
| `Nocturne.Styles.Pill` | `ContentControl` | Controls.Shared.xaml |
| `Nocturne.Styles.Pill.Success` | `ContentControl` | Controls.Shared.xaml |
| `Nocturne.Styles.Pill.Warning` | `ContentControl` | Controls.Shared.xaml |
| `Nocturne.Styles.Pill.Error` | `ContentControl` | Controls.Shared.xaml |
| `Nocturne.Styles.Badge` | `ContentControl` | Controls.Shared.xaml |
| `Nocturne.Styles.IconTile` | `ContentControl` | Controls.Shared.xaml |
| `Nocturne.Styles.IconTile.Small` | `ContentControl` | Controls.Shared.xaml |
| `Nocturne.Styles.Field.Base` | `Control` | Controls.Text.xaml |
| `Nocturne.Styles.Button.Field` | `ButtonBase` | Controls.Text.xaml |
| `Nocturne.Styles.TextBox.Search` | `TextBox` | Controls.Text.xaml |
| `Nocturne.Styles.TextBox.MultiLine` | `TextBox` | Controls.Text.xaml |
| `Nocturne.Styles.ToggleButton.Reveal` | `ToggleButton` | Controls.Text.xaml |
| `Nocturne.Styles.TextBox.Inner` | `TextBox` | Controls.Text.xaml |
| `Nocturne.Styles.RepeatButton.Field` | `RepeatButton` | Controls.Text.xaml |
| `Nocturne.Styles.Toggle.Base` | `ToggleButton` | Controls.Toggles.xaml |
| `Nocturne.Styles.ToggleSwitch` | `ToggleButton` | Controls.Toggles.xaml |
| `Nocturne.Styles.Button.WindowCaption` | `Button` | Controls.Window.xaml |
| `Nocturne.Styles.Button.WindowClose` | `Button` | Controls.Window.xaml |
