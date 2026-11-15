using UnityEngine;

// The lose condition of the round: when the time is up, the Game Over screen is loaded.
public class RoundRules : MonoBehaviour
{
    private GameSession session;

    private void OnEnable()
    {
        session = GameSession.Instance;
        session.TimeUp += OnTimeUp;
    }

    private void OnDisable()
    {
        if (session != null)
        {
            session.TimeUp -= OnTimeUp;
        }
    }

    private void OnTimeUp()
    {
        Scenes.Load(Scenes.GameOver);
    }
}
