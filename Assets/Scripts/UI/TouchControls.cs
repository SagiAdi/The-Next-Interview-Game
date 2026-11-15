using UnityEngine;
using UnityEngine.UI;

// On-screen controls for phones: a joystick (bottom left) and an Interact button (bottom right).
// It passes what the player touches to the PlayerInputReader.
public class TouchControls : MonoBehaviour
{
    private static readonly Vector2 StickSize = new Vector2(260, 260);
    private static readonly Vector2 KnobSize = new Vector2(110, 110);
    private static readonly Vector2 ButtonSize = new Vector2(300, 140);
    private static readonly Vector2 Margin = new Vector2(30, 30);

    [SerializeField] private PlayerInputReader input;

    private VirtualJoystick joystick;

    private void Awake()
    {
        if (input == null)
        {
            input = FindAnyObjectByType<PlayerInputReader>();
        }

        Canvas canvas = UIBuilder.CreateCanvas("Touch Controls", 1);

        // The knob is created before the joystick component, because the joystick looks for it in Awake.
        RectTransform stick = UIBuilder.CreatePanel(canvas.transform, Color.white, StickSize, Vector2.zero, "Joystick");
        UIBuilder.CreatePanel(stick, Color.white, KnobSize, Vector2.zero, "Knob");
        UIBuilder.PinToCorner(stick, Vector2.zero, Margin);
        joystick = stick.gameObject.AddComponent<VirtualJoystick>();

        Button button = UIBuilder.CreateButton(canvas.transform, "Interact (E)", Vector2.zero, ButtonSize, OnInteractClicked);
        UIBuilder.PinToCorner((RectTransform)button.transform, new Vector2(1f, 0f), new Vector2(-Margin.x, Margin.y));
    }

    private void OnEnable()
    {
        joystick.DirectionChanged += OnDirectionChanged;
    }

    private void OnDisable()
    {
        joystick.DirectionChanged -= OnDirectionChanged;
    }

    private void OnDirectionChanged(Vector2 direction)
    {
        input.SetTouchDirection(direction);
    }

    private void OnInteractClicked()
    {
        input.PressInteract();
    }
}
