using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Nocturne.Wpf.Helpers;

/// <summary>
/// Sends a context menu's commands to the element it was opened on. A MenuItem without a
/// <c>CommandTarget</c> runs its command on itself, inside the menu's popup, where a text box's Cut or
/// Paste is never handled; and it does not ask again whether the command can run when a
/// <c>CommandTarget</c> arrives later through a binding. With <c>TargetsPlacement</c> set, every
/// opening points the items at <see cref="ContextMenu.PlacementTarget"/> and asks again, so Cut and
/// Copy are dimmed exactly while the field has no selection. <c>Nocturne.ContextMenu.Text</c> uses it.
/// </summary>
public static class ContextMenuHelper
{
    public static readonly DependencyProperty TargetsPlacementProperty = DependencyProperty.RegisterAttached(
        "TargetsPlacement", typeof(bool), typeof(ContextMenuHelper), new PropertyMetadata(false, OnTargetsPlacementChanged));

    /// <summary>Point the menu's commands at the element it was opened on, each time it opens.</summary>
    public static bool GetTargetsPlacement(DependencyObject d) => (bool)d.GetValue(TargetsPlacementProperty);

    public static void SetTargetsPlacement(DependencyObject d, bool value) => d.SetValue(TargetsPlacementProperty, value);

    private static void OnTargetsPlacementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ContextMenu menu)
        {
            menu.Opened -= OnOpened;
            if (e.NewValue is true)
            {
                menu.Opened += OnOpened;
            }
        }
    }

    private static void OnOpened(object sender, RoutedEventArgs e)
    {
        var menu = (ContextMenu)sender;
        if (menu.PlacementTarget is not IInputElement target)
        {
            return;
        }

        foreach (var item in menu.Items.OfType<MenuItem>())
        {
            if (item.Command != null)
            {
                item.CommandTarget = target;
            }
        }
        CommandManager.InvalidateRequerySuggested();
    }
}
