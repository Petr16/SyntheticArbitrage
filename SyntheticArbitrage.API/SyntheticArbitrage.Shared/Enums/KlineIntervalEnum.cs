namespace SyntheticArbitrage.Shared.Enums;

public enum KlineIntervalEnum
{
    OneMinute,
    ThreeMinutes,
    FiveMinutes,
    FifteenMinutes,
    ThirtyMinutes,
    OneHour,
    TwoHours,
    FourHours,
    SixHours,
    EightHours,
    TwelveHours,
    OneDay,
    ThreeDays,
    OneWeek,
    OneMonth
}

public static class KlineIntervalExtensions
{
    public static string ToIntervalString(this KlineIntervalEnum interval)
    {
        switch(interval)
        {
            case KlineIntervalEnum.OneMinute: return "1m";
            case KlineIntervalEnum.ThreeMinutes: return "3m";
            case KlineIntervalEnum.FiveMinutes: return "5m";
            case KlineIntervalEnum.FifteenMinutes: return "15m";
            case KlineIntervalEnum.ThirtyMinutes: return "30m";
            case KlineIntervalEnum.OneHour: return "1h";
            case KlineIntervalEnum.TwoHours: return "2h";
            case KlineIntervalEnum.FourHours: return "4h";
            case KlineIntervalEnum.SixHours: return "6h";
            case KlineIntervalEnum.EightHours: return "8h";
            case KlineIntervalEnum.TwelveHours: return "12h";
            case KlineIntervalEnum.OneDay: return "1d";
            case KlineIntervalEnum.ThreeDays: return "3d";
            case KlineIntervalEnum.OneWeek: return "1w";
            case KlineIntervalEnum.OneMonth: return "1M";
            default: throw new ArgumentOutOfRangeException(nameof(interval), "Unsupported interval");
        };
    }
}

