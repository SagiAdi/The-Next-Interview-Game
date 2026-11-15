using System;
using System.Collections.Generic;
using UnityEngine;

// The state of the current round: projects, connections, time left and the chosen job.
// This is the only singleton in the game. The state has to survive scene changes
// (City -> Interview -> City or Win), so it lives on a DontDestroyOnLoad object.
public class GameSession : MonoBehaviour
{
    private const string SettingsResource = "GameSettings";

    private static GameSession instance;

    // Each connection is stored as the value of TimeLeft at which it ends.
    private readonly List<float> connectionEndTimes = new List<float>();
    private readonly HashSet<JobOffer> interviewedJobs = new HashSet<JobOffer>();
    private bool timeUpRaised;

    public event Action<int> ProjectsChanged;
    public event Action<int> ConnectionsChanged;
    public event Action<float> TimeChanged;
    public event Action TimeUp;

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

    public int ConnectionBonus => Mathf.Min(Settings.maxConnectionBonus, Connections * Settings.bonusPerConnection);

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        Settings = Resources.Load<GameSettings>(SettingsResource);
        if (Settings == null)
        {
            Settings = ScriptableObject.CreateInstance<GameSettings>();
        }

        StartRound();
    }

    public void StartRound()
    {
        Projects = 0;
        TimeLeft = Settings.roundSeconds;
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
        connectionEndTimes.Add(TimeLeft - Settings.connectionSeconds);
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
