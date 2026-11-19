using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace Nocturne.Avalonia.Controls;

/// <summary>The system material drawn behind a <see cref="NocturneWindow"/>.</summary>
public enum WindowBackdrop
{
    /// <summary>No system material: the window paints an opaque dark ground.</summary>
    None,

    /// <summary>Mica (Windows 11): a soft tint taken from the wallpaper. Elsewhere a blur where the platform has one.</summary>
    Mica,

    /// <summary>Mica Alt (Windows 11): a stronger wallpaper tint, as in tabbed apps. Elsewhere as <see cref="Mica"/>.</summary>
    MicaAlt,

    /// <summary>Acrylic: the windows and desktop behind it, blurred (the default; macOS draws its own vibrancy).</summary>
    Acrylic,
}

/// <summary>How a <see cref="NocturneWindow"/>'s title bar stands apart from the content.</summary>
public enum TitleBarStyle
{
    /// <summary>The title bar lies on the window's own ground, one with the content (the default).</summary>
    Seamless,

    /// <summary>A slightly darker strip with a hairline under it, for apps with busy content at the top.</summary>
    Separated,

    /// <summary>A strip tinted with the accent colour (it follows <see cref="NocturneTheme.SetAccent"/>).</summary>
    Accent,
}

/// <summary>The corners Windows 11 draws for a <see cref="NocturneWindow"/>. A maximised window is always square.</summary>
public enum WindowCornerStyle
{
    /// <summary>Rounded, 8px (the default).</summary>
    Round,

    /// <summary>Slightly rounded, 4px, for small tool windows.</summary>
    RoundSmall,

    /// <summary>Square, as on Windows 10.</summary>
    Square,
}

/// <summary>
/// Window with the Nocturne frame: a system backdrop (Acrylic by default, or Mica / Mica Alt on
/// Windows 11), a dark tint and a soft light from the top-left corner, under a 44px title bar with
/// its own caption buttons. Where the backdrop is not available (Windows 10, transparency effects
/// off, Remote Desktop, most Linux desktops) it paints an opaque ground of the same colour.
/// <para>
/// The title bar is drawn into the client area (ExtendClientAreaToDecorationsHint) and marked with
/// Avalonia's window decoration roles, so the platform moves, snaps and maximises the window from
/// it; on Windows the maximise button opens the snap layouts. macOS keeps its own traffic lights
/// (the template leaves room for them and hides its buttons). Where the platform cannot extend the
/// client area the system title bar stays and the template draws none.
/// </para>
/// </summary>
[TemplatePart(PartRoot, typeof(Control))]
[TemplatePart(PartLight, typeof(Image))]
[TemplatePart(PartMinButton, typeof(Button))]
[TemplatePart(PartMaxButton, typeof(Button))]
[TemplatePart(PartCloseButton, typeof(Button))]
[PseudoClasses(":maximized", ":macos", ":leftcommands")]
public class NocturneWindow : Window
{
    private const string PartRoot = "PART_Root";
    private const string PartLight = "PART_Light";
    private const string PartMinButton = "PART_MinButton";
    private const string PartMaxButton = "PART_MaxButton";
    private const string PartCloseButton = "PART_CloseButton";

    public static readonly StyledProperty<WindowBackdrop> BackdropProperty =
        AvaloniaProperty.Register<NocturneWindow, WindowBackdrop>(nameof(Backdrop), WindowBackdrop.Acrylic);

    public static readonly StyledProperty<bool> UseBackdropInRemoteSessionProperty =
        AvaloniaProperty.Register<NocturneWindow, bool>(nameof(UseBackdropInRemoteSession));

    public static readonly DirectProperty<NocturneWindow, bool> IsBackdropActiveProperty =
        AvaloniaProperty.RegisterDirect<NocturneWindow, bool>(nameof(IsBackdropActive), o => o.IsBackdropActive);

    public static readonly StyledProperty<double> TintOpacityProperty =
        AvaloniaProperty.Register<NocturneWindow, double>(nameof(TintOpacity), 0.5);

    public static readonly StyledProperty<double> TitleBarHeightProperty =
        AvaloniaProperty.Register<NocturneWindow, double>(nameof(TitleBarHeight), 44);

    public static readonly StyledProperty<bool> ShowIconProperty =
        AvaloniaProperty.Register<NocturneWindow, bool>(nameof(ShowIcon), true);

    public static readonly StyledProperty<bool> ShowMinButtonProperty =
        AvaloniaProperty.Register<NocturneWindow, bool>(nameof(ShowMinButton), true);

    public static readonly StyledProperty<bool> ShowMaxRestoreButtonProperty =
        AvaloniaProperty.Register<NocturneWindow, bool>(nameof(ShowMaxRestoreButton), true);

