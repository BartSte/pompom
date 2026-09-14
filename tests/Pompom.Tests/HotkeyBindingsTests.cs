using Pompom.Services;

namespace Pompom.Tests;

[TestClass]
public sealed class HotkeyBindingsTests
{
    [TestMethod]
    public void FixedShortcutsHaveUniqueIdsAndActions()
    {
        IReadOnlyList<HotkeyBinding> bindings = HotkeyBindings.All;

        CollectionAssert.AreEquivalent(
            new[] { 1, 2, 3, 4 },
            bindings.Select(binding => binding.Id).ToArray());
        CollectionAssert.AreEquivalent(
            Enum.GetValues<HotkeyAction>(),
            bindings.Select(binding => binding.Action).ToArray());
    }

    [TestMethod]
    public void FixedShortcutTextMatchesCommands()
    {
        var shortcuts = HotkeyBindings.All.ToDictionary(
            binding => binding.Action,
            binding => binding.Shortcut);

        Assert.AreEqual("Ctrl+Alt+P", shortcuts[HotkeyAction.Start]);
        Assert.AreEqual("Ctrl+Alt+X", shortcuts[HotkeyAction.Stop]);
        Assert.AreEqual("Ctrl+Alt+N", shortcuts[HotkeyAction.Skip]);
        Assert.AreEqual("Ctrl+Alt+M", shortcuts[HotkeyAction.ShowOrHide]);
    }
}
