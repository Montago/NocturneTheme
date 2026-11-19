using System.Collections;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;

namespace Nocturne.Avalonia.Helpers;

/// <summary>
/// Close buttons, badges and the "+" button of the document tabs (<c>Nocturne.Styles.TabControl.Document</c>).
/// </summary>
public class TabHelper
{
    /// <summary>Shows a close button on the tab. Set it on the TabControl for every tab, or per TabItem.</summary>
    public static readonly AttachedProperty<bool> ShowCloseButtonProperty =
        AvaloniaProperty.RegisterAttached<TabHelper, Control, bool>("ShowCloseButton", inherits: true);

    /// <summary>A small count or label after the tab header (amber, as for warnings).</summary>
    public static readonly AttachedProperty<object?> BadgeProperty =
        AvaloniaProperty.RegisterAttached<TabHelper, Control, object?>("Badge");

    /// <summary>Shows a "+" button after the last document tab; it raises <see cref="AddTabRequestedEvent"/> and runs <see cref="AddCommandProperty"/>.</summary>
    public static readonly AttachedProperty<bool> ShowAddButtonProperty =
        AvaloniaProperty.RegisterAttached<TabHelper, Control, bool>("ShowAddButton");

    /// <summary>A command the "+" button runs (with the TabControl as parameter), for view models.</summary>
    public static readonly AttachedProperty<ICommand?> AddCommandProperty =
        AvaloniaProperty.RegisterAttached<TabHelper, Control, ICommand?>("AddCommand");

    /// <summary>
    /// Set by the document TabControl theme: keeps the style class <c>separator</c> on its tabs up to
    /// date, so a thin line divides two unselected neighbours.
    /// </summary>
    public static readonly AttachedProperty<bool> TracksSeparatorsProperty =
        AvaloniaProperty.RegisterAttached<TabHelper, Control, bool>("TracksSeparators");

    /// <summary>Raised on the tab before it closes; set <see cref="TabClosingEventArgs.Cancel"/> to keep it.</summary>
    public static readonly RoutedEvent<TabClosingEventArgs> TabClosingEvent =
        RoutedEvent.Register<TabHelper, TabClosingEventArgs>("TabClosing", RoutingStrategies.Bubble);

    /// <summary>Raised on the TabControl when its "+" button is clicked: add the new tab there.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> AddTabRequestedEvent =
        RoutedEvent.Register<TabHelper, RoutedEventArgs>("AddTabRequested", RoutingStrategies.Bubble);

    /// <summary>Closes the TabItem given as parameter (<see cref="CloseTab"/>); the close button runs it.</summary>
    public static ICommand CloseTabCommand { get; } = new TabCommand(p => { if (p is TabItem tab) CloseTab(tab); });

    /// <summary>Asks the TabControl given as parameter for a new tab (<see cref="RequestAddTab"/>); the "+" button runs it.</summary>
    public static ICommand AddTabCommand { get; } = new TabCommand(p => { if (p is TabControl tabs) RequestAddTab(tabs); });

    static TabHelper()
    {
        TracksSeparatorsProperty.Changed.AddClassHandler<TabControl>(OnTracksSeparatorsChanged);
    }

    public static bool GetShowCloseButton(Control c) => c.GetValue(ShowCloseButtonProperty);
    public static void SetShowCloseButton(Control c, bool value) => c.SetValue(ShowCloseButtonProperty, value);
    public static object? GetBadge(Control c) => c.GetValue(BadgeProperty);
    public static void SetBadge(Control c, object? value) => c.SetValue(BadgeProperty, value);
    public static bool GetShowAddButton(Control c) => c.GetValue(ShowAddButtonProperty);
    public static void SetShowAddButton(Control c, bool value) => c.SetValue(ShowAddButtonProperty, value);
    public static ICommand? GetAddCommand(Control c) => c.GetValue(AddCommandProperty);
    public static void SetAddCommand(Control c, ICommand? value) => c.SetValue(AddCommandProperty, value);
    public static bool GetTracksSeparators(Control c) => c.GetValue(TracksSeparatorsProperty);
    public static void SetTracksSeparators(Control c, bool value) => c.SetValue(TracksSeparatorsProperty, value);

