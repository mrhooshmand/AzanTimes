using System.Globalization;

namespace AzanTimes.Core.Formatting;

public static class PersianFormat
{
    private static readonly string[] Months =
    [
        "",
        "فروردین",
        "اردیبهشت",
        "خرداد",
        "تیر",
        "مرداد",
        "شهریور",
        "مهر",
        "آبان",
        "آذر",
        "دی",
        "بهمن",
        "اسفند"
    ];

    public static string Digits(string value)
    {
        var chars = value.ToCharArray();
        for (var index = 0; index < chars.Length; index++)
        {
            if (chars[index] is >= '0' and <= '9')
                chars[index] = (char)('۰' + (chars[index] - '0'));
        }

        return new string(chars);
    }

    public static string Time(DateTime value) =>
        Digits(value.ToString("HH:mm", CultureInfo.InvariantCulture));

    public static string Date(DateTime value)
    {
        var calendar = new PersianCalendar();
        var day = calendar.GetDayOfMonth(value);
        var month = Months[calendar.GetMonth(value)];
        var year = calendar.GetYear(value);
        return Digits($"{day} {month} {year}");
    }

    public static string Countdown(TimeSpan remaining)
    {
        if (remaining < TimeSpan.Zero)
            remaining = TimeSpan.Zero;

        if (remaining.TotalMinutes < 1)
            return "کمتر از یک دقیقه";

        var hours = (int)remaining.TotalHours;
        var minutes = remaining.Minutes;
        if (hours <= 0)
            return Digits($"{minutes} دقیقه");

        if (minutes == 0)
            return Digits($"{hours} ساعت");

        return Digits($"{hours} ساعت و {minutes} دقیقه");
    }
}
