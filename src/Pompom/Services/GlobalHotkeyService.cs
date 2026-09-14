using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Pompom.Services;

internal sealed partial class GlobalHotkeyService : IGlobalHotkeyService
{
    private const int WmHotkey = 0x0312;
    private const uint ModAlt = 0x0001;
    private const uint ModControl = 0x0002;
    private const uint ModNoRepeat = 0x4000;

    private readonly List<HotkeyBinding> _registeredBindings = [];
    private HwndSource? _source;
    private nint _windowHandle;
    private bool _disposed;

    public event EventHandler<HotkeyPressedEventArgs>? HotkeyPressed;

    public IReadOnlyList<HotkeyBinding> Register(Window window)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(window);

        _windowHandle = new WindowInteropHelper(window).EnsureHandle();
        _source = HwndSource.FromHwnd(_windowHandle)
            ?? throw new Win32Exception("Pompom could not access the window handle.");
        _source.AddHook(WindowProcedure);

        List<HotkeyBinding> failures = [];
        foreach (HotkeyBinding binding in HotkeyBindings.All)
        {
            bool registered = RegisterHotKey(
                _windowHandle,
                binding.Id,
                ModControl | ModAlt | ModNoRepeat,
                binding.VirtualKey);

            if (registered)
            {
                _registeredBindings.Add(binding);
            }
            else
            {
                failures.Add(binding);
            }
        }

        return failures;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        foreach (HotkeyBinding binding in _registeredBindings)
        {
            _ = UnregisterHotKey(_windowHandle, binding.Id);
        }

        _registeredBindings.Clear();
        _source?.RemoveHook(WindowProcedure);
        _source = null;
        _disposed = true;
    }

    private nint WindowProcedure(
        nint windowHandle,
        int message,
        nint wordParameter,
        nint longParameter,
        ref bool handled)
    {
        if (message != WmHotkey)
        {
            return 0;
        }

        int id = wordParameter.ToInt32();
        HotkeyBinding? binding = HotkeyBindings.All.FirstOrDefault(item => item.Id == id);
        if (binding is null)
        {
            return 0;
        }

        handled = true;
        HotkeyPressed?.Invoke(this, new HotkeyPressedEventArgs(binding.Action));
        return 0;
    }

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool RegisterHotKey(
        nint windowHandle,
        int id,
        uint modifiers,
        uint virtualKey);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool UnregisterHotKey(nint windowHandle, int id);
}
