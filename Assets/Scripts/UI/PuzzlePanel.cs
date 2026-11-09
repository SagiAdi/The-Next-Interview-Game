using UnityEngine;
using UnityEngine.UI;

// Pop-up with a multiple-choice math question. Correct answer = project completed.
public class PuzzlePanel : MonoBehaviour
{
    static PuzzlePanel instance;

    GameObject root;
    Text question;
    Text feedback;
    readonly Text[] labels = new Text[4];
    System.Action onSolved;
    int correct;

    public static PuzzlePanel Get()
    {
        if (instance == null) instance = new GameObject("PuzzlePanel").AddComponent<PuzzlePanel>();
        return instance;
    }

    void Awake()
    {
        var canvas = UIBuilder.CreateCanvas("PuzzleCanvas", 10);
        root = UIBuilder.CreatePanel(canvas.transform, new Color(0f, 0f, 0f, 0.9f), new Vector2(1000, 650), Vector2.zero, "Puzzle").gameObject;

        UIBuilder.CreateText(root.transform, "Solve the puzzle to complete a project", 40, new Vector2(0, 250), new Vector2(900, 80));
        question = UIBuilder.CreateText(root.transform, "", 46, new Vector2(0, 150), new Vector2(900, 100));

        for (int i = 0; i < 4; i++)
        {
            int choice = i;
            var pos = new Vector2(i % 2 == 0 ? -240f : 240f, i < 2 ? 20f : -90f);
            var button = UIBuilder.CreateButton(root.transform, "", pos, new Vector2(440, 100), () => Choose(choice));
            labels[i] = button.GetComponentInChildren<Text>();
        }

        feedback = UIBuilder.CreateText(root.transform, "", 32, new Vector2(0, -190), new Vector2(900, 80));
        UIBuilder.CreateButton(root.transform, "Close", new Vector2(0, -280), new Vector2(260, 90), Close);
        root.SetActive(false);
    }

    public void Open(int difficulty, System.Action solved)
    {
        onSolved = solved;
        PuzzleQuestion q = PuzzleGenerator.Generate(difficulty);
        question.text = q.question;
        for (int i = 0; i < 4; i++) labels[i].text = q.options[i];
        correct = q.correctIndex;
        feedback.text = "Pick the correct answer.";

        PlayerController2D.Frozen = true;
        root.SetActive(true);
    }

    void Choose(int choice)
    {
        if (choice != correct)
        {
            feedback.text = "Not quite. Try again.";
            return;
        }

        var done = onSolved;
        Close();
        done?.Invoke();
    }

    void Close()
    {
        onSolved = null;
        PlayerController2D.Frozen = false;
        root.SetActive(false);
    }
}
