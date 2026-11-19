using System.Collections.Specialized;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Threading;

namespace Nocturne.Avalonia.Controls;

/// <summary>
/// Navigation pane on the left (a list of <see cref="HamburgerMenuItem"/>) and a content area on
/// the right. The selected item gets a light fill and a short accent bar.
/// </summary>
/// <remarks>
/// Two levels: an item with <see cref="HamburgerMenuItem.Items"/> is a group that opens and closes
/// (it is never selected itself). The menu keeps a group's sub-items right after it in its own
/// <see cref="ItemsControl.Items"/>, so <c>SelectedItem</c> is the selected page on either level.
/// </remarks>
[PseudoClasses(":compact")]
public class HamburgerMenu : ListBox
{
    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<HamburgerMenu, object?>(nameof(Content));

    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        AvaloniaProperty.Register<HamburgerMenu, IDataTemplate?>(nameof(ContentTemplate));

    public static readonly StyledProperty<bool> IsPaneOpenProperty =
        AvaloniaProperty.Register<HamburgerMenu, bool>(nameof(IsPaneOpen), true, defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<double> PaneWidthProperty =
        AvaloniaProperty.Register<HamburgerMenu, double>(nameof(PaneWidth), 170);

    public static readonly StyledProperty<double> CompactPaneWidthProperty =
        AvaloniaProperty.Register<HamburgerMenu, double>(nameof(CompactPaneWidth), 40);

    private bool syncing;
    private bool syncQueued;

    public HamburgerMenu()
    {
        Items.CollectionChanged += OnItemsChanged;
        SelectionChanged += OnOwnSelectionChanged;
    }

    protected override Type StyleKeyOverride => typeof(HamburgerMenu);

    /// <summary>What the content area shows; usually bound to the selected item.</summary>
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public IDataTemplate? ContentTemplate
    {
        get => GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }

    /// <summary>False shows only the icons; sub-items are hidden then, and their group shows the selection.</summary>
    public bool IsPaneOpen
    {
        get => GetValue(IsPaneOpenProperty);
        set => SetValue(IsPaneOpenProperty, value);
    }

    public double PaneWidth
    {
        get => GetValue(PaneWidthProperty);
        set => SetValue(PaneWidthProperty, value);
    }

    public double CompactPaneWidth
    {
        get => GetValue(CompactPaneWidthProperty);
        set => SetValue(CompactPaneWidthProperty, value);
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey) =>
        NeedsContainer<HamburgerMenuItem>(item, out recycleKey);

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) => new HamburgerMenuItem();

    internal IEnumerable<HamburgerMenuItem> MenuItems => Items.OfType<HamburgerMenuItem>();

    private bool HasGroups => MenuItems.Any(i => i.HasItems && i.Group is null);

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsPaneOpenProperty)
        {
            PseudoClasses.Set(":compact", !IsPaneOpen);
            foreach (var item in MenuItems)
                item.UpdateState();
        }
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        PseudoClasses.Set(":compact", !IsPaneOpen);
        SyncSubItems();
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Items cannot change while they announce a change; groups added or removed later get their
        // sub-items put in or taken out right after.
        if (IsInitialized && !syncing && !syncQueued)
        {
            syncQueued = true;
            Dispatcher.UIThread.Post(() =>
            {
                syncQueued = false;
                SyncSubItems();
            }, DispatcherPriority.Send);
        }
    }

    /// <summary>Puts each group's sub-items right after it in <see cref="ItemsControl.Items"/>.</summary>
    internal void SyncSubItems()
    {
        if (ItemsSource is not null || !IsInitialized)
            return;
        syncing = true;
        try
        {
            // Sub-items whose group has gone, or that left their group
            for (var i = Items.Count - 1; i >= 0; i--)
            {
                if (Items[i] is HamburgerMenuItem { Group: { } g } sub && (!Items.Contains(g) || !g.Items.Contains(sub)))
                {
                    sub.Group = null;
                    Items.RemoveAt(i);
                }
            }

            var index = 0;
            while (index < Items.Count)
            {
                var at = index + 1;
                if (Items[index] is HamburgerMenuItem { Group: null } group)
                {
                    foreach (var sub in group.Items)
                    {
                        var current = Items.IndexOf(sub);
                        if (current != at)
                        {
                            if (current >= 0)
                            {
                                Items.RemoveAt(current);
                                if (current < at)
                                    at--;
                            }
                            Items.Insert(at, sub);
                        }
                        sub.Group = group;
                        at++;
                    }
                }
                index = at;
            }
        }
        finally
        {
            syncing = false;
        }
        foreach (var item in MenuItems)
            item.Owner = this;

        // A sub-item marked IsSelected in XAML was not part of the menu when the selection was made
        if (MenuItems.FirstOrDefault(i => i.Group is not null && i.IsSelected) is { } marked && !ReferenceEquals(SelectedItem, marked))
            SelectedItem = marked;
        else if (SelectedItem is HamburgerMenuItem { HasItems: true, Group: null } group)
            SelectedItem = group.Items[0];
        UpdateGroups();
    }

    private void OnOwnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!ReferenceEquals(e.Source, this))
            return;
        // A group is not a page: SelectedIndex or IsSelected on one selects its first sub-item
        if (SelectedItem is HamburgerMenuItem { HasItems: true, Group: null } group && group.Items[0].Group == group)
        {
            SelectedItem = group.Items[0];
            return;
        }
        if (SelectedItem is HamburgerMenuItem { Group: { } parent })
            parent.IsExpanded = true;
        UpdateGroups();
    }

    private void UpdateGroups()
    {
        foreach (var item in MenuItems)
        {
            item.IsChildSelected = item.Group is null && SelectedItem is HamburgerMenuItem selected && item.Items.Contains(selected);
            item.UpdateState();
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        // With groups the arrow keys move over the visible items and select pages, but not groups
        if (HasGroups && e.KeyModifiers == KeyModifiers.None && e.Key is Key.Up or Key.Down or Key.Home or Key.End)
        {
            var visible = MenuItems.Where(i => i.IsVisible && i.IsEnabled).ToList();
            if (visible.Count > 0)
            {
                var focused = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() as HamburgerMenuItem;
                var current = focused != null ? visible.IndexOf(focused) : -1;
                var next = e.Key switch
                {
                    Key.Home => 0,
                    Key.End => visible.Count - 1,
                    Key.Up => Math.Max(0, current - 1),
                    _ => current < 0 ? 0 : Math.Min(visible.Count - 1, current + 1),
                };
                var target = visible[next];
                target.Focus(NavigationMethod.Directional);
                target.BringIntoView();
                if (!target.IsGroup)
                    SelectedItem = target;
            }
            e.Handled = true;
            return;
        }
        base.OnKeyDown(e);
    }
}

