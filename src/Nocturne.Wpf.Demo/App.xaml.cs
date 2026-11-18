using System.Globalization;
using System.Windows;
using System.Windows.Markup;

namespace Nocturne.Wpf.Demo;

public partial class App : Application
{
    public App()
    {
        // The demo is in English; format numbers and dates the same way everywhere.
        var culture = CultureInfo.GetCultureInfo("en-US");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
        FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
            new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(culture.IetfLanguageTag)));
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        // --render / --render-gallery: pictures of one control, no demo window (RenderCommand.cs)
        if (!RenderCommand.TryStart(this, e.Args))
        {
            StartupUri = new Uri("MainWindow.xaml", UriKind.Relative);
        }
    }
}
