using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Styling;

namespace Nocturne.Avalonia.Controls;

/// <summary>
/// Text column for <see cref="NocturneGrid"/>: shows its Binding like <see cref="DataGridTextColumn"/>
/// (and edits it the same way) and marks the words of the grid's search in the cell. A cell with hits gets
/// the style class <c>searchHit</c>.
/// </summary>
public class NocturneGridColumn : DataGridTextColumn
{
    /// <summary>Whether the grid's search looks in this column and marks its hits.</summary>
    public static readonly AttachedProperty<bool> IsSearchableProperty = NocturneGrid.IsSearchableProperty.AddOwner<NocturneGridColumn>();

    /// <summary>Whether the header shows the filter button.</summary>
    public static readonly AttachedProperty<bool> IsFilterableProperty = NocturneGrid.IsFilterableProperty.AddOwner<NocturneGridColumn>();

    /// <summary>Member the search and filter read instead of the Binding.</summary>
    public static readonly AttachedProperty<string?> FilterMemberPathProperty = NocturneGrid.FilterMemberPathProperty.AddOwner<NocturneGridColumn>();

    /// <summary>Values this column must equal, separated by spaces or commas (see <see cref="NocturneGrid.FilterTextProperty"/>).</summary>
    public static readonly AttachedProperty<string> FilterTextProperty = NocturneGrid.FilterTextProperty.AddOwner<NocturneGridColumn>();

    /// <summary>Days a column of dates must be one of, as the date tree in the header ticks them.</summary>
    public static readonly AttachedProperty<IReadOnlyList<DateTime?>?> FilterDatesProperty = NocturneGrid.FilterDatesProperty.AddOwner<NocturneGridColumn>();

    public static readonly StyledProperty<TextAlignment> TextAlignmentProperty =
        AvaloniaProperty.Register<NocturneGridColumn, TextAlignment>(nameof(TextAlignment));

    public bool IsSearchable
    {
        get => GetValue(IsSearchableProperty);
        set => SetValue(IsSearchableProperty, value);
    }

    public bool IsFilterable
    {
        get => GetValue(IsFilterableProperty);
        set => SetValue(IsFilterableProperty, value);
    }

    public string? FilterMemberPath
    {
        get => GetValue(FilterMemberPathProperty);
        set => SetValue(FilterMemberPathProperty, value);
    }

    public string FilterText
    {
        get => GetValue(FilterTextProperty);
        set => SetValue(FilterTextProperty, value);
    }

    public IReadOnlyList<DateTime?>? FilterDates
    {
        get => GetValue(FilterDatesProperty);
        set => SetValue(FilterDatesProperty, value);
    }

    /// <summary>Alignment of the text in the cells (Right for numbers).</summary>
    public TextAlignment TextAlignment
    {
        get => GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }

    protected override Control GenerateElement(DataGridCell cell, object dataItem)
    {
        var text = new GridCellText(cell, this) { Name = "CellTextBlock" };
        if (Application.Current is { } app
            && app.TryFindResource("DataGridCellTextBlockTheme", app.ActualThemeVariant, out var resource) && resource is ControlTheme theme)
        {
            text.Theme = theme;
        }
        if (IsSet(FontFamilyProperty))
        {
            text.FontFamily = FontFamily;
        }
        if (IsSet(FontSizeProperty))
        {
            text.FontSize = FontSize;
        }
        if (IsSet(FontStyleProperty))
        {
            text.FontStyle = FontStyle;
        }
        if (IsSet(FontWeightProperty))
        {
            text.FontWeight = FontWeight;
        }
        if (IsSet(ForegroundProperty))
        {
            text.Foreground = Foreground;
        }
        text.Bind(TextBlock.TextAlignmentProperty, this.GetObservable(TextAlignmentProperty));
        if (Binding != null)
        {
            text.Bind(GridCellText.CellTextProperty, Binding);
        }
        return text;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsSearchableProperty)
        {
            // The cells take the setting when the grid's search words are set again.
            SetValue(SearchSettingVersionProperty, GetValue(SearchSettingVersionProperty) + 1);
        }
    }

    // Bumped when IsSearchable changes, so the cells mark again.
    internal static readonly StyledProperty<int> SearchSettingVersionProperty =
        AvaloniaProperty.Register<NocturneGridColumn, int>("SearchSettingVersion");
}

