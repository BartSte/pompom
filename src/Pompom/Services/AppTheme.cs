using System.Windows;
using System.Windows.Media;
using MediaColor = System.Windows.Media.Color;

namespace Pompom.Services;

internal static class AppTheme
{
    private static readonly ThemeColor[] ThemeColors =
    [
        new("PompomAccentBrush", 0xE84F45, 0xFF6B60),
        new("PompomAccentHoverBrush", 0xD94239, 0xFF8178),
        new("PompomAccentSoftBrush", 0xFFF0ED, 0x3B2422),
        new("PompomGreenBrush", 0x397454, 0x58A979),
        new("PompomSurfaceBrush", 0xF6F2EC, 0x181716),
        new("PompomCardBrush", 0xFFFCF9, 0x232120),
        new("PompomTextBrush", 0x211E1C, 0xF3EEE8),
        new("PompomMutedTextBrush", 0x655E59, 0xB9AEA5),
        new("PompomSectionTextBrush", 0x514B47, 0xD1C6BD),
        new("PompomBorderBrush", 0xE5DDD5, 0x3A3532),
        new("PompomTrackBrush", 0xD8CEC4, 0x4A433F),
    ];

    public static bool IsDark { get; private set; }

    public static void Apply(bool useDarkTheme)
    {
        IsDark = useDarkTheme;
        System.Windows.Application? application = System.Windows.Application.Current;
        if (application is null)
        {
            return;
        }

#pragma warning disable WPF0001 // ThemeMode is the WPF API for changing the native control theme at runtime.
        application.ThemeMode = useDarkTheme ? ThemeMode.Dark : ThemeMode.Light;
#pragma warning restore WPF0001
        foreach (ThemeColor themeColor in ThemeColors)
        {
            uint value = useDarkTheme ? themeColor.Dark : themeColor.Light;
            application.Resources[themeColor.Key] = CreateBrush(value);
        }

        foreach (Window window in application.Windows)
        {
            WindowAppearance.ApplyThemeTitleBar(window, useDarkTheme);
        }
    }

    private static SolidColorBrush CreateBrush(uint value)
    {
        return new SolidColorBrush(MediaColor.FromRgb(
            (byte)(value >> 16),
            (byte)(value >> 8),
            (byte)value));
    }

    private sealed record ThemeColor(string Key, uint Light, uint Dark);
}
