using System.Collections.Generic;
using UnityEngine;

// Survives scene changes (DontDestroyOnLoad singleton).
// Holds everything the player earned in the current round.
public class GameData : MonoBehaviour
{
    public const float RoundSeconds = 300f;      // 5 minutes per round
    public const float ConnectionSeconds = 90f;  // how long a contact stays connected

    static GameData instance;

    // Created on first use, so no scene has to contain it.
    public static GameData Instance
    {
        get
        {
            if (instance == null) new GameObject("GameData").AddComponent<GameData>();
            return instance;
        }
    }

    public int Projects { get; private set; }
    public float TimeLeft { get; private set; }
    public JobOffer SelectedJob;
    public string Result = "";
    public string PendingMessage = "";
    public readonly HashSet<JobOffer> Interviewed = new HashSet<JobOffer>();

    readonly List<float> connectionExpiry = new List<float>();

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        ResetRound();
    }

    public void ResetRound()
    {
        Projects = 0;
        TimeLeft = RoundSeconds;
        SelectedJob = null;
        Result = "";
        PendingMessage = "";
        Interviewed.Clear();
        connectionExpiry.Clear();
    }

    public void Tick(float seconds) => TimeLeft = Mathf.Max(0f, TimeLeft - seconds);

    public void AddProject(int points) => Projects += points;

    public void AddConnection() => connectionExpiry.Add(Time.realtimeSinceStartup + ConnectionSeconds);

    public int ActiveConnections()
    {
        float now = Time.realtimeSinceStartup;
        connectionExpiry.RemoveAll(expiry => expiry <= now);
        return connectionExpiry.Count;
    }
}
