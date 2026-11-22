using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace Nocturne.Avalonia.Controls;

/// <summary>Windows calls for <see cref="NocturneWindow"/> (and the wheel setting for NocturneGrid). Every caller checks <c>OperatingSystem.IsWindows()</c> first.</summary>
[SupportedOSPlatform("windows")]
internal static class NativeMethods
{
    public const int DwmwaWindowCornerPreference = 33;
    public const int DwmwaSystemBackdropType = 38;

    // DWM_SYSTEMBACKDROP_TYPE: Mica Alt
    public const int DwmsbtTabbedWindow = 4;

    // DWM_WINDOW_CORNER_PREFERENCE values
    public const int DwmwcpDoNotRound = 1;
    public const int DwmwcpRound = 2;
    public const int DwmwcpRoundSmall = 3;

    private const int SmRemoteSession = 0x1000;

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int index);

    private const int SpiGetWheelScrollLines = 0x0068;

    [DllImport("user32.dll")]
    private static extern bool SystemParametersInfo(int action, int param, out int value, int winIni);

    /// <summary>The wheel's "lines to scroll" setting: 3 by default, -1 for "one screen at a time".</summary>
    public static int WheelScrollLines() =>
        SystemParametersInfo(SpiGetWheelScrollLines, 0, out var lines, 0) ? lines : 3;

    /// <summary>Sets a DWM window attribute; false where Windows does not know it (older versions).</summary>
    public static bool SetDwmAttribute(IntPtr hwnd, int attribute, int value) =>
        DwmSetWindowAttribute(hwnd, attribute, ref value, sizeof(int)) >= 0;

    /// <summary>True in a Remote Desktop session.</summary>
    public static bool IsRemoteSession() => GetSystemMetrics(SmRemoteSession) != 0;

    /// <summary>The "Transparency effects" switch in Windows' colour settings (on where it cannot be read).</summary>
    public static bool IsTransparencyEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            return key?.GetValue("EnableTransparency") is not int value || value != 0;
        }
        catch (Exception e) when (e is System.Security.SecurityException or UnauthorizedAccessException or IOException)
        {
            return true;
        }
    }
}
