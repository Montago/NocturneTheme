using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Nocturne.Avalonia.Helpers;

/// <summary>
/// Lets Enter and Tab take a match in an AutoCompleteBox without the arrow keys first (the Nocturne
/// AutoCompleteBox theme turns it on). Avalonia's own box only takes an item the user has moved to
/// with Up or Down: Enter just closes the list and Tab leaves the typed text. With the list open, both
/// now take the highlighted item, else the item that equals the text, else the first one in the list.
/// Enter stays in the box; Tab still moves focus on. With <see cref="AutoCompleteFilterMode.None"/>
/// the list is not filtered, so only a highlighted item is taken.
/// </summary>
public class AutoCompleteBoxHelper
{
    public static readonly AttachedProperty<bool> CommitMatchProperty =
        AvaloniaProperty.RegisterAttached<AutoCompleteBoxHelper, AutoCompleteBox, bool>("CommitMatch");

    private static readonly AttachedProperty<ListBox?> ListProperty =
        AvaloniaProperty.RegisterAttached<AutoCompleteBoxHelper, AutoCompleteBox, ListBox?>("List");

    static AutoCompleteBoxHelper()
    {
        CommitMatchProperty.Changed.AddClassHandler<AutoCompleteBox>(OnCommitMatchChanged);
    }

    public static bool GetCommitMatch(AutoCompleteBox box) => box.GetValue(CommitMatchProperty);

    public static void SetCommitMatch(AutoCompleteBox box, bool value) => box.SetValue(CommitMatchProperty, value);

    private static void OnCommitMatchChanged(AutoCompleteBox box, AvaloniaPropertyChangedEventArgs e)
    {
        box.RemoveHandler(InputElement.KeyDownEvent, OnKeyDown);
        box.TemplateApplied -= OnTemplateApplied;
        if (e.NewValue is true)
        {
            // Tunnel, so it runs before the box's own Enter handling and the window's Tab navigation.
            box.AddHandler(InputElement.KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
            box.TemplateApplied += OnTemplateApplied;
        }
    }

    private static void OnTemplateApplied(object? sender, TemplateAppliedEventArgs e) =>
        ((AutoCompleteBox)sender!).SetValue(ListProperty, e.NameScope.Find<ListBox>("PART_SelectingItemsControl"));

    private static void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Handled || e.Key is not (Key.Enter or Key.Tab) || e.KeyModifiers != KeyModifiers.None)
            return;
        var box = (AutoCompleteBox)sender!;
        var list = box.GetValue(ListProperty);
        if (!box.IsDropDownOpen || list == null || list.ItemCount == 0)
            return;

        var item = FindMatch(box, list);
        if (item == null)
            return;

        // Setting SelectedItem writes the item's text into the box (ValueMemberBinding, else ToString).
        box.SelectedItem = item;
        box.IsDropDownOpen = false;
        box.CaretIndex = box.Text?.Length ?? 0;
        if (e.Key == Key.Enter)
            e.Handled = true;
    }

    private static object? FindMatch(AutoCompleteBox box, ListBox list)
    {
        if (list.SelectedItem != null)
            return list.SelectedItem;
        if (box.FilterMode == AutoCompleteFilterMode.None)
            return null;

        var text = box.Text ?? "";
        if (text.Length == 0)
            return null;
        object? first = null;
        foreach (var item in list.Items)
        {
            first ??= item;
            if (string.Equals(TextOf(box, item), text, StringComparison.CurrentCultureIgnoreCase))
                return item;
        }
        return first;
    }

    private static string? TextOf(AutoCompleteBox box, object? item)
    {
        if (item == null || box.ValueMemberBinding == null)
            return item?.ToString();
        // Evaluate the binding on a throwaway control, as the box itself does.
        var probe = new ContentControl { DataContext = item };
        using (probe.Bind(ContentControl.ContentProperty, box.ValueMemberBinding))
            return probe.Content?.ToString();
    }
}
