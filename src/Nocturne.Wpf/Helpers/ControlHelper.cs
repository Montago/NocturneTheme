using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Nocturne.Wpf.Helpers;

/// <summary>Attached properties the Nocturne templates read.</summary>
public static class ControlHelper
{
    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.RegisterAttached(
        "CornerRadius", typeof(CornerRadius), typeof(ControlHelper), new FrameworkPropertyMetadata(new CornerRadius(8)));

    /// <summary>Corner radius of buttons, fields and items (8 by default).</summary>
    public static CornerRadius GetCornerRadius(DependencyObject d) => (CornerRadius)d.GetValue(CornerRadiusProperty);

    public static void SetCornerRadius(DependencyObject d, CornerRadius value) => d.SetValue(CornerRadiusProperty, value);

    public static readonly DependencyProperty HoverBackgroundProperty = DependencyProperty.RegisterAttached(
        "HoverBackground", typeof(Brush), typeof(ControlHelper), new FrameworkPropertyMetadata(null));

    /// <summary>Background a button fades to (120 ms) while the mouse is over it.</summary>
    public static Brush? GetHoverBackground(DependencyObject d) => (Brush?)d.GetValue(HoverBackgroundProperty);

    public static void SetHoverBackground(DependencyObject d, Brush? value) => d.SetValue(HoverBackgroundProperty, value);

    public static readonly DependencyProperty PressedBackgroundProperty = DependencyProperty.RegisterAttached(
        "PressedBackground", typeof(Brush), typeof(ControlHelper), new FrameworkPropertyMetadata(null));

    /// <summary>Background of a button while it is pressed.</summary>
    public static Brush? GetPressedBackground(DependencyObject d) => (Brush?)d.GetValue(PressedBackgroundProperty);

    public static void SetPressedBackground(DependencyObject d, Brush? value) => d.SetValue(PressedBackgroundProperty, value);

    public static readonly DependencyProperty HighlightBrushProperty = DependencyProperty.RegisterAttached(
        "HighlightBrush", typeof(Brush), typeof(ControlHelper), new FrameworkPropertyMetadata(null));

    /// <summary>The 1px light along the inside of the top edge (primary buttons, cards).</summary>
    public static Brush? GetHighlightBrush(DependencyObject d) => (Brush?)d.GetValue(HighlightBrushProperty);

    public static void SetHighlightBrush(DependencyObject d, Brush? value) => d.SetValue(HighlightBrushProperty, value);

    public static readonly DependencyProperty HasShadowProperty = DependencyProperty.RegisterAttached(
        "HasShadow", typeof(bool), typeof(ControlHelper), new FrameworkPropertyMetadata(false));

    /// <summary>A small drop shadow under the control (primary buttons).</summary>
    public static bool GetHasShadow(DependencyObject d) => (bool)d.GetValue(HasShadowProperty);

    public static void SetHasShadow(DependencyObject d, bool value) => d.SetValue(HasShadowProperty, value);

    public static readonly DependencyProperty HasErrorProperty = DependencyProperty.RegisterAttached(
        "HasError", typeof(bool), typeof(ControlHelper), new FrameworkPropertyMetadata(false));

    /// <summary>Shows the error look without a validation binding (validation errors show it too).</summary>
    public static bool GetHasError(DependencyObject d) => (bool)d.GetValue(HasErrorProperty);

    public static void SetHasError(DependencyObject d, bool value) => d.SetValue(HasErrorProperty, value);

    public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.RegisterAttached(
        "Placeholder", typeof(string), typeof(ControlHelper), new FrameworkPropertyMetadata(null));

    /// <summary>Hint text shown in an empty TextBox, PasswordBox or ComboBox.</summary>
    public static string? GetPlaceholder(DependencyObject d) => (string?)d.GetValue(PlaceholderProperty);

    public static void SetPlaceholder(DependencyObject d, string? value) => d.SetValue(PlaceholderProperty, value);

    public static readonly DependencyProperty IconProperty = DependencyProperty.RegisterAttached(
        "Icon", typeof(Geometry), typeof(ControlHelper), new FrameworkPropertyMetadata(null));

    /// <summary>A stroke icon (24 x 24 units) shown by templates that have room for one, e.g. menu items.</summary>
    public static Geometry? GetIcon(DependencyObject d) => (Geometry?)d.GetValue(IconProperty);

    public static void SetIcon(DependencyObject d, Geometry? value) => d.SetValue(IconProperty, value);

    public static readonly DependencyProperty ShowChevronProperty = DependencyProperty.RegisterAttached(
        "ShowChevron", typeof(bool), typeof(ControlHelper), new FrameworkPropertyMetadata(false));

    /// <summary>A chevron after a button's content, as on DropDownButton.</summary>
    public static bool GetShowChevron(DependencyObject d) => (bool)d.GetValue(ShowChevronProperty);

    public static void SetShowChevron(DependencyObject d, bool value) => d.SetValue(ShowChevronProperty, value);

    /// <summary>Empties the TextBox the command is raised in (the clear button of <c>ClearButton</c>).</summary>
    public static readonly RoutedCommand ClearTextCommand = new(nameof(ClearTextCommand), typeof(ControlHelper));

    public static readonly DependencyProperty ClearButtonProperty = DependencyProperty.RegisterAttached(
        "ClearButton", typeof(bool), typeof(ControlHelper), new FrameworkPropertyMetadata(false));

    /// <summary>An × button at the end of a TextBox that has text; it empties the box.</summary>
    public static bool GetClearButton(DependencyObject d) => (bool)d.GetValue(ClearButtonProperty);

    public static void SetClearButton(DependencyObject d, bool value) => d.SetValue(ClearButtonProperty, value);

    static ControlHelper()
    {
        CommandManager.RegisterClassCommandBinding(typeof(TextBox), new CommandBinding(ClearTextCommand, OnClearText, OnCanClearText));
    }

    private static void OnCanClearText(object sender, CanExecuteRoutedEventArgs e) =>
        e.CanExecute = sender is TextBox { IsReadOnly: false, IsEnabled: true };

    private static void OnClearText(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is TextBox box)
        {
            box.Clear();
            box.Focus();
            e.Handled = true;
        }
    }
}
