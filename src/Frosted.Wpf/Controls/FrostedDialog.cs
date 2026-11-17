using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Frosted.Wpf.Controls;

public enum MessageDialogResult
{
    /// <summary>Closed with Escape (or <see cref="FrostedDialog.Close"/>) without choosing.</summary>
    Canceled,

    /// <summary>The main button (<see cref="FrostedDialog.AffirmativeText"/>).</summary>
    Affirmative,

    /// <summary>The second button (<see cref="FrostedDialog.NegativeText"/>).</summary>
    Negative,
}

/// <summary>
/// A dialog inside the window: a glass card over a dimmed page, with a title, the content and
/// its buttons at the bottom right. Show it with <see cref="FrostedWindow.ShowDialogAsync"/>
/// (or <see cref="FrostedWindow.ShowMessageAsync"/> for a plain message); Escape cancels it.
/// </summary>
[TemplatePart(Name = PartAffirmative, Type = typeof(Button))]
[TemplatePart(Name = PartNegative, Type = typeof(Button))]
public class FrostedDialog : ContentControl
{
    private const string PartAffirmative = "PART_Affirmative";
    private const string PartNegative = "PART_Negative";

    private TaskCompletionSource<MessageDialogResult>? completion;
    private Button? affirmative;
    private Button? negative;

    static FrostedDialog()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(FrostedDialog), new FrameworkPropertyMetadata(typeof(FrostedDialog)));
    }

    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(FrostedDialog), new PropertyMetadata(string.Empty));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty AffirmativeTextProperty = DependencyProperty.Register(
        nameof(AffirmativeText), typeof(string), typeof(FrostedDialog), new PropertyMetadata("OK"));

    /// <summary>Text of the main (primary) button.</summary>
    public string AffirmativeText
    {
        get => (string)GetValue(AffirmativeTextProperty);
        set => SetValue(AffirmativeTextProperty, value);
    }

    public static readonly DependencyProperty NegativeTextProperty = DependencyProperty.Register(
        nameof(NegativeText), typeof(string), typeof(FrostedDialog), new PropertyMetadata(null));

    /// <summary>Text of the second button; the button is hidden while this is null.</summary>
    public string? NegativeText
    {
        get => (string?)GetValue(NegativeTextProperty);
        set => SetValue(NegativeTextProperty, value);
    }

    public static readonly DependencyProperty AffirmativeButtonStyleProperty = DependencyProperty.Register(
        nameof(AffirmativeButtonStyle), typeof(Style), typeof(FrostedDialog), new PropertyMetadata(null, (d, _) => ((FrostedDialog)d).ApplyAffirmativeStyle()));

    /// <summary>Style of the main button (Frosted.Styles.Button.Primary when null).</summary>
    public Style? AffirmativeButtonStyle
    {
        get => (Style?)GetValue(AffirmativeButtonStyleProperty);
        set => SetValue(AffirmativeButtonStyleProperty, value);
    }

    public override void OnApplyTemplate()
    {
        if (affirmative != null)
        {
            affirmative.Click -= OnAffirmativeClick;
        }
        if (negative != null)
        {
            negative.Click -= OnNegativeClick;
        }

        base.OnApplyTemplate();

        affirmative = GetTemplateChild(PartAffirmative) as Button;
        negative = GetTemplateChild(PartNegative) as Button;
        if (affirmative != null)
        {
            affirmative.Click += OnAffirmativeClick;
            ApplyAffirmativeStyle();
            Dispatcher.BeginInvoke(() => affirmative?.Focus(), DispatcherPriority.Input);
        }
        if (negative != null)
        {
            negative.Click += OnNegativeClick;
        }
    }

    private void ApplyAffirmativeStyle()
    {
        if (affirmative != null)
        {
            affirmative.Style = AffirmativeButtonStyle ?? TryFindResource("Frosted.Styles.Button.Primary") as Style;
        }
    }

    private void OnAffirmativeClick(object sender, RoutedEventArgs e) => Close(MessageDialogResult.Affirmative);

    private void OnNegativeClick(object sender, RoutedEventArgs e) => Close(MessageDialogResult.Negative);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Escape)
        {
            Close(MessageDialogResult.Canceled);
            e.Handled = true;
        }
    }

    internal Task<MessageDialogResult> WaitForResultAsync()
    {
        completion = new TaskCompletionSource<MessageDialogResult>();
        return completion.Task;
    }

    /// <summary>Closes the dialog with the given result.</summary>
    public void Close(MessageDialogResult result) => completion?.TrySetResult(result);
}
