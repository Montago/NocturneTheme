using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Frosted.Wpf.Controls;

/// <summary>A button that opens <see cref="Menu"/> under itself; a chevron follows its content.</summary>
public class DropDownButton : Button
{
    static DropDownButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DropDownButton), new FrameworkPropertyMetadata(typeof(DropDownButton)));
    }

    public static readonly DependencyProperty MenuProperty = DependencyProperty.Register(
        nameof(Menu), typeof(ContextMenu), typeof(DropDownButton), new PropertyMetadata(null));

    /// <summary>The menu that opens when the button is clicked.</summary>
    public ContextMenu? Menu
    {
        get => (ContextMenu?)GetValue(MenuProperty);
        set => SetValue(MenuProperty, value);
    }

    protected override void OnClick()
    {
        base.OnClick();
        MenuButtonHelper.Open(this, Menu);
    }
}

/// <summary>
/// A button in two parts: the main part raises <c>Click</c>, the chevron part (PART_Arrow) opens
/// <see cref="Menu"/> with the other choices.
/// </summary>
[TemplatePart(Name = PartArrow, Type = typeof(ToggleButton))]
public class SplitButton : Button
{
    private const string PartArrow = "PART_Arrow";
    private ToggleButton? arrow;

    static SplitButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SplitButton), new FrameworkPropertyMetadata(typeof(SplitButton)));
    }

    public static readonly DependencyProperty MenuProperty = DependencyProperty.Register(
        nameof(Menu), typeof(ContextMenu), typeof(SplitButton), new PropertyMetadata(null));

    /// <summary>The menu the chevron part opens.</summary>
    public ContextMenu? Menu
    {
        get => (ContextMenu?)GetValue(MenuProperty);
        set => SetValue(MenuProperty, value);
    }

    public override void OnApplyTemplate()
    {
        if (arrow != null)
        {
            arrow.Checked -= OnArrowChecked;
        }
        base.OnApplyTemplate();
        arrow = GetTemplateChild(PartArrow) as ToggleButton;
        if (arrow != null)
        {
            arrow.Checked += OnArrowChecked;
        }
    }

    private void OnArrowChecked(object sender, RoutedEventArgs e)
    {
        if (Menu == null)
        {
            arrow!.IsChecked = false;
            return;
        }
        Menu.Closed -= OnMenuClosed;
        Menu.Closed += OnMenuClosed;
        MenuButtonHelper.Open(this, Menu);
    }

    private void OnMenuClosed(object sender, RoutedEventArgs e)
    {
        if (arrow != null)
        {
            arrow.IsChecked = false;
        }
    }
}

internal static class MenuButtonHelper
{
    /// <summary>The popup surface keeps 14px on each side for the shadow.</summary>
    private const double ShadowMargin = 14;

    public static void Open(FrameworkElement owner, ContextMenu? menu)
    {
        if (menu == null)
        {
            return;
        }
        menu.PlacementTarget = owner;
        menu.Placement = PlacementMode.Bottom;
        menu.HorizontalOffset = -ShadowMargin;
        menu.VerticalOffset = 2;
        menu.MinWidth = Math.Max(menu.MinWidth, owner.ActualWidth + 2 * ShadowMargin);
        menu.IsOpen = true;
    }
}
