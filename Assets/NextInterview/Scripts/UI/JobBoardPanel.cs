using UnityEngine;

// The job board: a list of jobs with their requirements. Apply when you have enough projects.
public class JobBoardPanel : MonoBehaviour
{
    static JobBoardPanel instance;

    GameObject root;
    RectTransform list;

    public static JobBoardPanel Get()
    {
        if (instance == null) instance = new GameObject("JobBoardPanel").AddComponent<JobBoardPanel>();
        return instance;
    }

    void Awake()
    {
        var canvas = UIBuilder.CreateCanvas("JobBoardCanvas", 10);
        root = UIBuilder.CreatePanel(canvas.transform, new Color(0f, 0f, 0f, 0.92f), new Vector2(1100, 800), Vector2.zero, "JobBoard").gameObject;

        UIBuilder.CreateText(root.transform, "JOB BOARD", 56, new Vector2(0, 340), new Vector2(900, 90));
        list = UIBuilder.CreatePanel(root.transform, new Color(1f, 1f, 1f, 0f), new Vector2(1000, 520), new Vector2(0, -10), "List");
        UIBuilder.CreateButton(root.transform, "Leave", new Vector2(0, -340), new Vector2(260, 90), Close);
        root.SetActive(false);
    }

    public void Open(JobOffer[] jobs)
    {
        for (int i = list.childCount - 1; i >= 0; i--) Destroy(list.GetChild(i).gameObject);

        float y = 200f;
        foreach (JobOffer job in jobs)
        {
            if (job == null || GameData.Instance.Interviewed.Contains(job)) continue;

            JobOffer chosen = job;
            string label = job.title + "   |   salary " + job.salary + "/month   |   needs "
                           + job.requiredProjects + " projects and interview score " + job.requiredScore + "+";
            UIBuilder.CreateButton(list, label, new Vector2(0, y), new Vector2(1000, 110), () => Apply(chosen));
            y -= 130f;
        }

        PlayerController2D.Frozen = true;
        root.SetActive(true);
    }

    void Apply(JobOffer job)
    {
        var data = GameData.Instance;
        if (data.Projects < job.requiredProjects)
        {
            HUD.Say("Not yet for " + job.title + ": you have " + data.Projects + " of " + job.requiredProjects + " project points.");
            return;
        }

        data.SelectedJob = job;
        Scenes.Load(Scenes.Interview);
    }

    void Close()
    {
        root.SetActive(false);
        PlayerController2D.Frozen = false;
    }
}