    public static void AddTabClosingHandler(Interactive element, EventHandler<TabClosingEventArgs> handler) =>
        element.AddHandler(TabClosingEvent, handler);

    public static void RemoveTabClosingHandler(Interactive element, EventHandler<TabClosingEventArgs> handler) =>
        element.RemoveHandler(TabClosingEvent, handler);

    public static void AddAddTabRequestedHandler(Interactive element, EventHandler<RoutedEventArgs> handler) =>
        element.AddHandler(AddTabRequestedEvent, handler);

    public static void RemoveAddTabRequestedHandler(Interactive element, EventHandler<RoutedEventArgs> handler) =>
        element.RemoveHandler(AddTabRequestedEvent, handler);

    /// <summary>
    /// Closes <paramref name="tab"/> (what its close button does): raises <see cref="TabClosingEvent"/>
    /// and, unless a handler cancels it, removes the item from ItemsSource (when it is a writable list)
    /// or Items.
    /// </summary>
    public static void CloseTab(TabItem tab)
    {
        if ((tab.Parent as TabControl ?? tab.FindLogicalAncestorOfType<TabControl>()) is not { } tabControl)
            return;

        var item = tabControl.ItemFromContainer(tab) ?? tab;
        var args = new TabClosingEventArgs(tab, item);
        tab.RaiseEvent(args);
        if (args.Cancel)
            return;

        if (tabControl.ItemsSource is IList { IsReadOnly: false, IsFixedSize: false } list)
            list.Remove(item);
        else if (tabControl.ItemsSource == null)
            tabControl.Items.Remove(item);
    }

    /// <summary>What the "+" button does: raises <see cref="AddTabRequestedEvent"/> and runs <see cref="AddCommandProperty"/>.</summary>
    public static void RequestAddTab(TabControl tabControl)
    {
        tabControl.RaiseEvent(new RoutedEventArgs(AddTabRequestedEvent, tabControl));
        if (GetAddCommand(tabControl) is { } command && command.CanExecute(tabControl))
            command.Execute(tabControl);
    }

    private static void OnTracksSeparatorsChanged(TabControl tabControl, AvaloniaPropertyChangedEventArgs e)
    {
        tabControl.SelectionChanged -= OnSelectionChanged;
        tabControl.ContainerPrepared -= OnContainersChanged;
        tabControl.ContainerClearing -= OnContainersChanged;
        if (e.NewValue is true)
        {
            tabControl.SelectionChanged += OnSelectionChanged;
            tabControl.ContainerPrepared += OnContainersChanged;
            tabControl.ContainerClearing += OnContainersChanged;
            UpdateSeparators(tabControl);
        }
    }

    private static void OnContainersChanged(object? sender, EventArgs e)
    {
        if (sender is TabControl tabControl)
            UpdateSeparators(tabControl);
    }

    private static void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // Selection changes of lists inside the pages bubble up too; only the tabs' own count
        if (ReferenceEquals(e.Source, sender) && sender is TabControl tabControl)
            UpdateSeparators(tabControl);
    }

    private static void UpdateSeparators(TabControl tabControl)
    {
        TabItem? previous = null;
        for (var i = 0; i < tabControl.ItemCount; i++)
        {
            if (tabControl.ContainerFromIndex(i) is not TabItem tab)
                continue;
            tab.Classes.Set("separator", previous != null && !previous.IsSelected && !tab.IsSelected);
            previous = tab;
        }
    }
}

internal sealed class TabCommand(Action<object?> execute) : ICommand
{
    public event EventHandler? CanExecuteChanged { add { } remove { } }

    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter) => execute(parameter);
}

/// <summary>Data for <see cref="TabHelper.TabClosingEvent"/>; set <see cref="Cancel"/> to keep the tab.</summary>
public class TabClosingEventArgs : RoutedEventArgs
{
    public TabClosingEventArgs(TabItem tab, object item) : base(TabHelper.TabClosingEvent, tab)
    {
        Tab = tab;
        Item = item;
    }

    public TabItem Tab { get; }

    /// <summary>The data item behind the tab (the TabItem itself when the tabs are written in XAML).</summary>
    public object Item { get; }

    public bool Cancel { get; set; }
}
