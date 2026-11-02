using AzanTimes.Core.Formatting;

namespace AzanTimes.Core.Tests;

public class PersianFormatTests
{
    [Fact]
    public void Time_uses_persian_digits()
    {
        var text = PersianFormat.Time(new DateTime(2026, 11, 1, 5, 7, 0));
        Assert.Equal("۰۵:۰۷", text);
    }

    [Fact]
    public void Date_formats_10_aban_1405()
    {
        var text = PersianFormat.Date(new DateTime(2026, 11, 1));
        Assert.Equal("۱۰ آبان ۱۴۰۵", text);
    }

    [Fact]
    public void Countdown_joins_hours_and_minutes()
    {
        Assert.Equal("۲ ساعت و ۵ دقیقه", PersianFormat.Countdown(new TimeSpan(2, 5, 0)));
        Assert.Equal("کمتر از یک دقیقه", PersianFormat.Countdown(TimeSpan.FromSeconds(20)));
    }
}
