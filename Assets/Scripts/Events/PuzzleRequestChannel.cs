using System;
using UnityEngine;

// A request to show a puzzle. "Solved" is called once, when the player answers correctly.
public class PuzzleRequest
{
    public PuzzleRequest(int difficulty, Action solved)
    {
        Difficulty = difficulty;
        Solved = solved;
    }

    public int Difficulty { get; }

    public Action Solved { get; }
}

// Raised by a PuzzleStation in the world. The puzzle panel (UI) listens.
[CreateAssetMenu(menuName = "Next Interview/Events/Puzzle Request Channel")]
public class PuzzleRequestChannel : EventChannel<PuzzleRequest>
{
}
