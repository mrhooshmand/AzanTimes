using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using AzanTimes.Core.Formatting;
using AzanTimes.Core.Models;

namespace AzanTimes.Views;

public partial class PopupWindow : Window
{
    private readonly DispatcherTimer _timer;

    public PopupWindow(City city, PrayerMoment moment, bool sample)
    {
        InitializeComponent();
        TitleText.Text = moment.Title;
        TimeText.Text = PersianFormat.Time(moment.At);
        DetailText.Text = $"{city.Name}   ·   {PersianFormat.Date(moment.At)}";
        SampleText.Visibility = sample ? Visibility.Visible : Visibility.Collapsed;
        Opacity = 0;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(20) };
        _timer.Tick += (_, _) => Close();
        _timer.Start();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var area = SystemParameters.WorkArea;
        Left = area.Right - ActualWidth - 20;
        Top = area.Bottom - ActualHeight - 20;
        BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180)));
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Close();

    protected override void OnClosed(EventArgs e)
    {
        _timer.Stop();
        base.OnClosed(e);
    }
}
