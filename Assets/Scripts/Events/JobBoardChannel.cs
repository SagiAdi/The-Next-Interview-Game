using UnityEngine;

// Raised by the job board zone in the world, with the jobs it offers. The job board panel (UI) listens.
[CreateAssetMenu(menuName = "Next Interview/Events/Job Board Channel")]
public class JobBoardChannel : EventChannel<JobOffer[]>
{
}
