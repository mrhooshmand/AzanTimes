using AzanTimes.Core.Data;
using AzanTimes.Core.Models;
using AzanTimes.Core.Prayer;

namespace AzanTimes.Core.Tests;

public class PrayerCalculatorTests
{
    private readonly PrayerCalculator _calculator = new();

    [Fact]
    public void Tehran_on_1_November_2026_matches_praytimes()
    {
        var times = Calculate("tehran", new DateTime(2026, 11, 1));

        AssertClock(times.Fajr, 2026, 11, 1, 5, 1);
        AssertClock(times.Sunrise, 2026, 11, 1, 6, 26);
        AssertClock(times.Dhuhr, 2026, 11, 1, 11, 48);
        AssertClock(times.Asr, 2026, 11, 1, 14, 46);
        AssertClock(times.Sunset, 2026, 11, 1, 17, 9);
        AssertClock(times.Maghrib, 2026, 11, 1, 17, 28);
        AssertClock(times.Isha, 2026, 11, 1, 18, 16);
        AssertClock(times.Midnight, 2026, 11, 1, 23, 5);
    }

    [Fact]
    public void Tehran_on_summer_solstice_2026_matches_praytimes()
    {
        var times = Calculate("tehran", new DateTime(2026, 6, 21));

        AssertClock(times.Fajr, 2026, 6, 21, 3, 2);
        AssertClock(times.Sunrise, 2026, 6, 21, 4, 49);
        AssertClock(times.Dhuhr, 2026, 6, 21, 12, 6);
        AssertClock(times.Asr, 2026, 6, 21, 15, 55);
        AssertClock(times.Sunset, 2026, 6, 21, 19, 24);
        AssertClock(times.Maghrib, 2026, 6, 21, 19, 45);
        AssertClock(times.Isha, 2026, 6, 21, 20, 44);
        AssertClock(times.Midnight, 2026, 6, 21, 23, 13);
    }

    [Fact]
    public void Mashhad_on_1_November_2026_matches_praytimes()
    {
        var times = Calculate("mashhad", new DateTime(2026, 11, 1));

        AssertClock(times.Fajr, 2026, 11, 1, 4, 29);
        AssertClock(times.Sunrise, 2026, 11, 1, 5, 54);
        AssertClock(times.Dhuhr, 2026, 11, 1, 11, 15);
        AssertClock(times.Asr, 2026, 11, 1, 14, 13);
        AssertClock(times.Sunset, 2026, 11, 1, 16, 36);
        AssertClock(times.Maghrib, 2026, 11, 1, 16, 54);
        AssertClock(times.Isha, 2026, 11, 1, 17, 43);
        AssertClock(times.Midnight, 2026, 11, 1, 22, 32);
    }

    private DayPrayerTimes Calculate(string cityId, DateTime date)
    {
        var city = CityCatalog.Load().GetOrDefault(cityId);
        return _calculator.Calculate(city, date);
    }

    private static void AssertClock(DateTime actual, int year, int month, int day, int hour, int minute)
    {
        var expected = new DateTime(year, month, day, hour, minute, 0);
        Assert.Equal(expected, actual);
    }
}
