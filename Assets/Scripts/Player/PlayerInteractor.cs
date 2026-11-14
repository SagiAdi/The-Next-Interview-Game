using UnityEngine;

// Uses the thing in front of the player when the Interact input is pressed.
[RequireComponent(typeof(PlayerInputReader), typeof(InteractionSensor))]
public class PlayerInteractor : MonoBehaviour
{
    private PlayerInputReader input;
    private InteractionSensor sensor;

    private void Awake()
    {
        input = GetComponent<PlayerInputReader>();
        sensor = GetComponent<InteractionSensor>();
    }

    private void OnEnable()
    {
        input.InteractPressed += Use;
    }

    private void OnDisable()
    {
        input.InteractPressed -= Use;
    }

    private void Use()
    {
        if (GameSession.Instance.IsPaused || sensor.Target == null)
        {
            return;
        }

        sensor.Target.Interact();
    }
}
