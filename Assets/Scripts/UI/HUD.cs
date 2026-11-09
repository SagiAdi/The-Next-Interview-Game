using UnityEngine;
using UnityEngine.UI;

// City screen: timer, stats, messages, the "Interact" hint, and on-screen controls for phones.
public class HUD : MonoBehaviour
{
    static HUD instance;

    Text timerText;
    Text statsText;
    Text messageText;
    Text promptText;
    float messageLeft;
    bool roundEnded;
    PlayerController2D player;

    public static void Say(string message)
    {
        if (instance != null) instance.Show(message);
    }

    void Awake()
    {
        instance = this;
        var canvas = UIBuilder.CreateCanvas("HUD", 0);
        var t = canvas.transform;

        timerText = UIBuilder.CreateText(t, "", 44, new Vector2(0, 480), new Vector2(800, 60));
        statsText = UIBuilder.CreateText(t, "", 34, new Vector2(0, 425), new Vector2(1200, 50));
        promptText = UIBuilder.CreateText(t, "", 36, new Vector2(0, -220), new Vector2(900, 60));
        messageText = UIBuilder.CreateText(t, "", 36, new Vector2(0, -400), new Vector2(1500, 60));

        // On-screen joystick (bottom left) and interact button (bottom right)
        var stick = UIBuilder.CreatePanel(t, new Color(1f, 1f, 1f, 0.2f), new Vector2(260, 260), new Vector2(-760, -330), "Joystick");
        stick.gameObject.AddComponent<VirtualJoystick>();
        UIBuilder.CreatePanel(stick, new Color(1f, 1f, 1f, 0.5f), new Vector2(110, 110), Vector2.zero, "Knob")
            .GetComponent<Image>().raycastTarget = false;
        // Keep the Interact button in the bottom-right corner, so it is never cut off
        Button interactButton = UIBuilder.CreateButton(t, "Interact (E)", new Vector2(760, -330), new Vector2(300, 140), Interact);
        RectTransform buttonRect = interactButton.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1f, 0f);
        buttonRect.anchorMax = new Vector2(1f, 0f);
        buttonRect.pivot = new Vector2(1f, 0f);
        buttonRect.anchoredPosition = new Vector2(-30f, 30f);

        var data = GameData.Instance;
        if (!string.IsNullOrEmpty(data.PendingMessage))
        {
            Show(data.PendingMessage);
            data.PendingMessage = "";
        }
    }

    void Update()
    {
        var data = GameData.Instance;
        if (!PlayerController2D.Frozen && !roundEnded) data.Tick(Time.deltaTime);

        int secs = Mathf.CeilToInt(data.TimeLeft);
        timerText.text = "Time  " + (secs / 60) + ":" + (secs % 60).ToString("00");
        statsText.text = "Projects " + data.Projects + "      Connections " + data.ActiveConnections();

        // Show a hint when something can be used nearby
        if (player == null) player = FindFirstObjectByType<PlayerController2D>();
        Interactable near = (player != null && !PlayerController2D.Frozen) ? player.FindNearest() : null;
        promptText.text = near != null ? "Interact: " + near.Prompt : "";

        if (messageLeft > 0f)
        {
            messageLeft -= Time.deltaTime;
            if (messageLeft <= 0f) messageText.text = "";
        }

        if (!roundEnded && data.TimeLeft <= 0f)
        {
            roundEnded = true;
            Scenes.Load(Scenes.GameOver);
        }
    }

    void Interact()
    {
        if (player != null) player.TryInteract();
    }

    void Show(string message)
    {
        messageText.text = message;
        messageLeft = 4f;
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }
}
