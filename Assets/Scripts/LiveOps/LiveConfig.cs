using System;
using UnityEngine;

// The whole remote config file. Today it holds one event; new sections can be added later
// without breaking old builds, because JsonUtility ignores fields it does not know.
[Serializable]
public class LiveConfig
{
    public int version = 1;
    public LiveEvent liveEvent = new LiveEvent();

    // Returns null when the text is not valid JSON, so the caller can use its fallback.
    public static LiveConfig Parse(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonUtility.FromJson<LiveConfig>(json);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }
}
