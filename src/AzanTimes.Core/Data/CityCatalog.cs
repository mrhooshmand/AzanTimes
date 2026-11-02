using System.Globalization;
using System.Text.Json;
using AzanTimes.Core.Models;

namespace AzanTimes.Core.Data;

public sealed class CityCatalog
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly List<City> _cities;

    private CityCatalog(List<City> cities)
    {
        _cities = cities;
    }

    public IReadOnlyList<City> All => _cities;

    public static CityCatalog Load()
    {
        var assembly = typeof(CityCatalog).Assembly;
        var resourceName = assembly
            .GetManifestResourceNames()
            .Single(name => name.EndsWith("cities.json", StringComparison.OrdinalIgnoreCase));

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException("فهرست شهرها در برنامه پیدا نشد.");

        var cities = JsonSerializer.Deserialize<List<City>>(stream, JsonOptions) ?? [];
        if (cities.Count == 0 || cities.All(city => city.Id != "tehran"))
            throw new InvalidOperationException("شهر تهران در فهرست شهرها نیست.");

        return new CityCatalog(cities);
    }

    public City GetOrDefault(string? id)
    {
        var match = _cities.FirstOrDefault(city => string.Equals(city.Id, id, StringComparison.OrdinalIgnoreCase));
        return match ?? _cities.First(city => city.Id == "tehran");
    }

    public IReadOnlyList<City> Ordered()
    {
        var comparer = StringComparer.Create(CultureInfo.GetCultureInfo("fa-IR"), ignoreCase: false);
        return _cities.OrderBy(city => city.Name, comparer).ToList();
    }
}
