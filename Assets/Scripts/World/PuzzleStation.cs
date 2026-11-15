using UnityEngine;

// A math puzzle in the puzzle room. Solving it gives project points.
public class PuzzleStation : Interactable
{
    [SerializeField] private PuzzleRequestChannel puzzleRequests;
    [SerializeField] private MessageChannel messages;

    [SerializeField, Tooltip("1 = easy, 3 = hard. Harder puzzles give more project points.")]
    private int minDifficulty = 1;

    [SerializeField] private int maxDifficulty = 3;

    private int difficulty;

    public override string Prompt => "Solve puzzle";

    private void Awake()
    {
        difficulty = Random.Range(minDifficulty, maxDifficulty + 1);
    }

    public override void Interact()
    {
        puzzleRequests.Raise(new PuzzleRequest(difficulty, OnSolved));
    }

    private void OnSolved()
    {
        GameSession.Instance.AddProject(difficulty);
        messages.Raise("Project completed! +" + difficulty + " project point(s).");
        Destroy(gameObject);
    }
}
