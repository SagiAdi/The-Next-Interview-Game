using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// The interview scene: three open questions, one answer at a time.
// The final score = average answer score + a bonus for connections (max +15).
// Reaching the job's required score wins the game. Otherwise the job disappears
// and the player goes back to the city.
public class InterviewManager : MonoBehaviour
{
    [Tooltip("Optional: URL of your own server that scores answers with AI. Leave empty to use the offline evaluator.")]
    [SerializeField] private string aiEndpoint = "";

    static readonly string[] questions =
    {
        "Tell me about a project you are proud of and what you built.",
        "Why do you want this job, and what do you know about the company?",
        "Describe how you handle a difficult problem inside a team."
    };

    IInterviewEvaluator evaluator;
    JobOffer job;
    int index;
    readonly System.Collections.Generic.List<int> scores = new System.Collections.Generic.List<int>();

    Text questionText;
    Text feedbackText;
    InputField answerField;
    Button submitButton;

    void Start()
    {
        job = GameData.Instance.SelectedJob;
        if (job == null)
        {
            Scenes.Load(Scenes.City);
            return;
        }

        evaluator = string.IsNullOrEmpty(aiEndpoint)
            ? (IInterviewEvaluator)new MockEvaluator()
            : new AiEvaluator(aiEndpoint);

        BuildUI();
        ShowQuestion();
    }

    void BuildUI()
    {
        var canvas = UIBuilder.CreateCanvas("InterviewCanvas", 0);
        var t = canvas.transform;

        UIBuilder.CreatePanel(t, new Color(0.08f, 0.1f, 0.16f), new Vector2(1920, 1080), Vector2.zero, "Background");
        UIBuilder.CreateText(t,
            "Interview for " + job.title + "   (salary " + job.salary + "/month, needs " + job.requiredScore + "+)",
            40, new Vector2(0, 440), new Vector2(1700, 80));

        questionText = UIBuilder.CreateText(t, "", 40, new Vector2(0, 240), new Vector2(1600, 200));
        answerField = UIBuilder.CreateInputField(t, new Vector2(0, -60), new Vector2(1500, 300));
        submitButton = UIBuilder.CreateButton(t, "Submit answer", new Vector2(0, -280), new Vector2(380, 110), OnSubmit);
        feedbackText = UIBuilder.CreateText(t, "", 36, new Vector2(0, -400), new Vector2(1500, 80));
    }

    void ShowQuestion()
    {
        questionText.text = "Question " + (index + 1) + " of " + questions.Length + "\n\n" + questions[index];
        answerField.text = "";
        submitButton.interactable = true;
    }

    void OnSubmit()
    {
        string answer = answerField.text.Trim();
        if (answer.Length < 10)
        {
            feedbackText.text = "Write a full sentence or two so we can evaluate you.";
            return;
        }

        submitButton.interactable = false;
        feedbackText.text = "Evaluating...";
        string question = questions[index];

        evaluator.Score(this, question, answer, s =>
        {
            if (s >= 0) OnScored(s);
            else new MockEvaluator().Score(this, question, answer, OnScored); // AI failed: offline fallback
        });
    }

    void OnScored(int score)
    {
        scores.Add(score);
        feedbackText.text = "Answer score: " + score + " / 100";
        index++;

        if (index < questions.Length)
        {
            ShowQuestion();
        }
        else
        {
            questionText.text = "Interview complete. Making the decision...";
            answerField.text = "";
            StartCoroutine(Decide());
        }
    }

    IEnumerator Decide()
    {
        int sum = 0;
        foreach (int s in scores) sum += s;
        int average = sum / scores.Count;
        int bonus = Mathf.Min(15, GameData.Instance.ActiveConnections() * 5);
        int final = Mathf.Min(100, average + bonus);

        feedbackText.text = "Average " + average + " + connections bonus " + bonus + " = final score " + final;
        yield return new WaitForSeconds(3f);

        var data = GameData.Instance;
        data.Interviewed.Add(job);

        if (final >= job.requiredScore)
        {
            data.Result = "Hired as " + job.title + "! Salary: " + job.salary + " per month. Final score: "
                          + final + " (needed " + job.requiredScore + ").";
            Scenes.Load(Scenes.Win);
        }
        else
        {
            data.SelectedJob = null;
            data.PendingMessage = "Rejected by " + job.title + ". Score " + final + ", needed " + job.requiredScore + ". Keep building!";
            Scenes.Load(Scenes.City);
        }
    }
}
