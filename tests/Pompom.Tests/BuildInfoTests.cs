using System.Text.RegularExpressions;
using Pompom.Services;

namespace Pompom.Tests;

[TestClass]
public sealed class BuildInfoTests
{
    [TestMethod]
    public void VersionComesFromTheApplicationAssembly()
    {
        Assert.AreEqual("0.1.0", BuildInfo.Version);
    }

    [TestMethod]
    public void GitCommitComesFromBuildMetadata()
    {
        Assert.IsTrue(Regex.IsMatch(BuildInfo.GitCommit, "^[0-9a-f]{12}$"));
    }
}
