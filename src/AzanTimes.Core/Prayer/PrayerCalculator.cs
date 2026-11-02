using AzanTimes.Core.Models;

namespace AzanTimes.Core.Prayer;

/// <summary>
/// اوقات شرعی به روش مؤسسه ژئوفیزیک دانشگاه تهران.
/// زاویه‌ها و گرد کردن دقیقه مطابق PrayTimes 2.5 است.
/// https://praytimes.org
/// </summary>
public sealed class PrayerCalculator
{
    private const double TimeZoneHours = 3.5;

    public DayPrayerTimes Calculate(City city, DateTime localDate)
    {
        var date = localDate.Date;
        var context = new SolarContext(
            city.Latitude,
            city.Longitude,
            Julian(date.Year, date.Month, date.Day) - city.Longitude / 360.0);

        var estimate = new RawTimes(5, 5, 6, 12, 13, 18, 18, 18);
        var times = ComputePrayerTimes(context, estimate.Divide(24));
        times = Adjust(context, times);
        var midnight = times.Sunset + FixHour((times.Fajr + 24) - times.Sunset) / 2.0;

        return new DayPrayerTimes(
        [
            Moment("fajr", date, times.Fajr),
            Moment("sunrise", date, times.Sunrise),
            Moment("dhuhr", date, times.Dhuhr),
            Moment("asr", date, times.Asr),
            Moment("sunset", date, times.Sunset),
            Moment("maghrib", date, times.Maghrib),
            Moment("isha", date, times.Isha),
            Moment("midnight", date, midnight)
        ]);
    }

    private static PrayerMoment Moment(string id, DateTime date, double hours) =>
        new(id, PrayerKinds.TitleOf(id), ToClock(date, hours));

    private static RawTimes ComputePrayerTimes(SolarContext context, RawTimes portion)
    {
        return new RawTimes(
            SunAngleTime(context, 10, portion.Imsak, ccw: true),
            SunAngleTime(context, 17.7, portion.Fajr, ccw: true),
            SunAngleTime(context, RiseSetAngle(), portion.Sunrise, ccw: true),
            MidDay(context, portion.Dhuhr),
            AsrTime(context, 1, portion.Asr),
            SunAngleTime(context, RiseSetAngle(), portion.Sunset, ccw: false),
            SunAngleTime(context, 4.5, portion.Maghrib, ccw: false),
            SunAngleTime(context, 14, portion.Isha, ccw: false));
    }

    private static RawTimes Adjust(SolarContext context, RawTimes times)
    {
        times = times.Shift(TimeZoneHours - context.Longitude / 15.0);
        times = AdjustHighLatitudes(times);
        times = times with { Imsak = times.Fajr - 10.0 / 60.0 };
        return times;
    }

    private static RawTimes AdjustHighLatitudes(RawTimes times)
    {
        var night = FixHour(times.Sunrise - times.Sunset);
        return times with
        {
            Imsak = AdjustHighLatitude(times.Imsak, times.Sunrise, night, ccw: true),
            Fajr = AdjustHighLatitude(times.Fajr, times.Sunrise, night, ccw: true),
            Isha = AdjustHighLatitude(times.Isha, times.Sunset, night, ccw: false),
            Maghrib = AdjustHighLatitude(times.Maghrib, times.Sunset, night, ccw: false)
        };
    }

    private static double AdjustHighLatitude(double time, double basis, double night, bool ccw)
    {
        var portion = night / 2.0;
        var difference = ccw ? FixHour(basis - time) : FixHour(time - basis);
        if (double.IsNaN(time) || difference > portion)
            time = basis + (ccw ? -portion : portion);

        return time;
    }

    private static double AsrTime(SolarContext context, double factor, double time)
    {
        var declination = SunPosition(context.JulianDate + time).Declination;
        var angle = -ArcCot(factor + Tan(Math.Abs(context.Latitude - declination)));
        return SunAngleTime(context, angle, time, ccw: false);
    }

    private static double SunAngleTime(SolarContext context, double angle, double time, bool ccw)
    {
        var declination = SunPosition(context.JulianDate + time).Declination;
        var noon = MidDay(context, time);
        var cosine = (-Sin(angle) - Sin(declination) * Sin(context.Latitude))
            / (Cos(declination) * Cos(context.Latitude));
        var offset = ArcCos(cosine) / 15.0;
        return noon + (ccw ? -offset : offset);
    }

