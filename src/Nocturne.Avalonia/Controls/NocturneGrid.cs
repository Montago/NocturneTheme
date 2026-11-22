using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Controls.Metadata;
using Avalonia.Media;
using Avalonia.Threading;

namespace Nocturne.Avalonia.Controls;

/// <summary>Event data for <see cref="NocturneGrid.Filtered"/>.</summary>
public sealed class GridFilteredEventArgs(RoutedEvent routedEvent, object source, int count, int totalCount) : RoutedEventArgs(routedEvent, source)
{
    /// <summary>Rows left after the search and the column filters.</summary>
    public int Count { get; } = count;

    /// <summary>Rows in the source.</summary>
    public int TotalCount { get; } = totalCount;
}

/// <summary>Event data for <see cref="NocturneGrid.SelectedItemChanged"/>.</summary>
public sealed class GridSelectedItemChangedEventArgs(RoutedEvent routedEvent, object source, object? oldValue, object? newValue) : RoutedEventArgs(routedEvent, source)
{
    public object? OldValue { get; } = oldValue;

    public object? NewValue { get; } = newValue;
}

/// <summary>One value in the tick list of a <see cref="NocturneGrid"/> header filter.</summary>
public sealed class GridFilterOption : INotifyPropertyChanged
{
    private readonly Action<GridFilterOption> changed;
    private bool isChecked;

    internal GridFilterOption(string text, int count, Action<GridFilterOption> changed)
    {
        Text = text;
        Count = count;
        this.changed = changed;
    }

    /// <summary>The value as the cells show it.</summary>
    public string Text { get; }

    /// <summary>Rows in the source with this value.</summary>
    public int Count { get; }

    /// <summary>Whether the value is in the column's FilterText; ticking it adds the value there.</summary>
    public bool IsChecked
    {
        get => isChecked;
        set
        {
            if (SetChecked(value))
            {
                changed(this);
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    // Follows FilterText without writing back to it.
    internal bool SetChecked(bool value)
    {
        if (isChecked == value)
        {
            return false;
        }
        isChecked = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsChecked)));
        return true;
    }
}

/// <summary>
/// Data grid for large collections, on Avalonia's DataGrid: a word search across the columns with the hits
/// marked in the cells (<see cref="NocturneGridColumn"/>), a value filter in every column header (values to
/// tick, a tree of years, months and days for dates, or values to type), three-state sorting, grouping
/// (<see cref="GroupMemberPath"/>) and paging (<see cref="PageSize"/>). The grid shows its rows through a
/// <see cref="DataGridCollectionView"/>: one it makes for the ItemsSource, or the one given as ItemsSource.
/// </summary>
[TemplatePart(PartPager, typeof(Control))]
[TemplatePart(PartFirstPage, typeof(Button))]
[TemplatePart(PartPreviousPage, typeof(Button))]
[TemplatePart(PartNextPage, typeof(Button))]
[TemplatePart(PartLastPage, typeof(Button))]
[TemplatePart(PartPageText, typeof(TextBlock))]
[TemplatePart(PartRangeText, typeof(TextBlock))]
public partial class NocturneGrid : DataGrid
{
    private const string PartPager = "PART_Pager";
    private const string PartFirstPage = "PART_FirstPage";
    private const string PartPreviousPage = "PART_PreviousPage";
    private const string PartNextPage = "PART_NextPage";
    private const string PartLastPage = "PART_LastPage";
    private const string PartPageText = "PART_PageText";
    private const string PartRangeText = "PART_RangeText";

    private readonly DispatcherTimer timer;
    private DataGridCollectionView? view;
    private IEnumerable? source;
    private DataGridPathGroupDescription? groupDescription;
    private IEnumerable? wrappedSource;
    private DataGridCollectionView? wrappedView;
    private bool showingPage;
    private bool refreshPending;
    private bool sortPending;
    private Button? firstPage, previousPage, nextPage, lastPage;
    private TextBlock? pageText, rangeText;

    static NocturneGrid()
    {
        AutoGenerateColumnsProperty.OverrideDefaultValue<NocturneGrid>(false);
        IsReadOnlyProperty.OverrideDefaultValue<NocturneGrid>(true);
        // The grid filters, pages and groups through a view; one it makes for any other ItemsSource. Coerced, so
        // the DataGrid sees the view from the start (setting ItemsSource again while it takes the first is lost).
        ItemsSourceProperty.OverrideMetadata<NocturneGrid>(new StyledPropertyMetadata<IEnumerable>(coerce: CoerceItemsSource));
        FilterTextProperty.Changed.AddClassHandler<DataGridColumn>(OnFilterTextChanged);
        FilterDatesProperty.Changed.AddClassHandler<DataGridColumn>(OnFilterDatesChanged);
        IsSearchableProperty.Changed.AddClassHandler<DataGridColumn>((c, _) => OnColumnSettingChanged(c));
        IsFilterableProperty.Changed.AddClassHandler<DataGridColumn>((c, _) => OnColumnSettingChanged(c));
        FilterMemberPathProperty.Changed.AddClassHandler<DataGridColumn>((c, _) => OnColumnSettingChanged(c));
        TemplateAppliedEvent.AddClassHandler<DataGridColumnHeader>(OnHeaderTemplateApplied);
    }

    public NocturneGrid()
    {
        timer = new DispatcherTimer(DispatcherPriority.Input);
        timer.Tick += (_, _) => ApplyFilters();
        Columns.CollectionChanged += OnColumnsChanged;
    }

