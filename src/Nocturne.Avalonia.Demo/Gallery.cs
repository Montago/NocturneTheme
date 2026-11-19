using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Nocturne.Avalonia.Controls;

namespace Nocturne.Avalonia.Demo;

/// <summary>Attached helpers for the gallery samples (Views/GalleryPage.axaml).</summary>
public class Gallery : AvaloniaObject
{
    /// <summary>Open this element's drop-down, menu or tool tip for the sample's picture.</summary>
    public static readonly AttachedProperty<bool> OpenProperty =
        AvaloniaProperty.RegisterAttached<Gallery, Control, bool>("Open");

    public static bool GetOpen(Control control) => control.GetValue(OpenProperty);
    public static void SetOpen(Control control, bool value) => control.SetValue(OpenProperty, value);

    /// <summary>Opens what <paramref name="control"/> drops down, as a user would; returns what closes it again.</summary>
    public static Action Open(Control control)
    {
        switch (control)
        {
            case NocturneGrid grid:
            {
                // The filter of the first filtered column, as in the WPF gallery
                var button = grid.GetVisualDescendants().OfType<DataGridColumnHeader>()
                    .Where(h => DataGridColumn.GetColumnContainingElement(h) is { } column && NocturneGrid.GetHasFilter(column))
                    .SelectMany(h => h.GetVisualDescendants().OfType<ToggleButton>().Where(b => b.Name == "PART_FilterButton"))
                    .FirstOrDefault();
                if (button == null)
                {
                    return () => { };
                }
                button.IsChecked = true;
                return () => button.IsChecked = false;
            }
            case ComboBox combo:
                combo.IsDropDownOpen = true;
                return () => combo.IsDropDownOpen = false;
            case CalendarDatePicker picker:
                picker.IsDropDownOpen = true;
                return () => picker.IsDropDownOpen = false;
            case AutoCompleteBox auto:
                auto.IsDropDownOpen = true;
                return () => auto.IsDropDownOpen = false;
            case ColorPicker colour:
                colour.IsDropDownOpen = true;
                return () => colour.IsDropDownOpen = false;
            case SearchComboBox search:
                search.IsDropDownOpen = true;
                return () => search.IsDropDownOpen = false;
            case DatePicker or TimePicker:
            {
                // No open property: the field is a button that opens the popup
                control.GetVisualDescendants().OfType<Button>().First(b => b.Name == "PART_FlyoutButton")
                    .RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                var popup = control.GetVisualDescendants().OfType<global::Avalonia.Controls.Primitives.Popup>().First();
                return () => popup.IsOpen = false;
            }
            case MenuItem item:
                item.Open();
                return item.Close;
            case Button { Flyout: { } flyout } button:
                flyout.ShowAt(button);
                return flyout.Hide;
            case SplitButton { Flyout: { } splitFlyout } split:
                splitFlyout.ShowAt(split);
                return splitFlyout.Hide;
            case TextBox text when DataValidationErrors.GetHasErrors(text):
                // The validation callout shows while the field has focus
                text.Focus();
                return () => TopLevel.GetTopLevel(text)?.Focus();
            case TextBox { ContextFlyout: PopupFlyoutBase textMenu } text:
                // The text fields' menu, opened a little into the field as a right-click would
                text.Focus();
                textMenu.Placement = PlacementMode.AnchorAndGravity;
                textMenu.PlacementAnchor = PopupAnchor.TopLeft;
                textMenu.PlacementGravity = PopupGravity.BottomRight;
                textMenu.HorizontalOffset = 90;
                textMenu.VerticalOffset = 24;
                textMenu.ShowAt(text);
                return textMenu.Hide;
            case { ContextMenu: { } menu }:
                menu.Placement = PlacementMode.AnchorAndGravity;
                menu.PlacementAnchor = PopupAnchor.TopLeft;
                menu.PlacementGravity = PopupGravity.BottomRight;
                menu.HorizontalOffset = 120;
                menu.VerticalOffset = 40;
                menu.Open(control);
                return menu.Close;
            default:
                if (ToolTip.GetTip(control) == null)
                    return () => { };
                ToolTip.SetIsOpen(control, true);
                return () => ToolTip.SetIsOpen(control, false);
        }
    }
}
