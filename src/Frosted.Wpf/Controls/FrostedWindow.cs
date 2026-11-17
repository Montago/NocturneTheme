using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;
using System.Windows.Threading;

namespace Frosted.Wpf.Controls;

/// <summary>The Windows 11 material drawn behind a <see cref="FrostedWindow"/>.</summary>
public enum WindowBackdrop
{
    /// <summary>No system material: the window paints an opaque dark ground.</summary>
    None,

    /// <summary>Mica: a soft tint taken from the wallpaper.</summary>
    Mica,

    /// <summary>Mica Alt: a stronger wallpaper tint, as in tabbed apps.</summary>
    MicaAlt,

    /// <summary>Acrylic: the windows and desktop behind it, blurred (the default).</summary>
    Acrylic,
}

/// <summary>
/// Window with the Frosted Glass frame: a Windows 11 backdrop (Acrylic by default, or Mica / Mica Alt),
/// a dark tint and a soft light from the top-left corner, under a 44px
/// title bar with its own caption buttons. Where the backdrop is not available (Windows 10,
/// transparency effects off) it paints an opaque ground of the same colour.
/// </summary>
public class FrostedWindow : Window
{
    private const string PartRoot = "PART_Root";
    private const string PartLight = "PART_Light";

    private FrameworkElement? root;
    private Image? light;
    private DispatcherTimer? lightTimer;
    private Color? surfaceColor;
    private bool frameExtended;
    private bool transparencyEnabled = true;

