using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

// Scores one interview answer from 0 to 100.
// onScored receives -1 when the evaluation failed (the manager then uses the offline evaluator).
public interface IInterviewEvaluator
{
    void Score(MonoBehaviour runner, string question, string answer, System.Action<int> onScored);
}

// Offline evaluator, works without internet. It checks how many key words of the question
// appear in the answer, and rewards longer answers.
public class MockEvaluator : IInterviewEvaluator
{
    public void Score(MonoBehaviour runner, string question, string answer, System.Action<int> onScored)
    {
        string[] keywords = Words(question);
        string[] given = Words(answer);

        int hits = 0;
        foreach (string word in keywords)
        {
            if (System.Array.IndexOf(given, word) >= 0) hits++;
        }

        float coverage = keywords.Length == 0 ? 0f : (float)hits / keywords.Length;
        float depth = Mathf.Clamp01(given.Length / 40f);
        onScored(Mathf.RoundToInt(100f * (0.6f * coverage + 0.4f * depth)));
    }

    static string[] Words(string text)
    {
        var list = new System.Collections.Generic.List<string>();
        char[] separators = { ' ', ',', '.', '?', '!', '\n', '\r', '\t' };
        foreach (string w in text.ToLowerInvariant().Split(separators, System.StringSplitOptions.RemoveEmptyEntries))
        {
            if (w.Length > 3) list.Add(w);
        }
        return list.ToArray();
    }
}

// Sends the answer to YOUR server, which calls the AI model and returns a score.
// Never put an AI API key inside the game: anyone can read it in a WebGL or APK build.
// Expected server reply: {"score": 75}
public class AiEvaluator : IInterviewEvaluator
{
    readonly string endpoint;

    public AiEvaluator(string endpoint)
    {
        this.endpoint = endpoint;
    }

    [System.Serializable]
    public class ScoreRequest
    {
        public string question;
        public string answer;
    }

    [System.Serializable]
    public class ScoreReply
    {
        public int score;
    }

    public void Score(MonoBehaviour runner, string question, string answer, System.Action<int> onScored)
    {
        runner.StartCoroutine(Send(question, answer, onScored));
    }

    IEnumerator Send(string question, string answer, System.Action<int> onScored)
    {
        string body = JsonUtility.ToJson(new ScoreRequest { question = question, answer = answer });

        using (var request = new UnityWebRequest(endpoint, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 15;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onScored(-1);
                yield break;
            }

            // A bad reply must not leave the interview stuck, so parse inside try/catch.
            try
            {
                int score = JsonUtility.FromJson<ScoreReply>(request.downloadHandler.text).score;
                onScored(Mathf.Clamp(score, 0, 100));
            }
            catch (System.Exception)
            {
                onScored(-1);
            }
        }
    }
}
