using UnityEngine;

// The Win and Game Over screens. The "won" checkbox decides which one this is.
public class EndScreen : MonoBehaviour
{
    [SerializeField, Tooltip("On in the Win scene, off in the GameOver scene.")]
    private bool won;

    private void Start()
    {
        Canvas canvas = UIBuilder.CreateCanvas("End Screen", 0);
        Transform t = canvas.transform;
        UIBuilder.CreatePanel(t, new Color(0.08f, 0.1f, 0.16f), new Vector2(4000, 4000), Vector2.zero, "Background");

        string title = won ? "You got the job!" : "Time is up";
        string body = won
            ? GameSession.Instance.ResultText
            : "No offer this round. Build more projects and connections, then try again.";

        UIBuilder.CreateText(t, title, 90, new Vector2(0, 250), new Vector2(1600, 140));
        UIBuilder.CreateText(t, body, 40, new Vector2(0, 0), new Vector2(1500, 300));
        UIBuilder.CreateButton(t, "Play again", new Vector2(0, -330), new Vector2(360, 110), PlayAgain);
    }

    private void PlayAgain()
    {
        GameSession.Instance.StartRound();
        Scenes.Load(Scenes.City);
    }
}
