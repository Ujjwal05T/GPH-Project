// GPH/Helpers/TimeZoneHelper.cs
namespace GPH.Helpers;

public static class TimeZoneHelper
{
    private static readonly TimeZoneInfo IndianStandardTime = GetIndianTimeZone();

    private static TimeZoneInfo GetIndianTimeZone()
    {
        try
        {
            // For Windows
            return TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            // For Linux/macOS
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
        }
    }

    /// <summary>
    /// Converts a UTC DateTime to Indian Standard Time (IST).
    /// </summary>
    /// <param name="utcDateTime">The DateTime in UTC.</param>
    /// <returns>The corresponding DateTime in IST.</returns>
    public static DateTime ConvertUtcToIst(DateTime utcDateTime)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, IndianStandardTime);
    }

    /// <summary>
    /// Gets the current time in Indian Standard Time (IST).
    /// </summary>
    /// <returns>The current DateTime in IST.</returns>
    public static DateTime GetCurrentIstTime()
    {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IndianStandardTime);
    }
}