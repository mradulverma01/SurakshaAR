using NUnit.Framework;
using SurakshaAR.Infrastructure.Localization;

namespace SurakshaAR.Infrastructure.Tests;

public sealed class JsonLocalizationCatalogTests
{
    private static readonly string LocalizationDirectory = Path.GetFullPath(Path.Combine(
        TestContext.CurrentContext.TestDirectory,
        "../../../../Assets/StreamingAssets/Localization"));

    [Test]
    public async Task Reads_Hindi_interface_text()
    {
        var catalog = new JsonLocalizationCatalog(LocalizationDirectory);

        Assert.That(await catalog.Get("hi", "action.start"), Is.EqualTo("प्रशिक्षण शुरू करें"));
    }

    [Test]
    public async Task Falls_back_to_English_for_unreviewed_Ol_Chiki_text()
    {
        var catalog = new JsonLocalizationCatalog(LocalizationDirectory);

        Assert.That(await catalog.Get("sat-Olck", "action.start"), Is.EqualTo("Start training"));
    }
}
