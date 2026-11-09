using UnityEngine;

// A math puzzle spawned in the puzzle room. Solving it gives project points.
public class PuzzleStation : Interactable
{
    int difficulty;

    public override string Prompt => "Solve puzzle";

    void Awake()
    {
        difficulty = Random.Range(1, 4); // 1 = easy, 3 = hard (hard gives more points)
    }

    public override void Interact(PlayerController2D player)
    {
        PuzzlePanel.Get().Open(difficulty, Solved);
    }

    void Solved()
    {
        GameData.Instance.AddProject(difficulty);
        HUD.Say("Project completed! +" + difficulty + " project point(s).");
        Destroy(gameObject);
    }
}