/// <summary>
/// The text of a <see cref="NocturneGridColumn"/> cell: plain text, or runs with the grid's search hits in
/// <see cref="NocturneGrid.HighlightForeground"/> (behind them <see cref="NocturneGrid.HighlightBrush"/>).
/// </summary>
internal sealed class GridCellText : TextBlock
{
    public static readonly StyledProperty<string?> CellTextProperty =
        AvaloniaProperty.Register<GridCellText, string?>(nameof(CellText));

    private readonly DataGridCell cell;
    private readonly NocturneGridColumn column;

    public GridCellText(DataGridCell cell, NocturneGridColumn column)
    {
        this.cell = cell;
        this.column = column;
        Bind(SearchVersionProperty, column.GetObservable(NocturneGridColumn.SearchSettingVersionProperty));
    }

    private static readonly StyledProperty<int> SearchVersionProperty =
        AvaloniaProperty.Register<GridCellText, int>("SearchVersion");

    protected override Type StyleKeyOverride => typeof(TextBlock);

    public string? CellText
    {
        get => GetValue(CellTextProperty);
        set => SetValue(CellTextProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == CellTextProperty || change.Property == NocturneGrid.SearchWordsProperty
            || change.Property == NocturneGrid.HighlightForegroundProperty || change.Property == NocturneGrid.HighlightBrushProperty
            || change.Property == SearchVersionProperty)
        {
            Update();
        }
    }

    private void Update()
    {
        var text = CellText ?? string.Empty;
        var hits = column.IsSearchable ? FindHits(text, NocturneGrid.GetSearchWords(this)) : null;
        cell.Classes.Set("searchHit", hits != null);
        if (hits == null)
        {
            if (Inlines is { Count: > 0 })
            {
                Inlines.Clear();
            }
            Text = text;
            return;
        }

        var foreground = NocturneGrid.GetHighlightForeground(this);
        var background = NocturneGrid.GetHighlightBrush(this);
        var inlines = new InlineCollection();
        var at = 0;
        foreach (var (start, length) in hits)
        {
            if (start > at)
            {
                inlines.Add(new Run(text[at..start]));
            }
            var hit = new Run(text.Substring(start, length));
            if (foreground != null)
            {
                hit.Foreground = foreground;
            }
            if (background != null)
            {
                hit.Background = background;
            }
            inlines.Add(hit);
            at = start + length;
        }
        if (at < text.Length)
        {
            inlines.Add(new Run(text[at..]));
        }
        Text = null;
        Inlines = inlines;
    }

    /// <summary>Every place a search word is found (ignoring case), merged where they overlap; null for none.</summary>
    private static List<(int Start, int Length)>? FindHits(string text, IReadOnlyList<string> words)
    {
        if (words.Count == 0 || text.Length == 0)
        {
            return null;
        }
        var marked = new bool[text.Length];
        var any = false;
        foreach (var word in words)
        {
            if (word.Length == 0)
            {
                continue;
            }
            for (var i = text.IndexOf(word, StringComparison.OrdinalIgnoreCase); i >= 0; i = text.IndexOf(word, i + 1, StringComparison.OrdinalIgnoreCase))
            {
                for (var j = i; j < i + word.Length && j < marked.Length; j++)
                {
                    marked[j] = true;
                }
                any = true;
            }
        }
        if (!any)
        {
            return null;
        }
        var hits = new List<(int, int)>();
        for (var i = 0; i < marked.Length; i++)
        {
            if (!marked[i])
            {
                continue;
            }
            var start = i;
            while (i < marked.Length && marked[i])
            {
                i++;
            }
            hits.Add((start, i - start));
        }
        return hits;
    }
}