/// <summary>A row of a <see cref="HamburgerMenu"/>: an icon, a label and, on a group, its sub-items.</summary>
[PseudoClasses(":group", ":subitem", ":expanded", ":showschild")]
public class HamburgerMenuItem : ListBoxItem
{
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<HamburgerMenuItem, object?>(nameof(Icon));

    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<HamburgerMenuItem, bool>(nameof(IsExpanded), defaultBindingMode: BindingMode.TwoWay);

    private HamburgerMenuItem? group;

    public HamburgerMenuItem()
    {
        Items.CollectionChanged += OnSubItemsChanged;
    }

    protected override Type StyleKeyOverride => typeof(HamburgerMenuItem);

    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Sub-items: they make this item a group that opens and closes. Sub-items show no icon, and
    /// their own sub-items are ignored (two levels).
    /// </summary>
    public AvaloniaList<HamburgerMenuItem> Items { get; } = [];

    /// <summary>True when the item has sub-items (a group).</summary>
    public bool HasItems => Items.Count > 0;

    /// <summary>Whether a group shows its sub-items. Selecting a sub-item opens its group.</summary>
    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <summary>True for the sub-items of a group (the second level).</summary>
    public bool IsSubItem => group is not null;

    /// <summary>True for a group when one of its sub-items is the selected page.</summary>
    public bool IsChildSelected { get; internal set; }

    /// <summary>
    /// True for a group that stands in for its selected sub-item: the group is closed, or the pane
    /// shows only icons. The template then gives the group the selected look.
    /// </summary>
    public bool ShowsChildSelection => IsChildSelected && (!IsExpanded || Menu is { IsPaneOpen: false });

    /// <summary>The group this item is a sub-item of, while both are in a menu.</summary>
    internal HamburgerMenuItem? Group
    {
        get => group;
        set
        {
            group = value;
            UpdateState();
        }
    }

    internal bool IsGroup => HasItems && group is null;

    /// <summary>The menu the item is in (set by the menu; the logical parent may not be there yet).</summary>
    internal HamburgerMenu? Owner { get; set; }

    private HamburgerMenu? Menu => Owner ?? Parent as HamburgerMenu;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsExpandedProperty)
        {
            foreach (var sub in Items)
                sub.UpdateState();
            UpdateState();
        }
    }

    private void OnSubItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateState();
        Menu?.SyncSubItems();
    }

    /// <summary>Visibility and the pseudo-classes the template reads.</summary>
    internal void UpdateState()
    {
        IsVisible = group is not { } g || (g.IsExpanded && g.Menu is not { IsPaneOpen: false });
        PseudoClasses.Set(":group", IsGroup);
        PseudoClasses.Set(":subitem", IsSubItem);
        PseudoClasses.Set(":expanded", IsExpanded);
        PseudoClasses.Set(":showschild", ShowsChildSelection);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (IsGroup && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            // A group opens and closes; it is not selected. With the pane showing only icons it
            // opens the pane, since the sub-items have no icons to show.
            e.Handled = true;
            Focus(NavigationMethod.Pointer);
            if (Menu is { IsPaneOpen: false } menu)
            {
                menu.IsPaneOpen = true;
                IsExpanded = true;
            }
            else
            {
                IsExpanded = !IsExpanded;
            }
            return;
        }
        base.OnPointerPressed(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (IsGroup && e.KeyModifiers == KeyModifiers.None)
        {
            switch (e.Key)
            {
                case Key.Enter or Key.Space:
                    IsExpanded = !IsExpanded;
                    e.Handled = true;
                    return;
                case Key.Right when !IsExpanded:
                    IsExpanded = true;
                    e.Handled = true;
                    return;
                case Key.Left when IsExpanded:
                    IsExpanded = false;
                    e.Handled = true;
                    return;
            }
        }
        else if (group is { } g && e.Key == Key.Left && e.KeyModifiers == KeyModifiers.None)
        {
            g.Focus(NavigationMethod.Directional);
            e.Handled = true;
            return;
        }
        base.OnKeyDown(e);
    }
}
