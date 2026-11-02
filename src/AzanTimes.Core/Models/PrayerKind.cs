namespace AzanTimes.Core.Models;

public sealed record PrayerKind(string Id, string Title);

public static class PrayerKinds
{
    public static readonly IReadOnlyList<PrayerKind> All =
    [
        new("fajr", "اذان صبح"),
        new("sunrise", "طلوع آفتاب"),
        new("dhuhr", "اذان ظهر"),
        new("asr", "عصر"),
        new("sunset", "غروب آفتاب"),
        new("maghrib", "اذان مغرب"),
        new("isha", "عشا"),
        new("midnight", "نیمه‌شب شرعی"),
    ];

    public static readonly string[] DefaultPopups = ["fajr", "dhuhr", "maghrib"];

    public static string TitleOf(string id)
    {
        foreach (var kind in All)
        {
            if (kind.Id == id)
                return kind.Title;
        }

        return id;
    }
}
