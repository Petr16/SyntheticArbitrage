namespace SyntheticArbitrage.Infrastructure.Utils;

public class DateTimeUtil
{
    //Пригодится, если нужно дулать выборку из Binance API по дате unixTimestamp(long)
    //public static long GetUnixTimestampMs(DateTime dateTime)
    //{
    //    return new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();
    //}

    public static DateTime? GetDTFromUnix(long? unixTimestamp)
    {
        if (unixTimestamp.HasValue)
            return DateTimeOffset.FromUnixTimeMilliseconds(unixTimestamp.Value).UtcDateTime;
        return null;
    }
}
