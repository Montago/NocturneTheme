using System.Globalization;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;

namespace Nocturne.Avalonia.Demo;

/// <summary>
/// <c>--render &lt;snippet.axaml&gt; &lt;out.png&gt; [--scale 1]</c>: draws one XAML element on the
/// window ground with 16px around it, in about a second. The snippet needs no xmlns: the default
/// Avalonia namespace, <c>x:</c> and <c>nc:</c> are declared for it. Errors go to the console and to
/// <c>&lt;out&gt;.error.txt</c>.
/// </summary>
internal static partial class RenderCommand
{
    public static int Run(string[] args)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine("Usage: --render <snippet.axaml> <out.png> [--scale 1]");
            return 2;
        }

        var output = Path.GetFullPath(args[2]);
        var errorFile = Path.ChangeExtension(output, ".error.txt");
        File.Delete(errorFile);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(output)!);
            var scale = double.Parse(Headless.Option(args, "--scale") ?? "1", CultureInfo.InvariantCulture);
            var snippet = Load(File.ReadAllText(args[1]));

            var window = new Window
            {
                SizeToContent = SizeToContent.WidthAndHeight,
                Content = new Border
                {
                    Padding = new(16),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Child = snippet,
                },
            };
            window.SetRenderScaling(scale);
            window.Show();
            Headless.Capture(window).Save(output, PngBitmapEncoderOptions.Default);
            window.Close();
            Console.WriteLine(output);
            return 0;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            File.WriteAllText(errorFile, e.ToString());
            return 1;
        }
    }

    /// <summary>
    /// <c>--render-gallery &lt;Tag,…|all&gt; &lt;folder&gt; [--scale 1]</c>: draws chosen samples of the
    /// gallery (Views/GalleryPage.axaml) to <c>&lt;folder&gt;/&lt;Tag&gt;.png</c>, popups open as marked.
    /// </summary>
    public static int RunGallery(string[] args)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine("Usage: --render-gallery <Tag,...|all> <folder> [--scale 1]");
            return 2;
        }
        try
        {
            var scale = double.Parse(Headless.Option(args, "--scale") ?? "1", CultureInfo.InvariantCulture);
            var only = args[1] == "all" ? null : args[1].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            GallerySnapshot.Save(Path.GetFullPath(args[2]), only, scale);
            return 0;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            return 1;
        }
    }

    /// <summary>Adds the namespaces to the snippet's root element and loads it.</summary>
    private static Control Load(string xaml)
    {
        const string Namespaces =
            " xmlns=\"https://github.com/avaloniaui\"" +
            " xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"" +
            " xmlns:nc=\"urn:nocturne-avalonia\"";
        var root = RootElement().Match(xaml);
        if (!root.Success)
            throw new FormatException("The snippet has no root element.");
        xaml = xaml.Insert(root.Index + root.Length, Namespaces);
        return AvaloniaRuntimeXamlLoader.Parse<Control>(xaml, typeof(RenderCommand).Assembly);
    }

    [GeneratedRegex(@"<(?![?!])[\w.:]+")]
    private static partial Regex RootElement();
}
