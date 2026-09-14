namespace Pompom.Services;

internal static class HotkeyBindings
{
    public static IReadOnlyList<HotkeyBinding> All { get; } =
    [
        new(1, HotkeyAction.Start, 0x50, "Ctrl+Alt+P"),
        new(2, HotkeyAction.Stop, 0x58, "Ctrl+Alt+X"),
        new(3, HotkeyAction.Skip, 0x4E, "Ctrl+Alt+N"),
        new(4, HotkeyAction.ShowOrHide, 0x4D, "Ctrl+Alt+M"),
    ];
}
