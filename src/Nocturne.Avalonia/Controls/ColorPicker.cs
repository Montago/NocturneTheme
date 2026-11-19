using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Nocturne.Avalonia.Controls;

/// <summary>
/// A colour field that opens a grid of swatches (<see cref="Palette"/>; <see cref="DefaultPalette"/> by default).
/// A click takes a colour. From the keyboard, Space, Alt+Down or F4 opens the grid with the current colour
/// focused; the arrow keys move, Enter, Space or Tab takes the colour and Escape closes without a change.
/// Avalonia's own ColorPicker (spectrum, sliders, hex entry) lives in a separate package; this is the
/// theme's small swatch picker, the same as in Nocturne.Wpf.
/// </summary>
[TemplatePart("PART_Palette", typeof(ListBox))]
[PseudoClasses(":dropdownopen")]
public class ColorPicker : TemplatedControl
{
    private ListBox? palette;

    public static readonly IReadOnlyList<Color> DefaultPalette = new[]
    {
        "#1F9D57", "#3FD27F", "#8EF0B8", "#0E7A6A", "#1FB5A8", "#7DE3D8", "#1F78D1", "#5AA8F0",
        "#6B5BD6", "#A497F5", "#B04BC4", "#E58CF2", "#D13A3A", "#FF8F85", "#D9661A", "#F5A15E",
        "#D1A112", "#FFD27A", "#6B7A70", "#A9B5AD", "#E6ECE8", "#FFFFFF", "#2A3A31", "#101E16",
    }.Select(Color.Parse).ToArray();

    public static readonly StyledProperty<Color> SelectedColorProperty =
        AvaloniaProperty.Register<ColorPicker, Color>(nameof(SelectedColor), Color.FromRgb(0x1F, 0x78, 0xD1), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<IEnumerable> PaletteProperty =
        AvaloniaProperty.Register<ColorPicker, IEnumerable>(nameof(Palette), DefaultPalette);

    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        AvaloniaProperty.Register<ColorPicker, bool>(nameof(IsDropDownOpen), defaultBindingMode: BindingMode.TwoWay);

    static ColorPicker()
    {
        FocusableProperty.OverrideDefaultValue<ColorPicker>(true);
    }

    public Color SelectedColor
    {
        get => GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    public IEnumerable Palette
    {
        get => GetValue(PaletteProperty);
        set => SetValue(PaletteProperty, value);
    }

    public bool IsDropDownOpen
    {
        get => GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (palette != null)
        {
            palette.RemoveHandler(InputElement.TappedEvent, OnPaletteTapped);
            palette.RemoveHandler(KeyDownEvent, OnPaletteKeyDown);
        }
        base.OnApplyTemplate(e);
        palette = e.NameScope.Find<ListBox>("PART_Palette");
        if (palette != null)
        {
            // The arrow keys only move the highlight; a click (tap) takes the colour.
            palette.AddHandler(InputElement.TappedEvent, OnPaletteTapped);
            palette.AddHandler(KeyDownEvent, OnPaletteKeyDown, RoutingStrategies.Tunnel);
        }
        Sync();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SelectedColorProperty || change.Property == PaletteProperty)
        {
            Sync();
        }
        else if (change.Property == IsDropDownOpenProperty)
        {
            PseudoClasses.Set(":dropdownopen", IsDropDownOpen);
            OnDropDownChanged(IsDropDownOpen);
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Handled || IsDropDownOpen)
        {
            return;
        }
        var alt = e.KeyModifiers.HasFlag(KeyModifiers.Alt);
        if (e.Key is Key.F4 or Key.Space || (e.Key is Key.Down or Key.Up && alt))
        {
            IsDropDownOpen = true;
            e.Handled = true;
        }
    }

    private void OnPaletteTapped(object? sender, TappedEventArgs e)
    {
        var item = (e.Source as Visual)?.FindAncestorOfType<ListBoxItem>(includeSelf: true);
        if (item?.DataContext is Color c)
        {
            Choose(c);
            e.Handled = true;
        }
    }

    private void OnPaletteKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
            case Key.Space:
            case Key.Tab:
                if (palette?.SelectedItem is Color c)
                {
                    Choose(c);
                }
                else
                {
                    IsDropDownOpen = false;
                }
                e.Handled = true;
                break;
            case Key.Escape:
            case Key.F4:
                IsDropDownOpen = false;
                e.Handled = true;
                break;
            case Key.Up or Key.Down when e.KeyModifiers.HasFlag(KeyModifiers.Alt):
                IsDropDownOpen = false;
                e.Handled = true;
                break;
        }
    }

    private void Choose(Color color)
    {
        SelectedColor = color;
        IsDropDownOpen = false;
    }

    private void OnDropDownChanged(bool open)
    {
        if (palette == null)
        {
            return;
        }
        if (open)
        {
            // Focus the current colour (or the first) once the popup is shown, so the arrow keys work.
            Dispatcher.UIThread.Post(() =>
            {
                if (!IsDropDownOpen || palette.ItemCount == 0)
                {
                    return;
                }
                var index = palette.SelectedIndex >= 0 ? palette.SelectedIndex : 0;
                palette.ContainerFromIndex(index)?.Focus(NavigationMethod.Directional);
            }, DispatcherPriority.Loaded);
        }
        else
        {
            var hadFocus = palette.IsKeyboardFocusWithin;
            Sync();
            if (hadFocus)
            {
                Focus();
            }
        }
    }

    private void Sync()
    {
        if (palette != null)
        {
            palette.SelectedItem = Palette.Cast<object>().OfType<Color>().Contains(SelectedColor) ? SelectedColor : null;
        }
    }
}
