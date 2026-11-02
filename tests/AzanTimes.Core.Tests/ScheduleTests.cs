using AzanTimes.Core.Models;
using AzanTimes.Core.Scheduling;

namespace AzanTimes.Core.Tests;

public class ScheduleTests
{
    [Fact]
    public void Popup_is_due_only_inside_two_minutes_and_once()
    {
        var moment = new PrayerMoment("fajr", "اذان صبح", new DateTime(2026, 11, 1, 5, 1, 0));
        var times = new DayPrayerTimes([moment]);

        var due = PopupSchedule.Due(times, moment.At.AddSeconds(30), ["fajr"], []);
        Assert.Equal("fajr", Assert.Single(due).Id);

        Assert.Empty(PopupSchedule.Due(times, moment.At.AddMinutes(2), ["fajr"], []));
        Assert.Empty(PopupSchedule.Due(times, moment.At.AddSeconds(30), ["fajr"], [PopupSchedule.Key(moment)]));
        Assert.Empty(PopupSchedule.Due(times, moment.At.AddSeconds(30), ["dhuhr"], []));
    }

    [Fact]
    public void Next_prayer_walks_into_tomorrow_after_midnight()
    {
        var calculator = new Prayer.PrayerCalculator();
        var tehran = Data.CityCatalog.Load().GetOrDefault("tehran");
        var today = calculator.Calculate(tehran, new DateTime(2026, 11, 1));
        var tomorrow = calculator.Calculate(tehran, new DateTime(2026, 11, 2));

        var beforeFajr = NextPrayer.Find(new DateTime(2026, 11, 1, 4, 0, 0), today, tomorrow);
        Assert.Equal("fajr", beforeFajr?.Id);
        Assert.Equal(new DateTime(2026, 11, 1, 5, 1, 0), beforeFajr?.At);

        var beforeMidnight = NextPrayer.Find(new DateTime(2026, 11, 1, 23, 0, 0), today, tomorrow);
        Assert.Equal("midnight", beforeMidnight?.Id);

        var afterMidnight = NextPrayer.Find(new DateTime(2026, 11, 1, 23, 30, 0), today, tomorrow);
        Assert.Equal("fajr", afterMidnight?.Id);
        Assert.Equal(2, afterMidnight?.At.Day);
    }
}