    public static readonly StyledProperty<object?> RightWindowCommandsProperty =
        AvaloniaProperty.Register<NocturneWindow, object?>(nameof(RightWindowCommands));

    public static readonly StyledProperty<object?> LeftWindowCommandsProperty =
        AvaloniaProperty.Register<NocturneWindow, object?>(nameof(LeftWindowCommands));

    public static readonly StyledProperty<TitleBarStyle> TitleBarStyleProperty =
        AvaloniaProperty.Register<NocturneWindow, TitleBarStyle>(nameof(TitleBarStyle));

    public static readonly StyledProperty<WindowCornerStyle> CornerStyleProperty =
        AvaloniaProperty.Register<NocturneWindow, WindowCornerStyle>(nameof(CornerStyle));

    public static readonly StyledProperty<object?> FlyoutsProperty =
        AvaloniaProperty.Register<NocturneWindow, object?>(nameof(Flyouts));

    public static readonly DirectProperty<NocturneWindow, bool> IsTitleBarDrawnProperty =
        AvaloniaProperty.RegisterDirect<NocturneWindow, bool>(nameof(IsTitleBarDrawn), o => o.IsTitleBarDrawn);

    public static readonly DirectProperty<NocturneWindow, Bitmap?> IconImageProperty =
        AvaloniaProperty.RegisterDirect<NocturneWindow, Bitmap?>(nameof(IconImage), o => o.IconImage);

    private Control? root;
    private Image? light;
    private Button? minButton, maxButton, closeButton;
    private DispatcherTimer? lightTimer;
    private DispatcherTimer? sessionTimer;
    private bool isBackdropActive;
    private bool transparencyEnabled = true;
    private bool remoteSession;
    private Bitmap? iconImage;
    private bool isTitleBarDrawn;

    public NocturneWindow()
    {
        ExtendClientAreaToDecorationsHint = true;
        ExtendClientAreaTitleBarHeightHint = TitleBarHeight;
        PseudoClasses.Set(":macos", OperatingSystem.IsMacOS());
        IsTitleBarDrawn = IsExtendedIntoWindowDecorations || AlwaysDrawTitleBar;
        ScalingChanged += (_, _) => UpdateLight();
    }

    /// <summary>Subclasses (<c>MainWindow : NocturneWindow</c>) take this theme too.</summary>
    protected override Type StyleKeyOverride => typeof(NocturneWindow);

    /// <summary>
    /// The system material behind the window. Left out over Remote Desktop unless
    /// <see cref="UseBackdropInRemoteSession"/> is set, and where Windows' transparency effects are off.
    /// </summary>
    public WindowBackdrop Backdrop
    {
        get => GetValue(BackdropProperty);
        set => SetValue(BackdropProperty, value);
    }

    /// <summary>
    /// Keeps the backdrop over Remote Desktop (false by default). A see-through window has to be
    /// composed and sent again with everything behind it, so the window paints its opaque ground
    /// there instead. It follows a session that moves between the console and Remote Desktop while
    /// the app runs (checked every two seconds; Windows only).
    /// </summary>
    public bool UseBackdropInRemoteSession
    {
        get => GetValue(UseBackdropInRemoteSessionProperty);
        set => SetValue(UseBackdropInRemoteSessionProperty, value);
    }

    /// <summary>True while the platform draws the backdrop, so the template leaves out its opaque ground.</summary>
    public bool IsBackdropActive
    {
        get => isBackdropActive;
        private set => SetAndRaise(IsBackdropActiveProperty, ref isBackdropActive, value);
    }

    /// <summary>How strongly the dark tint (<c>Nocturne.Brushes.Window.Tint</c>) covers the backdrop (0.5 by default).</summary>
    public double TintOpacity
    {
        get => GetValue(TintOpacityProperty);
        set => SetValue(TintOpacityProperty, value);
    }

    public double TitleBarHeight
    {
        get => GetValue(TitleBarHeightProperty);
        set => SetValue(TitleBarHeightProperty, value);
    }

    /// <summary>Shows <see cref="Window.Icon"/> (or a small accent glyph when none is set) before the title.</summary>
    public bool ShowIcon
    {
        get => GetValue(ShowIconProperty);
        set => SetValue(ShowIconProperty, value);
    }

    public bool ShowMinButton
    {
        get => GetValue(ShowMinButtonProperty);
        set => SetValue(ShowMinButtonProperty, value);
    }

    public bool ShowMaxRestoreButton
    {
        get => GetValue(ShowMaxRestoreButtonProperty);
        set => SetValue(ShowMaxRestoreButtonProperty, value);
    }

