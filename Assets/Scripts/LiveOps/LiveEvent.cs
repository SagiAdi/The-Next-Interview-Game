using System;
using System.Globalization;

// One live event, described in the remote config file. All the numbers are neutral by default
// (multiplier 1, delta 0), so an event only changes the values that its JSON mentions.
[Serializable]
public class LiveEvent
{
    public bool enabled;
    public string id = "";
    public string title = "";
    public string message = "";

    // ISO 8601 time in UTC, for example 2026-11-20T00:00:00Z. Empty means no limit.
    public string startUtc = "";
    public string endUtc = "";

    public float roundSecondsMultiplier = 1f;
    public float connectionSecondsMultiplier = 1f;

    // 0 means: keep the value from GameSettings.
    public int bonusPerConnection;
    public int maxConnectionBonus;

    public float distractionSpeedMultiplier = 1f;
    public int requiredProjectsDelta;
    public int requiredScoreDelta;

    // The event is on when it is enabled and the current time is inside its time window.
    public bool IsActive(DateTime utcNow)
    {
        if (!enabled)
        {
            return false;
        }

        if (TryParseUtc(startUtc, out DateTime start) && utcNow < start)
        {
            return false;
        }

        if (TryParseUtc(endUtc, out DateTime end) && utcNow >= end)
        {
            return false;
        }

        return true;
    }

    private static bool TryParseUtc(string text, out DateTime time)
    {
        time = default;
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        return DateTime.TryParse(
            text, CultureInfo.InvariantCulture,
            DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out time);
    }
}
