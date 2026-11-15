using UnityEngine;
using UnityEngine.UI;

// Pop-up with a multiple-choice math question. It opens when a puzzle is requested on the channel,
// and tells the request when the player picked the correct answer.
public class PuzzlePanel : MonoBehaviour
{
    private const int OptionCount = 4;

    [SerializeField] private PuzzleRequestChannel puzzleRequests;

    private readonly Text[] optionLabels = new Text[OptionCount];
    private GameObject root;
    private Text questionText;
    private Text feedbackText;
    private PuzzleRequest current;
    private int correctIndex;

    private void Awake()
    {
        Canvas canvas = UIBuilder.CreateCanvas("Puzzle Panel", 10);
        Transform panel = UIBuilder.CreatePanel(
            canvas.transform, new Color(0f, 0f, 0f, 0.9f), new Vector2(1000, 650), Vector2.zero, "Puzzle");
        root = canvas.gameObject;

        UIBuilder.CreateText(panel, "Solve the puzzle to complete a project", 40, new Vector2(0, 250), new Vector2(900, 80));
        questionText = UIBuilder.CreateText(panel, "", 46, new Vector2(0, 150), new Vector2(900, 100));

        for (int i = 0; i < OptionCount; i++)
        {
            int choice = i;
            var pos = new Vector2(i % 2 == 0 ? -240f : 240f, i < 2 ? 20f : -90f);
            Button button = UIBuilder.CreateButton(panel, "", pos, new Vector2(440, 100), () => Choose(choice));
            optionLabels[i] = button.GetComponentInChildren<Text>();
        }

        feedbackText = UIBuilder.CreateText(panel, "", 32, new Vector2(0, -190), new Vector2(900, 80));
        UIBuilder.CreateButton(panel, "Close", new Vector2(0, -280), new Vector2(260, 90), Close);
        root.SetActive(false);
    }

    private void OnEnable()
    {
        puzzleRequests.Raised += Open;
    }

    private void OnDisable()
    {
        puzzleRequests.Raised -= Open;
    }

    private void Open(PuzzleRequest request)
    {
        current = request;
        PuzzleQuestion puzzle = PuzzleGenerator.Generate(request.Difficulty);
        questionText.text = puzzle.question;
        for (int i = 0; i < OptionCount; i++)
        {
            optionLabels[i].text = puzzle.options[i];
        }

        correctIndex = puzzle.correctIndex;
        feedbackText.text = "Pick the correct answer.";

        GameSession.Instance.SetPaused(true);
        root.SetActive(true);
    }

    private void Choose(int choice)
    {
        if (choice != correctIndex)
        {
            feedbackText.text = "Not quite. Try again.";
            return;
        }

        PuzzleRequest solved = current;
        Close();
        solved.Solved?.Invoke();
    }

    private void Close()
    {
        current = null;
        GameSession.Instance.SetPaused(false);
        root.SetActive(false);
    }
}
