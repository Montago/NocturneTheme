using Avalonia;
using Avalonia.Controls;

namespace Nocturne.Avalonia.Controls;

/// <summary>
/// Check box column for <see cref="NocturneGrid"/>. The header filter and the search read its values as
/// <see cref="TrueText"/> and <see cref="FalseText"/> ("Yes" and "No"); the search skips it unless
/// <see cref="IsSearchable"/> is set.
/// </summary>
public class NocturneGridCheckBoxColumn : DataGridCheckBoxColumn
{
    public static readonly AttachedProperty<bool> IsSearchableProperty = NocturneGrid.IsSearchableProperty.AddOwner<NocturneGridCheckBoxColumn>();
    public static readonly AttachedProperty<bool> IsFilterableProperty = NocturneGrid.IsFilterableProperty.AddOwner<NocturneGridCheckBoxColumn>();
    public static readonly AttachedProperty<string?> FilterMemberPathProperty = NocturneGrid.FilterMemberPathProperty.AddOwner<NocturneGridCheckBoxColumn>();
    public static readonly AttachedProperty<string> FilterTextProperty = NocturneGrid.FilterTextProperty.AddOwner<NocturneGridCheckBoxColumn>();

    public static readonly StyledProperty<string> TrueTextProperty =
        AvaloniaProperty.Register<NocturneGridCheckBoxColumn, string>(nameof(TrueText), "Yes");

    public static readonly StyledProperty<string> FalseTextProperty =
        AvaloniaProperty.Register<NocturneGridCheckBoxColumn, string>(nameof(FalseText), "No");

    static NocturneGridCheckBoxColumn()
    {
        IsSearchableProperty.OverrideDefaultValue<NocturneGridCheckBoxColumn>(false);
    }

    /// <summary>Whether the grid's search looks in this column (for <see cref="TrueText"/> and <see cref="FalseText"/>); off by default.</summary>
    public bool IsSearchable
    {
        get => GetValue(IsSearchableProperty);
        set => SetValue(IsSearchableProperty, value);
    }

    /// <summary>Whether the header shows the filter button.</summary>
    public bool IsFilterable
    {
        get => GetValue(IsFilterableProperty);
        set => SetValue(IsFilterableProperty, value);
    }

    /// <summary>Member the search and filter read instead of the Binding.</summary>
    public string? FilterMemberPath
    {
        get => GetValue(FilterMemberPathProperty);
        set => SetValue(FilterMemberPathProperty, value);
    }

    /// <summary>Values this column must equal, such as "yes" (see <see cref="NocturneGrid.FilterTextProperty"/>).</summary>
    public string FilterText
    {
        get => GetValue(FilterTextProperty);
        set => SetValue(FilterTextProperty, value);
    }

    /// <summary>The text of a ticked cell in the header filter and the search.</summary>
    public string TrueText
    {
        get => GetValue(TrueTextProperty);
        set => SetValue(TrueTextProperty, value);
    }

    /// <summary>The text of a cell that is not ticked in the header filter and the search.</summary>
    public string FalseText
    {
        get => GetValue(FalseTextProperty);
        set => SetValue(FalseTextProperty, value);
    }
}
