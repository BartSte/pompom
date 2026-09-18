using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Pompom.Services;

internal static class WindowAppearance
{
    private const int ImmersiveDarkModeAttribute = 20;
    private const int BorderColorAttribute = 34;
    private const int CaptionColorAttribute = 35;
    private const int TextColorAttribute = 36;

    public static void UseThemeTitleBar(Window window)
    {
        window.SourceInitialized += (_, _) => ApplyThemeTitleBar(window, AppTheme.IsDark);
    }

    public static void ApplyThemeTitleBar(Window window, bool useDarkTheme)
    {
        IntPtr handle = new WindowInteropHelper(window).Handle;
        if (handle == IntPtr.Zero)
        {
            return;
        }

        int useImmersiveDarkMode = useDarkTheme ? 1 : 0;
        SetValue(handle, ImmersiveDarkModeAttribute, useImmersiveDarkMode);
        SetValue(
            handle,
            BorderColorAttribute,
            useDarkTheme ? 0x0032353A : 0x00D5DDE5);
        SetValue(
            handle,
            CaptionColorAttribute,
            useDarkTheme ? 0x00161718 : 0x00ECF2F6);
        SetValue(
            handle,
            TextColorAttribute,
            useDarkTheme ? 0x00E8EEF3 : 0x001C1E21);
    }

    private static void SetValue(IntPtr handle, int attribute, int value)
    {
        _ = DwmSetWindowAttribute(handle, attribute, ref value, sizeof(int));
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(
        IntPtr windowHandle,
        int attribute,
        ref int attributeValue,
        int attributeSize);
}
