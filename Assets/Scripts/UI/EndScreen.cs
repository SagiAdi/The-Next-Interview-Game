using UnityEngine;

// Used by the Win and GameOver scenes. The setup wizard sets "won" for each scene.
public class EndScreen : MonoBehaviour
{
    [SerializeField, Tooltip("True in the Win scene, false in the GameOver scene.")]
    private bool won;

    // Used by the setup wizard.
    public void Setup(bool isWin)
    {
        won = isWin;
    }

    void Start()
    {
        var canvas = UIBuilder.CreateCanvas("EndCanvas", 0);
        UIBuilder.CreatePanel(canvas.transform, new Color(0.08f, 0.1f, 0.16f), new Vector2(1920, 1080), Vector2.zero, "Background");

        string title = won ? "You got the job!" : "Time is up";
        string body = won
            ? GameData.Instance.Result
            : "No offer this round. Build more projects and connections, then try again.";

        UIBuilder.CreateText(canvas.transform, title, 90, new Vector2(0, 250), new Vector2(1600, 140));
        UIBuilder.CreateText(canvas.transform, body, 40, new Vector2(0, 0), new Vector2(1500, 300));
        UIBuilder.CreateButton(canvas.transform, "Play again", new Vector2(0, -330), new Vector2(360, 110), () =>
        {
            GameData.Instance.ResetRound();
            Scenes.Load(Scenes.City);
        });
    }
}
