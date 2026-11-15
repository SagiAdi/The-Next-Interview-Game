using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

// Sends the answer to YOUR server, which calls the AI model and returns a score.
// Never put an AI API key inside the game: anyone can read it in a WebGL or APK build.
// Expected server reply: {"score": 75}
public class AiEvaluator : IInterviewEvaluator
{
    private const int TimeoutSeconds = 15;

    private readonly string endpoint;

    public AiEvaluator(string endpoint)
    {
        this.endpoint = endpoint;
    }

    public void Score(MonoBehaviour runner, string question, string answer, System.Action<int> onScored)
    {
        runner.StartCoroutine(Send(question, answer, onScored));
    }

    private IEnumerator Send(string question, string answer, System.Action<int> onScored)
    {
        string body = JsonUtility.ToJson(new ScoreRequest { question = question, answer = answer });

        using (var request = new UnityWebRequest(endpoint, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = TimeoutSeconds;

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
}
