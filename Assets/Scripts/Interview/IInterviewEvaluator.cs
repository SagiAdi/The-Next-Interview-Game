using UnityEngine;

// Scores one interview answer from 0 to 100.
// onScored receives -1 when the evaluation failed (the interview then uses the offline evaluator).
public interface IInterviewEvaluator
{
    void Score(MonoBehaviour runner, string question, string answer, System.Action<int> onScored);
}
