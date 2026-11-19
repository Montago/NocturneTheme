using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Nocturne.Avalonia.Controls;

// The search and the column filters: reading a column's text without a cell, the filter values and ranges.
// The same rules as NocturneGrid in Nocturne.Wpf.
public partial class NocturneGrid
{
    private static bool Matches(object item, string[] words, ColumnReader[] searched, ColumnFilter[] filters)
    {
        foreach (var filter in filters)
        {
            if (!filter.Matches(item))
            {
                return false;
            }
        }
        if (words.Length == 0)
        {
            return true;
        }

        var texts = new string?[searched.Length];
        foreach (var word in words)
        {
            var found = false;
            for (var i = 0; i < searched.Length && !found; i++)
            {
                texts[i] ??= searched[i].GetText(item);
                found = texts[i]!.Contains(word, StringComparison.OrdinalIgnoreCase);
            }
            if (!found)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>Splits on spaces, commas and semicolons; double quotes keep a value with spaces together.</summary>
    internal static List<string> Tokenize(string? text) => Tokenize(text, joinRanges: false);

    /// <summary>The values of a header filter: as <see cref="Tokenize(string?)"/>, with "&gt; 5" and "2019 .. 2021" kept together.</summary>
    internal static List<string> FilterTokens(string? text) => Tokenize(text, joinRanges: true);

    private static List<string> Tokenize(string? text, bool joinRanges)
    {
        var tokens = new List<string>();
        if (string.IsNullOrWhiteSpace(text))
        {
            return tokens;
        }

        var current = new StringBuilder();
        var quoted = false;
        // An operator, or a value and "..", waiting for the value after it.
        string? pending = null;
        void Flush()
        {
            var token = current.ToString().Trim();
            current.Clear();
            if (token.Length == 0)
            {
                return;
            }
            if (joinRanges)
            {
                if (token == ".." && pending == null && tokens.Count > 0)
                {
                    pending = tokens[^1] + token;
                    tokens.RemoveAt(tokens.Count - 1);
                    return;
                }
                if (token is ">" or "<" or ">=" or "<=")
                {
                    pending += token;
                    return;
                }
                if (pending != null)
                {
                    token = pending + token;
                    pending = null;
                }
            }
            if (!tokens.Contains(token, StringComparer.OrdinalIgnoreCase))
            {
                tokens.Add(token);
            }
        }

        foreach (var ch in text)
        {
            if (ch == '"')
            {
                Flush();
                quoted = !quoted;
            }
            else if (!quoted && (char.IsWhiteSpace(ch) || ch == ',' || ch == ';'))
            {
                Flush();
            }
            else
            {
                current.Append(ch);
            }
        }
        Flush();
        if (pending != null && !tokens.Contains(pending, StringComparer.OrdinalIgnoreCase))
        {
            tokens.Add(pending);
        }
        return tokens;
    }

    // ---------------- Reading column values ----------------

    /// <summary>Reads a column's text for an item the way the cell shows it, without a cell.</summary>
    internal sealed class ColumnReader
    {
        private static readonly ConcurrentDictionary<(Type, string), PropertyInfo?> Properties = new();

        private readonly string[]? path;
        private readonly IValueConverter? converter;
        private readonly object? converterParameter;
        private readonly string? format;
        private readonly object? targetNullValue;
        private readonly CultureInfo culture;
        // A check box column's texts for true and false (NocturneGridCheckBoxColumn).
        private readonly (string True, string False)? checkTexts;

        private ColumnReader(string[]? path, BindingBase? binding, CultureInfo culture, DataGridColumn column)
        {
            this.path = path;
            var (conv, parameter, stringFormat, nullValue, bindingCulture) = binding switch
            {
                CompiledBinding c => (c.Converter, c.ConverterParameter, c.StringFormat, c.TargetNullValue, c.ConverterCulture),
                ReflectionBinding r => (r.Converter, r.ConverterParameter, r.StringFormat, r.TargetNullValue, r.ConverterCulture),
                _ => (null, null, null, null, null),
            };
            this.culture = bindingCulture ?? culture;
            converter = conv;
            converterParameter = parameter;
            targetNullValue = nullValue == AvaloniaProperty.UnsetValue ? null : nullValue;
            if (column is NocturneGridCheckBoxColumn check)
            {
                checkTexts = (check.TrueText, check.FalseText);
                return;
            }
            format = string.IsNullOrEmpty(stringFormat) ? null : stringFormat.Contains('{') ? stringFormat : "{0:" + stringFormat + "}";
        }

        /// <summary>The path of a binding to a member of the row's item; null for a binding to anything else.</summary>
        internal static string? PathOf(BindingBase binding) => binding switch
        {
            CompiledBinding { Source: null or UnsetValueType } c => c.Path?.ToString(),
            ReflectionBinding { Source: null or UnsetValueType, RelativeSource: null } r when string.IsNullOrEmpty(r.ElementName) => r.Path,
            _ => null,
        };

        public static ColumnReader? Create(DataGridColumn column, CultureInfo culture)
        {
            var memberPath = GetFilterMemberPath(column);
            if (!string.IsNullOrEmpty(memberPath))
            {
                return new ColumnReader(Split(memberPath), null, culture, column);
            }
            if (column is DataGridBoundColumn { Binding: { } binding } && PathOf(binding) is { } bindingPath && Split(bindingPath) is { } split)
            {
                return new ColumnReader(split, binding, culture, column);
            }
            return string.IsNullOrEmpty(column.SortMemberPath) ? null : new ColumnReader(Split(column.SortMemberPath), null, culture, column);
        }

        // An empty array reads the item itself; null means a path this reader does not follow (indexers, casts, attached properties).
        private static string[]? Split(string? memberPath)
        {
            if (string.IsNullOrEmpty(memberPath) || memberPath == ".")
            {
                return Array.Empty<string>();
            }
            return memberPath.IndexOfAny(['[', '(', '/', '$', '#', '^']) >= 0 ? null : memberPath.Split('.');
        }

        /// <summary>The value at the path, before the binding's converter and format (what the range filter compares).</summary>
        public object? GetRawValue(object item)
        {
            if (path == null)
            {
                return null;
            }
            object? value = item;
            foreach (var name in path)
            {
                if (value == null)
                {
                    break;
                }
                value = GetMember(value, name);
            }
            return value;
        }

        /// <summary>The value the cell gets: through the binding's converter, without its format.</summary>
        public object? GetValue(object item)
        {
            var value = GetRawValue(item);
            if (converter != null)
            {
                value = converter.Convert(value, checkTexts != null ? typeof(bool?) : typeof(string), converterParameter, culture);
            }
            if (value == null || value == AvaloniaProperty.UnsetValue || value is BindingNotification)
            {
                value = targetNullValue;
            }
            return value;
        }

        public string GetText(object item)
        {
            if (path == null)
            {
                return string.Empty;
            }

            var value = GetValue(item);
            if (checkTexts is var (yes, no))
            {
                return value switch { true => yes, false => no, _ => string.Empty };
            }
            if (value == null)
            {
                return string.Empty;
            }
            if (format != null)
            {
                return string.Format(culture, format, value);
            }
            return value is IFormattable formattable ? formattable.ToString(null, culture) : value.ToString() ?? string.Empty;
        }

        private static object? GetMember(object target, string name)
        {
            if (target is ICustomTypeDescriptor)
            {
                // DataRowView and other rows that describe their own columns.
                return TypeDescriptor.GetProperties(target)[name]?.GetValue(target);
            }
            var property = Properties.GetOrAdd((target.GetType(), name), static key =>
            {
                try
                {
                    return key.Item1.GetProperty(key.Item2, BindingFlags.Public | BindingFlags.Instance);
                }
                catch (AmbiguousMatchException)
                {
                    return key.Item1.GetProperties(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(p => p.Name == key.Item2);
                }
            });
            return property != null && property.GetIndexParameters().Length == 0 ? property.GetValue(target) : null;
        }
    }

    private sealed class ColumnFilter(ColumnReader reader, FilterValue[] values, IReadOnlyList<DateTime?>? dates)
    {
        private readonly bool hasRanges = values.Any(v => v.Range != null);
        // The ticked days of the date tree (FilterDates), and whether rows without a date pass; null for none.
        private readonly HashSet<DateTime>? days = dates is { Count: > 0 } ? dates.OfType<DateTime>().Select(d => d.Date).ToHashSet() : null;
        private readonly bool blankDays = dates?.Contains(null) == true;

        public bool Matches(object item)
        {
            var raw = hasRanges || days != null ? reader.GetRawValue(item) : null;
            if (days != null && !(GridDateFilterNode.TryGetDay(raw, out var day) ? days.Contains(day) : raw == null && blankDays))
            {
                return false;
            }
            if (values.Length == 0)
            {
                return true;
            }
            string? text = null;
            foreach (var value in values)
            {
                if (value.Range is { } range && range.TryMatch(raw, out var inRange))
                {
                    if (inRange)
                    {
                        return true;
                    }
                    continue;
                }
                text ??= reader.GetText(item).Trim();
                if (value.Matches(text))
                {
                    return true;
                }
            }
            return false;
        }
    }

    /// <summary>A filter value; <see cref="Range"/> is set for "&gt;5", "&lt;=2019" and "2020-01-01..2020-06-30".</summary>
    private readonly record struct FilterValue(string Text, bool AnyStart, bool AnyEnd, FilterRange? Range)
    {
        public static FilterValue Parse(string token, CultureInfo culture)
        {
            var anyStart = token.StartsWith('*');
            var anyEnd = token.EndsWith('*') && token.Length > 1;
            return new FilterValue(token.Trim('*'), anyStart, anyEnd, FilterRange.Parse(token, culture));
        }

        public bool Matches(string text) => (AnyStart, AnyEnd) switch
        {
            _ when Text.Length == 0 => text.Length > 0,
            (true, true) => text.Contains(Text, StringComparison.OrdinalIgnoreCase),
            (true, false) => text.EndsWith(Text, StringComparison.OrdinalIgnoreCase),
            (false, true) => text.StartsWith(Text, StringComparison.OrdinalIgnoreCase),
            _ => text.Equals(Text, StringComparison.OrdinalIgnoreCase),
        };
    }

    /// <summary>
    /// A range of numbers or dates, compared with the raw value of the column (not the text the cell shows).
    /// A date without a time stands for the whole day, and a year ("2019") or a month ("2019-03") for all
    /// of it, so "&lt;=2019" takes in 31 December 2019 and "&gt;2019" starts on 1 January 2020.
    /// </summary>
    private sealed class FilterRange
    {
        private readonly Bound? from;
        private readonly Bound? to;
        // "&gt;": above all of From (else from its start on); "&lt;": below the start of To (else up to its end).
        private readonly bool above;
        private readonly bool below;

        private FilterRange(Bound? from, bool above, Bound? to, bool below)
        {
            this.from = from;
            this.above = above;
            this.to = to;
            this.below = below;
        }

        public static FilterRange? Parse(string token, CultureInfo culture)
        {
            string? low = null, high = null;
            bool above = false, below = false;
            if (token.StartsWith(">=", StringComparison.Ordinal))
            {
                low = token[2..];
            }
            else if (token.StartsWith("<=", StringComparison.Ordinal))
            {
                high = token[2..];
            }
            else if (token.StartsWith('>'))
            {
                low = token[1..];
                above = true;
            }
            else if (token.StartsWith('<'))
            {
                high = token[1..];
                below = true;
            }
            else if (token.IndexOf("..", StringComparison.Ordinal) is var dots and >= 0)
            {
                low = token[..dots];
                high = token[(dots + 2)..];
            }
            else
            {
                return null;
            }

            low = low?.Trim();
            high = high?.Trim();
            if (string.IsNullOrEmpty(low) && string.IsNullOrEmpty(high))
            {
                return null;
            }
            Bound? from = null, to = null;
            if ((!string.IsNullOrEmpty(low) && (from = Bound.Parse(low, culture)) == null)
                || (!string.IsNullOrEmpty(high) && (to = Bound.Parse(high, culture)) == null))
            {
                return null;
            }
            return new FilterRange(from, above, to, below);
        }

        /// <summary>
        /// Whether a value is in the range. Returns false when the range does not apply to the value (text, or
        /// a bound that is no date for a date), which is then matched as text.
        /// </summary>
        public bool TryMatch(object? value, out bool result)
        {
            switch (value)
            {
                case null:
                    result = false;
                    return true;
                case DateTime date:
                    return TryMatchDate(date, out result);
                case DateTimeOffset offset:
                    return TryMatchDate(offset.DateTime, out result);
                case DateOnly day:
                    return TryMatchDate(day.ToDateTime(TimeOnly.MinValue), out result);
                case double or float:
                    var number = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                    return TryMatch(number, b => b.Number is { } n ? ((double)n, (double)n, true) : null, out result) && !double.IsNaN(number);
                case decimal or int or long or short or byte or sbyte or uint or ulong or ushort:
                    return TryMatch(Convert.ToDecimal(value, CultureInfo.InvariantCulture), b => b.Number is { } n ? (n, n, true) : null, out result);
                default:
                    result = false;
                    return false;
            }
        }

        private bool TryMatchDate(DateTime date, out bool result) =>
            TryMatch(date, b => b.Start is { } start ? (start, b.End ?? start, b.End == null) : null, out result);

        // Each bound as (Low, High, HighInclusive) in T: Low and High are equal for a number; a day, month or
        // year runs from Low up to High, exclusive. False when a bound cannot be read as T.
        private bool TryMatch<T>(T value, Func<Bound, (T Low, T High, bool HighInclusive)?> read, out bool result) where T : IComparable<T>
        {
            result = false;
            (T Low, T High, bool HighInclusive)? low = null, high = null;
            if ((from != null && (low = read(from)) == null) || (to != null && (high = read(to)) == null))
            {
                return false;
            }
            if (low is var (fromLow, fromHigh, fromInclusive))
            {
                var c = value.CompareTo(above ? fromHigh : fromLow);
                if (c < 0 || (above && c == 0 && fromInclusive))
                {
                    return true;
                }
            }
            if (high is var (toLow, toHigh, toInclusive))
            {
                var c = value.CompareTo(below ? toLow : toHigh);
                if (below ? c >= 0 : c > 0 || (c == 0 && !toInclusive))
                {
                    return true;
                }
            }
            result = true;
            return true;
        }

        /// <summary>One end of a range, read both as a number and as a date (whichever the column's values are).</summary>
        private sealed record Bound(decimal? Number, DateTime? Start, DateTime? End)
        {
            private static readonly string[] MonthFormats = ["yyyy-M", "yyyy-MM"];

            public static Bound? Parse(string text, CultureInfo culture)
            {
                decimal? number = decimal.TryParse(text, NumberStyles.Number, culture, out var n) ? n : null;
                DateTime? start = null, end = null;
                if (text.Length == 4 && int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var year) && year >= 1)
                {
                    start = new DateTime(year, 1, 1);
                    end = year < 9999 ? start.Value.AddYears(1) : DateTime.MaxValue;
                }
                else if (DateTime.TryParseExact(text, MonthFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var month))
                {
                    start = month;
                    end = month.AddMonths(1);
                }
                else if (DateTime.TryParse(text, culture, DateTimeStyles.AllowWhiteSpaces, out var date))
                {
                    start = date;
                    // Without a time of day, the whole day.
                    end = date.TimeOfDay == TimeSpan.Zero && text.IndexOf(':') < 0 && date < DateTime.MaxValue.Date ? date.AddDays(1) : null;
                }
                return number == null && start == null ? null : new Bound(number, start, end);
            }
        }
    }}
