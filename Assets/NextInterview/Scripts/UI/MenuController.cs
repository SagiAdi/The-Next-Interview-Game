using UnityEngine;

// Title screen with the instructions and a Start button.
public class MenuController : MonoBehaviour
{
    void Start()
    {
        var canvas = UIBuilder.CreateCanvas("MenuCanvas", 0);
        UIBuilder.CreatePanel(canvas.transform, new Color(0.08f, 0.1f, 0.16f), new Vector2(1920, 1080), Vector2.zero, "Background");

        UIBuilder.CreateText(canvas.transform, "The Next Interview", 96, new Vector2(0, 300), new Vector2(1600, 140));
        UIBuilder.CreateText(canvas.transform,
            "Every job starts with one project. The interview is where you prove it.", 40,
            new Vector2(0, 190), new Vector2(1500, 100));
        UIBuilder.CreateText(canvas.transform,
            "Move: arrow keys / WASD, or the on-screen stick.\n" +
            "Interact (E or button): solve puzzles for projects, talk to contacts for connections.\n" +
            "Walk into the Job Board to apply once you have enough projects.\n" +
            "You have 5 minutes to get hired.", 34,
            new Vector2(0, -40), new Vector2(1500, 320));

        UIBuilder.CreateButton(canvas.transform, "Start", new Vector2(0, -320), new Vector2(360, 110), () =>
        {
            GameData.Instance.ResetRound();
            Scenes.Load(Scenes.City);
        });
    }
}
