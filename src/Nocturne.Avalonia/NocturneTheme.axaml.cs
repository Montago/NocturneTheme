using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Nocturne.Avalonia;

/// <summary>
/// The Nocturne theme: brushes, fonts, icons and a ControlTheme for every control. Add it to
/// <c>Application.Styles</c>. It is a dark theme; the app's <c>RequestedThemeVariant</c> should be Dark.
/// <see cref="SetAccent"/> switches the accent at run time.
/// </summary>
public partial class NocturneTheme : Styles
{
    public NocturneTheme(IServiceProvider? serviceProvider = null)
    {
        AvaloniaXamlLoader.Load(serviceProvider, this);
    }
}