    /// <summary>Content shown in the title bar before the caption buttons.</summary>
    public object? RightWindowCommands
    {
        get => GetValue(RightWindowCommandsProperty);
        set => SetValue(RightWindowCommandsProperty, value);
    }

    /// <summary>
    /// Content shown at the start of the title bar, before the icon and the title, such as a back
    /// button or one that folds the navigation out. A thin divider separates it from the title.
    /// </summary>
    public object? LeftWindowCommands
    {
        get => GetValue(LeftWindowCommandsProperty);
        set => SetValue(LeftWindowCommandsProperty, value);
    }

    /// <summary>How the title bar stands apart from the content (<see cref="Controls.TitleBarStyle.Seamless"/> by default).</summary>
    public TitleBarStyle TitleBarStyle
    {
        get => GetValue(TitleBarStyleProperty);
        set => SetValue(TitleBarStyleProperty, value);
    }

    /// <summary>The window's corners on Windows 11 (<see cref="WindowCornerStyle.Round"/> by default).</summary>
    public WindowCornerStyle CornerStyle
    {
        get => GetValue(CornerStyleProperty);
        set => SetValue(CornerStyleProperty, value);
    }

    /// <summary>The layer above the content (under the title bar) for flyout panels, usually a Grid.</summary>
    public object? Flyouts
    {
        get => GetValue(FlyoutsProperty);
        set => SetValue(FlyoutsProperty, value);
    }

    /// <summary>
    /// Draws the title bar even where the platform keeps its own (the client area is not extended).
    /// For pictures taken off screen (Avalonia's headless platform cannot extend it); apps leave it off.
    /// </summary>
    public static bool AlwaysDrawTitleBar { get; set; }

    /// <summary>True when the template draws its title bar: the client area reaches into the decorations.</summary>
    public bool IsTitleBarDrawn
    {
        get => isTitleBarDrawn;
        private set => SetAndRaise(IsTitleBarDrawnProperty, ref isTitleBarDrawn, value);
    }

    /// <summary><see cref="Window.Icon"/> as a bitmap for the title bar; null when the window has no icon.</summary>
    public Bitmap? IconImage
    {
        get => iconImage;
        private set => SetAndRaise(IconImageProperty, ref iconImage, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (root != null)
            root.SizeChanged -= OnRootSizeChanged;
        root = e.NameScope.Find<Control>(PartRoot);
        light = e.NameScope.Find<Image>(PartLight);
        if (root != null)
            root.SizeChanged += OnRootSizeChanged;

        Hook(ref minButton, e.NameScope.Find<Button>(PartMinButton), (_, _) => WindowState = WindowState.Minimized);
        Hook(ref maxButton, e.NameScope.Find<Button>(PartMaxButton), (_, _) =>
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized);
        Hook(ref closeButton, e.NameScope.Find<Button>(PartCloseButton), (_, _) => Close());
        UpdateCaptionButtons();
        UpdateLight();
    }

    private static void Hook(ref Button? field, Button? button, EventHandler<RoutedEventArgs> click)
    {
        // The template is applied once per window; a new template replaces the buttons.
        field = button;
        if (button != null)
            button.Click += click;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == BackdropProperty || change.Property == UseBackdropInRemoteSessionProperty)
            ApplyBackdrop();
        else if (change.Property == ActualTransparencyLevelProperty)
            UpdateBackdropActive();
        else if (change.Property == CornerStyleProperty)
            ApplyCornerStyle();
        else if (change.Property == TitleBarHeightProperty)
            ExtendClientAreaTitleBarHeightHint = TitleBarHeight;
        else if (change.Property == WindowStateProperty)
            PseudoClasses.Set(":maximized", WindowState == WindowState.Maximized);
        else if (change.Property == IconProperty)
            IconImage = ToBitmap(Icon);
        else if (change.Property == IsExtendedIntoWindowDecorationsProperty)
            IsTitleBarDrawn = IsExtendedIntoWindowDecorations || AlwaysDrawTitleBar;
        else if (change.Property == LeftWindowCommandsProperty)
            PseudoClasses.Set(":leftcommands", LeftWindowCommands != null);

        if (change.Property == ShowMinButtonProperty || change.Property == ShowMaxRestoreButtonProperty
            || change.Property == CanResizeProperty || change.Property == CanMinimizeProperty || change.Property == CanMaximizeProperty)
        {
            UpdateCaptionButtons();
        }
    }

