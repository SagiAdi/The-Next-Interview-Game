using UnityEngine;
using UnityEngine.UI;

// The on-screen hint ("Interact (E): Talk") for the thing in front of the player.
public class InteractPrompt : MonoBehaviour
{
    [SerializeField] private InteractionSensor sensor;

    private Text label;

    private void Awake()
    {
        if (sensor == null)
        {
            sensor = FindAnyObjectByType<InteractionSensor>();
        }

        Canvas canvas = UIBuilder.CreateCanvas("Interact Prompt", 0);
        label = UIBuilder.CreateText(canvas.transform, "", 36, new Vector2(0, -220), new Vector2(900, 60));
    }

    private void OnEnable()
    {
        sensor.TargetChanged += Show;
    }

    private void OnDisable()
    {
        sensor.TargetChanged -= Show;
    }

    private void Show(Interactable target)
    {
        label.text = target != null ? "Interact (E): " + target.Prompt : "";
    }
}
