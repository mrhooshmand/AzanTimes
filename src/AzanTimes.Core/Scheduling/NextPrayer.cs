using AzanTimes.Core.Models;

namespace AzanTimes.Core.Scheduling;

public static class NextPrayer
{
    public static PrayerMoment? Find(DateTime now, params DayPrayerTimes[] days)
    {
        PrayerMoment? best = null;
        foreach (var day in days)
        {
            foreach (var moment in day.Moments)
            {
                if (moment.At <= now)
                    continue;

                if (best is null || moment.At < best.At)
                    best = moment;
            }
        }

        return best;
    }
}
