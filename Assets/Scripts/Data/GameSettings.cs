using UnityEngine;

// The numbers that tune one round. The asset lives in Assets/Resources/GameSettings.asset.
[CreateAssetMenu(menuName = "Next Interview/Game Settings")]
public class GameSettings : ScriptableObject
{
    [Tooltip("Length of one round, in seconds.")]
    public float roundSeconds = 300f;

    [Tooltip("How long a contact stays connected, in seconds of round time.")]
    public float connectionSeconds = 90f;

    [Tooltip("Interview bonus for every active connection.")]
    public int bonusPerConnection = 5;

    [Tooltip("The connections bonus never goes above this.")]
    public int maxConnectionBonus = 15;
}
