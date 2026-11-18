using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Nocturne.Wpf.Controls;

/// <summary>
/// A time field: type a time (09:30, 9.30 or 9) or pick one from a drop-down list in <see cref="MinuteStep"/>
/// steps. As in a ComboBox, Alt+Down or F4 opens the list; while it is open the arrow keys move through it,
/// Enter takes the time and Escape closes it.
/// </summary>
[TemplatePart(Name = "PART_TextBox", Type = typeof(TextBox))]
[TemplatePart(Name = "PART_Popup", Type = typeof(Popup))]
[TemplatePart(Name = "PART_List", Type = typeof(ListBox))]
public class TimePicker : Control
{
    private TextBox? textBox;
    private ListBox? list;
    private bool updating;

    static TimePicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TimePicker), new FrameworkPropertyMetadata(typeof(TimePicker)));
    }

    public static readonly DependencyProperty SelectedTimeProperty = DependencyProperty.Register(
        nameof(SelectedTime), typeof(TimeSpan?), typeof(TimePicker),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (d, _) => ((TimePicker)d).UpdateText()));

    public TimeSpan? SelectedTime
    {
        get => (TimeSpan?)GetValue(SelectedTimeProperty);
        set => SetValue(SelectedTimeProperty, value);
    }

    public static readonly DependencyProperty MinuteStepProperty = DependencyProperty.Register(
        nameof(MinuteStep), typeof(int), typeof(TimePicker), new PropertyMetadata(15, (d, _) => ((TimePicker)d).FillList()));

    public int MinuteStep
    {
        get => (int)GetValue(MinuteStepProperty);
        set => SetValue(MinuteStepProperty, value);
    }

    public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register(
        nameof(IsDropDownOpen), typeof(bool), typeof(TimePicker), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnDropDownChanged));

    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        textBox = GetTemplateChild("PART_TextBox") as TextBox;
        list = GetTemplateChild("PART_List") as ListBox;
        if (textBox != null)
        {
            textBox.LostKeyboardFocus += (_, _) => Commit();
            textBox.KeyDown += (_, e) => { if (e.Key == Key.Enter) Commit(); };
            textBox.PreviewKeyDown += OnTextPreviewKeyDown;
        }
        if (list != null)
        {
            list.SelectionChanged += (_, _) =>
            {
                if (!updating && list.SelectedItem is TimeSpan t)
                {
                    var hadFocus = list.IsKeyboardFocusWithin;
                    SelectedTime = t;
                    IsDropDownOpen = false;
                    if (hadFocus)
                    {
                        textBox?.Focus();
                    }
                }
            };
        }
        FillList();
        UpdateText();
    }

    private void OnTextPreviewKeyDown(object sender, KeyEventArgs e)
    {
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (key == Key.F4 || (key is Key.Down or Key.Up && Keyboard.Modifiers.HasFlag(ModifierKeys.Alt)))
        {
            if (!IsDropDownOpen)
            {
                Commit();
            }
            IsDropDownOpen = !IsDropDownOpen;
            e.Handled = true;
            return;
        }
        if (!IsDropDownOpen || list == null || list.Items.Count == 0)
        {
            return;
        }

        switch (key)
        {
            case Key.Down:
            case Key.Up:
            case Key.PageDown:
            case Key.PageUp:
                // Moves the highlight only; the time is taken with Enter or a click.
                var step = key switch { Key.Down => 1, Key.Up => -1, Key.PageDown => 8, _ => -8 };
                var index = list.SelectedIndex < 0 ? (step > 0 ? 0 : list.Items.Count - 1) : list.SelectedIndex + step;
                updating = true;
                list.SelectedIndex = Math.Clamp(index, 0, list.Items.Count - 1);
                updating = false;
                list.ScrollIntoView(list.SelectedItem);
                e.Handled = true;
                break;
            case Key.Enter:
                if (list.SelectedItem is TimeSpan t)
                {
                    SelectedTime = t;
                    UpdateText();
                    textBox?.SelectAll();
                }
                IsDropDownOpen = false;
                e.Handled = true;
                break;
            case Key.Escape:
                IsDropDownOpen = false;
                e.Handled = true;
                break;
        }
    }

    private static void OnDropDownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var p = (TimePicker)d;
        if ((bool)e.NewValue && p.list != null)
        {
            p.updating = true;
            p.list.SelectedItem = p.list.Items.Cast<TimeSpan>().LastOrDefault(t => p.SelectedTime is { } s && t <= s);
            p.updating = false;
            if (p.list.SelectedItem != null)
            {
                p.Dispatcher.BeginInvoke(() => p.list.ScrollIntoView(p.list.SelectedItem));
            }
        }
    }

    private void FillList()
    {
        if (list == null)
        {
            return;
        }
        var step = Math.Max(1, MinuteStep);
        list.Items.Clear();
        for (var m = 0; m < 24 * 60; m += step)
        {
            list.Items.Add(TimeSpan.FromMinutes(m));
        }
    }

    private void Commit()
    {
        if (textBox == null)
        {
            return;
        }
        var text = textBox.Text.Trim().Replace('.', ':');
        if (text.Length == 0)
        {
            SelectedTime = null;
        }
        else if (TimeSpan.TryParse(text, CultureInfo.CurrentCulture, out var t) && t < TimeSpan.FromDays(1))
        {
            SelectedTime = t;
        }
        else if (int.TryParse(text, out var hours) && hours is >= 0 and < 24)
        {
            SelectedTime = TimeSpan.FromHours(hours);
        }
        UpdateText();
    }

    private void UpdateText()
    {
        if (textBox != null)
        {
            textBox.Text = SelectedTime?.ToString(@"hh\:mm") ?? string.Empty;
        }
    }
}

