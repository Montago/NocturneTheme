using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace Nocturne.Avalonia.Controls;

/// <summary>
/// The wheel scrolls by rows, as the WPF NocturneGrid does (ScrollUnit Item): a notch moves Windows' "lines
/// per notch" rows (3 elsewhere) and leaves a row's top edge at the top of the rows. Avalonia's DataGrid
/// scrolls 50 pixels a notch, so rows stop part-way; the grid hands it a wheel turn of the exact distance
/// to the row edge instead. Turns from a touchpad add up until they make a whole row. A grouped grid
/// (headers of their own height), shown row details, Shift and sideways turns keep the DataGrid's own
/// scrolling. Turns stay in the grid at its ends; a grid with no rows to scroll past passes them on.
/// </summary>
public partial class NocturneGrid
{
    // Avalonia's DataGrid scrolls Delta.Y times this many pixels (DATAGRID_mouseWheelDelta).
    private const double DataGridWheelPixels = 50;

    private ScrollBar? verticalBar;
    private double wheelRows;

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        if (e.Handled || e.Delta.Y == 0 || e.Delta.X != 0 || e.KeyModifiers != KeyModifiers.None)
        {
            base.OnPointerWheelChanged(e);
            return;
        }
        if (verticalBar is not { IsVisible: true } bar || bar.Maximum <= 0)
        {
            // No rows to scroll past: the turn goes on to the page, as in the theme's scroll viewers (the
            // DataGrid would keep it, since the theme turns its scroll chaining off).
            return;
        }
        var rowHeight = WheelRowHeight();
        if (rowHeight <= 0)
        {
            base.OnPointerWheelChanged(e);
            return;
        }

        var offset = bar.Value;
        var up = e.Delta.Y > 0;
        if (up ? offset <= 0 : offset >= bar.Maximum)
        {
            // Nothing left that way: the DataGrid decides whether the turn goes on to an outer scroll viewer.
            wheelRows = 0;
            base.OnPointerWheelChanged(e);
            return;
        }

        if (wheelRows != 0 && wheelRows > 0 != up)
        {
            wheelRows = 0;
        }
        wheelRows += e.Delta.Y * WheelLines(bar, rowHeight);
        var rows = (int)Math.Truncate(wheelRows);
        e.Handled = true;
        if (rows == 0)
        {
            return;
        }
        wheelRows -= rows;

        // The first row that shows (at least partly), moved by whole rows: its top edge lands at the top.
        var first = Math.Floor(offset / rowHeight + 0.01);
        var target = Math.Clamp((first - rows) * rowHeight, 0, bar.Maximum);
        var distance = target - offset;
        if (Math.Abs(distance) < 0.5)
        {
            return;
        }
        var root = TopLevel.GetTopLevel(this) as Visual ?? this;
        base.OnPointerWheelChanged(new PointerWheelEventArgs(e.Source ?? this, e.Pointer, root, e.GetPosition(root), e.Timestamp,
            e.Properties, e.KeyModifiers, new Vector(0, -distance / DataGridWheelPixels))
        {
            RoutedEvent = PointerWheelChangedEvent,
        });
    }

    /// <summary>The height of a row when every row has the same one, else 0 (the DataGrid's own scrolling then).</summary>
    private double WheelRowHeight()
    {
        if (view?.GroupDescriptions.Count > 0
            || (RowDetailsTemplate != null && RowDetailsVisibilityMode != DataGridRowDetailsVisibilityMode.Collapsed))
        {
            return 0;
        }
        return double.IsNaN(RowHeight) ? 0 : RowHeight;
    }

    /// <summary>Rows a notch moves: Windows' wheel setting (a page when it says "one screen"), else 3.</summary>
    private static double WheelLines(ScrollBar bar, double rowHeight)
    {
        var lines = OperatingSystem.IsWindows() ? NativeMethods.WheelScrollLines() : 3;
        return lines < 0 ? Math.Max(1, Math.Floor(bar.ViewportSize / rowHeight)) : lines;
    }
}
