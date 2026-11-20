---
title: Helpers and custom templates
parent: Reference
grand_parent: Nocturne for WPF
nav_order: 2
---

# Helpers and custom templates

The helpers are attached properties in `Nocturne.Wpf.Helpers`, under the same `nc:` namespace as the
controls. The theme's templates read them, so they work on the standard controls without a new style.

## ControlHelper

| Property | On | |
| --- | --- | --- |
| `Placeholder` | `TextBox`, `PasswordBox`, `ComboBox`, the pickers | Hint text shown while the field is empty |
| `HasError` | Fields | The error look without a validation binding (validation errors show it too) |
| `ClearButton` | `TextBox` | An × at the end while there is text; it empties the box (`ClearTextCommand`) |
| `Icon` | `MenuItem`, `AutoSuggestBox` | A 24×24 stroke geometry (`Nocturne.Icons.*`) shown before the content |
| `ShowChevron` | `Button` | A chevron after the content, as on `DropDownButton` |
| `CornerRadius` | Buttons, fields, items | Corner radius (8) |
| `HoverBackground` | Buttons | The background faded to (120 ms) under the mouse |
| `PressedBackground` | Buttons | The background while pressed |
| `HighlightBrush` | Primary buttons, cards | The 1px light along the inside of the top edge |
| `HasShadow` | Buttons | A small drop shadow under the control |

```xml
<TextBox nc:ControlHelper.Placeholder="Search files" nc:ControlHelper.ClearButton="True" />
<MenuItem Header="Copy" nc:ControlHelper.Icon="{StaticResource Nocturne.Icons.Copy}" />
<Button Content="Options" nc:ControlHelper.ShowChevron="True" />
```

## PasswordBoxHelper

| Property | |
| --- | --- |
| `CanReveal` | Shows an eye button that toggles the password between dots and plain text |
| `IsRevealed` | Whether the password is shown as text (the eye button sets it) |
| `HasPassword` | Read-only: true while the box holds a password (for the placeholder) |

## TabHelper

For `Nocturne.Styles.TabControl.Document` ([Document tabs](navigation.md#document-tabs)).

| Member | |
| --- | --- |
| `ShowCloseButton` | A close button on the tab; set it on the `TabControl` for every tab, or per `TabItem` |
| `ShowAddButton` | A "+" button after the last tab; it runs `AddTabCommand` |
| `Badge` | A small count or label after a tab's header |
| `CloseTabCommand` | Closes the `TabItem` in the parameter: raises `TabClosing`, then removes the tab or its item |
| `AddTabCommand` | Run by the "+" button; handle it with a `CommandBinding` and add the tab there |
| `TabClosing` | Attached event; set `e.Cancel` to keep the tab. `e.Item` is the data item (or the `TabItem`) |

```xml
<TabControl Style="{StaticResource Nocturne.Styles.TabControl.Document}" ItemsSource="{Binding Documents}"
            nc:TabHelper.ShowCloseButton="True" nc:TabHelper.TabClosing="OnTabClosing" />
```

## ScrollViewerHelper

`ChainMouseWheel` passes a wheel turn that a scroll viewer cannot use (it has nothing to scroll, or is
at its end in that direction) to the nearest scroll viewer around it that can. WPF's `ScrollViewer`
swallows every turn otherwise, so a `ListBox` or `DataGrid` inside a scrolling page stops the page.
The theme's `ScrollViewer` style turns it on; only visual parents are searched, so a popup never
scrolls the page under it.

`AutoHide` fades the scroll bars out (150 ms) while the mouse is away from the scroll viewer: they
show while the mouse is over it, and for a second after the content scrolls. The bars keep their room.
The theme's `ScrollViewer` styles turn it on; set it to `False` on a scroll viewer whose bars should
always show.

```xml
<ScrollViewer nc:ScrollViewerHelper.AutoHide="False">...</ScrollViewer>
```

## ContextMenuHelper

`TargetsPlacement` sends a context menu's commands to the element it was opened on. A `MenuItem`
without a `CommandTarget` runs its command on itself, inside the menu's popup, where a text box's Cut
or Paste is never handled. With `TargetsPlacement="True"` on the `ContextMenu`, every opening points
the items at the element and asks again whether each command can run, so Cut and Copy are dimmed
exactly while nothing is selected. The text fields' own menu uses it ([The text menu](navigation.md#the-text-menu)).

## FocusHelper

`FocusVisualStyle` draws around the whole element, which for a `TreeViewItem` means its whole
subtree. A template can draw its own ring instead: `TracksKeyboardFocus="True"` on the element keeps
the read-only `IsKeyboardFocusVisible` up to date (true while it has keyboard focus and the keyboard
was the last input used), and a trigger shows the ring.

```xml
<Setter Property="nc:FocusHelper.TracksKeyboardFocus" Value="True" />
...
<Trigger Property="nc:FocusHelper.IsKeyboardFocusVisible" Value="True">
    <Setter TargetName="FocusRing" Property="Visibility" Value="Visible" />
</Trigger>
```

## Template parts

When you replace the template of one of Nocturne's controls, keep these named parts; the control's
code looks for them.

| Control | Parts |
| --- | --- |
| `NocturneWindow` | `PART_Root`, `PART_Light` (the dithered light), `PART_DialogHost` (a `Panel` for dialogs) |
| `NocturneDialog` | `PART_Affirmative`, `PART_Negative` (buttons) |
| `Flyout`, `InfoBar` | `PART_Close` (button) |
| `SplitButton` | `PART_Arrow` (`ToggleButton`, the chevron part) |
| `NumericUpDown` | `PART_TextBox`, `PART_Up`, `PART_Down` |
| `TimePicker`, `AutoSuggestBox`, `SearchComboBox` | `PART_TextBox`, `PART_Popup`, `PART_List` (`ListBox`) |
| `ColorPicker` | `PART_Palette` (`ListBox`) |
| `RangeSlider` | `PART_Track`, `PART_Fill`, `PART_LowerThumb`, `PART_UpperThumb` |
| `Rating` | `PART_Host` (a `Panel` for the stars) |
| `BusyIndicator` | `PART_Content` (the element that is blurred) |
| `NocturneGrid` | The row template's `DGR_Border`, the scroll viewer's `PART_ScrubLayer`, a `GridColumnHeadersPresenter`, and the column header's `PART_FilterButton` and `PART_FilterBox` |

The theme's own templates are in `src/Nocturne.Wpf/Styles/Controls.*.xaml`, one file per group of
controls; copy one as the starting point.