    protected override Type StyleKeyOverride => typeof(NocturneGrid);

    // ---------------- Search ----------------

    public static readonly StyledProperty<string> SearchTextProperty =
        AvaloniaProperty.Register<NocturneGrid, string>(nameof(SearchText), string.Empty, defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Words that must all be found in a row (in any searchable column); quotes keep a phrase together.</summary>
    public string SearchText
    {
        get => GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    public static readonly StyledProperty<TimeSpan> SearchDelayProperty =
        AvaloniaProperty.Register<NocturneGrid, TimeSpan>(nameof(SearchDelay), TimeSpan.FromMilliseconds(300));

    /// <summary>Pause after the last change to the search or a filter before the rows are filtered; zero filters at once.</summary>
    public TimeSpan SearchDelay
    {
        get => GetValue(SearchDelayProperty);
        set => SetValue(SearchDelayProperty, value);
    }

    public static readonly StyledProperty<int> FilterListLimitProperty =
        AvaloniaProperty.Register<NocturneGrid, int>(nameof(FilterListLimit), 20);

    /// <summary>
    /// A header filter lists the column's values to tick when the column has at most this many
    /// different values; with more, they are typed. Zero never shows the list.
    /// </summary>
    public int FilterListLimit
    {
        get => GetValue(FilterListLimitProperty);
        set => SetValue(FilterListLimitProperty, value);
    }

    /// <summary>The words of <see cref="SearchText"/>, inherited by the cells, which mark them.</summary>
    public static readonly AttachedProperty<IReadOnlyList<string>> SearchWordsProperty =
        AvaloniaProperty.RegisterAttached<NocturneGrid, Control, IReadOnlyList<string>>("SearchWords", Array.Empty<string>(), inherits: true);

    /// <summary>The words of <see cref="SearchText"/> the rows are filtered by and the cells mark.</summary>
    public IReadOnlyList<string> SearchWords => GetValue(SearchWordsProperty);

    public static IReadOnlyList<string> GetSearchWords(Control element) => element.GetValue(SearchWordsProperty);

    /// <summary>Background behind the search hits in <see cref="NocturneGridColumn"/> cells; none by default.</summary>
    public static readonly AttachedProperty<IBrush?> HighlightBrushProperty =
        AvaloniaProperty.RegisterAttached<NocturneGrid, Control, IBrush?>(nameof(HighlightBrush), inherits: true);

    public IBrush? HighlightBrush
    {
        get => GetValue(HighlightBrushProperty);
        set => SetValue(HighlightBrushProperty, value);
    }

    public static IBrush? GetHighlightBrush(Control element) => element.GetValue(HighlightBrushProperty);
    public static void SetHighlightBrush(Control element, IBrush? value) => element.SetValue(HighlightBrushProperty, value);

    /// <summary>
    /// Text colour of the search hits in <see cref="NocturneGridColumn"/> cells: the accent by default
    /// (<c>Nocturne.Brushes.Accent.Text</c>); null keeps the cell's own colour. Inherited, so a selected row
    /// takes it away (its hits keep the selection's text colour).
    /// </summary>
    public static readonly AttachedProperty<IBrush?> HighlightForegroundProperty =
        AvaloniaProperty.RegisterAttached<NocturneGrid, Control, IBrush?>(nameof(HighlightForeground), inherits: true);

    public IBrush? HighlightForeground
    {
        get => GetValue(HighlightForegroundProperty);
        set => SetValue(HighlightForegroundProperty, value);
    }

    public static IBrush? GetHighlightForeground(Control element) => element.GetValue(HighlightForegroundProperty);
    public static void SetHighlightForeground(Control element, IBrush? value) => element.SetValue(HighlightForegroundProperty, value);

    public static readonly StyledProperty<IBrush?> HighlightCellBackgroundProperty =
        AvaloniaProperty.Register<NocturneGrid, IBrush?>(nameof(HighlightCellBackground));

    /// <summary>
    /// Background of the <see cref="NocturneGridColumn"/> cells with search hits (they get the style class
    /// <c>searchHit</c>): a faint light by default (<c>Nocturne.Brushes.DataGrid.SearchHit</c>); null for none.
    /// </summary>
    public IBrush? HighlightCellBackground
    {
        get => GetValue(HighlightCellBackgroundProperty);
        set => SetValue(HighlightCellBackgroundProperty, value);
    }

    // ---------------- Grouping and counts ----------------

    public static readonly StyledProperty<string?> GroupMemberPathProperty =
        AvaloniaProperty.Register<NocturneGrid, string?>(nameof(GroupMemberPath));

    /// <summary>Member the rows are grouped by; empty for no groups. Each group gets a header that folds it up.</summary>
    public string? GroupMemberPath
    {
        get => GetValue(GroupMemberPathProperty);
        set => SetValue(GroupMemberPathProperty, value);
    }

    public static readonly DirectProperty<NocturneGrid, int> FilteredItemCountProperty =
        AvaloniaProperty.RegisterDirect<NocturneGrid, int>(nameof(FilteredItemCount), g => g.FilteredItemCount);

    private int filteredItemCount;

    /// <summary>Rows left after the search and the column filters.</summary>
    public int FilteredItemCount
    {
        get => filteredItemCount;
        private set => SetAndRaise(FilteredItemCountProperty, ref filteredItemCount, value);
    }

    public static readonly DirectProperty<NocturneGrid, int> TotalItemCountProperty =
        AvaloniaProperty.RegisterDirect<NocturneGrid, int>(nameof(TotalItemCount), g => g.TotalItemCount);

    private int totalItemCount;

    /// <summary>Rows in the source, filtered or not.</summary>
    public int TotalItemCount
    {
        get => totalItemCount;
        private set => SetAndRaise(TotalItemCountProperty, ref totalItemCount, value);
    }

    public static readonly DirectProperty<NocturneGrid, bool> IsFilteredProperty =
        AvaloniaProperty.RegisterDirect<NocturneGrid, bool>(nameof(IsFiltered), g => g.IsFiltered);

    private bool isFiltered;

    /// <summary>True while a search or a column filter is applied.</summary>
    public bool IsFiltered
    {
        get => isFiltered;
        private set => SetAndRaise(IsFilteredProperty, ref isFiltered, value);
    }

    // ---------------- Paging ----------------

    public static readonly StyledProperty<int> PageSizeProperty =
        AvaloniaProperty.Register<NocturneGrid, int>(nameof(PageSize));

    /// <summary>
    /// Rows per page. When more rows than this pass the search and filters, the grid shows one page
    /// at a time and a pager below the rows; 0 (the default) shows every row.
    /// </summary>
    public int PageSize
    {
        get => GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }

    public static readonly StyledProperty<int> PageIndexProperty =
        AvaloniaProperty.Register<NocturneGrid, int>(nameof(PageIndex), defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay,
            coerce: (_, value) => Math.Max(0, value));

    /// <summary>The page shown, from 0. A new search, filter or sort goes back to the first page.</summary>
    public int PageIndex
    {
        get => GetValue(PageIndexProperty);
        set => SetValue(PageIndexProperty, value);
    }

    public static readonly DirectProperty<NocturneGrid, int> PageCountProperty =
        AvaloniaProperty.RegisterDirect<NocturneGrid, int>(nameof(PageCount), g => g.PageCount);

    private int pageCount = 1;

    /// <summary>Number of pages; 1 when not paging.</summary>
    public int PageCount
    {
        get => pageCount;
        private set => SetAndRaise(PageCountProperty, ref pageCount, value);
    }

    public static readonly DirectProperty<NocturneGrid, bool> IsPagingProperty =
        AvaloniaProperty.RegisterDirect<NocturneGrid, bool>(nameof(IsPaging), g => g.IsPaging);

    private bool isPaging;

    /// <summary>True when there is more than one page; the pager is shown then.</summary>
    public bool IsPaging
    {
        get => isPaging;
        private set => SetAndRaise(IsPagingProperty, ref isPaging, value);
    }

    public static readonly DirectProperty<NocturneGrid, int> PageNumberProperty =
        AvaloniaProperty.RegisterDirect<NocturneGrid, int>(nameof(PageNumber), g => g.PageNumber);

    private int pageNumber = 1;

    /// <summary>The page shown, from 1 (for display).</summary>
    public int PageNumber
    {
        get => pageNumber;
        private set => SetAndRaise(PageNumberProperty, ref pageNumber, value);
    }

    public static readonly DirectProperty<NocturneGrid, int> PageFirstRowProperty =
        AvaloniaProperty.RegisterDirect<NocturneGrid, int>(nameof(PageFirstRow), g => g.PageFirstRow);

    private int pageFirstRow;

    /// <summary>Number (from 1) of the first row on the page, among the rows that pass the filters.</summary>
    public int PageFirstRow
    {
        get => pageFirstRow;
        private set => SetAndRaise(PageFirstRowProperty, ref pageFirstRow, value);
    }

    public static readonly DirectProperty<NocturneGrid, int> PageLastRowProperty =
        AvaloniaProperty.RegisterDirect<NocturneGrid, int>(nameof(PageLastRow), g => g.PageLastRow);

    private int pageLastRow;

    /// <summary>Number (from 1) of the last row on the page.</summary>
    public int PageLastRow
    {
        get => pageLastRow;
        private set => SetAndRaise(PageLastRowProperty, ref pageLastRow, value);
    }

    // ---------------- Column settings (attached to any DataGridColumn) ----------------

    /// <summary>Whether the search looks in this column (and marks its hits).</summary>
    public static readonly AttachedProperty<bool> IsSearchableProperty =
        AvaloniaProperty.RegisterAttached<NocturneGrid, DataGridColumn, bool>("IsSearchable", true);

    public static bool GetIsSearchable(DataGridColumn column) => column.GetValue(IsSearchableProperty);
    public static void SetIsSearchable(DataGridColumn column, bool value) => column.SetValue(IsSearchableProperty, value);

    /// <summary>Whether the column header shows the filter button.</summary>
    public static readonly AttachedProperty<bool> IsFilterableProperty =
        AvaloniaProperty.RegisterAttached<NocturneGrid, DataGridColumn, bool>("IsFilterable", true);

    public static bool GetIsFilterable(DataGridColumn column) => column.GetValue(IsFilterableProperty);
    public static void SetIsFilterable(DataGridColumn column, bool value) => column.SetValue(IsFilterableProperty, value);

    /// <summary>
    /// Member the search and the filter read for this column. Defaults to the column's Binding
    /// (with its StringFormat and Converter), or SortMemberPath for template columns.
    /// </summary>
    public static readonly AttachedProperty<string?> FilterMemberPathProperty =
        AvaloniaProperty.RegisterAttached<NocturneGrid, DataGridColumn, string?>("FilterMemberPath");

    public static string? GetFilterMemberPath(DataGridColumn column) => column.GetValue(FilterMemberPathProperty);
    public static void SetFilterMemberPath(DataGridColumn column, string? value) => column.SetValue(FilterMemberPathProperty, value);

    /// <summary>
    /// Values the column must equal (ignoring case), separated by spaces or commas; a row passes if it
    /// matches any of them. Quotes keep a value with spaces together; * matches any text ("vol*").
    /// Ranges compare numbers and dates by value: "&gt;50000", "&lt;=2019", "2020-01-01..2020-06-30".
    /// </summary>
    public static readonly AttachedProperty<string> FilterTextProperty =
        AvaloniaProperty.RegisterAttached<NocturneGrid, DataGridColumn, string>("FilterText", string.Empty,
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    public static string GetFilterText(DataGridColumn column) => column.GetValue(FilterTextProperty);
    public static void SetFilterText(DataGridColumn column, string value) => column.SetValue(FilterTextProperty, value);

    /// <summary>
    /// Days the column's date (DateTime, DateTimeOffset or DateOnly; its time does not count) must be one
    /// of; null or empty for no filter. The date tree in the header filter fills it, and a null entry stands
    /// for rows without a date. A row must also pass the column's <c>FilterText</c>.
    /// </summary>
    public static readonly AttachedProperty<IReadOnlyList<DateTime?>?> FilterDatesProperty =
        AvaloniaProperty.RegisterAttached<NocturneGrid, DataGridColumn, IReadOnlyList<DateTime?>?>("FilterDates",
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    public static IReadOnlyList<DateTime?>? GetFilterDates(DataGridColumn column) => column.GetValue(FilterDatesProperty);
    public static void SetFilterDates(DataGridColumn column, IReadOnlyList<DateTime?>? value) => column.SetValue(FilterDatesProperty, value);

    /// <summary>True when the column's <c>FilterText</c> holds at least one value, or it has <c>FilterDates</c> (set by the grid).</summary>
    public static readonly AttachedProperty<bool> HasFilterProperty =
        AvaloniaProperty.RegisterAttached<NocturneGrid, DataGridColumn, bool>("HasFilter");

    public static bool GetHasFilter(DataGridColumn column) => column.GetValue(HasFilterProperty);

    /// <summary>
    /// The column's different values, read when its filter box opens; null when the column has more
    /// than <see cref="FilterListLimit"/> of them, and for a column of dates (see <see cref="FilterDateTreeProperty"/>).
    /// </summary>
    public static readonly AttachedProperty<IReadOnlyList<GridFilterOption>?> FilterOptionsProperty =
        AvaloniaProperty.RegisterAttached<NocturneGrid, DataGridColumn, IReadOnlyList<GridFilterOption>?>("FilterOptions");

    public static IReadOnlyList<GridFilterOption>? GetFilterOptions(DataGridColumn column) => column.GetValue(FilterOptionsProperty);

    /// <summary>
    /// The years, months and days of a column whose values are all dates, read when its filter box opens;
    /// null for other columns. Ticking them fills the column's <c>FilterDates</c>.
    /// </summary>
    public static readonly AttachedProperty<IReadOnlyList<GridDateFilterNode>?> FilterDateTreeProperty =
        AvaloniaProperty.RegisterAttached<NocturneGrid, DataGridColumn, IReadOnlyList<GridDateFilterNode>?>("FilterDateTree");

    public static IReadOnlyList<GridDateFilterNode>? GetFilterDateTree(DataGridColumn column) => column.GetValue(FilterDateTreeProperty);

    // The grid a column belongs to, so a changed FilterText can reach it.
    private static readonly AttachedProperty<NocturneGrid?> OwnerProperty =
        AvaloniaProperty.RegisterAttached<NocturneGrid, DataGridColumn, NocturneGrid?>("Owner");

    // ---------------- Events ----------------

    public static readonly RoutedEvent<GridFilteredEventArgs> FilteredEvent =
        RoutedEvent.Register<NocturneGrid, GridFilteredEventArgs>(nameof(Filtered), RoutingStrategies.Bubble);

    /// <summary>Raised after the rows have been filtered by the search and the column filters.</summary>
    public event EventHandler<GridFilteredEventArgs> Filtered
    {
        add => AddHandler(FilteredEvent, value);
        remove => RemoveHandler(FilteredEvent, value);
    }

    public static readonly RoutedEvent<GridSelectedItemChangedEventArgs> SelectedItemChangedEvent =
        RoutedEvent.Register<NocturneGrid, GridSelectedItemChangedEventArgs>(nameof(SelectedItemChanged), RoutingStrategies.Bubble);

    /// <summary>Raised when <c>SelectedItem</c> changes (unlike SelectionChanged, not by combo boxes inside the cells).</summary>
    public event EventHandler<GridSelectedItemChangedEventArgs> SelectedItemChanged
    {
        add => AddHandler(SelectedItemChangedEvent, value);
        remove => RemoveHandler(SelectedItemChangedEvent, value);
    }

    // ---------------- Public methods ----------------

    /// <summary>The view the grid shows its rows through (sorted, filtered, grouped, paged); null without ItemsSource.</summary>
    public DataGridCollectionView? View => view;

    /// <summary>
    /// The rows left after the search and the filters, in the order shown (sorted, grouped); with
    /// paging, the rows of every page.
    /// </summary>
    public IReadOnlyList<object> GetVisibleItems()
    {
        if (view == null)
        {
            return Array.Empty<object>();
        }
        var size = view.PageSize;
        if (size <= 0)
        {
            return view.Cast<object>().ToList();
        }
        // The view enumerates one page; for a moment it has none
        var index = view.PageIndex;
        var selected = SelectedItem;
        showingPage = true;
        try
        {
            view.PageSize = 0;
            var items = view.Cast<object>().ToList();
            view.PageSize = size;
            view.MoveToPage(index);
            return items;
        }
        finally
        {
            showingPage = false;
            SelectedItem = selected != null && view.Contains(selected) ? selected : null;
        }
    }

    /// <summary>Clears the search and every column filter.</summary>
    public void ClearFilters()
    {
        SetCurrentValue(SearchTextProperty, string.Empty);
        foreach (var column in Columns)
        {
            column.SetCurrentValue(FilterTextProperty, string.Empty);
            column.SetCurrentValue(FilterDatesProperty, null);
        }
        ApplyFilters();
    }

    /// <summary>Clears the filter of one column and filters the rows again.</summary>
    public void ClearFilter(DataGridColumn column)
    {
        column.SetCurrentValue(FilterTextProperty, string.Empty);
        column.SetCurrentValue(FilterDatesProperty, null);
        ApplyFilters();
    }

    /// <summary>
    /// Filters the rows now instead of after <see cref="SearchDelay"/>, and goes back to the first
    /// page; also re-reads changed item values.
    /// </summary>
    public void ApplyFilters() => Refilter(keepPage: false);

    /// <summary>Unfolds every group.</summary>
    public void ExpandAllGroups()
    {
        foreach (var group in view?.Groups?.OfType<DataGridCollectionViewGroup>() ?? [])
        {
            ExpandRowGroup(group, expandAllSubgroups: true);
        }
    }

    /// <summary>Folds every group up to its header.</summary>
    public void CollapseAllGroups()
    {
        foreach (var group in view?.Groups?.OfType<DataGridCollectionViewGroup>() ?? [])
        {
            CollapseRowGroup(group, collapseAllSubgroups: true);
        }
    }

    public void MoveToFirstPage() => PageIndex = 0;
    public void MoveToPreviousPage() => PageIndex = Math.Max(0, PageIndex - 1);
    public void MoveToNextPage() => PageIndex = Math.Min(PageCount - 1, PageIndex + 1);
    public void MoveToLastPage() => PageIndex = PageCount - 1;

    // ---------------- Overrides ----------------

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ItemsSourceProperty)
        {
            OnItemsSourceChanged(change.GetNewValue<IEnumerable?>());
        }
        else if (change.Property == SearchTextProperty)
        {
            ScheduleFilter();
        }
        else if (change.Property == GroupMemberPathProperty)
        {
            UpdateGrouping();
            Refilter(keepPage: false);
        }
        else if (change.Property == PageSizeProperty)
        {
            Refilter(keepPage: false);
        }
        else if (change.Property == PageIndexProperty && !showingPage)
        {
            ShowPage(change.GetNewValue<int>());
        }
        else if (change.Property == SelectedItemProperty)
        {
            RaiseEvent(new GridSelectedItemChangedEventArgs(SelectedItemChangedEvent, this, change.OldValue, change.NewValue));
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        foreach (var button in new[] { firstPage, previousPage, nextPage, lastPage })
        {
            if (button != null)
            {
                button.Click -= OnPagerClick;
            }
        }
        firstPage = e.NameScope.Find<Button>(PartFirstPage);
        previousPage = e.NameScope.Find<Button>(PartPreviousPage);
        nextPage = e.NameScope.Find<Button>(PartNextPage);
        lastPage = e.NameScope.Find<Button>(PartLastPage);
        pageText = e.NameScope.Find<TextBlock>(PartPageText);
        rangeText = e.NameScope.Find<TextBlock>(PartRangeText);
        foreach (var button in new[] { firstPage, previousPage, nextPage, lastPage })
        {
            if (button != null)
            {
                button.Click += OnPagerClick;
            }
        }
        UpdatePager();
    }

    /// <summary>Clicking a header sorts ascending, then descending, then back to the source order.</summary>
    protected override void OnColumnSorting(DataGridColumnEventArgs e)
    {
        base.OnColumnSorting(e);
        if (e.Handled || view == null || SortPath(e.Column) is not { } path)
        {
            return;
        }
        var sort = view.SortDescriptions.FirstOrDefault(s => s.HasPropertyPath && s.PropertyPath == path);
        if (sort is { Direction: ListSortDirection.Descending })
        {
            e.Handled = true;
            view.SortDescriptions.Remove(sort);
        }
    }

    // ---------------- Implementation ----------------

    private static IEnumerable CoerceItemsSource(AvaloniaObject target, IEnumerable value)
    {
        var grid = (NocturneGrid)target;
        if (value is null or DataGridCollectionView)
        {
            return value!;
        }
        if (!ReferenceEquals(value, grid.wrappedSource) || grid.wrappedView == null)
        {
            grid.wrappedSource = value;
            grid.wrappedView = new DataGridCollectionView(value);
        }
        return grid.wrappedView;
    }

    private void OnItemsSourceChanged(IEnumerable? value)
    {
        DetachView();
        view = value as DataGridCollectionView;
        source = view?.SourceCollection;
        AttachView();
        UpdateGrouping();
        Refilter(keepPage: false);
    }

    private void AttachView()
    {
        if (view != null)
        {
            view.PageChanged += OnViewPageChanged;
            view.SortDescriptions.CollectionChanged += OnSortsChanged;
        }
        if (source is INotifyCollectionChanged changing)
        {
            changing.CollectionChanged += OnSourceChanged;
        }
    }

    private void DetachView()
    {
        if (view != null)
        {
            view.PageChanged -= OnViewPageChanged;
            view.SortDescriptions.CollectionChanged -= OnSortsChanged;
        }
        if (source is INotifyCollectionChanged changing)
        {
            changing.CollectionChanged -= OnSourceChanged;
        }
        groupDescription = null;
    }

    // Rows added or removed in the source: the counts and pages move along, on the same page.
    private void OnSourceChanged(object? sender, NotifyCollectionChangedEventArgs e) => ScheduleRefresh();

    // A new sort goes back to the first page, as a new search does; once the sort is done (it changes the
    // descriptions while the view defers its refresh).
    private void OnSortsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (PageSize > 0 && !showingPage && !sortPending)
        {
            sortPending = true;
            Dispatcher.UIThread.Post(() =>
            {
                sortPending = false;
                ShowPage(0);
            }, DispatcherPriority.Background);
        }
    }

    private void OnViewPageChanged(object? sender, EventArgs e)
    {
        if (!showingPage)
        {
            UpdateCounts();
        }
    }

    private void ScheduleRefresh()
    {
        if (refreshPending)
        {
            return;
        }
        refreshPending = true;
        Dispatcher.UIThread.Post(() =>
        {
            refreshPending = false;
            UpdateCounts();
        }, DispatcherPriority.Background);
    }

    private void UpdateGrouping()
    {
        if (view == null)
        {
            return;
        }
        if (groupDescription != null)
        {
            view.GroupDescriptions.Remove(groupDescription);
            groupDescription = null;
        }
        if (!string.IsNullOrEmpty(GroupMemberPath))
        {
            groupDescription = new DataGridPathGroupDescription(GroupMemberPath);
            view.GroupDescriptions.Add(groupDescription);
        }
    }

    private void Refilter(bool keepPage)
    {
        timer.Stop();
        if (view == null)
        {
            UpdateCounts();
            return;
        }
        if (view.IsEditingItem || view.IsAddingNew)
        {
            // A refresh is not allowed during an edit; try again once the row is valid.
            if (!CommitEdit(DataGridEditingUnit.Row, true))
            {
                timer.Interval = TimeSpan.FromMilliseconds(200);
                timer.Start();
                return;
            }
        }

        var culture = CultureInfo.CurrentCulture;
        var words = Tokenize(SearchText);
        if (!words.SequenceEqual(SearchWords, StringComparer.Ordinal))
        {
            SetValue(SearchWordsProperty, words.ToArray());
        }

        var searched = words.Count == 0
            ? Array.Empty<ColumnReader>()
            : Columns.Where(c => c.IsVisible && GetIsSearchable(c)).Select(c => ColumnReader.Create(c, culture)).OfType<ColumnReader>().ToArray();
        var filters = Columns
            .Where(c => GetIsFilterable(c) && GetHasFilter(c))
            .Select(c => ColumnReader.Create(c, culture) is { } reader
                ? new ColumnFilter(reader, FilterTokens(GetFilterText(c)).Select(t => FilterValue.Parse(t, culture)).ToArray(), GetFilterDates(c))
                : null)
            .OfType<ColumnFilter>()
            .ToArray();
        var searchWords = words.ToArray();

        Func<object, bool>? predicate = searchWords.Length == 0 && filters.Length == 0
            ? null
            : item => Matches(item, searchWords, searched, filters);

        var selected = SelectedItem;
        showingPage = true;
        try
        {
            using (view.DeferRefresh())
            {
                view.Filter = predicate;
                view.PageSize = Math.Max(0, PageSize);
            }
            if (predicate == null && view.NeedsRefresh)
            {
                view.Refresh();
            }
        }
        finally
        {
            showingPage = false;
        }
        ShowPage(keepPage ? PageIndex : 0, selected);

        IsFiltered = predicate != null;
        RaiseEvent(new GridFilteredEventArgs(FilteredEvent, this, FilteredItemCount, TotalItemCount));
    }

    /// <summary>
    /// Shows one page of the rows that pass the filters (with no paging, all of them). The view makes its first
    /// row current, which the DataGrid would select: the row that was selected stays so if it is on the page,
    /// and otherwise nothing is.
    /// </summary>
    private void ShowPage(int index) => ShowPage(index, SelectedItem);

    private void ShowPage(int index, object? selected)
    {
        if (view != null && view.PageSize > 0)
        {
            var pages = Math.Max(1, (view.ItemCount + view.PageSize - 1) / view.PageSize);
            index = Math.Clamp(index, 0, pages - 1);
            if (view.PageIndex != index)
            {
                showingPage = true;
                try
                {
                    view.MoveToPage(index);
                }
                finally
                {
                    showingPage = false;
                }
            }
        }
        else
        {
            index = 0;
        }
        showingPage = true;
        try
        {
            SetCurrentValue(PageIndexProperty, index);
        }
        finally
        {
            showingPage = false;
        }
        UpdateCounts();
        var keep = selected != null && view?.Contains(selected) == true ? selected : null;
        if (view != null)
        {
            // The DataGrid selects the view's current item, which a refresh or a new page moves to the first row
            if (keep != null)
            {
                view.MoveCurrentTo(keep);
            }
            else
            {
                view.MoveCurrentToPosition(-1);
            }
        }
        SelectedItem = keep;
        if (view?.Count > 0)
        {
            ScrollIntoView(SelectedItem ?? view.GetItemAt(0), null);
        }
    }

    private void UpdateCounts()
    {
        var total = source switch
        {
            null => 0,
            ICollection collection => collection.Count,
            _ => source.Cast<object>().Count(),
        };
        var count = view?.ItemCount ?? 0;
        var size = view?.PageSize ?? 0;
        var pages = size > 0 && count > size ? (count + size - 1) / size : 1;
        var index = pages > 1 ? Math.Max(0, view!.PageIndex) : 0;

        FilteredItemCount = count;
        TotalItemCount = total;
        PageCount = pages;
        IsPaging = pages > 1;
        PageNumber = index + 1;
        PageFirstRow = pages > 1 ? index * size + 1 : Math.Min(1, count);
        PageLastRow = pages > 1 ? Math.Min(count, (index + 1) * size) : count;
        UpdatePager();
    }

    private void UpdatePager()
    {
        if (pageText != null)
        {
            pageText.Text = string.Format(CultureInfo.CurrentCulture, "Page {0:N0} of {1:N0}", PageNumber, PageCount);
        }
        if (rangeText != null)
        {
            rangeText.Text = string.Format(CultureInfo.CurrentCulture, "{0:N0}–{1:N0} of {2:N0}", PageFirstRow, PageLastRow, FilteredItemCount);
        }
        if (firstPage != null)
        {
            firstPage.IsEnabled = PageIndex > 0;
        }
        if (previousPage != null)
        {
            previousPage.IsEnabled = PageIndex > 0;
        }
        if (nextPage != null)
        {
            nextPage.IsEnabled = PageIndex < PageCount - 1;
        }
        if (lastPage != null)
        {
            lastPage.IsEnabled = PageIndex < PageCount - 1;
        }
    }

    private void OnPagerClick(object? sender, RoutedEventArgs e)
    {
        if (sender == firstPage)
        {
            MoveToFirstPage();
        }
        else if (sender == previousPage)
        {
            MoveToPreviousPage();
        }
        else if (sender == nextPage)
        {
            MoveToNextPage();
        }
        else if (sender == lastPage)
        {
            MoveToLastPage();
        }
        e.Handled = true;
    }

    private void ScheduleFilter()
    {
        timer.Stop();
        if (SearchDelay <= TimeSpan.Zero)
        {
            ApplyFilters();
            return;
        }
        timer.Interval = SearchDelay;
        timer.Start();
    }

    private void OnColumnsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        foreach (var column in e.OldItems?.OfType<DataGridColumn>() ?? [])
        {
            if (column.GetValue(OwnerProperty) == this)
            {
                column.ClearValue(OwnerProperty);
            }
        }
        foreach (var column in Columns)
        {
            column.SetValue(OwnerProperty, this);
        }
        if (IsFiltered || Columns.Any(GetHasFilter))
        {
            ScheduleFilter();
        }
    }

    private static void OnColumnSettingChanged(DataGridColumn column)
    {
        if (column.GetValue(OwnerProperty) is { } grid && (grid.IsFiltered || GetHasFilter(column)))
        {
            grid.ScheduleFilter();
        }
    }

    private static void OnFilterTextChanged(DataGridColumn column, AvaloniaPropertyChangedEventArgs e)
    {
        var tokens = FilterTokens(e.NewValue as string);
        UpdateHasFilter(column);
        foreach (var option in GetFilterOptions(column) ?? [])
        {
            option.SetChecked(tokens.Contains(option.Text, StringComparer.OrdinalIgnoreCase));
        }
        column.GetValue(OwnerProperty)?.ScheduleFilter();
    }

    private static void OnFilterDatesChanged(DataGridColumn column, AvaloniaPropertyChangedEventArgs e)
    {
        UpdateHasFilter(column);
        if (GetFilterDateTree(column) is { } tree)
        {
            GridDateFilterNode.Tick(tree, e.NewValue as IReadOnlyList<DateTime?>);
        }
        column.GetValue(OwnerProperty)?.ScheduleFilter();
    }

    private static void UpdateHasFilter(DataGridColumn column) =>
        column.SetValue(HasFilterProperty, FilterTokens(GetFilterText(column)).Count > 0 || GetFilterDates(column) is { Count: > 0 });

    /// <summary>
    /// Reads the column's days for the date tree when all its values are dates; otherwise its different
    /// values for the tick list, unless there are more than <see cref="FilterListLimit"/>.
    /// </summary>
    private void LoadFilterOptions(DataGridColumn column)
    {
        List<GridFilterOption>? options = null;
        List<GridDateFilterNode>? tree = null;
        var culture = CultureInfo.CurrentCulture;
        if (FilterListLimit > 0 && source != null && ColumnReader.Create(column, culture) is { } reader)
        {
            // Day -> rows. The first value that is no date (and not empty) ends it: the column gets the tick list.
            Dictionary<DateTime, int>? days = new();
            var blanks = 0;
            foreach (var item in source)
            {
                var value = reader.GetRawValue(item);
                if (GridDateFilterNode.TryGetDay(value, out var day))
                {
                    days[day] = days.GetValueOrDefault(day) + 1;
                }
                else if (value == null)
                {
                    blanks++;
                }
                else
                {
                    days = null;
                    break;
                }
            }
            if (days is { Count: > 0 })
            {
                tree = GridDateFilterNode.Build(days, blanks, culture, () => OnFilterDateChanged(column));
                GridDateFilterNode.Tick(tree, GetFilterDates(column));
                GridDateFilterNode.ExpandPartlyTicked(tree);
                column.SetValue(FilterOptionsProperty, null);
                column.SetValue(FilterDateTreeProperty, tree);
                return;
            }

            var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in source)
            {
                if (reader.GetText(item).Trim() is not { Length: > 0 } text)
                {
                    continue;
                }
                if (counts.TryGetValue(text, out var count))
                {
                    counts[text] = count + 1;
                }
                else if (counts.Count == FilterListLimit)
                {
                    counts.Clear();
                    break;
                }
                else
                {
                    counts[text] = 1;
                }
            }

            if (counts.Count > 0)
            {
                // Numbers in numeric order, everything else alphabetically.
                var numbers = counts.Keys.ToDictionary(k => k, k => decimal.TryParse(k, NumberStyles.Any, culture, out var n) ? n : (decimal?)null);
                IEnumerable<string> keys = numbers.Values.All(n => n.HasValue)
                    ? counts.Keys.OrderBy(k => numbers[k])
                    : counts.Keys.OrderBy(k => k, StringComparer.Create(culture, true));
                options = keys.Select(k => new GridFilterOption(k, counts[k], _ => OnFilterOptionChanged(column))).ToList();
                var tokens = FilterTokens(GetFilterText(column));
                foreach (var option in options)
                {
                    option.SetChecked(tokens.Contains(option.Text, StringComparer.OrdinalIgnoreCase));
                }
            }
        }
        column.SetValue(FilterOptionsProperty, options);
        column.SetValue(FilterDateTreeProperty, tree);
    }

