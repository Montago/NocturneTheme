using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Nocturne.Avalonia.Controls;

/// <summary>How <see cref="SearchComboBox"/> matches typed text against the items.</summary>
public enum SearchFilterMode
{
    /// <summary>Every word of the text (split on white space) appears somewhere in the item, in any order.</summary>
    Contains,

    /// <summary>The item starts with the whole text.</summary>
    StartsWith,
}

/// <summary>
/// Combo box that can be opened and clicked like a <see cref="ComboBox"/>, or typed into to filter
/// and complete the items. The drop-down is virtualized, so lists of thousands of items stay fast.
/// </summary>
[TemplatePart("PART_TextBox", typeof(TextBox))]
[TemplatePart("PART_List", typeof(ListBox))]
[PseudoClasses(":dropdownopen", ":empty", ":nomatches")]
public class SearchComboBox : TemplatedControl
{
    private readonly record struct Entry(object Item, string Text);

    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<SearchComboBox, IEnumerable?>(nameof(ItemsSource));

    public static readonly StyledProperty<object?> SelectedItemProperty =
        AvaloniaProperty.Register<SearchComboBox, object?>(nameof(SelectedItem), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<object?> SelectedValueProperty =
        AvaloniaProperty.Register<SearchComboBox, object?>(nameof(SelectedValue), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string> SelectedValuePathProperty =
        AvaloniaProperty.Register<SearchComboBox, string>(nameof(SelectedValuePath), string.Empty);

    public static readonly StyledProperty<string> DisplayMemberPathProperty =
        AvaloniaProperty.Register<SearchComboBox, string>(nameof(DisplayMemberPath), string.Empty);

    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<SearchComboBox, IDataTemplate?>(nameof(ItemTemplate));

    public static readonly DirectProperty<SearchComboBox, string> TextProperty =
        AvaloniaProperty.RegisterDirect<SearchComboBox, string>(nameof(Text), o => o.Text);

    public static readonly StyledProperty<SearchFilterMode> FilterModeProperty =
        AvaloniaProperty.Register<SearchComboBox, SearchFilterMode>(nameof(FilterMode));

    public static readonly StyledProperty<bool> IsFilteringEnabledProperty =
        AvaloniaProperty.Register<SearchComboBox, bool>(nameof(IsFilteringEnabled), true);

    public static readonly StyledProperty<bool> IsAutoCompleteEnabledProperty =
        AvaloniaProperty.Register<SearchComboBox, bool>(nameof(IsAutoCompleteEnabled), true);

    public static readonly StyledProperty<bool> OpenOnFocusProperty =
        AvaloniaProperty.Register<SearchComboBox, bool>(nameof(OpenOnFocus), true);

    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        AvaloniaProperty.Register<SearchComboBox, bool>(nameof(IsDropDownOpen), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<double> MaxDropDownHeightProperty =
        AvaloniaProperty.Register<SearchComboBox, double>(nameof(MaxDropDownHeight), 260);

    public static readonly StyledProperty<string> NoMatchesTextProperty =
        AvaloniaProperty.Register<SearchComboBox, string>(nameof(NoMatchesText), "No matches");

    public static readonly StyledProperty<string?> PlaceholderTextProperty =
        AvaloniaProperty.Register<SearchComboBox, string?>(nameof(PlaceholderText));

    private TextBox? textBox;
    private ListBox? list;
    private INotifyCollectionChanged? observed;
    private List<Entry> entries = [];
    private List<object> allItems = [];
    private string text = string.Empty;
    private string lastBoxText = string.Empty;

    // Typed text the list is filtered by; null while the box shows the selected item.
    private string? filterText;
    // Item that leaving the box or Tab commits; follows the highlight once the user types or navigates.
    private object? pending;
    // The text the control last put in the box itself. TextBox raises TextChanged later (posted to the
    // dispatcher), so a flag set around the change has gone by then; the handler compares with this instead.
    private string? ownText;
    private bool syncing;

    protected override Type StyleKeyOverride => typeof(SearchComboBox);

    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>The <see cref="SelectedValuePath"/> member of <see cref="SelectedItem"/> (the item itself without a path).</summary>
    public object? SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    public string SelectedValuePath
    {
        get => GetValue(SelectedValuePathProperty);
        set => SetValue(SelectedValuePathProperty, value);
    }

    /// <summary>Member that gives the text shown, matched and completed; <c>ToString()</c> when empty.</summary>
    public string DisplayMemberPath
    {
        get => GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }

    /// <summary>Template for the items in the drop-down; the text box still shows <see cref="DisplayMemberPath"/>.</summary>
    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>The text in the box, including any completion that is still selected.</summary>
    public string Text
    {
        get => text;
        private set => SetAndRaise(TextProperty, ref text, value);
    }

    public SearchFilterMode FilterMode
    {
        get => GetValue(FilterModeProperty);
        set => SetValue(FilterModeProperty, value);
    }

    /// <summary>When false, typing only highlights and completes; the drop-down keeps every item.</summary>
    public bool IsFilteringEnabled
    {
        get => GetValue(IsFilteringEnabledProperty);
        set => SetValue(IsFilteringEnabledProperty, value);
    }

    /// <summary>Fills in the rest of the first item that starts with the typed text, selected so the next key replaces it.</summary>
    public bool IsAutoCompleteEnabled
    {
        get => GetValue(IsAutoCompleteEnabledProperty);
        set => SetValue(IsAutoCompleteEnabledProperty, value);
    }

    public bool OpenOnFocus
    {
        get => GetValue(OpenOnFocusProperty);
        set => SetValue(OpenOnFocusProperty, value);
    }

    public bool IsDropDownOpen
    {
        get => GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    public double MaxDropDownHeight
    {
        get => GetValue(MaxDropDownHeightProperty);
        set => SetValue(MaxDropDownHeightProperty, value);
    }

    /// <summary>Shown in the drop-down when no item matches the typed text.</summary>
    public string NoMatchesText
    {
        get => GetValue(NoMatchesTextProperty);
        set => SetValue(NoMatchesTextProperty, value);
    }

    /// <summary>Hint text shown while the box is empty.</summary>
    public string? PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (textBox != null)
        {
            textBox.TextChanged -= OnTextChanged;
            textBox.RemoveHandler(KeyDownEvent, OnTextKeyDown);
            textBox.GotFocus -= OnTextGotFocus;
            textBox.RemoveHandler(PointerPressedEvent, OnTextPointerPressed);
        }
        list?.RemoveHandler(PointerPressedEvent, OnListPointerPressed);

        textBox = e.NameScope.Find<TextBox>("PART_TextBox");
        list = e.NameScope.Find<ListBox>("PART_List");

        if (textBox != null)
        {
            textBox.TextChanged += OnTextChanged;
            textBox.AddHandler(KeyDownEvent, OnTextKeyDown, RoutingStrategies.Tunnel);
            textBox.GotFocus += OnTextGotFocus;
            textBox.AddHandler(PointerPressedEvent, OnTextPointerPressed, RoutingStrategies.Tunnel, handledEventsToo: true);
        }
        list?.AddHandler(PointerPressedEvent, OnListPointerPressed, RoutingStrategies.Tunnel);
        UpdateDisplayMember();
        ShowSelected();
        UpdatePseudoClasses();
    }

    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);
        // Focus() on the control itself (or a Label target) goes on to the text box.
        if (ReferenceEquals(e.Source, this))
            textBox?.Focus(e.NavigationMethod);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsKeyboardFocusWithinProperty && change.NewValue is false)
        {
            IsDropDownOpen = false;
            CommitText();
        }
        else if (change.Property == IsDropDownOpenProperty)
        {
            OnDropDownChanged(change.GetNewValue<bool>());
        }
        else if (change.Property == ItemsSourceProperty)
        {
            if (observed != null)
                observed.CollectionChanged -= OnCollectionChanged;
            observed = change.NewValue as INotifyCollectionChanged;
            if (observed != null)
                observed.CollectionChanged += OnCollectionChanged;
            RebuildEntries();
        }
        else if (change.Property == DisplayMemberPathProperty)
        {
            UpdateDisplayMember();
            RebuildEntries();
            ShowSelected();
        }
        else if (change.Property == SelectedItemProperty)
        {
            if (!syncing)
            {
                syncing = true;
                SelectedValue = GetMember(change.NewValue, SelectedValuePath);
                syncing = false;
            }
            ShowSelected();
        }
        else if (change.Property == SelectedValueProperty)
        {
            if (!syncing)
                SelectFromValue();
        }
        else if (change.Property == SelectedValuePathProperty && SelectedItem != null)
        {
            syncing = true;
            SelectedValue = GetMember(SelectedItem, SelectedValuePath);
            syncing = false;
        }
        else if (change.Property == TextProperty)
        {
            UpdatePseudoClasses();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        IsDropDownOpen = false;
    }

    // Avalonia's ListBox takes a binding, not a path
    private void UpdateDisplayMember()
    {
        if (list != null)
            list.DisplayMemberBinding = string.IsNullOrEmpty(DisplayMemberPath) ? null : new ReflectionBinding(DisplayMemberPath);
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":dropdownopen", IsDropDownOpen);
        PseudoClasses.Set(":empty", string.IsNullOrEmpty(Text));
        PseudoClasses.Set(":nomatches", list != null && list.ItemCount == 0);
    }

