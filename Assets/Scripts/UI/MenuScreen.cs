using UnityEngine;
using UnityEngine.UI;

// Title screen with the instructions and a Start button.
// It also announces the live event that comes from the remote config.
public class MenuScreen : MonoBehaviour
{
    private Text eventText;
    private GameSession session;

    private void OnEnable()
    {
        session = GameSession.Instance;
        session.LiveEventAvailable += ShowEvent;
    }

    private void OnDisable()
    {
        if (session != null)
        {
            session.LiveEventAvailable -= ShowEvent;
        }
    }

    private void Start()
    {
        Canvas canvas = UIBuilder.CreateCanvas("Menu", 0);
        Transform t = canvas.transform;
        UIBuilder.CreatePanel(t, new Color(0.08f, 0.1f, 0.16f), new Vector2(4000, 4000), Vector2.zero, "Background");

        UIBuilder.CreateText(t, "The Next Interview", 96, new Vector2(0, 330), new Vector2(1600, 140));
        UIBuilder.CreateText(
            t, "Every job starts with one project. The interview is where you prove it.", 40,
            new Vector2(0, 220), new Vector2(1500, 100));
        UIBuilder.CreateText(
            t,
            "Move: arrow keys / WASD, or the on-screen stick.\n" +
            "Face a puzzle or a person and press E (or the Interact button).\n" +
            "Puzzles give projects. People give connections, a bonus in the interview.\n" +
            "Dodge the red distractions: every hit costs you time.\n" +
            "Use the Job Board when you have enough projects. You have 5 minutes to get hired.",
            34, new Vector2(0, -30), new Vector2(1500, 360));

        eventText = UIBuilder.CreateText(t, "", 34, new Vector2(0, -190), new Vector2(1700, 110));
        eventText.color = new Color(1f, 0.85f, 0.3f);
        ShowEvent(session.PendingEvent);

        UIBuilder.CreateButton(t, "Start", new Vector2(0, -330), new Vector2(360, 110), StartGame);
    }

    private void ShowEvent(LiveEvent liveEvent)
    {
        if (eventText == null)
        {
            return;
        }

        bool active = liveEvent.IsActive(System.DateTime.UtcNow);
        eventText.text = active ? "LIVE EVENT: " + liveEvent.title + "\n" + liveEvent.message : "";
    }

    private void StartGame()
    {
        GameSession.Instance.StartRound();
        Scenes.Load(Scenes.City);
    }
}
