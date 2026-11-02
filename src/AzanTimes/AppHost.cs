using System.Windows;
using AzanTimes.Core.Data;
using AzanTimes.Core.Models;
using AzanTimes.Core.Prayer;
using AzanTimes.Core.Scheduling;
using AzanTimes.Core.Settings;
using AzanTimes.Services;
using AzanTimes.Tray;
using AzanTimes.Views;

namespace AzanTimes;

internal sealed class AppHost : IDisposable
{
    private readonly Application _app;
    private readonly SettingsStore _store = new();
    private readonly CityCatalog _cities = CityCatalog.Load();
    private readonly PrayerCalculator _calculator = new();
    private readonly AppSettings _settings;
    private Mutex? _mutex;
    private WidgetWindow? _widget;
    private PopupWindow? _popup;
    private TrayController? _tray;
    private PrayerScheduler? _scheduler;
    private bool _disposed;

    public AppHost(Application app)
    {
        _app = app;
        _settings = _store.Load();
    }

    public bool Start()
    {
        _mutex = new Mutex(true, "AzanTimes.SingleInstance", out var created);
        if (!created)
        {
            _mutex.Dispose();
            _mutex = null;
            return false;
        }

        StartupRegistration.Apply(_settings.StartWithWindows);

        _tray = new TrayController();
        _tray.ShowWidgetRequested += () => ShowWidget(save: true);
        _tray.SettingsRequested += OpenSettings;
        _tray.SamplePopupRequested += ShowSamplePopup;
        _tray.ExitRequested += Exit;

        if (_settings.WidgetVisible)
            ShowWidget(save: false);

        _scheduler = new PrayerScheduler(
            _cities,
            _calculator,
            _settings,
            _store,
            OnTick,
            (city, moment) => ShowPopup(city, moment, sample: false));
        _scheduler.Start();
        _app.SessionEnding += (_, _) => Exit();
        return true;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _scheduler?.Dispose();
        if (_widget is not null)
        {
            _widget.AllowClose();
            _widget.Close();
        }

        _popup?.Close();
        _tray?.Dispose();

        if (_mutex is null)
            return;

        try
        {
            _mutex.ReleaseMutex();
        }
        catch (ApplicationException)
        {
        }

        _mutex.Dispose();
        _mutex = null;
    }

    private void EnsureWidget()
    {
        if (_widget is not null)
            return;

        _widget = new WidgetWindow();
        _widget.Place(_settings.WidgetLeft, _settings.WidgetTop);
        _widget.HideRequested += () =>
        {
            _settings.WidgetVisible = false;
            _store.Save(_settings);
            _widget.Hide();
        };
    }

    private void ShowWidget(bool save)
    {
        EnsureWidget();
        _settings.WidgetVisible = true;
        if (save)
            _store.Save(_settings);

        _widget!.ApplyChrome(_settings);
        _widget.Show();
        _scheduler?.RefreshNow();
    }

    private void OpenSettings()
    {
        var window = new SettingsWindow(_settings, _cities)
        {
            Topmost = true
        };
        if (_widget is { IsVisible: true })
            window.Owner = _widget;

        if (window.ShowDialog() != true)
            return;

        _store.Save(_settings);
        StartupRegistration.Apply(_settings.StartWithWindows);
        _widget?.ApplyChrome(_settings);
        if (_settings.WidgetVisible)
            ShowWidget(save: false);
        else
            _widget?.Hide();

        _scheduler?.RefreshNow();
    }

    private void ShowSamplePopup()
    {
        var now = Core.Time.IranClock.Now;
        var city = _cities.GetOrDefault(_settings.CityId);
        var today = _calculator.Calculate(city, now.Date);
        var tomorrow = _calculator.Calculate(city, now.Date.AddDays(1));
        var moment = NextPrayer.Find(now, today, tomorrow) ?? today.Moments[0];
        ShowPopup(city, moment, sample: true);
    }

    private void ShowPopup(City city, PrayerMoment moment, bool sample)
    {
        _popup?.Close();
        var popup = new PopupWindow(city, moment, sample);
        _popup = popup;
        popup.Closed += (_, _) =>
        {
            if (ReferenceEquals(_popup, popup))
                _popup = null;
        };
        popup.Show();
    }

    private void OnTick(City city, DayPrayerTimes today, DayPrayerTimes tomorrow, DateTime now)
    {
        if (_widget is { IsVisible: true })
            _widget.Update(city, today, tomorrow, now);

        var next = NextPrayer.Find(now, today, tomorrow);
        var text = next is null
            ? $"AzanTimes - {city.Name}"
            : $"AzanTimes | {city.Name} | {next.Title} {next.At:HH:mm}";
        _tray?.SetTooltip(text);
    }

    private void Exit()
    {
        Dispose();
        _app.Shutdown();
    }
}
