using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AzanTimes.Core.Formatting;
using AzanTimes.Core.Models;
using AzanTimes.Core.Scheduling;

namespace AzanTimes.Views;

public partial class WidgetWindow : Window
{
    private double? _left;
    private double? _top;
    private bool _allowClose;

    public WidgetWindow()
    {
        InitializeComponent();
    }

    public event Action? HideRequested;

    public event Action<double, double>? PositionChanged;

    public void Place(double? left, double? top)
    {
        _left = left;
        _top = top;
    }

    public void ApplyChrome(AppSettings settings)
    {
        Topmost = settings.WidgetTopmost;
    }

    public void AllowClose() => _allowClose = true;

    public void Update(City city, DayPrayerTimes today, DayPrayerTimes tomorrow, DateTime now)
    {
        CityDateText.Text = $"{city.Name}   ·   {PersianFormat.Date(now)}";
        var next = NextPrayer.Find(now, today, tomorrow);
        TimesPanel.Children.Clear();

        foreach (var moment in today.Moments)
        {
            var isNext = next?.Id == moment.Id && next.At.Date == moment.At.Date;
            TimesPanel.Children.Add(CreateRow(moment, isNext));
        }

        NextText.Text = next is null
            ? ""
            : $"{next.Title} تا {PersianFormat.Countdown(next.At - now)}";
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (_allowClose)
        {
            base.OnClosing(e);
            return;
        }

        e.Cancel = true;
        Dispatcher.BeginInvoke(new Action(() => HideRequested?.Invoke()));
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var width = ActualWidth > 0 ? ActualWidth : 300;
        var height = ActualHeight > 0 ? ActualHeight : 360;
        if (_left is double left && _top is double top && IsOnScreen(left, top, width, height))
        {
            Left = left;
            Top = top;
            return;
        }

        var area = SystemParameters.WorkArea;
        Left = area.Right - width - 16;
        Top = area.Top + 16;
    }

    private void Card_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is DependencyObject source && IsInsideButton(source))
            return;

        try
        {
            DragMove();
            PositionChanged?.Invoke(Left, Top);
        }
        catch (InvalidOperationException)
        {
        }
    }

    private void Hide_Click(object sender, RoutedEventArgs e) => HideRequested?.Invoke();

    private static UIElement CreateRow(PrayerMoment moment, bool highlight)
    {
        var name = new TextBlock
        {
            Text = moment.Title,
            Foreground = new SolidColorBrush(highlight ? Color.FromRgb(15, 108, 189) : Color.FromRgb(31, 35, 40)),
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 13
        };
        var time = new TextBlock
        {
            Text = PersianFormat.Time(moment.At),
            Foreground = new SolidColorBrush(highlight ? Color.FromRgb(15, 108, 189) : Color.FromRgb(31, 35, 40)),
            FontWeight = FontWeights.SemiBold,
            FontSize = 13,
            VerticalAlignment = VerticalAlignment.Center
        };

        var grid = new Grid { FlowDirection = FlowDirection.RightToLeft };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        Grid.SetColumn(name, 0);
        Grid.SetColumn(time, 1);
        grid.Children.Add(name);
        grid.Children.Add(time);

        return new Border
        {
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(10, 6, 10, 6),
            Margin = new Thickness(0, 1, 0, 1),
            Background = highlight
                ? new SolidColorBrush(Color.FromRgb(239, 246, 255))
                : Brushes.Transparent,
            Child = grid
        };
    }

    private static bool IsInsideButton(DependencyObject source)
    {
        while (source is not null)
        {
            if (source is Button)
                return true;

            source = VisualTreeHelper.GetParent(source);
        }

        return false;
    }

    private static bool IsOnScreen(double left, double top, double width, double height)
    {
        var rect = new Rect(left, top, width, height);
        var screen = new Rect(
            SystemParameters.VirtualScreenLeft,
            SystemParameters.VirtualScreenTop,
            SystemParameters.VirtualScreenWidth,
            SystemParameters.VirtualScreenHeight);
        return screen.IntersectsWith(rect);
    }
}