    private static double MidDay(SolarContext context, double time)
    {
        var equation = SunPosition(context.JulianDate + time).Equation;
        return FixHour(12 - equation);
    }

    private static (double Declination, double Equation) SunPosition(double julianDate)
    {
        var days = julianDate - 2451545.0;
        var anomaly = FixAngle(357.529 + 0.98560028 * days);
        var meanLongitude = FixAngle(280.459 + 0.98564736 * days);
        var eclipticLongitude = FixAngle(meanLongitude + 1.915 * Sin(anomaly) + 0.020 * Sin(2 * anomaly));
        var obliquity = 23.439 - 0.00000036 * days;
        var rightAscension = ArcTan2(Cos(obliquity) * Sin(eclipticLongitude), Cos(eclipticLongitude)) / 15.0;
        var equation = meanLongitude / 15.0 - FixHour(rightAscension);
        var declination = ArcSin(Sin(obliquity) * Sin(eclipticLongitude));
        return (declination, equation);
    }

    private static double Julian(int year, int month, int day)
    {
        if (month <= 2)
        {
            year -= 1;
            month += 12;
        }

        var century = Math.Floor(year / 100.0);
        var gregorian = 2 - century + Math.Floor(century / 4.0);
        return Math.Floor(365.25 * (year + 4716))
            + Math.Floor(30.6001 * (month + 1))
            + day
            + gregorian
            - 1524.5;
    }

    private static double RiseSetAngle() => 0.833;

    private static DateTime ToClock(DateTime date, double hours)
    {
        var rounded = hours + 0.5 / 60.0;
        var day = date.Date;
        while (rounded < 0)
        {
            rounded += 24;
            day = day.AddDays(-1);
        }

        while (rounded >= 24)
        {
            rounded -= 24;
            day = day.AddDays(1);
        }

        var hour = (int)Math.Floor(rounded);
        var minute = (int)Math.Floor((rounded - hour) * 60.0);
        if (minute >= 60)
        {
            minute -= 60;
            hour++;
        }

        if (hour >= 24)
        {
            hour -= 24;
            day = day.AddDays(1);
        }

        return day.AddHours(hour).AddMinutes(minute);
    }

    private static double Sin(double degrees) => Math.Sin(degrees * Math.PI / 180.0);

    private static double Cos(double degrees) => Math.Cos(degrees * Math.PI / 180.0);

    private static double Tan(double degrees) => Math.Tan(degrees * Math.PI / 180.0);

    private static double ArcSin(double value) => Degrees(Math.Asin(value));

    private static double ArcCos(double value) => Degrees(Math.Acos(Math.Clamp(value, -1, 1)));

    private static double ArcTan2(double y, double x) => Degrees(Math.Atan2(y, x));

    private static double ArcCot(double value) => Degrees(Math.Atan(1 / value));

    private static double Degrees(double radians) => radians * 180.0 / Math.PI;

    private static double FixAngle(double angle) => Fix(angle, 360);

    private static double FixHour(double hours) => Fix(hours, 24);

    private static double Fix(double value, double period)
    {
        value -= period * Math.Floor(value / period);
        return value < 0 ? value + period : value;
    }

    private readonly record struct SolarContext(double Latitude, double Longitude, double JulianDate);

    private readonly record struct RawTimes(
        double Imsak,
        double Fajr,
        double Sunrise,
        double Dhuhr,
        double Asr,
        double Sunset,
        double Maghrib,
        double Isha)
    {
        public RawTimes Divide(double divisor) => new(
            Imsak / divisor,
            Fajr / divisor,
            Sunrise / divisor,
            Dhuhr / divisor,
            Asr / divisor,
            Sunset / divisor,
            Maghrib / divisor,
            Isha / divisor);

        public RawTimes Shift(double hours) => new(
            Imsak + hours,
            Fajr + hours,
            Sunrise + hours,
            Dhuhr + hours,
            Asr + hours,
            Sunset + hours,
            Maghrib + hours,
            Isha + hours);
    }
}
