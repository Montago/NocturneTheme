using System.Windows;
using System.Windows.Controls;

namespace Frosted.Wpf.Helpers;

/// <summary>
/// Placeholder and reveal support for PasswordBox. The Frosted style turns
/// <see cref="IsMonitoringProperty"/> on; the template reads <see cref="HasPasswordProperty"/>
/// for the placeholder and shows <see cref="RevealedTextProperty"/> while the eye button is on.
/// </summary>
public static class PasswordBoxHelper
{
    public static readonly DependencyProperty IsMonitoringProperty = DependencyProperty.RegisterAttached(
        "IsMonitoring", typeof(bool), typeof(PasswordBoxHelper), new PropertyMetadata(false, OnIsMonitoringChanged));

    public static bool GetIsMonitoring(DependencyObject d) => (bool)d.GetValue(IsMonitoringProperty);

    public static void SetIsMonitoring(DependencyObject d, bool value) => d.SetValue(IsMonitoringProperty, value);

    private static readonly DependencyPropertyKey HasPasswordPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
        "HasPassword", typeof(bool), typeof(PasswordBoxHelper), new PropertyMetadata(false));

    public static readonly DependencyProperty HasPasswordProperty = HasPasswordPropertyKey.DependencyProperty;

    public static bool GetHasPassword(DependencyObject d) => (bool)d.GetValue(HasPasswordProperty);

    public static readonly DependencyProperty CanRevealProperty = DependencyProperty.RegisterAttached(
        "CanReveal", typeof(bool), typeof(PasswordBoxHelper), new PropertyMetadata(false));

    /// <summary>Shows an eye button that toggles the password between dots and plain text.</summary>
    public static bool GetCanReveal(DependencyObject d) => (bool)d.GetValue(CanRevealProperty);

    public static void SetCanReveal(DependencyObject d, bool value) => d.SetValue(CanRevealProperty, value);

    public static readonly DependencyProperty IsRevealedProperty = DependencyProperty.RegisterAttached(
        "IsRevealed", typeof(bool), typeof(PasswordBoxHelper),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsRevealedChanged));

    public static bool GetIsRevealed(DependencyObject d) => (bool)d.GetValue(IsRevealedProperty);

    public static void SetIsRevealed(DependencyObject d, bool value) => d.SetValue(IsRevealedProperty, value);

    private static readonly DependencyPropertyKey RevealedTextPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
        "RevealedText", typeof(string), typeof(PasswordBoxHelper), new PropertyMetadata(string.Empty));

    /// <summary>The password as text, filled only while <see cref="IsRevealedProperty"/> is true.</summary>
    public static readonly DependencyProperty RevealedTextProperty = RevealedTextPropertyKey.DependencyProperty;

    public static string GetRevealedText(DependencyObject d) => (string)d.GetValue(RevealedTextProperty);

    private static void OnIsMonitoringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PasswordBox box)
        {
            return;
        }

        box.PasswordChanged -= OnPasswordChanged;
        if ((bool)e.NewValue)
        {
            box.PasswordChanged += OnPasswordChanged;
            Update(box);
        }
    }

    private static void OnIsRevealedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PasswordBox box)
        {
            Update(box);
            if (!(bool)e.NewValue)
            {
                box.Focus();
            }
        }
    }

    private static void OnPasswordChanged(object sender, RoutedEventArgs e) => Update((PasswordBox)sender);

    private static void Update(PasswordBox box)
    {
        box.SetValue(HasPasswordPropertyKey, box.SecurePassword.Length > 0);
        box.SetValue(RevealedTextPropertyKey, GetIsRevealed(box) ? box.Password : string.Empty);
    }
}
