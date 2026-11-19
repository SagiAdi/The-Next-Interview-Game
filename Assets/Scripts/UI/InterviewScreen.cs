using UnityEngine;
using UnityEngine.UI;

// The interview screen: shows the question, takes the answer, and shows the scores.
// It listens to the InterviewSession events and sends it the answers.
public class InterviewScreen : MonoBehaviour
{
    [SerializeField] private InterviewSession interview;

    private Text questionText;
    private Text feedbackText;
    private InputField answerField;
    private Button submitButton;

    private void Awake()
    {
        if (interview == null)
        {
            interview = FindAnyObjectByType<InterviewSession>();
        }

        Canvas canvas = UIBuilder.CreateCanvas("Interview Screen", 0);
        Transform t = canvas.transform;
        UIBuilder.CreatePanel(t, new Color(0.08f, 0.1f, 0.16f), new Vector2(4000, 4000), Vector2.zero, "Background");

        JobOffer job = GameSession.Instance.SelectedJob;
        string header = job == null
            ? ""
            : "Interview for " + job.title + "   (salary " + job.salary + "/month, needs " + GameSession.Instance.RequiredScore(job) + "+)";
        UIBuilder.CreateText(t, header, 40, new Vector2(0, 440), new Vector2(1700, 80));

        questionText = UIBuilder.CreateText(t, "", 40, new Vector2(0, 240), new Vector2(1600, 200));
        answerField = UIBuilder.CreateInputField(t, new Vector2(0, -60), new Vector2(1500, 300));
        submitButton = UIBuilder.CreateButton(t, "Submit answer", new Vector2(0, -280), new Vector2(380, 110), Submit);
        feedbackText = UIBuilder.CreateText(t, "", 36, new Vector2(0, -400), new Vector2(1500, 80));
    }

    private void OnEnable()
    {
        interview.QuestionAsked += ShowQuestion;
        interview.AnswerRejected += ShowRejection;
        interview.AnswerScored += ShowScore;
        interview.Finished += ShowResult;
    }

    private void OnDisable()
    {
        interview.QuestionAsked -= ShowQuestion;
        interview.AnswerRejected -= ShowRejection;
        interview.AnswerScored -= ShowScore;
        interview.Finished -= ShowResult;
    }

    private void Submit()
    {
        submitButton.interactable = false;
        feedbackText.text = "Evaluating...";
        interview.SubmitAnswer(answerField.text);
    }

    private void ShowQuestion(int number, int total, string question)
    {
        questionText.text = "Question " + number + " of " + total + "\n\n" + question;
        answerField.text = "";
        submitButton.interactable = true;
    }

    private void ShowRejection(string reason)
    {
        feedbackText.text = reason;
        submitButton.interactable = true;
    }

    private void ShowScore(int score)
    {
        feedbackText.text = "Answer score: " + score + " / 100";
    }

    private void ShowResult(InterviewResult result)
    {
        questionText.text = "Interview complete. Making the decision...";
        answerField.text = "";
        submitButton.interactable = false;
        feedbackText.text = "Average " + result.Average + " + connections bonus " + result.Bonus
                            + " = final score " + result.Final + " (needed " + result.Required + ")";
    }
}
