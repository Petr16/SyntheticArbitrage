namespace SyntheticArbitrage.Infrastructure.Utils;

public class DateTimeUtil
{
    public static long GetUnixTimestampMs(DateTime dateTime)
    {
        return new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();
    }

    public static DateTime? GetDT(long? unixTimestamp)
    {
        if (unixTimestamp.HasValue)
            return DateTimeOffset.FromUnixTimeSeconds(unixTimestamp.Value).UtcDateTime;
        return null;
    }
}
