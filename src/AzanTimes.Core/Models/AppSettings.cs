namespace AzanTimes.Core.Models;

public sealed class AppSettings
{
    public string CityId { get; set; } = "tehran";

    public bool WidgetVisible { get; set; } = true;

    public double? WidgetLeft { get; set; }

    public double? WidgetTop { get; set; }

    public bool WidgetTopmost { get; set; } = true;

    public bool PopupEnabled { get; set; } = true;

    public List<string> PopupEvents { get; set; } = new(PrayerKinds.DefaultPopups);

    public bool StartWithWindows { get; set; } = true;

    public List<string> NotifiedKeys { get; set; } = [];
}
