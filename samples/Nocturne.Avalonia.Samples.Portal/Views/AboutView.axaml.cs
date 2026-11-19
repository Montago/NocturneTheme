using Avalonia.Controls;
using Nocturne.Avalonia.Controls;

namespace Nocturne.Avalonia.Samples.Portal.Views;

public partial class AboutView : UserControl
{
    public AboutView()
    {
        InitializeComponent();
        Version.Text = $"Built with Nocturne.Avalonia {typeof(NocturneWindow).Assembly.GetName().Version?.ToString(3)}";
    }
}