/// <summary>
/// A colour field that opens a grid of swatches (<see cref="Palette"/>; <see cref="DefaultPalette"/> by default).
/// A click takes a colour. From the keyboard, Space, Alt+Down or F4 opens the grid with the current colour
/// focused; the arrow keys move, Enter, Space or Tab takes the colour and Escape closes without a change.
/// </summary>
[TemplatePart(Name = "PART_Palette", Type = typeof(ListBox))]
public class ColorPicker : Control
{
    private ListBox? palette;

    public static readonly IReadOnlyList<Color> DefaultPalette = new[]
    {
        "#1F9D57", "#3FD27F", "#8EF0B8", "#0E7A6A", "#1FB5A8", "#7DE3D8", "#1F78D1", "#5AA8F0",
        "#6B5BD6", "#A497F5", "#B04BC4", "#E58CF2", "#D13A3A", "#FF8F85", "#D9661A", "#F5A15E",
        "#D1A112", "#FFD27A", "#6B7A70", "#A9B5AD", "#E6ECE8", "#FFFFFF", "#2A3A31", "#101E16",
    }.Select(s => (Color)ColorConverter.ConvertFromString(s)).ToArray();

    static ColorPicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorPicker), new FrameworkPropertyMetadata(typeof(ColorPicker)));
    }

    public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register(
        nameof(SelectedColor), typeof(Color), typeof(ColorPicker),
        new FrameworkPropertyMetadata(Color.FromRgb(0x1F, 0x78, 0xD1), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (d, _) => ((ColorPicker)d).Sync()));

    public Color SelectedColor
    {
        get => (Color)GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    public static readonly DependencyProperty PaletteProperty = DependencyProperty.Register(
        nameof(Palette), typeof(IEnumerable), typeof(ColorPicker), new PropertyMetadata(DefaultPalette));

    public IEnumerable Palette
    {
        get => (IEnumerable)GetValue(PaletteProperty);
        set => SetValue(PaletteProperty, value);
    }

    public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register(
        nameof(IsDropDownOpen), typeof(bool), typeof(ColorPicker),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (d, e) => ((ColorPicker)d).OnDropDownChanged((bool)e.NewValue)));

    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        palette = GetTemplateChild("PART_Palette") as ListBox;
        if (palette != null)
        {
            palette.SelectionChanged += (_, _) =>
            {
                // The arrow keys only move the highlight; a click takes the colour.
                if (InputManager.Current.MostRecentInputDevice is KeyboardDevice)
                {
                    return;
                }
                if (palette.SelectedItem is Color c && c != SelectedColor)
                {
                    Choose(c);
                }
            };
            palette.PreviewKeyDown += OnPalettePreviewKeyDown;
        }
        Sync();
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (!IsDropDownOpen && (key is Key.F4 or Key.Space || (key is Key.Down or Key.Up && Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))))
        {
            IsDropDownOpen = true;
            e.Handled = true;
        }
    }

    private void OnPalettePreviewKeyDown(object sender, KeyEventArgs e)
    {
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        switch (key)
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
            case Key.Up or Key.Down when Keyboard.Modifiers.HasFlag(ModifierKeys.Alt):
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
            Dispatcher.BeginInvoke(() =>
            {
                if (!IsDropDownOpen || palette.Items.Count == 0)
                {
                    return;
                }
                var item = palette.SelectedItem ?? palette.Items[0];
                (palette.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem)?.Focus();
            }, System.Windows.Threading.DispatcherPriority.Loaded);
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

/// <summary>A text box that suggests matching entries from <see cref="ItemsSource"/> in a drop-down while you type.</summary>
[TemplatePart(Name = "PART_TextBox", Type = typeof(TextBox))]
[TemplatePart(Name = "PART_Popup", Type = typeof(Popup))]
[TemplatePart(Name = "PART_List", Type = typeof(ListBox))]
public class AutoSuggestBox : Control
{
    private TextBox? textBox;
    private Popup? popup;
    private ListBox? list;
    private bool choosing;

    static AutoSuggestBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoSuggestBox), new FrameworkPropertyMetadata(typeof(AutoSuggestBox)));
    }

    public static readonly RoutedEvent SuggestionChosenEvent = EventManager.RegisterRoutedEvent(
        nameof(SuggestionChosen), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(AutoSuggestBox));

    public event RoutedEventHandler SuggestionChosen
    {
        add => AddHandler(SuggestionChosenEvent, value);
        remove => RemoveHandler(SuggestionChosenEvent, value);
    }

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text), typeof(string), typeof(AutoSuggestBox), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource), typeof(IEnumerable), typeof(AutoSuggestBox), new PropertyMetadata(null));

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty MaxSuggestionsProperty = DependencyProperty.Register(
        nameof(MaxSuggestions), typeof(int), typeof(AutoSuggestBox), new PropertyMetadata(8));

    public int MaxSuggestions
    {
        get => (int)GetValue(MaxSuggestionsProperty);
        set => SetValue(MaxSuggestionsProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        textBox = GetTemplateChild("PART_TextBox") as TextBox;
        popup = GetTemplateChild("PART_Popup") as Popup;
        list = GetTemplateChild("PART_List") as ListBox;

        if (textBox != null)
        {
            textBox.TextChanged += (_, _) => { if (!choosing) Suggest(); };
            textBox.PreviewKeyDown += OnTextKeyDown;
            textBox.LostKeyboardFocus += (_, e) =>
            {
                if (popup != null && list != null && !list.IsKeyboardFocusWithin)
                {
                    popup.IsOpen = false;
                }
            };
        }
        if (list != null)
        {
            list.PreviewMouseLeftButtonUp += (_, _) => Choose();
            list.KeyDown += (_, e) =>
            {
                if (e.Key == Key.Enter) { Choose(); e.Handled = true; }
                if (e.Key == Key.Escape && popup != null) { popup.IsOpen = false; textBox?.Focus(); }
            };
        }
    }

    private void OnTextKeyDown(object sender, KeyEventArgs e)
    {
        if (popup is not { IsOpen: true } || list == null)
        {
            return;
        }
        switch (e.Key)
        {
            case Key.Down:
                list.SelectedIndex = Math.Min(list.Items.Count - 1, list.SelectedIndex + 1);
                e.Handled = true;
                break;
            case Key.Up:
                list.SelectedIndex = Math.Max(0, list.SelectedIndex - 1);
                e.Handled = true;
                break;
            case Key.Enter:
                Choose();
                e.Handled = true;
                break;
            case Key.Escape:
                popup.IsOpen = false;
                e.Handled = true;
                break;
        }
    }

    private void Suggest()
    {
        if (popup == null || list == null || ItemsSource == null)
        {
            return;
        }
        var text = textBox?.Text ?? string.Empty;
        var matches = text.Length == 0
            ? new List<object>()
            : ItemsSource.Cast<object>()
                .Where(i => i?.ToString()?.Contains(text, StringComparison.CurrentCultureIgnoreCase) == true)
                .Take(MaxSuggestions)
                .ToList();
        list.ItemsSource = matches;
        list.SelectedIndex = matches.Count > 0 ? 0 : -1;
        popup.IsOpen = matches.Count > 0 && IsKeyboardFocusWithin;
    }

    private void Choose()
    {
        if (list?.SelectedItem is not { } item || textBox == null)
        {
            return;
        }
        choosing = true;
        Text = item.ToString() ?? string.Empty;
        textBox.CaretIndex = textBox.Text.Length;
        choosing = false;
        if (popup != null)
        {
            popup.IsOpen = false;
        }
        textBox.Focus();
        RaiseEvent(new RoutedEventArgs(SuggestionChosenEvent, item));
    }
}
