using System.Windows;

namespace Pompom.Services;

internal interface IGlobalHotkeyService : IDisposable
{
    event EventHandler<HotkeyPressedEventArgs>? HotkeyPressed;

    IReadOnlyList<HotkeyBinding> Register(Window window);
}