    static FrostedWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(FrostedWindow), new FrameworkPropertyMetadata(typeof(FrostedWindow)));
    }

    public FrostedWindow()
    {
        WindowChrome.SetWindowChrome(this, new WindowChrome
        {
            CaptionHeight = 44,
            ResizeBorderThickness = new Thickness(6),
            GlassFrameThickness = new Thickness(0),
            CornerRadius = new CornerRadius(0),
            UseAeroCaptionButtons = false,
        });

        CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, (_, _) => SystemCommands.MinimizeWindow(this)));
        CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, (_, _) => SystemCommands.MaximizeWindow(this)));
        CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, (_, _) => SystemCommands.RestoreWindow(this)));
        CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, (_, _) => SystemCommands.CloseWindow(this)));
    }

    public static readonly DependencyProperty BackdropProperty = DependencyProperty.Register(
        nameof(Backdrop), typeof(WindowBackdrop), typeof(FrostedWindow),
        new PropertyMetadata(WindowBackdrop.Acrylic, (d, _) => ((FrostedWindow)d).ApplyBackdrop()));

    /// <summary>The system material behind the window. Needs Windows 11 22H2 with transparency effects on.</summary>
    public WindowBackdrop Backdrop
    {
        get => (WindowBackdrop)GetValue(BackdropProperty);
        set => SetValue(BackdropProperty, value);
    }

    private static readonly DependencyPropertyKey IsBackdropActivePropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(IsBackdropActive), typeof(bool), typeof(FrostedWindow), new PropertyMetadata(false));

    public static readonly DependencyProperty IsBackdropActiveProperty = IsBackdropActivePropertyKey.DependencyProperty;

    /// <summary>True while Windows draws the backdrop, so the template leaves out its opaque ground.</summary>
    public bool IsBackdropActive => (bool)GetValue(IsBackdropActiveProperty);

    public static readonly DependencyProperty TintOpacityProperty = DependencyProperty.Register(
        nameof(TintOpacity), typeof(double), typeof(FrostedWindow), new PropertyMetadata(0.3));

    /// <summary>How strongly the dark tint (<c>Frosted.Brushes.Window.Tint</c>) covers the backdrop.</summary>
    public double TintOpacity
    {
        get => (double)GetValue(TintOpacityProperty);
        set => SetValue(TintOpacityProperty, value);
    }

    public static readonly DependencyProperty TitleBarHeightProperty = DependencyProperty.Register(
        nameof(TitleBarHeight), typeof(double), typeof(FrostedWindow), new PropertyMetadata(44d, OnTitleBarHeightChanged));

    public double TitleBarHeight
    {
        get => (double)GetValue(TitleBarHeightProperty);
        set => SetValue(TitleBarHeightProperty, value);
    }

    private static void OnTitleBarHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (WindowChrome.GetWindowChrome((Window)d) is { } chrome)
        {
            chrome.CaptionHeight = (double)e.NewValue;
        }
    }

    public static readonly DependencyProperty ShowIconProperty = DependencyProperty.Register(
        nameof(ShowIcon), typeof(bool), typeof(FrostedWindow), new PropertyMetadata(true));

    /// <summary>Shows <see cref="Window.Icon"/> (or a small accent glyph when none is set) before the title.</summary>
    public bool ShowIcon
    {
        get => (bool)GetValue(ShowIconProperty);
        set => SetValue(ShowIconProperty, value);
    }

    public static readonly DependencyProperty ShowMinButtonProperty = DependencyProperty.Register(
        nameof(ShowMinButton), typeof(bool), typeof(FrostedWindow), new PropertyMetadata(true));

    public bool ShowMinButton
    {
        get => (bool)GetValue(ShowMinButtonProperty);
        set => SetValue(ShowMinButtonProperty, value);
    }

    public static readonly DependencyProperty ShowMaxRestoreButtonProperty = DependencyProperty.Register(
        nameof(ShowMaxRestoreButton), typeof(bool), typeof(FrostedWindow), new PropertyMetadata(true));

    public bool ShowMaxRestoreButton
    {
        get => (bool)GetValue(ShowMaxRestoreButtonProperty);
        set => SetValue(ShowMaxRestoreButtonProperty, value);
    }

    public static readonly DependencyProperty RightWindowCommandsProperty = DependencyProperty.Register(
        nameof(RightWindowCommands), typeof(object), typeof(FrostedWindow), new PropertyMetadata(null));

    /// <summary>Content shown in the title bar before the caption buttons.</summary>
    public object? RightWindowCommands
    {
        get => GetValue(RightWindowCommandsProperty);
        set => SetValue(RightWindowCommandsProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        if (root != null)
        {
            root.SizeChanged -= OnRootSizeChanged;
        }
        root = GetTemplateChild(PartRoot) as FrameworkElement;
        light = GetTemplateChild(PartLight) as Image;
        if (root != null)
        {
            root.SizeChanged += OnRootSizeChanged;
        }
        UpdateMaximizedMargin();
        UpdateLight();
    }

    protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
    {
        base.OnDpiChanged(oldDpi, newDpi);
        UpdateLight();
    }

    private void OnRootSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (light == null)
        {
            return;
        }

        // While the window is being resized, stretch the light drawn for the old size and draw a
        // new one once the size has settled for a moment.
        var region = LightTexture.Region(e.NewSize.Width, e.NewSize.Height);
        light.Width = region.Width;
        light.Height = region.Height;
        lightTimer ??= new DispatcherTimer(TimeSpan.FromMilliseconds(150), DispatcherPriority.Background, (_, _) =>
        {
            lightTimer!.Stop();
            UpdateLight();
        }, Dispatcher);
        lightTimer.Stop();
        lightTimer.Start();
    }

    /// <summary>
    /// Draws the light from the top-left corner as a dithered bitmap for the current size and
    /// display scaling, placed pixel for pixel (a gradient brush shows diagonal bands).
    /// </summary>
    private void UpdateLight()
    {
        if (light == null || root == null || root.ActualWidth <= 0 || root.ActualHeight <= 0)
        {
            return;
        }

        var dpi = VisualTreeHelper.GetDpi(this);
        var bitmap = LightTexture.Create(
            (int)Math.Round(root.ActualWidth * dpi.DpiScaleX),
            (int)Math.Round(root.ActualHeight * dpi.DpiScaleY));
        light.Source = bitmap;
        light.Width = bitmap.PixelWidth / dpi.DpiScaleX;
        light.Height = bitmap.PixelHeight / dpi.DpiScaleY;
        RenderOptions.SetBitmapScalingMode(light, BitmapScalingMode.NearestNeighbor);
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        var hwnd = new WindowInteropHelper(this).Handle;
        HwndSource.FromHwnd(hwnd)?.AddHook(WndProc);

        // The theme is always dark: dark system frame and dark materials. Rounded corners on Windows 11.
        NativeMethods.SetDwmAttribute(hwnd, NativeMethods.DwmwaUseImmersiveDarkMode, 1);
        NativeMethods.SetDwmAttribute(hwnd, NativeMethods.DwmwaWindowCornerPreference, NativeMethods.DwmwcpRound);
        ApplyBackdrop();
    }

    private void ApplyBackdrop()
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        // The backdrop type fails before Windows 11 22H2. With transparency effects off Windows
        // paints a plain fill instead, so the template keeps its opaque ground.
        var active = false;
        transparencyEnabled = NativeMethods.IsTransparencyEnabled();
        if (Backdrop != WindowBackdrop.None && transparencyEnabled)
        {
            var type = Backdrop switch
            {
                WindowBackdrop.MicaAlt => NativeMethods.DwmsbtTabbedWindow,
                WindowBackdrop.Acrylic => NativeMethods.DwmsbtTransientWindow,
                _ => NativeMethods.DwmsbtMainWindow,
            };
            active = NativeMethods.SetDwmAttribute(hwnd, NativeMethods.DwmwaSystemBackdropType, type);
        }
        else if (IsBackdropActive)
        {
            NativeMethods.SetDwmAttribute(hwnd, NativeMethods.DwmwaSystemBackdropType, NativeMethods.DwmsbtNone);
        }

        // DWM only draws the backdrop where the frame reaches, so extend it over the whole window
        // and make the WPF surface transparent. Put the surface colour back when it turns off.
        var target = HwndSource.FromHwnd(hwnd)?.CompositionTarget;
        if (WindowChrome.GetWindowChrome(this) is { } chrome)
        {
            chrome.GlassFrameThickness = active ? new Thickness(-1) : new Thickness(0);
        }
        if (target != null)
        {
            if (active)
            {
                surfaceColor ??= target.BackgroundColor;
                target.BackgroundColor = Colors.Transparent;
            }
            else if (surfaceColor is { } color)
            {
                target.BackgroundColor = color;
                surfaceColor = null;
            }
        }

        // Windows draws its own caption buttons wherever the frame reaches, on top of the
        // template's. They need WS_SYSMENU, so leave it out while the frame is extended.
        if (active != frameExtended)
        {
            frameExtended = active;
            NativeMethods.SetStyleBits(hwnd, NativeMethods.WsSysMenu, !active);
        }

        // Once the surface has been transparent WPF keeps rendering text in grey scale; with the
        // opaque ground back, tell it ClearType is safe.
        if (root != null)
        {
            if (active)
            {
                root.ClearValue(RenderOptions.ClearTypeHintProperty);
            }
            else
            {
                RenderOptions.SetClearTypeHint(root, ClearTypeHint.Enabled);
            }
        }
        SetValue(IsBackdropActivePropertyKey, active);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        // WPF puts WS_SYSMENU back when it rebuilds the style (ResizeMode, WindowStyle).
        if (msg == NativeMethods.WmStyleChanging && frameExtended && wParam.ToInt32() == NativeMethods.GwlStyle)
        {
            var style = Marshal.PtrToStructure<NativeMethods.StyleStruct>(lParam);
            if ((style.StyleNew & NativeMethods.WsSysMenu) != 0)
            {
                style.StyleNew &= ~NativeMethods.WsSysMenu;
                Marshal.StructureToPtr(style, lParam, false);
            }
        }

        // Without WS_SYSMENU, Windows ignores Alt+Space; open the system menu ourselves.
        if (msg == NativeMethods.WmSysCommand && frameExtended
            && (wParam.ToInt32() & 0xFFF0) == NativeMethods.ScKeyMenu && lParam.ToInt32() == ' ')
        {
            var corner = PointToScreen(new Point(0, TitleBarHeight));
            if (HwndSource.FromHwnd(hwnd)?.CompositionTarget is { } target)
            {
                corner = target.TransformFromDevice.Transform(corner);
            }
            SystemCommands.ShowSystemMenu(this, corner);
            handled = true;
        }

        // Windows broadcasts a setting change when transparency effects are turned on or off.
        if (msg == NativeMethods.WmSettingChange && Backdrop != WindowBackdrop.None
            && NativeMethods.IsTransparencyEnabled() != transparencyEnabled)
        {
            Dispatcher.BeginInvoke(ApplyBackdrop);
        }

        // A new accent colour in Windows: FrostedTheme follows it when the app uses FrostedAccent.System.
        if (msg is NativeMethods.WmSettingChange or NativeMethods.WmDwmColorizationColorChanged)
        {
            Dispatcher.BeginInvoke(FrostedTheme.OnSystemSettingsChanged);
        }
        return IntPtr.Zero;
    }

    protected override void OnStateChanged(EventArgs e)
    {
        base.OnStateChanged(e);
        UpdateMaximizedMargin();
    }

    private void UpdateMaximizedMargin()
    {
        if (root == null)
        {
            return;
        }

        // A maximised chromeless window hangs over the screen edge by the resize border.
        var b = SystemParameters.WindowResizeBorderThickness;
        root.Margin = WindowState == WindowState.Maximized
            ? new Thickness(b.Left + 4, b.Top + 4, b.Right + 4, b.Bottom + 4)
            : new Thickness(0);
    }
}