    // The first click into the box selects the text, so typing searches anew instead of adding to the
    // selected item's name; a click in a box that already has focus places the caret as usual.
    private void OnTextPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (textBox != null && !textBox.IsKeyboardFocusWithin && e.GetCurrentPoint(textBox).Properties.IsLeftButtonPressed)
        {
            textBox.Focus(NavigationMethod.Pointer);
            textBox.SelectAll();
            e.Handled = true;
        }
    }

    private void OnTextGotFocus(object? sender, FocusChangedEventArgs e)
    {
        // Tabbing in selects the text so typing replaces it (a first click does the same).
        if (e.NavigationMethod != NavigationMethod.Pointer)
            textBox?.SelectAll();
        if (OpenOnFocus && e.NavigationMethod != NavigationMethod.Unspecified && !IsDropDownOpen)
            IsDropDownOpen = true;
    }

    private void OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        var boxText = textBox!.Text ?? string.Empty;
        var added = boxText.Length > lastBoxText.Length;
        lastBoxText = boxText;
        Text = boxText;
        var own = ownText != null && boxText == ownText;
        ownText = null;
        if (own)
            return;

        filterText = boxText;
        pending = boxText.Length == 0 ? null : FindBest(boxText);

        // Complete only when something was typed or pasted, not when Backspace or Delete removed text.
        if (IsAutoCompleteEnabled && pending != null && added && textBox.CaretIndex == boxText.Length)
        {
            var best = GetText(pending);
            if (best.Length > boxText.Length && best.StartsWith(boxText, StringComparison.CurrentCultureIgnoreCase))
            {
                // Keeps what the user typed; the item's own casing appears once it is committed.
                SetBoxText(boxText + best[boxText.Length..]);
                textBox.SelectionStart = boxText.Length;
                textBox.SelectionEnd = best.Length;
            }
        }

        if (IsDropDownOpen)
            Refresh();
        else if (IsKeyboardFocusWithin)
            IsDropDownOpen = true;
    }

    private void OnTextKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Down when e.KeyModifiers == KeyModifiers.Alt:
            case Key.Up when e.KeyModifiers == KeyModifiers.Alt:
            case Key.F4:
                IsDropDownOpen = !IsDropDownOpen;
                e.Handled = true;
                break;
            case Key.Down:
                Move(1, e);
                break;
            case Key.Up:
                Move(-1, e);
                break;
            case Key.PageDown:
                Move(PageSize(), e);
                break;
            case Key.PageUp:
                Move(-PageSize(), e);
                break;
            case Key.Enter:
                if (IsDropDownOpen)
                {
                    if (list?.SelectedItem is { } chosen)
                    {
                        Commit(chosen, closeAfter: true);
                    }
                    else
                    {
                        IsDropDownOpen = false;
                        CommitText();
                    }
                    textBox?.SelectAll();
                    e.Handled = true;
                }
                else
                {
                    CommitText();
                }
                break;
            case Key.Tab:
                // Not handled, so focus still moves on to the next control.
                if (IsDropDownOpen && list?.SelectedItem is { } highlighted)
                    Commit(highlighted, closeAfter: true);
                break;
            case Key.Escape:
                if (IsDropDownOpen || filterText != null || pending != null)
                {
                    IsDropDownOpen = false;
                    ShowSelected();
                    textBox?.SelectAll();
                    e.Handled = true;
                }
                break;
        }
    }

    private void Move(int delta, KeyEventArgs e)
    {
        e.Handled = true;
        if (!IsDropDownOpen)
        {
            IsDropDownOpen = true;
            return;
        }
        if (list == null || list.ItemCount == 0)
            return;
        var index = list.SelectedIndex < 0 && delta > 0 ? 0 : Math.Clamp(list.SelectedIndex + delta, 0, list.ItemCount - 1);
        list.SelectedIndex = index;
        if (list.SelectedItem is { } item)
            list.ScrollIntoView(item);
        pending = list.SelectedItem;
        SetBoxText(GetText(pending));
        textBox?.SelectAll();
    }

    private int PageSize()
    {
        var item = list?.ContainerFromIndex(Math.Max(0, list.SelectedIndex));
        return item is { Bounds.Height: > 0 } ? Math.Max(1, (int)(MaxDropDownHeight / item.Bounds.Height) - 1) : 10;
    }

    private void OnListPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Picks the item on press and keeps keyboard focus in the text box; scroll bar clicks pass through.
        if ((e.Source as Visual)?.FindAncestorOfType<ListBoxItem>(includeSelf: true) is { } container && list != null)
        {
            Commit(list.ItemFromContainer(container), closeAfter: true);
            textBox?.Focus(NavigationMethod.Pointer);
            if (textBox != null)
                textBox.CaretIndex = textBox.Text?.Length ?? 0;
            e.Handled = true;
        }
    }

    private void OnDropDownChanged(bool open)
    {
        UpdatePseudoClasses();
        if (!open)
            return;
        Refresh();
        if (!IsKeyboardFocusWithin && IsAttachedToVisualTree())
            textBox?.Focus();
        // The list only has containers once the popup has been laid out.
        Dispatcher.UIThread.Post(() =>
        {
            if (list?.SelectedItem is { } item)
                list.ScrollIntoView(item);
        }, DispatcherPriority.Loaded);
    }

    private bool IsAttachedToVisualTree() => TopLevel.GetTopLevel(this) != null;

    /// <summary>Fills the drop-down from the current filter and highlights the item that would be committed.</summary>
    private void Refresh()
    {
        if (list == null)
            return;
        var showAll = string.IsNullOrEmpty(filterText) || !IsFilteringEnabled;
        var items = showAll ? allItems : Filter(filterText!).Select(x => x.Item).ToList();
        if (!ReferenceEquals(list.ItemsSource, items))
            list.ItemsSource = items;
        var highlight = pending ?? (filterText == null ? SelectedItem : null);
        list.SelectedItem = highlight;
        if (highlight != null)
            list.ScrollIntoView(highlight);
        else if (items.Count > 0)
            list.ScrollIntoView(items[0]);
        UpdatePseudoClasses();
    }

    private IEnumerable<Entry> Filter(string value)
    {
        if (FilterMode == SearchFilterMode.StartsWith)
            return entries.Where(x => x.Text.StartsWith(value, StringComparison.CurrentCultureIgnoreCase));
        var words = value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return entries.Where(x => words.All(w => x.Text.Contains(w, StringComparison.CurrentCultureIgnoreCase)));
    }

    // The first item starting with the whole text, else (when filtering by Contains) the first one containing
    // every word. Only the first kind is completed: OnTextChanged checks that the text is a prefix of it.
    private object? FindBest(string value)
    {
        foreach (var x in entries)
        {
            if (x.Text.StartsWith(value, StringComparison.CurrentCultureIgnoreCase))
                return x.Item;
        }
        return FilterMode == SearchFilterMode.Contains ? Filter(value).Select(x => x.Item).FirstOrDefault() : null;
    }

    /// <summary>Decides what the typed text means when the user leaves the box or presses Enter.</summary>
    private void CommitText()
    {
        if (textBox == null || (filterText == null && pending == null))
            return;
        var boxText = textBox.Text ?? string.Empty;
        if (boxText.Length == 0)
        {
            Commit(null, closeAfter: false);
        }
        else if (pending != null)
        {
            Commit(pending, closeAfter: false);
        }
        else
        {
            var exact = entries.Where(x => string.Equals(x.Text, boxText, StringComparison.CurrentCultureIgnoreCase)).Select(x => x.Item).FirstOrDefault();
            // Text that matches nothing snaps back to the selected item.
            Commit(exact ?? SelectedItem, closeAfter: false);
        }
    }

    private void Commit(object? item, bool closeAfter)
    {
        if (closeAfter)
            IsDropDownOpen = false;
        SelectedItem = item;
        ShowSelected();
    }

    private void ShowSelected()
    {
        filterText = null;
        pending = null;
        SetBoxText(GetText(SelectedItem));
    }

    private void SetBoxText(string value)
    {
        if (textBox == null || textBox.Text == value)
            return;
        ownText = value;
        textBox.Text = value;
        textBox.CaretIndex = value.Length;
    }

    private string GetText(object? item) =>
        item == null ? string.Empty : Convert.ToString(GetMember(item, DisplayMemberPath), CultureInfo.CurrentCulture) ?? string.Empty;

    private static object? GetMember(object? item, string? path)
    {
        if (string.IsNullOrEmpty(path))
            return item;
        foreach (var name in path.Split('.'))
        {
            if (item == null)
                return null;
            item = item.GetType().GetProperty(name)?.GetValue(item);
        }
        return item;
    }

    private void RebuildEntries()
    {
        allItems = ItemsSource?.Cast<object>().Where(i => i != null).ToList() ?? [];
        entries = allItems.Select(i => new Entry(i, GetText(i))).ToList();
        if (SelectedItem == null && SelectedValue != null)
            SelectFromValue();
        if (IsDropDownOpen)
            Refresh();
        else if (list != null)
            list.ItemsSource = null;
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => RebuildEntries();

    // Leaves SelectedValue alone when no item has it yet; it is looked up again when the items arrive.
    private void SelectFromValue()
    {
        var value = SelectedValue;
        var item = value == null ? null : allItems.FirstOrDefault(i => Equals(GetMember(i, SelectedValuePath), value));
        syncing = true;
        SelectedItem = item;
        syncing = false;
    }
}
