using System.Windows;
using System.Windows.Controls;
using AzanTimes.Core.Data;
using AzanTimes.Core.Models;

namespace AzanTimes.Views;

public partial class SettingsWindow : Window
{
    private readonly AppSettings _settings;
    private readonly Dictionary<string, CheckBox> _events = [];

    public SettingsWindow(AppSettings settings, CityCatalog cities)
    {
        InitializeComponent();
        _settings = settings;

        var ordered = cities.Ordered();
        CityBox.ItemsSource = ordered;
        CityBox.SelectedItem = ordered.FirstOrDefault(city =>
            string.Equals(city.Id, settings.CityId, StringComparison.OrdinalIgnoreCase))
            ?? cities.GetOrDefault("tehran");

        PopupEnabledBox.IsChecked = settings.PopupEnabled;
        WidgetVisibleBox.IsChecked = settings.WidgetVisible;
        TopmostBox.IsChecked = settings.WidgetTopmost;
        StartupBox.IsChecked = settings.StartWithWindows;

        foreach (var kind in PrayerKinds.All)
        {
            var box = new CheckBox
            {
                Content = kind.Title,
                IsChecked = settings.PopupEvents.Contains(kind.Id),
                Margin = new Thickness(0, 0, 8, 8)
            };
            _events[kind.Id] = box;
            EventGrid.Children.Add(box);
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (CityBox.SelectedItem is not City city)
            return;

        _settings.CityId = city.Id;
        _settings.PopupEnabled = PopupEnabledBox.IsChecked == true;
        _settings.PopupEvents = _events.Where(pair => pair.Value.IsChecked == true).Select(pair => pair.Key).ToList();
        _settings.WidgetVisible = WidgetVisibleBox.IsChecked == true;
        _settings.WidgetTopmost = TopmostBox.IsChecked == true;
        _settings.StartWithWindows = StartupBox.IsChecked == true;
        DialogResult = true;
    }
}
