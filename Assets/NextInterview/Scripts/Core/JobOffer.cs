using UnityEngine;

// A job the player can apply for. Create more with: Right click > Next Interview > Job Offer
[CreateAssetMenu(menuName = "Next Interview/Job Offer")]
public class JobOffer : ScriptableObject
{
    public string title = "Junior Developer";
    public int salary = 8000;
    public int requiredProjects = 1;
    [Range(0, 100)] public int requiredScore = 60;
}
