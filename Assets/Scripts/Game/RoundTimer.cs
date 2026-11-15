using UnityEngine;

// Advances the round clock while the city scene is running.
public class RoundTimer : MonoBehaviour
{
    private void Update()
    {
        GameSession.Instance.SpendTime(Time.deltaTime);
    }
}
