using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace Nocturne.Wpf.Controls;

/// <summary>
/// Text column for <see cref="NocturneGrid"/>: shows its Binding like <see cref="DataGridTextColumn"/>
/// (and edits it the same way) and marks the words of the grid's search in the cell.
/// </summary>
public class NocturneGridColumn : DataGridTextColumn
{
    public static readonly DependencyProperty IsSearchableProperty = NocturneGrid.IsSearchableProperty.AddOwner(typeof(NocturneGridColumn));

    /// <summary>Whether the grid's search looks in this column and marks its hits.</summary>
    public bool IsSearchable
    {
        get => (bool)GetValue(IsSearchableProperty);
        set => SetValue(IsSearchableProperty, value);
    }

    public static readonly DependencyProperty IsFilterableProperty = NocturneGrid.IsFilterableProperty.AddOwner(typeof(NocturneGridColumn));

    /// <summary>Whether the header shows the filter button.</summary>
    public bool IsFilterable
    {
        get => (bool)GetValue(IsFilterableProperty);
        set => SetValue(IsFilterableProperty, value);
    }

    public static readonly DependencyProperty FilterMemberPathProperty = NocturneGrid.FilterMemberPathProperty.AddOwner(typeof(NocturneGridColumn));

    /// <summary>Member the search and filter read instead of the Binding.</summary>
    public string? FilterMemberPath
    {
        get => (string?)GetValue(FilterMemberPathProperty);
        set => SetValue(FilterMemberPathProperty, value);
    }

    public static readonly DependencyProperty FilterTextProperty = NocturneGrid.FilterTextProperty.AddOwner(typeof(NocturneGridColumn));

    /// <summary>Values this column must equal, separated by spaces or commas (see <see cref="NocturneGrid.GetFilterText"/>).</summary>
    public string FilterText
    {
        get => (string)GetValue(FilterTextProperty);
        set => SetValue(FilterTextProperty, value);
    }

    public static readonly DependencyProperty FilterDatesProperty = NocturneGrid.FilterDatesProperty.AddOwner(typeof(NocturneGridColumn));

    /// <summary>Days a column of dates must be one of, as the date tree in the header ticks them (see <see cref="NocturneGrid.GetFilterDates"/>).</summary>
    public IReadOnlyList<DateTime?>? FilterDates
    {
        get => (IReadOnlyList<DateTime?>?)GetValue(FilterDatesProperty);
        set => SetValue(FilterDatesProperty, value);
    }

    public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register(
        nameof(TextAlignment), typeof(TextAlignment), typeof(NocturneGridColumn),
        new PropertyMetadata(TextAlignment.Left, (d, _) => ((NocturneGridColumn)d).NotifyPropertyChanged(nameof(TextAlignment))));

    /// <summary>Alignment of the text in the cells (Right for numbers); use it rather than an ElementStyle, which turns off the drawn text.</summary>
    public TextAlignment TextAlignment
    {
        get => (TextAlignment)GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }

    // The cell's text, its column and the grid's search words; any change rebuilds the shown text.
    private static readonly DependencyProperty CellTextProperty = DependencyProperty.RegisterAttached(
        "CellText", typeof(string), typeof(NocturneGridColumn), new PropertyMetadata(null, OnCellChanged));

