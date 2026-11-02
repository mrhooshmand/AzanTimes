namespace AzanTimes.Core.Models;

public sealed record PrayerMoment(string Id, string Title, DateTime At);

public sealed class DayPrayerTimes
{
    public DayPrayerTimes(IReadOnlyList<PrayerMoment> moments)
    {
        Moments = moments;
    }

    public IReadOnlyList<PrayerMoment> Moments { get; }

    public DateTime Fajr => TimeOf("fajr");

    public DateTime Sunrise => TimeOf("sunrise");

    public DateTime Dhuhr => TimeOf("dhuhr");

    public DateTime Asr => TimeOf("asr");

    public DateTime Sunset => TimeOf("sunset");

    public DateTime Maghrib => TimeOf("maghrib");

    public DateTime Isha => TimeOf("isha");

    public DateTime Midnight => TimeOf("midnight");

    public DateTime TimeOf(string id)
    {
        if (!TryGet(id, out var at))
            throw new KeyNotFoundException(id);

        return at;
    }

    public bool TryGet(string id, out DateTime at)
    {
        foreach (var moment in Moments)
        {
            if (moment.Id == id)
            {
                at = moment.At;
                return true;
            }
        }

        at = default;
        return false;
    }

    public PrayerMoment? Moment(string id)
    {
        foreach (var moment in Moments)
        {
            if (moment.Id == id)
                return moment;
        }

        return null;
    }
}
