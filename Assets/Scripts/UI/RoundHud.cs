using UnityEngine;
using UnityEngine.UI;

// Top of the city screen: time left, projects and connections.
// It only listens to the GameSession events. The game never calls it.
public class RoundHud : MonoBehaviour
{
    private Text timerText;
    private Text statsText;
    private GameSession session;
    private int shownSeconds = -1;

    private void Awake()
    {
        Canvas canvas = UIBuilder.CreateCanvas("Round HUD", 0);
        timerText = UIBuilder.CreateText(canvas.transform, "", 44, new Vector2(0, 480), new Vector2(800, 60));
        statsText = UIBuilder.CreateText(canvas.transform, "", 34, new Vector2(0, 425), new Vector2(1200, 50));
    }

    private void OnEnable()
    {
        session = GameSession.Instance;
        session.TimeChanged += ShowTime;
        session.ProjectsChanged += OnStatsChanged;
        session.ConnectionsChanged += OnStatsChanged;

        ShowTime(session.TimeLeft);
        ShowStats();
    }

    private void OnDisable()
    {
        if (session == null)
        {
            return;
        }

        session.TimeChanged -= ShowTime;
        session.ProjectsChanged -= OnStatsChanged;
        session.ConnectionsChanged -= OnStatsChanged;
    }

    private void ShowTime(float timeLeft)
    {
        // The event arrives every frame, but the text only changes once a second.
        int seconds = Mathf.CeilToInt(timeLeft);
        if (seconds == shownSeconds)
        {
            return;
        }

        shownSeconds = seconds;
        timerText.text = "Time  " + (seconds / 60) + ":" + (seconds % 60).ToString("00");
    }

    private void OnStatsChanged(int newValue)
    {
        ShowStats();
    }

    private void ShowStats()
    {
        statsText.text = "Projects " + session.Projects + "      Connections " + session.Connections;
    }
}