    /// <summary>Writes the ticked days of the date tree into FilterDates.</summary>
    private static void OnFilterDateChanged(DataGridColumn column)
    {
        var ticked = GridDateFilterNode.GetTicked(GetFilterDateTree(column) ?? []);
        column.SetCurrentValue(FilterDatesProperty, ticked.Count == 0 ? null : ticked);
    }

    /// <summary>Writes the ticked values into FilterText, keeping typed values that are not in the list (such as "vol*").</summary>
    private static void OnFilterOptionChanged(DataGridColumn column)
    {
        var options = GetFilterOptions(column) ?? [];
        var typed = FilterTokens(GetFilterText(column)).Where(t => !options.Any(o => o.Text.Equals(t, StringComparison.OrdinalIgnoreCase)));
        var values = typed.Concat(options.Where(o => o.IsChecked).Select(o => o.Text))
            .Select(v => v.IndexOfAny([' ', ',', ';', '\t']) >= 0 ? $"\"{v}\"" : v);
        column.SetCurrentValue(FilterTextProperty, string.Join(", ", values));
    }

    // The path the view sorts a column by: its SortMemberPath, or its Binding's path.
    private static string? SortPath(DataGridColumn column) =>
        !string.IsNullOrEmpty(column.SortMemberPath) ? column.SortMemberPath
            : column is DataGridBoundColumn { Binding: { } binding } ? ColumnReader.PathOf(binding) : null;

