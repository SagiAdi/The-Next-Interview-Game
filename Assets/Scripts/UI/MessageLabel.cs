using UnityEngine;
using UnityEngine.UI;

// Shows the short messages that the game raises on the message channel, for a few seconds each.
public class MessageLabel : MonoBehaviour
{
    [SerializeField] private MessageChannel messages;
    [SerializeField] private float visibleSeconds = 4f;

    private Text label;
    private float hideTime;

    private void Awake()
    {
        Canvas canvas = UIBuilder.CreateCanvas("Messages", 0);
        label = UIBuilder.CreateText(canvas.transform, "", 36, new Vector2(0, -400), new Vector2(1500, 60));
    }

    private void OnEnable()
    {
        messages.Raised += Show;
    }

    private void OnDisable()
    {
        messages.Raised -= Show;
    }

    private void Start()
    {
        // A message left by the previous scene, for example "Rejected by ...".
        GameSession session = GameSession.Instance;
        if (!string.IsNullOrEmpty(session.PendingMessage))
        {
            Show(session.PendingMessage);
            session.PendingMessage = "";
        }
    }

    private void Update()
    {
        if (label.text.Length > 0 && Time.time >= hideTime)
        {
            label.text = "";
        }
    }

    private void Show(string message)
    {
        label.text = message;
        hideTime = Time.time + visibleSeconds;
    }
}
