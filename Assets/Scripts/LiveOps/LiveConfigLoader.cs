using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

// Downloads the live config (a small JSON file) when the game starts.
// If the download fails, it falls back to Resources/LiveConfig.json, which ships inside the build.
// It only fetches and parses. What the numbers mean is decided by GameSession.
public class LiveConfigLoader : MonoBehaviour
{
    private const string FallbackResource = "LiveConfig";

    [SerializeField, Tooltip("Address of the config file. Raw GitHub files allow requests from a browser.")]
    private string url = "https://raw.githubusercontent.com/SagiAdi/The-Next-Interview-Game/main/liveops/config.json";

    [SerializeField] private int timeoutSeconds = 5;

    private bool loading;

    // Raised every time a config is ready, from the server or from the fallback.
    public event Action<LiveConfig> Loaded;

    public void Refresh()
    {
        if (!loading)
        {
            StartCoroutine(Download());
        }
    }

    private IEnumerator Download()
    {
        loading = true;

        // The time in the address makes every request unique, so no cache keeps an old config.
        string address = url + "?t=" + DateTime.UtcNow.Ticks;
        using (UnityWebRequest request = UnityWebRequest.Get(address))
        {
            request.timeout = timeoutSeconds;
            yield return request.SendWebRequest();

            LiveConfig config = null;
            if (request.result == UnityWebRequest.Result.Success)
            {
                config = LiveConfig.Parse(request.downloadHandler.text);
            }

            if (config == null)
            {
                Debug.LogWarning("Live config download failed (" + request.error + "). Using the built-in file.");
                config = LoadFallback();
            }

            Loaded?.Invoke(config);
        }

        loading = false;
    }

    private static LiveConfig LoadFallback()
    {
        TextAsset file = Resources.Load<TextAsset>(FallbackResource);
        LiveConfig config = file != null ? LiveConfig.Parse(file.text) : null;
        return config ?? new LiveConfig();
    }
}