    private void UpdateCaptionButtons()
    {
        if (minButton != null)
            minButton.IsVisible = ShowMinButton && CanMinimize;
        if (maxButton != null)
            maxButton.IsVisible = ShowMaxRestoreButton && CanMaximize && CanResize;
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        ApplyCornerStyle();
        ApplyBackdrop();

        if (OperatingSystem.IsWindows())
        {
            // Avalonia has no public hook for WM_WTSSESSION_CHANGE / WM_SETTINGCHANGE, so look at
            // the session and the transparency switch every two seconds (two cheap calls).
            sessionTimer = new DispatcherTimer(TimeSpan.FromSeconds(2), DispatcherPriority.Background, (_, _) =>
            {
                if (OperatingSystem.IsWindows())
                    CheckSession();
            });
            sessionTimer.Start();
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private void CheckSession()
    {
        if (Backdrop != WindowBackdrop.None
            && (NativeMethods.IsRemoteSession() != remoteSession || NativeMethods.IsTransparencyEnabled() != transparencyEnabled))
        {
            ApplyBackdrop();
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        sessionTimer?.Stop();
        lightTimer?.Stop();
        base.OnClosed(e);
    }

    private void OnRootSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (light == null)
            return;
        if (light.Source == null)
        {
            // The first size: draw at once, so the window never shows without its light.
            Dispatcher.UIThread.Post(UpdateLight, DispatcherPriority.Render);
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
        });
        lightTimer.Stop();
        lightTimer.Start();
    }

    /// <summary>
    /// Draws the light from the top-left corner as a dithered bitmap for the current size and
    /// display scaling, placed pixel for pixel (a gradient brush shows diagonal bands).
    /// </summary>
    private void UpdateLight()
    {
        if (light == null || root == null || root.Bounds.Width <= 0 || root.Bounds.Height <= 0)
            return;

        var scaling = RenderScaling;
        var bitmap = LightTexture.Create(
            (int)Math.Round(root.Bounds.Width * scaling),
            (int)Math.Round(root.Bounds.Height * scaling));
        var old = light.Source as IDisposable;
        light.Source = bitmap;
        light.Width = bitmap.PixelSize.Width / scaling;
        light.Height = bitmap.PixelSize.Height / scaling;
        old?.Dispose();
    }

    /// <summary>
    /// Asks the platform for the backdrop, or for none where it is not wanted: over Remote Desktop
    /// (unless <see cref="UseBackdropInRemoteSession"/>) and with Windows' transparency effects off.
    /// </summary>
    private void ApplyBackdrop()
    {
        var wanted = Backdrop != WindowBackdrop.None;
        if (OperatingSystem.IsWindows())
        {
            transparencyEnabled = NativeMethods.IsTransparencyEnabled();
            remoteSession = NativeMethods.IsRemoteSession();
            wanted &= transparencyEnabled && (!remoteSession || UseBackdropInRemoteSession);
        }

        TransparencyLevelHint = !wanted
            ? [WindowTransparencyLevel.None]
            : Backdrop == WindowBackdrop.Acrylic
                ? [WindowTransparencyLevel.AcrylicBlur, WindowTransparencyLevel.Blur]
                : [WindowTransparencyLevel.Mica, WindowTransparencyLevel.AcrylicBlur, WindowTransparencyLevel.Blur];

        // Avalonia's Mica is the main-window material; Mica Alt is the tabbed one.
        if (OperatingSystem.IsWindows() && wanted && Backdrop == WindowBackdrop.MicaAlt
            && ActualTransparencyLevel == WindowTransparencyLevel.Mica && TryGetPlatformHandle()?.Handle is { } hwnd)
        {
            NativeMethods.SetDwmAttribute(hwnd, NativeMethods.DwmwaSystemBackdropType, NativeMethods.DwmsbtTabbedWindow);
        }
        UpdateBackdropActive();
    }

    private void UpdateBackdropActive() =>
        IsBackdropActive = ActualTransparencyLevel != WindowTransparencyLevel.None
                           && ActualTransparencyLevel != WindowTransparencyLevel.Transparent;

    private void ApplyCornerStyle()
    {
        if (!OperatingSystem.IsWindows() || TryGetPlatformHandle()?.Handle is not { } hwnd)
            return;

        // Windows 11 draws the corners; before Windows 11 the attribute fails and they stay square.
        NativeMethods.SetDwmAttribute(hwnd, NativeMethods.DwmwaWindowCornerPreference, CornerStyle switch
        {
            WindowCornerStyle.RoundSmall => NativeMethods.DwmwcpRoundSmall,
            WindowCornerStyle.Square => NativeMethods.DwmwcpDoNotRound,
            _ => NativeMethods.DwmwcpRound,
        });
    }

    private static Bitmap? ToBitmap(WindowIcon? icon)
    {
        if (icon == null)
            return null;
        using var stream = new MemoryStream();
        icon.Save(stream);
        stream.Position = 0;
        return new Bitmap(stream);
    }
}
