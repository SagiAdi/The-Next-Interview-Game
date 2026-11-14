using UnityEngine;

// The job board in the city. Using it asks the UI to show the list of jobs.
public class JobBoardZone : Interactable
{
    [SerializeField] private JobBoardChannel jobBoardOpened;

    [SerializeField, Tooltip("The jobs on this board. Create more with Right click > Create > Next Interview > Job Offer.")]
    private JobOffer[] jobs;

    public override string Prompt => "Job board";

    public override void Interact()
    {
        jobBoardOpened.Raise(jobs);
    }
}
