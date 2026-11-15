using UnityEngine;
using UnityEngine.UI;

// The job board: a list of jobs with their requirements. It opens when the job board channel is raised.
// The player applies when they have enough projects.
public class JobBoardPanel : MonoBehaviour
{
    [SerializeField] private JobBoardChannel jobBoardOpened;

    private GameObject root;
    private RectTransform list;
    private Text feedbackText;

    private void Awake()
    {
        Canvas canvas = UIBuilder.CreateCanvas("Job Board Panel", 10);
        Transform panel = UIBuilder.CreatePanel(
            canvas.transform, new Color(0f, 0f, 0f, 0.92f), new Vector2(1100, 800), Vector2.zero, "JobBoard");
        root = canvas.gameObject;

        UIBuilder.CreateText(panel, "JOB BOARD", 56, new Vector2(0, 340), new Vector2(900, 90));
        list = UIBuilder.CreatePanel(panel, new Color(1f, 1f, 1f, 0f), new Vector2(1000, 420), new Vector2(0, 60), "List");
        feedbackText = UIBuilder.CreateText(panel, "", 32, new Vector2(0, -220), new Vector2(1000, 80));
        UIBuilder.CreateButton(panel, "Leave", new Vector2(0, -330), new Vector2(260, 90), Close);
        root.SetActive(false);
    }

    private void OnEnable()
    {
        jobBoardOpened.Raised += Open;
    }

    private void OnDisable()
    {
        jobBoardOpened.Raised -= Open;
    }

    private void Open(JobOffer[] jobs)
    {
        for (int i = list.childCount - 1; i >= 0; i--)
        {
            Destroy(list.GetChild(i).gameObject);
        }

        GameSession session = GameSession.Instance;
        float y = 150f;
        foreach (JobOffer job in jobs)
        {
            if (job == null || session.HasInterviewed(job))
            {
                continue;
            }

            JobOffer chosen = job;
            string label = job.title + "   |   salary " + job.salary + "/month   |   needs "
                           + job.requiredProjects + " projects and interview score " + job.requiredScore + "+";
            UIBuilder.CreateButton(list, label, new Vector2(0, y), new Vector2(1000, 110), () => Apply(chosen));
            y -= 130f;
        }

        feedbackText.text = "You have " + session.Projects + " project point(s).";
        session.SetPaused(true);
        root.SetActive(true);
    }

    private void Apply(JobOffer job)
    {
        GameSession session = GameSession.Instance;
        if (session.Projects < job.requiredProjects)
        {
            feedbackText.text = "Not yet for " + job.title + ": you have " + session.Projects + " of "
                                + job.requiredProjects + " project points.";
            return;
        }

        session.SelectedJob = job;
        Scenes.Load(Scenes.Interview);
    }

    private void Close()
    {
        GameSession.Instance.SetPaused(false);
        root.SetActive(false);
    }
}
