namespace Pompom.Services;

internal sealed record HotkeyBinding(
    int Id,
    HotkeyAction Action,
    uint VirtualKey,
    string Shortcut);