    // ---------------- Filter box in the column header ----------------

    /// <summary>
    /// The header template's filter parts get the column as their DataContext: the button shows for filterable
    /// columns, the popup lists the column's values. The headers of plain DataGrids are left alone.
    /// </summary>
    private static void OnHeaderTemplateApplied(DataGridColumnHeader header, TemplateAppliedEventArgs e)
    {
        if (e.NameScope.Find<Control>("PART_FilterHost") is not { } host)
        {
            return;
        }
        var column = DataGridColumn.GetColumnContainingElement(header);
        if (column?.GetValue(OwnerProperty) == null)
        {
            // A corner header, the filler after the last column or the indent of the group headers
            host.IsVisible = false;
            return;
        }
        host.DataContext = column;
        host.Bind(IsVisibleProperty, column.GetObservable(IsFilterableProperty));

        var button = e.NameScope.Find<ToggleButton>("PART_FilterButton");
        var box = e.NameScope.Find<TextBox>("PART_FilterBox");
        if (button != null)
        {
            button.IsCheckedChanged += (_, _) =>
            {
                if (button.IsChecked == true && column.GetValue(OwnerProperty) is { } grid)
                {
                    grid.LoadFilterOptions(column);
                    Dispatcher.UIThread.Post(() =>
                    {
                        box?.Focus();
                        box?.SelectAll();
                    }, DispatcherPriority.Input);
                }
            };
        }
        box?.AddHandler(KeyDownEvent, (_, args) =>
        {
            if (args.Key is Key.Enter or Key.Escape)
            {
                CloseFilter(column, button);
                args.Handled = true;
            }
        }, RoutingStrategies.Tunnel);
        if (e.NameScope.Find<Button>("PART_ClearFilter") is { } clear)
        {
            clear.Click += (_, _) => column.GetValue(OwnerProperty)?.ClearFilter(column);
        }
        if (e.NameScope.Find<Button>("PART_CloseFilter") is { } done)
        {
            done.Click += (_, _) => CloseFilter(column, button);
        }
        // Events bubble out of a popup to its Popup, so a wheel turn the filter's lists leave (over the box
        // or the buttons, or a list at its end) would scroll the grid under it. The popup keeps them.
        if (e.NameScope.Find<Popup>("PART_FilterPopup")?.Child is { } surface)
        {
            surface.AddHandler(PointerWheelChangedEvent, (_, args) => args.Handled = true);
        }
    }

    private static void CloseFilter(DataGridColumn column, ToggleButton? button)
    {
        if (button != null)
        {
            button.IsChecked = false;
        }
        if (column.GetValue(OwnerProperty) is { } grid)
        {
            if (grid.timer.IsEnabled)
            {
                grid.ApplyFilters();
            }
            grid.Focus();
        }
    }
}
