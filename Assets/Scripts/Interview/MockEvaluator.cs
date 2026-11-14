using System.Collections.Generic;
using UnityEngine;

// Offline evaluator, works without internet. It checks how many key words of the question
// appear in the answer, and rewards longer answers.
public class MockEvaluator : IInterviewEvaluator
{
    private const float CoverageWeight = 0.6f;
    private const float DepthWeight = 0.4f;
    private const float WordsForFullDepth = 40f;
    private const int MinKeywordLength = 4;

    public void Score(MonoBehaviour runner, string question, string answer, System.Action<int> onScored)
    {
        string[] keywords = Words(question);
        string[] given = Words(answer);

        int hits = 0;
        foreach (string word in keywords)
        {
            if (System.Array.IndexOf(given, word) >= 0)
            {
                hits++;
            }
        }

        float coverage = keywords.Length == 0 ? 0f : (float)hits / keywords.Length;
        float depth = Mathf.Clamp01(given.Length / WordsForFullDepth);
        onScored(Mathf.RoundToInt(100f * (CoverageWeight * coverage + DepthWeight * depth)));
    }

    private static string[] Words(string text)
    {
        var list = new List<string>();
        char[] separators = { ' ', ',', '.', '?', '!', '\n', '\r', '\t' };
        foreach (string word in text.ToLowerInvariant().Split(separators, System.StringSplitOptions.RemoveEmptyEntries))
        {
            if (word.Length >= MinKeywordLength)
            {
                list.Add(word);
            }
        }

        return list.ToArray();
    }
}
