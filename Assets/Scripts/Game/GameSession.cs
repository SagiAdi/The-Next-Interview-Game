using System;
using System.Collections.Generic;
using UnityEngine;

// The state of the current round: projects, connections, time left and the chosen job.
// This is the only singleton in the game. The state has to survive scene changes
// (City -> Interview -> City or Win), so it lives on a DontDestroyOnLoad object.
public class GameSession : MonoBehaviour
{
    private const string SettingsResource = "GameSettings";
    private const float LiveRefreshSeconds = 60f;

    private static GameSession instance;

    // Each connection is stored as the value of TimeLeft at which it ends.
    private readonly List<float> connectionEndTimes = new List<float>();
    private readonly HashSet<JobOffer> interviewedJobs = new HashSet<JobOffer>();
    private bool timeUpRaised;
    private LiveConfigLoader liveConfigLoader;
    private LiveEvent pendingEvent = new LiveEvent();
    private readonly LiveEvent neutralEvent = new LiveEvent();
    private LiveEvent roundEvent;
    private float lastLiveRefresh = float.NegativeInfinity;

    public event Action<int> ProjectsChanged;
    public event Action<int> ConnectionsChanged;
    public event Action<float> TimeChanged;
    public event Action TimeUp;

    // Raised when a new live config arrives. It is used by the menu to announce the event.
    public event Action<LiveEvent> LiveEventAvailable;

    // Created on first use, so every scene can be played directly from the editor.
    public static GameSession Instance
    {
        get
        {
            if (instance == null)
            {
                new GameObject("GameSession").AddComponent<GameSession>();
            }

            return instance;
        }
    }

    public GameSettings Settings { get; private set; }

    public int Projects { get; private set; }

    public float TimeLeft { get; private set; }

    public bool IsPaused { get; private set; }

    public JobOffer SelectedJob { get; set; }

    public string ResultText { get; set; }

    public string PendingMessage { get; set; }

    public int Connections => connectionEndTimes.Count;

    // The live event that was announced most recently. It starts to count at the next round.
    public LiveEvent PendingEvent => pendingEvent;

    // The live event of the round in progress, or null when there is none.
    public LiveEvent ActiveEvent => roundEvent == neutralEvent ? null : roundEvent;

    // The numbers of the round: GameSettings first, then the live event on top.
    public float RoundSeconds => Settings.roundSeconds * RoundEvent.roundSecondsMultiplier;

    public float ConnectionSeconds => Settings.connectionSeconds * RoundEvent.connectionSecondsMultiplier;

    public int BonusPerConnection =>
        RoundEvent.bonusPerConnection > 0 ? RoundEvent.bonusPerConnection : Settings.bonusPerConnection;

    public int MaxConnectionBonus =>
        RoundEvent.maxConnectionBonus > 0 ? RoundEvent.maxConnectionBonus : Settings.maxConnectionBonus;

    public float DistractionSpeedMultiplier => RoundEvent.distractionSpeedMultiplier;

    public int ConnectionBonus => Mathf.Min(MaxConnectionBonus, Connections * BonusPerConnection);

    private LiveEvent RoundEvent => roundEvent;

    public int RequiredProjects(JobOffer job)
    {
        return Mathf.Max(1, job.requiredProjects + RoundEvent.requiredProjectsDelta);
    }

    public int RequiredScore(JobOffer job)
    {
        return Mathf.Clamp(job.requiredScore + RoundEvent.requiredScoreDelta, 0, 100);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        roundEvent = neutralEvent;
        DontDestroyOnLoad(gameObject);

        Settings = Resources.Load<GameSettings>(SettingsResource);
        if (Settings == null)
        {
            Settings = ScriptableObject.CreateInstance<GameSettings>();
        }

        liveConfigLoader = gameObject.AddComponent<LiveConfigLoader>();
        liveConfigLoader.Loaded += OnLiveConfigLoaded;

        StartRound();
    }

    private void OnDestroy()
    {
        if (liveConfigLoader != null)
        {
            liveConfigLoader.Loaded -= OnLiveConfigLoaded;
        }
    }

    private void OnLiveConfigLoaded(LiveConfig config)
    {
        pendingEvent = config.liveEvent ?? new LiveEvent();
        LiveEventAvailable?.Invoke(pendingEvent);
    }

    public void StartRound()
    {
        // The event is fixed for the whole round, so a config that arrives later cannot change the rules in the middle.
        // A disabled or expired event is replaced by the neutral one, which changes nothing.
        roundEvent = pendingEvent.IsActive(DateTime.UtcNow) ? pendingEvent : neutralEvent;
        if (Time.realtimeSinceStartup - lastLiveRefresh > LiveRefreshSeconds)
        {
            lastLiveRefresh = Time.realtimeSinceStartup;
            liveConfigLoader.Refresh();
        }

        Projects = 0;
        TimeLeft = RoundSeconds;
        IsPaused = false;
        SelectedJob = null;
        ResultText = "";
        PendingMessage = "";
        timeUpRaised = false;
        interviewedJobs.Clear();
        connectionEndTimes.Clear();

        ProjectsChanged?.Invoke(Projects);
        ConnectionsChanged?.Invoke(Connections);
        TimeChanged?.Invoke(TimeLeft);
    }

    public void SetPaused(bool paused)
    {
        IsPaused = paused;
    }

    // Takes time off the round clock. Used by the round timer and by time penalties.
    public void SpendTime(float seconds)
    {
        if (IsPaused || timeUpRaised)
        {
            return;
        }

        TimeLeft = Mathf.Max(0f, TimeLeft - seconds);
        TimeChanged?.Invoke(TimeLeft);

        int expired = connectionEndTimes.RemoveAll(endTime => TimeLeft <= endTime);
        if (expired > 0)
        {
            ConnectionsChanged?.Invoke(Connections);
        }

        if (TimeLeft <= 0f)
        {
            timeUpRaised = true;
            TimeUp?.Invoke();
        }
    }

    public void AddProject(int points)
    {
        Projects += points;
        ProjectsChanged?.Invoke(Projects);
    }

    public void AddConnection()
    {
        connectionEndTimes.Add(TimeLeft - ConnectionSeconds);
        ConnectionsChanged?.Invoke(Connections);
    }

    public bool HasInterviewed(JobOffer job)
    {
        return interviewedJobs.Contains(job);
    }

    public void MarkInterviewed(JobOffer job)
    {
        interviewedJobs.Add(job);
    }
}
