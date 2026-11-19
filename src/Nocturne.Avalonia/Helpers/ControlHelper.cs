using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Nocturne.Avalonia.Helpers;

/// <summary>
/// Attached properties the Nocturne templates read. Corner radii use the controls' own
/// <c>CornerRadius</c>; placeholders use Avalonia's <c>PlaceholderText</c>. <see cref="ClearButtonProperty"/>
/// and <see cref="HasErrorProperty"/> set the style classes <c>clearButton</c> and <c>hasError</c>,
/// which the themes select on (the classes can also be set directly, as in Avalonia's own themes).
/// </summary>
public class ControlHelper
{
    static ControlHelper()
    {
        ClearButtonProperty.Changed.AddClassHandler<Control>((c, e) => c.Classes.Set("clearButton", e.NewValue is true));
        HasErrorProperty.Changed.AddClassHandler<Control>((c, e) => c.Classes.Set("hasError", e.NewValue is true));
    }

    /// <summary>Background a button fades to (120 ms) while the pointer is over it.</summary>
    public static readonly AttachedProperty<IBrush?> HoverBackgroundProperty =
        AvaloniaProperty.RegisterAttached<ControlHelper, Control, IBrush?>("HoverBackground");

    /// <summary>Background of a button while it is pressed.</summary>
    public static readonly AttachedProperty<IBrush?> PressedBackgroundProperty =
        AvaloniaProperty.RegisterAttached<ControlHelper, Control, IBrush?>("PressedBackground");

    /// <summary>The 1px light along the inside of the top edge (primary buttons, cards).</summary>
    public static readonly AttachedProperty<IBrush?> HighlightBrushProperty =
        AvaloniaProperty.RegisterAttached<ControlHelper, Control, IBrush?>("HighlightBrush");

    /// <summary>A small drop shadow under the control (primary buttons).</summary>
    public static readonly AttachedProperty<bool> HasShadowProperty =
        AvaloniaProperty.RegisterAttached<ControlHelper, Control, bool>("HasShadow");

    /// <summary>Shows the error look without a validation binding (validation errors show it too).</summary>
    public static readonly AttachedProperty<bool> HasErrorProperty =
        AvaloniaProperty.RegisterAttached<ControlHelper, Control, bool>("HasError");

    /// <summary>A stroke icon (24 x 24 units) shown by templates that have room for one.</summary>
    public static readonly AttachedProperty<Geometry?> IconProperty =
        AvaloniaProperty.RegisterAttached<ControlHelper, Control, Geometry?>("Icon");

    /// <summary>A chevron after a button's content, as on DropDownButton.</summary>
    public static readonly AttachedProperty<bool> ShowChevronProperty =
        AvaloniaProperty.RegisterAttached<ControlHelper, Control, bool>("ShowChevron");

    /// <summary>An × button at the end of a TextBox that has text; it empties the box.</summary>
    public static readonly AttachedProperty<bool> ClearButtonProperty =
        AvaloniaProperty.RegisterAttached<ControlHelper, Control, bool>("ClearButton");

    public static IBrush? GetHoverBackground(Control c) => c.GetValue(HoverBackgroundProperty);
    public static void SetHoverBackground(Control c, IBrush? value) => c.SetValue(HoverBackgroundProperty, value);
    public static IBrush? GetPressedBackground(Control c) => c.GetValue(PressedBackgroundProperty);
    public static void SetPressedBackground(Control c, IBrush? value) => c.SetValue(PressedBackgroundProperty, value);
    public static IBrush? GetHighlightBrush(Control c) => c.GetValue(HighlightBrushProperty);
    public static void SetHighlightBrush(Control c, IBrush? value) => c.SetValue(HighlightBrushProperty, value);
    public static bool GetHasShadow(Control c) => c.GetValue(HasShadowProperty);
    public static void SetHasShadow(Control c, bool value) => c.SetValue(HasShadowProperty, value);
    public static bool GetHasError(Control c) => c.GetValue(HasErrorProperty);
    public static void SetHasError(Control c, bool value) => c.SetValue(HasErrorProperty, value);
    public static Geometry? GetIcon(Control c) => c.GetValue(IconProperty);
    public static void SetIcon(Control c, Geometry? value) => c.SetValue(IconProperty, value);
    public static bool GetShowChevron(Control c) => c.GetValue(ShowChevronProperty);
    public static void SetShowChevron(Control c, bool value) => c.SetValue(ShowChevronProperty, value);
    public static bool GetClearButton(Control c) => c.GetValue(ClearButtonProperty);
    public static void SetClearButton(Control c, bool value) => c.SetValue(ClearButtonProperty, value);
}
