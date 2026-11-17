using System.Windows;
using System.Windows.Input;

namespace Nocturne.Wpf.Helpers;

/// <summary>
/// Keyboard focus that came from the keyboard. FocusVisualStyle does this for most controls, but
/// it draws around the whole element, which for a TreeViewItem means its whole subtree. A template
/// can instead draw its own ring while <c>IsKeyboardFocusVisible</c> is true:
/// set <c>TracksKeyboardFocus</c> in the style and trigger on <c>IsKeyboardFocusVisible</c>.
/// </summary>
public static class FocusHelper
{
    public static readonly DependencyProperty TracksKeyboardFocusProperty = DependencyProperty.RegisterAttached(
        "TracksKeyboardFocus", typeof(bool), typeof(FocusHelper), new FrameworkPropertyMetadata(false, OnTracksKeyboardFocusChanged));

    /// <summary>Keep <c>IsKeyboardFocusVisible</c> up to date for this element.</summary>
    public static bool GetTracksKeyboardFocus(DependencyObject d) => (bool)d.GetValue(TracksKeyboardFocusProperty);

    public static void SetTracksKeyboardFocus(DependencyObject d, bool value) => d.SetValue(TracksKeyboardFocusProperty, value);

    private static readonly DependencyPropertyKey IsKeyboardFocusVisiblePropertyKey = DependencyProperty.RegisterAttachedReadOnly(
        "IsKeyboardFocusVisible", typeof(bool), typeof(FocusHelper), new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty IsKeyboardFocusVisibleProperty = IsKeyboardFocusVisiblePropertyKey.DependencyProperty;

    /// <summary>True while the element has keyboard focus and the keyboard was the last input used.</summary>
    public static bool GetIsKeyboardFocusVisible(DependencyObject d) => (bool)d.GetValue(IsKeyboardFocusVisibleProperty);

    private static void OnTracksKeyboardFocusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element)
        {
            return;
        }

        element.GotKeyboardFocus -= OnGotKeyboardFocus;
        element.LostKeyboardFocus -= OnLostKeyboardFocus;
        element.PreviewKeyDown -= OnPreviewKeyDown;
        if (e.NewValue is true)
        {
            element.GotKeyboardFocus += OnGotKeyboardFocus;
            element.LostKeyboardFocus += OnLostKeyboardFocus;
            element.PreviewKeyDown += OnPreviewKeyDown;
        }
        else
        {
            element.ClearValue(IsKeyboardFocusVisiblePropertyKey);
        }
    }

    private static void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        // The event bubbles: only the element that now has focus counts.
        if (ReferenceEquals(e.NewFocus, sender))
        {
            ((DependencyObject)sender).SetValue(IsKeyboardFocusVisiblePropertyKey, InputManager.Current.MostRecentInputDevice is KeyboardDevice);
        }
    }

    private static void OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (ReferenceEquals(e.OldFocus, sender))
        {
            ((DependencyObject)sender).SetValue(IsKeyboardFocusVisiblePropertyKey, false);
        }
    }

    /// <summary>Focused with the mouse, then a key pressed: from now on it was the keyboard.</summary>
    private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is UIElement { IsKeyboardFocused: true } element)
        {
            element.SetValue(IsKeyboardFocusVisiblePropertyKey, true);
        }
    }
}
