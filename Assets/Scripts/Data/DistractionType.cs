using UnityEngine;

// One kind of distraction that bounces around the city (data only, no logic).
// To add a new kind: create a new asset and add it to the Distraction Spawner list.
[CreateAssetMenu(menuName = "Next Interview/Distraction Type")]
public class DistractionType : ScriptableObject
{
    public string displayName = "Phone notification";
    public Color color = new Color(1f, 0.35f, 0.35f);

    [Tooltip("Diameter in world units.")]
    public float size = 0.5f;

    public float mass = 0.5f;

    [Tooltip("The speed it keeps while it bounces around.")]
    public float cruiseSpeed = 3f;

    [Tooltip("Spin given at launch (AddTorque).")]
    public float spinTorque = 0.05f;

    [Tooltip("Seconds of round time lost for every unit of impact speed.")]
    public float secondsPerImpactSpeed = 1.5f;

    public float minPenaltySeconds = 3f;
    public float maxPenaltySeconds = 10f;
}
