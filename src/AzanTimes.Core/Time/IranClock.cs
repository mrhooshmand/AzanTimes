namespace AzanTimes.Core.Time;

public static class IranClock
{
    public static readonly TimeSpan Offset = TimeSpan.FromMinutes(210);

    public static DateTime Now =>
        DateTime.SpecifyKind(DateTime.UtcNow.Add(Offset), DateTimeKind.Unspecified);
}
