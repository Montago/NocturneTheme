using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Frosted.Wpf.Helpers;

/// <summary>
/// Close buttons and badges for the document tabs (<c>Frosted.Styles.TabControl.Document</c>).
/// </summary>
public static class TabHelper
{
    /// <summary>
    /// Closes the TabItem given as parameter: raises <see cref="TabClosingEvent"/> and, unless a
    /// handler cancels it, removes the item from ItemsSource (when it is a writable list) or Items.
    /// </summary>
    public static RoutedCommand CloseTabCommand { get; } = new(nameof(CloseTabCommand), typeof(TabHelper));

    /// <summary>
    /// Run by the "+" button of a document TabControl (<see cref="ShowAddButtonProperty"/>).
    /// Handle it with a CommandBinding on the TabControl or the window and add the new tab there.
    /// </summary>
    public static RoutedCommand AddTabCommand { get; } = new(nameof(AddTabCommand), typeof(TabHelper));

    public static readonly DependencyProperty ShowAddButtonProperty = DependencyProperty.RegisterAttached(
        "ShowAddButton", typeof(bool), typeof(TabHelper), new FrameworkPropertyMetadata(false));

    /// <summary>Shows a "+" button after the last document tab; it runs <see cref="AddTabCommand"/>.</summary>
    public static bool GetShowAddButton(DependencyObject d) => (bool)d.GetValue(ShowAddButtonProperty);

    public static void SetShowAddButton(DependencyObject d, bool value) => d.SetValue(ShowAddButtonProperty, value);

    public static readonly DependencyProperty TracksSeparatorsProperty = DependencyProperty.RegisterAttached(
        "TracksSeparators", typeof(bool), typeof(TabHelper), new FrameworkPropertyMetadata(false, OnTracksSeparatorsChanged));

    /// <summary>
    /// Set by the document TabControl style: keeps <see cref="ShowSeparatorProperty"/> up to date
    /// on its tabs, so a thin line divides two unselected neighbours.
    /// </summary>
    public static bool GetTracksSeparators(DependencyObject d) => (bool)d.GetValue(TracksSeparatorsProperty);

    public static void SetTracksSeparators(DependencyObject d, bool value) => d.SetValue(TracksSeparatorsProperty, value);

    private static readonly DependencyPropertyKey ShowSeparatorPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
        "ShowSeparator", typeof(bool), typeof(TabHelper), new FrameworkPropertyMetadata(false));

    /// <summary>True on a tab that has an unselected tab before it and is not selected itself.</summary>
    public static readonly DependencyProperty ShowSeparatorProperty = ShowSeparatorPropertyKey.DependencyProperty;

    public static bool GetShowSeparator(DependencyObject d) => (bool)d.GetValue(ShowSeparatorProperty);

    private static void OnTracksSeparatorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TabControl tabControl)
        {
            return;
        }

        tabControl.SelectionChanged -= OnSeparatorsChanged;
        tabControl.ItemContainerGenerator.StatusChanged -= OnGeneratorStatusChanged;
        tabControl.Loaded -= OnSeparatorsChanged;
        if ((bool)e.NewValue)
        {
            tabControl.SelectionChanged += OnSeparatorsChanged;
            tabControl.ItemContainerGenerator.StatusChanged += OnGeneratorStatusChanged;
            tabControl.Loaded += OnSeparatorsChanged;
            UpdateSeparators(tabControl);
        }

        void OnGeneratorStatusChanged(object? sender, EventArgs args) => UpdateSeparators(tabControl);
    }

    private static void OnSeparatorsChanged(object sender, RoutedEventArgs e)
    {
        // Selection changes of child selectors bubble up too; only react to the tabs' own.
        if (ReferenceEquals(e.OriginalSource, sender) && sender is TabControl tabControl)
        {
            UpdateSeparators(tabControl);
        }
    }

    private static void UpdateSeparators(TabControl tabControl)
    {
        TabItem? previous = null;
        for (var i = 0; i < tabControl.Items.Count; i++)
        {
            if (tabControl.ItemContainerGenerator.ContainerFromIndex(i) is not TabItem tab)
            {
                continue;
            }

            var show = previous != null && !previous.IsSelected && !tab.IsSelected;
            tab.SetValue(ShowSeparatorPropertyKey, show);
            previous = tab;
        }
    }

    public static readonly RoutedEvent TabClosingEvent = EventManager.RegisterRoutedEvent(
        "TabClosing", RoutingStrategy.Bubble, typeof(EventHandler<TabClosingEventArgs>), typeof(TabHelper));

    public static void AddTabClosingHandler(DependencyObject d, EventHandler<TabClosingEventArgs> handler) =>
        (d as UIElement)?.AddHandler(TabClosingEvent, handler);

    public static void RemoveTabClosingHandler(DependencyObject d, EventHandler<TabClosingEventArgs> handler) =>
        (d as UIElement)?.RemoveHandler(TabClosingEvent, handler);

    public static readonly DependencyProperty ShowCloseButtonProperty = DependencyProperty.RegisterAttached(
        "ShowCloseButton", typeof(bool), typeof(TabHelper),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

    /// <summary>Shows a close button on the tab. Set it on the TabControl for every tab, or per TabItem.</summary>
    public static bool GetShowCloseButton(DependencyObject d) => (bool)d.GetValue(ShowCloseButtonProperty);

    public static void SetShowCloseButton(DependencyObject d, bool value) => d.SetValue(ShowCloseButtonProperty, value);

    public static readonly DependencyProperty BadgeProperty = DependencyProperty.RegisterAttached(
        "Badge", typeof(object), typeof(TabHelper), new FrameworkPropertyMetadata(null));

    /// <summary>A small count or label after the tab header (amber, as for warnings).</summary>
    public static object? GetBadge(DependencyObject d) => d.GetValue(BadgeProperty);

    public static void SetBadge(DependencyObject d, object? value) => d.SetValue(BadgeProperty, value);

    static TabHelper()
    {
        CommandManager.RegisterClassCommandBinding(typeof(TabControl), new CommandBinding(CloseTabCommand, OnCloseTab));
    }

    private static void OnCloseTab(object sender, ExecutedRoutedEventArgs e)
    {
        if (e.Parameter is not TabItem tab || sender is not TabControl tabControl)
        {
            return;
        }

        e.Handled = true;
        var item = tabControl.ItemContainerGenerator.ItemFromContainer(tab);
        if (item == DependencyProperty.UnsetValue)
        {
            item = tab;
        }

        var args = new TabClosingEventArgs(TabClosingEvent, tab, item);
        tab.RaiseEvent(args);
        if (args.Cancel)
        {
            return;
        }

        if (tabControl.ItemsSource is IList { IsReadOnly: false, IsFixedSize: false } list)
        {
            list.Remove(item);
        }
        else if (tabControl.ItemsSource == null)
        {
            tabControl.Items.Remove(item);
        }
    }
}

/// <summary>Data for <see cref="TabHelper.TabClosingEvent"/>; set <see cref="Cancel"/> to keep the tab.</summary>
public class TabClosingEventArgs : RoutedEventArgs
{
    public TabClosingEventArgs(RoutedEvent routedEvent, TabItem tab, object item) : base(routedEvent, tab)
    {
        Tab = tab;
        Item = item;
    }

    public TabItem Tab { get; }

    /// <summary>The data item behind the tab (the TabItem itself when the tabs are written in XAML).</summary>
    public object Item { get; }

    public bool Cancel { get; set; }
}
