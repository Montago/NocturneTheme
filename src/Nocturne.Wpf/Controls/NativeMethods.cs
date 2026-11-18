using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Nocturne.Wpf.Controls;

internal static class NativeMethods
{
    // DWMWINDOWATTRIBUTE values (Windows 11, build 22000+; the backdrop type needs 22621+)
    public const int DwmwaUseImmersiveDarkMode = 20;
    public const int DwmwaWindowCornerPreference = 33;
    public const int DwmwaSystemBackdropType = 38;

    // DWM_SYSTEMBACKDROP_TYPE values
    public const int DwmsbtNone = 1;
    public const int DwmsbtMainWindow = 2;      // Mica
    public const int DwmsbtTransientWindow = 3; // Acrylic
    public const int DwmsbtTabbedWindow = 4;    // Mica Alt

    // DWM_WINDOW_CORNER_PREFERENCE values
    public const int DwmwcpDoNotRound = 1;
    public const int DwmwcpRound = 2;
    public const int DwmwcpRoundSmall = 3;

    // Window style bits and messages for keeping the system caption buttons away
    public const int WmStyleChanging = 0x007C;
    public const int WmSysCommand = 0x0112;
    public const int WmSettingChange = 0x001A;
    public const int WmDwmColorizationColorChanged = 0x0320;
    public const int WmDestroy = 0x0002;

    // Session changes (WTSRegisterSessionNotification): a Remote Desktop connection taking over the
    // session or the local console taking it back
    public const int WmWtsSessionChange = 0x02B1;
    public const int WtsConsoleConnect = 1, WtsConsoleDisconnect = 2, WtsRemoteConnect = 3, WtsRemoteDisconnect = 4;
    private const int NotifyForThisSession = 0;
    private const int SmRemoteSession = 0x1000;
    public const int ScKeyMenu = 0xF100;
    public const int GwlStyle = -16;
    public const int WsSysMenu = 0x00080000;
    private const int SwpNoSize = 0x1, SwpNoMove = 0x2, SwpNoZOrder = 0x4, SwpNoActivate = 0x10, SwpFrameChanged = 0x20;

    /// <summary>STYLESTRUCT, sent with WM_STYLECHANGING.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct StyleStruct
    {
        public int StyleOld;
        public int StyleNew;
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
    private static extern int GetWindowLong(IntPtr hwnd, int index);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongW")]
    private static extern int SetWindowLong(IntPtr hwnd, int index, int value);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hwnd, IntPtr after, int x, int y, int cx, int cy, int flags);

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int index);

    [DllImport("wtsapi32.dll")]
    private static extern bool WTSRegisterSessionNotification(IntPtr hwnd, int flags);

    [DllImport("wtsapi32.dll")]
    private static extern bool WTSUnRegisterSessionNotification(IntPtr hwnd);

    /// <summary>True while the session is shown over Remote Desktop (asked each time, never cached).</summary>
    public static bool IsRemoteSession() => GetSystemMetrics(SmRemoteSession) != 0;

    /// <summary>Sends the window WM_WTSSESSION_CHANGE when the session moves between console and remote.</summary>
    public static void RegisterSessionNotification(IntPtr hwnd, bool register)
    {
        try
        {
            _ = register ? WTSRegisterSessionNotification(hwnd, NotifyForThisSession) : WTSUnRegisterSessionNotification(hwnd);
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException)
        {
            // No Terminal Services API (Server Core without it): the state at start-up is used.
        }
    }

    /// <summary>Sets or clears window style bits and lets the frame pick up the change.</summary>
    public static void SetStyleBits(IntPtr hwnd, int bits, bool set)
    {
        var style = GetWindowLong(hwnd, GwlStyle);
        var updated = set ? style | bits : style & ~bits;
        if (updated != style)
        {
            SetWindowLong(hwnd, GwlStyle, updated);
            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SwpNoSize | SwpNoMove | SwpNoZOrder | SwpNoActivate | SwpFrameChanged);
        }
    }

    /// <summary>
    /// The "Transparency effects" switch in Settings &gt; Personalization &gt; Colors. Missing
    /// means on, as on a fresh install.
    /// </summary>
    public static bool IsTransparencyEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            return key?.GetValue("EnableTransparency") is not int value || value != 0;
        }
        catch (Exception ex) when (ex is System.Security.SecurityException or UnauthorizedAccessException or IOException)
        {
            return true;
        }
    }

    /// <summary>Sets an int-sized DWM attribute; returns false where the OS does not support it.</summary>
    public static bool SetDwmAttribute(IntPtr hwnd, int attribute, int value)
    {
        try
        {
            return DwmSetWindowAttribute(hwnd, attribute, ref value, sizeof(int)) == 0;
        }
        catch (DllNotFoundException)
        {
            return false;
        }
        catch (EntryPointNotFoundException)
        {
            return false;
        }
    }
}
