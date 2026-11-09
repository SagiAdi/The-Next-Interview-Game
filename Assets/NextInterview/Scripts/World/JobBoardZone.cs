using UnityEngine;

// The job board room. Interact to open the list of jobs.
public class JobBoardZone : Interactable
{
    [SerializeField, Tooltip("The jobs shown on this board. Create more with Right click > Next Interview > Job Offer.")]
    private JobOffer[] jobs;

    public override string Prompt => "Job board";

    // Used by the setup wizard.
    public void Setup(JobOffer[] jobs)
    {
        this.jobs = jobs;
    }

    public override void Interact(PlayerController2D player)
    {
        JobBoardPanel.Get().Open(jobs);
    }
}
