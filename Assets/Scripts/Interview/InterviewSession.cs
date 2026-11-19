using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The rules of the interview: asks the questions one by one, scores every answer,
// and decides at the end. Final score = average answer score + connections bonus.
// It knows nothing about the screen: the InterviewScreen listens to the events below.
public class InterviewSession : MonoBehaviour
{
    [SerializeField, Tooltip("Optional: URL of your own server that scores answers with AI. Empty = offline evaluator.")]
    private string aiEndpoint = "";

    [SerializeField]
    private string[] questions =
    {
        "Tell me about a project you are proud of and what you built.",
        "Why do you want this job, and what do you know about the company?",
        "Describe how you handle a difficult problem inside a team.",
    };

    [SerializeField] private int minAnswerLength = 10;

    [SerializeField, Tooltip("Seconds the final decision stays on screen before the next scene loads.")]
    private float decisionSeconds = 3f;

    private readonly List<int> scores = new List<int>();
    private readonly IInterviewEvaluator offlineEvaluator = new MockEvaluator();
    private IInterviewEvaluator evaluator;
    private JobOffer job;
    private int index;
    private bool waitingForScore;

    // Question number (from 1), number of questions, question text.
    public event System.Action<int, int, string> QuestionAsked;
    public event System.Action<string> AnswerRejected;
    public event System.Action<int> AnswerScored;
    public event System.Action<InterviewResult> Finished;

    private void Start()
    {
        job = GameSession.Instance.SelectedJob;
        if (job == null)
        {
            Scenes.Load(Scenes.City);
            return;
        }

        evaluator = string.IsNullOrEmpty(aiEndpoint) ? offlineEvaluator : new AiEvaluator(aiEndpoint);
        AskCurrentQuestion();
    }

    // Called by the screen when the player submits an answer.
    public void SubmitAnswer(string answer)
    {
        if (job == null || waitingForScore || index >= questions.Length)
        {
            return;
        }

        answer = answer.Trim();
        if (answer.Length < minAnswerLength)
        {
            AnswerRejected?.Invoke("Write a full sentence or two so we can evaluate you.");
            return;
        }

        waitingForScore = true;
        string question = questions[index];
        evaluator.Score(this, question, answer, score =>
        {
            if (score >= 0)
            {
                OnScored(score);
            }
            else
            {
                // The AI server failed: fall back to the offline evaluator.
                offlineEvaluator.Score(this, question, answer, OnScored);
            }
        });
    }

    private void AskCurrentQuestion()
    {
        QuestionAsked?.Invoke(index + 1, questions.Length, questions[index]);
    }

    private void OnScored(int score)
    {
        waitingForScore = false;
        scores.Add(score);
        AnswerScored?.Invoke(score);
        index++;

        if (index < questions.Length)
        {
            AskCurrentQuestion();
        }
        else
        {
            StartCoroutine(Decide());
        }
    }

    private IEnumerator Decide()
    {
        GameSession session = GameSession.Instance;

        int sum = 0;
        foreach (int score in scores)
        {
            sum += score;
        }

        var result = new InterviewResult();
        result.Average = sum / scores.Count;
        result.Bonus = session.ConnectionBonus;
        result.Final = Mathf.Min(100, result.Average + result.Bonus);
        result.Required = session.RequiredScore(job);
        result.Hired = result.Final >= result.Required;
        Finished?.Invoke(result);

        yield return new WaitForSeconds(decisionSeconds);

        session.MarkInterviewed(job);
        if (result.Hired)
        {
            session.ResultText = "Hired as " + job.title + "! Salary: " + job.salary + " per month. Final score: "
                                 + result.Final + " (needed " + result.Required + ").";
            Scenes.Load(Scenes.Win);
        }
        else
        {
            session.SelectedJob = null;
            session.PendingMessage = "Rejected by " + job.title + ". Score " + result.Final + ", needed "
                                     + result.Required + ". Keep building!";
            Scenes.Load(Scenes.City);
        }
    }
}
