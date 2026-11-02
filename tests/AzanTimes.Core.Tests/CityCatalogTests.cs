using AzanTimes.Core.Data;

namespace AzanTimes.Core.Tests;

public class CityCatalogTests
{
    [Fact]
    public void Catalog_contains_provincial_capitals_and_tehran()
    {
        var catalog = CityCatalog.Load();

        Assert.Equal(31, catalog.All.Count);
        Assert.Equal("تهران", catalog.GetOrDefault("tehran").Name);
        Assert.Equal("تهران", catalog.GetOrDefault("missing").Name);
        Assert.Contains(catalog.Ordered(), city => city.Id == "mashhad");
    }
}
