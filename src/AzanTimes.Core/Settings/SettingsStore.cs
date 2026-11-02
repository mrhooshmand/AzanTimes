using System.Text.Json;
using AzanTimes.Core.Models;

namespace AzanTimes.Core.Settings;

public sealed class SettingsStore
{
    public const string SelectedCityFileName = "selected-city.txt";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public SettingsStore(string? directory = null)
    {
        DirectoryPath = directory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AzanTimes");
    }

    public string DirectoryPath { get; }

    public string SettingsPath => Path.Combine(DirectoryPath, "settings.json");

    public string SelectedCityPath => Path.Combine(DirectoryPath, SelectedCityFileName);

    public AppSettings Load()
    {
        var settings = ReadSettings() ?? new AppSettings();
        settings.PopupEvents ??= new List<string>(PrayerKinds.DefaultPopups);
        settings.NotifiedKeys ??= [];

        if (!File.Exists(SelectedCityPath))
            return settings;

        var cityId = File.ReadAllText(SelectedCityPath).Trim();
        try
        {
            File.Delete(SelectedCityPath);
        }
        catch (IOException)
        {
        }

        if (!string.IsNullOrWhiteSpace(cityId))
        {
            settings.CityId = cityId;
            Save(settings);
        }

        return settings;
    }

    public void Save(AppSettings settings)
    {
        Directory.CreateDirectory(DirectoryPath);
        var json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(SettingsPath, json);
    }

    private AppSettings? ReadSettings()
    {
        if (!File.Exists(SettingsPath))
            return null;

        try
        {
            var json = File.ReadAllText(SettingsPath);
            return JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
        }
        catch (JsonException)
        {
            var broken = SettingsPath + ".bad";
            try
            {
                File.Copy(SettingsPath, broken, overwrite: true);
            }
            catch (IOException)
            {
            }

            return null;
        }
    }
}
