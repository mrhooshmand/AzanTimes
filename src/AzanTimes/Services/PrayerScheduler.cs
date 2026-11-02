using System.Windows.Threading;
using AzanTimes.Core.Data;
using AzanTimes.Core.Models;
using AzanTimes.Core.Prayer;
using AzanTimes.Core.Scheduling;
using AzanTimes.Core.Settings;
using AzanTimes.Core.Time;

namespace AzanTimes.Services;

internal sealed class PrayerScheduler : IDisposable
{
    private readonly DispatcherTimer _timer;
    private readonly CityCatalog _cities;
    private readonly PrayerCalculator _calculator;
    private readonly AppSettings _settings;
    private readonly SettingsStore _store;
    private readonly Action<City, DayPrayerTimes, DayPrayerTimes, DateTime> _onTick;
    private readonly Action<City, PrayerMoment> _onDue;

    public PrayerScheduler(
        CityCatalog cities,
        PrayerCalculator calculator,
        AppSettings settings,
        SettingsStore store,
        Action<City, DayPrayerTimes, DayPrayerTimes, DateTime> onTick,
        Action<City, PrayerMoment> onDue)
    {
        _cities = cities;
        _calculator = calculator;
        _settings = settings;
        _store = store;
        _onTick = onTick;
        _onDue = onDue;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(15) };
        _timer.Tick += (_, _) => Refresh();
    }

    public void Start()
    {
        Refresh();
        _timer.Start();
    }

    public void RefreshNow() => Refresh();

    public void Dispose() => _timer.Stop();

    private void Refresh()
    {
        var now = IranClock.Now;
        var city = _cities.GetOrDefault(_settings.CityId);
        var today = _calculator.Calculate(city, now.Date);
        var tomorrow = _calculator.Calculate(city, now.Date.AddDays(1));
        _onTick(city, today, tomorrow, now);

        if (!_settings.PopupEnabled)
            return;

        var due = PopupSchedule.Due(today, now, _settings.PopupEvents, _settings.NotifiedKeys);
        if (due.Count == 0)
            return;

        foreach (var moment in due)
            _settings.NotifiedKeys.Add(PopupSchedule.Key(moment));

        PruneNotifiedKeys(now);
        _store.Save(_settings);
        _onDue(city, due[^1]);
    }

    private void PruneNotifiedKeys(DateTime now)
    {
        var cutoff = now.Date.AddDays(-2).ToString("yyyy-MM-dd");
        _settings.NotifiedKeys.RemoveAll(key => string.CompareOrdinal(key, cutoff) < 0);
    }
}
