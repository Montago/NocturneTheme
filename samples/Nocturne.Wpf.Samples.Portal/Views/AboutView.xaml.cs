using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;
using Nocturne.Wpf.Controls;

namespace Nocturne.Wpf.Samples.Portal.Views;

public partial class AboutView : UserControl
{
    public AboutView()
    {
        InitializeComponent();
        Version.Text = $"Built with Nocturne.Wpf {typeof(NocturneWindow).Assembly.GetName().Version?.ToString(3)}";
    }

    private void OnRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }
}