    // Set on the grid and inherited by every cell, so the cells need no binding to the grid.
    internal static readonly DependencyProperty WordsProperty = DependencyProperty.RegisterAttached(
        "Words", typeof(IReadOnlyList<string>), typeof(NocturneGridColumn), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, OnCellChanged));

    private static readonly DependencyProperty ColumnProperty = DependencyProperty.RegisterAttached(
        "Column", typeof(NocturneGridColumn), typeof(NocturneGridColumn), new PropertyMetadata(null));

    // The text a cell shows in the thumb-drag preview (ShowPreview), until RestoreText.
    private static readonly DependencyProperty PreviewTextProperty = DependencyProperty.RegisterAttached(
        "PreviewText", typeof(string), typeof(NocturneGridColumn), new PropertyMetadata(null));

    private static readonly DependencyProperty IsMarkedProperty =DependencyProperty.RegisterAttached(
        "IsMarked", typeof(bool), typeof(NocturneGridColumn), new PropertyMetadata(false));

    /// <summary>The grid's <see cref="NocturneGrid.EnableLightweightTextRendering"/> changed: the cells get a new element.</summary>
    internal void OnRenderingChanged() => NotifyPropertyChanged(nameof(NocturneGrid.EnableLightweightTextRendering));

    protected override void RefreshCellContent(FrameworkElement element, string propertyName)
    {
        if (element is DataGridCell { IsEditing: false } cell
            && (propertyName == nameof(NocturneGrid.EnableLightweightTextRendering)
                || (cell.Content is GridCellText && propertyName is nameof(FontFamily) or nameof(FontSize) or nameof(FontStyle) or nameof(FontWeight) or nameof(Foreground))))
        {
            cell.Content = GenerateElement(cell, cell.DataContext);
            return;
        }
        if (element is DataGridCell { Content: GridCellText or TextBlock } shown && propertyName == nameof(TextAlignment))
        {
            if (shown.Content is GridCellText text)
            {
                text.TextAlignment = TextAlignment;
            }
            else
            {
                ((TextBlock)shown.Content).TextAlignment = TextAlignment;
            }
            return;
        }
        base.RefreshCellContent(element, propertyName);
    }

    /// <summary>
    /// Copying puts the text the cell shows on the clipboard, with the binding's format (09/20/2015, 183,140,
    /// 145 hp), rather than the bound value as .NET writes it (a date with midnight as its time). A column with
    /// its own <see cref="DataGridColumn.ClipboardContentBinding"/> copies that instead, and so does a
    /// <see cref="DataGridColumn.CopyingCellClipboardContent"/> handler that changes the content.
    /// </summary>
    public override object? OnCopyingCellClipboardContent(object item)
    {
        var content = base.OnCopyingCellClipboardContent(item);
        if (ReferenceEquals(ClipboardContentBinding, Binding) && DataGridOwner is { } grid
            && NocturneGrid.ColumnReader.ForCell(this, grid.Language.GetSpecificCulture()) is { } reader
            && Equals(content, reader.GetValue(item)))
        {
            return reader.GetText(item);
        }
        return content;
    }

    protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
    {
        // One element and one binding (the text) per cell. The search words come down from the grid by inheritance.
        FrameworkElement element;
        if (DataGridOwner is NocturneGrid { EnableLightweightTextRendering: true, FlowDirection: FlowDirection.LeftToRight }
            && (ElementStyle == null || ElementStyle == DefaultElementStyle))
        {
            // The margin of the DataGrid's default element style, so the text sits where a TextBlock's would.
            element = new GridCellText { TextAlignment = TextAlignment, Margin = new Thickness(2, 0, 2, 0) };
            CopyIfSet(FontFamilyProperty, TextElement.FontFamilyProperty, element);
            CopyIfSet(FontSizeProperty, TextElement.FontSizeProperty, element);
            CopyIfSet(FontStyleProperty, TextElement.FontStyleProperty, element);
            CopyIfSet(FontWeightProperty, TextElement.FontWeightProperty, element);
            CopyIfSet(ForegroundProperty, TextElement.ForegroundProperty, element);
        }
        else
        {
            // The base element carries the ElementStyle and the column's font settings; only its text is ours.
            var block = (TextBlock)base.GenerateElement(cell, dataItem);
            BindingOperations.ClearBinding(block, TextBlock.TextProperty);
            block.TextAlignment = TextAlignment;
            element = block;
        }
        element.SetValue(ColumnProperty, this);
        if (Binding != null)
        {
            BindingOperations.SetBinding(element, CellTextProperty, Binding);
        }
        return element;
    }

    private void CopyIfSet(DependencyProperty columnProperty, DependencyProperty elementProperty, FrameworkElement element)
    {
        if (ReadLocalValue(columnProperty) != DependencyProperty.UnsetValue)
        {
            element.SetValue(elementProperty, GetValue(columnProperty));
        }
    }

    private static void OnCellChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FrameworkElement element && element.GetValue(ColumnProperty) is NocturneGridColumn column)
        {
            column.UpdateText(element);
        }
    }

    /// <summary><see cref="NocturneGrid.HighlightForegroundProperty"/> changed on a cell's element (an accent change, a row selected).</summary>
    internal static void OnHighlightForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FrameworkElement element && element.GetValue(ColumnProperty) is NocturneGridColumn column
            && element.GetValue(WordsProperty) is IReadOnlyList<string> { Count: > 0 })
        {
            // A cell in the thumb-drag preview keeps the previewed text.
            if (element.GetValue(PreviewTextProperty) is string preview)
            {
                column.ShowText(element, preview, measure: false);
            }
            else
            {
                column.UpdateText(element);
            }
        }
    }

    /// <summary>
    /// Shows another row's text in the cell without its item or binding (the grid's thumb-drag
    /// preview); a drawn cell only draws it again. False for an editing cell.
    /// </summary>
    internal bool ShowPreview(FrameworkElement element, string text)
    {
        if (element is not (GridCellText or TextBlock) || element.GetValue(ColumnProperty) != this)
        {
            return false;
        }
        element.SetValue(PreviewTextProperty, text);
        ShowText(element, text, measure: false);
        return true;
    }

    /// <summary>Shows the cell's own text again, after <see cref="ShowPreview"/>.</summary>
    internal void RestoreText(FrameworkElement element)
    {
        if (element.GetValue(ColumnProperty) == this)
        {
            element.ClearValue(PreviewTextProperty);
            UpdateText(element);
        }
    }

    private void UpdateText(FrameworkElement element) => ShowText(element, (string?)element.GetValue(CellTextProperty) ?? string.Empty, measure: true);

    private void ShowText(FrameworkElement element, string text, bool measure)
    {
        var grid = DataGridOwner as NocturneGrid;
        var words = element.GetValue(WordsProperty) as IReadOnlyList<string>;
        var hits = words is { Count: > 0 } && IsSearchable ? FindHits(text, words) : null;
        var hitForeground = hits == null ? null : NocturneGrid.GetHighlightForeground(element);
        if (element.Parent is DataGridCell cell && NocturneGrid.GetIsSearchHit(cell) != (hits != null))
        {
            NocturneGrid.SetIsSearchHit(cell, hits != null);
        }

        if (element is GridCellText cellText)
        {
            cellText.SetText(text, hits, grid?.HighlightBrush, hitForeground, measure);
            return;
        }
        var block = (TextBlock)element;
        if (hits == null)
        {
            if ((bool)block.GetValue(IsMarkedProperty))
            {
                // Text may still hold the same string from before the runs, so setting it alone would not replace them.
                block.ClearValue(IsMarkedProperty);
                block.ClearValue(TextBlock.TextProperty);
            }
            block.Text = text;
            return;
        }

        block.SetValue(IsMarkedProperty, true);
        block.Inlines.Clear();
        var position = 0;
        foreach (var (start, end) in hits)
        {
            if (start > position)
            {
                block.Inlines.Add(new Run(text[position..start]));
            }
            var run = new Run(text[start..end]);
            if (grid?.HighlightBrush is { } background)
            {
                run.Background = background;
            }
            if (hitForeground != null)
            {
                run.Foreground = hitForeground;
            }
            block.Inlines.Add(run);
            position = end;
        }
        if (position < text.Length)
        {
            block.Inlines.Add(new Run(text[position..]));
        }
    }

    /// <summary>Ranges (start, end) of every occurrence of the words, ignoring case, merged where they overlap.</summary>
    private static List<(int Start, int End)>? FindHits(string text, IReadOnlyList<string> words)
    {
        List<(int Start, int End)>? hits = null;
        foreach (var word in words)
        {
            for (var index = text.IndexOf(word, StringComparison.OrdinalIgnoreCase); index >= 0;
                 index = text.IndexOf(word, index + word.Length, StringComparison.OrdinalIgnoreCase))
            {
                (hits ??= new()).Add((index, index + word.Length));
            }
        }
        if (hits == null)
        {
            return null;
        }

        hits.Sort();
        var merged = new List<(int Start, int End)> { hits[0] };
        foreach (var hit in hits.Skip(1))
        {
            var last = merged[^1];
            if (hit.Start <= last.End)
            {
                merged[^1] = (last.Start, Math.Max(last.End, hit.End));
            }
            else
            {
                merged.Add(hit);
            }
        }
        return merged;
    }
}
