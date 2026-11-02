using AzanTimes.Core.Models;
using AzanTimes.Core.Settings;

namespace AzanTimes.Core.Tests;

public class SettingsStoreTests
{
    [Fact]
    public void Selected_city_file_updates_city_and_keeps_other_settings()
    {
        var directory = Path.Combine(Path.GetTempPath(), "AzanTimes-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var store = new SettingsStore(directory);
            store.Save(new AppSettings
            {
                CityId = "tehran",
                WidgetVisible = false,
                StartWithWindows = false
            });
            File.WriteAllText(store.SelectedCityPath, "shiraz");

            var loaded = store.Load();

            Assert.Equal("shiraz", loaded.CityId);
            Assert.False(loaded.WidgetVisible);
            Assert.False(loaded.StartWithWindows);
            Assert.False(File.Exists(store.SelectedCityPath));

            var again = store.Load();
            Assert.Equal("shiraz", again.CityId);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
