using System.Globalization;

namespace ProjectTimer.Core;

public static class Format
{
    public static string Hms(TimeSpan value)
    {
        if (value.TotalHours >= 100)
            return ((int)value.TotalHours).ToString(CultureInfo.InvariantCulture);

        return string.Create(
            CultureInfo.InvariantCulture,
            $"{(int)value.TotalHours:00}:{value.Minutes:00}:{value.Seconds:00}");
    }

    public static string Long(TimeSpan value)
    {
        if (value <= TimeSpan.Zero)
            return "0 min";

        if (value.TotalHours >= 1)
            return $"{(int)value.TotalHours}h {value.Minutes:00}m";

        return $"{(int)value.TotalMinutes} min";
    }
}