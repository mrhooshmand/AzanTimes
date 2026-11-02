using AzanTimes.Core.Models;

namespace AzanTimes.Core.Scheduling;

public static class PopupSchedule
{
    public static string Key(PrayerMoment moment) => $"{moment.At:yyyy-MM-dd}|{moment.Id}";

    public static IReadOnlyList<PrayerMoment> Due(
        DayPrayerTimes times,
        DateTime now,
        IEnumerable<string> enabledEvents,
        IEnumerable<string> alreadyNotified)
    {
        var enabled = new HashSet<string>(enabledEvents, StringComparer.OrdinalIgnoreCase);
        var notified = new HashSet<string>(alreadyNotified, StringComparer.Ordinal);
        var due = new List<PrayerMoment>();

        foreach (var moment in times.Moments)
        {
            if (!enabled.Contains(moment.Id))
                continue;

            if (now < moment.At || now >= moment.At.AddMinutes(2))
                continue;

            if (notified.Contains(Key(moment)))
                continue;

            due.Add(moment);
        }

        return due;
    }
}
