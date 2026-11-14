using UnityEngine;

// The player's focus. A distraction that hits the player costs round time.
public class PlayerFocus : MonoBehaviour, IDistractable
{
    [SerializeField] private MessageChannel messages;

    [SerializeField, Tooltip("After a hit, the player cannot be distracted again for this long.")]
    private float recoverSeconds = 1.5f;

    private float readyTime;

    public void LoseFocus(float seconds)
    {
        GameSession session = GameSession.Instance;
        if (session.IsPaused || Time.time < readyTime)
        {
            return;
        }

        readyTime = Time.time + recoverSeconds;
        session.SpendTime(seconds);

        if (messages != null)
        {
            messages.Raise("Distracted! You lost " + Mathf.RoundToInt(seconds) + " seconds.");
        }
    }
}
