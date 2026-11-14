using System;
using UnityEngine;
using UnityEngine.InputSystem;

// Reads what the player wants to do: keyboard (Input System actions) and the on-screen touch controls.
public class PlayerInputReader : MonoBehaviour
{
    private InputAction moveAction;
    private InputAction interactAction;
    private Vector2 touchDirection;

    public event Action InteractPressed;

    // The direction the player wants to move in. Its length is between 0 and 1.
    public Vector2 Move
    {
        get
        {
            Vector2 keys = moveAction.ReadValue<Vector2>();
            return Vector2.ClampMagnitude(keys + touchDirection, 1f);
        }
    }

    private void Awake()
    {
        // Movement: a 2D vector made from four keys (arrows or WASD).
        moveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/s")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/a")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/d")
            .With("Right", "<Keyboard>/rightArrow");

        // Interact: E or Space.
        interactAction = new InputAction("Interact", InputActionType.Button);
        interactAction.AddBinding("<Keyboard>/e");
        interactAction.AddBinding("<Keyboard>/space");
    }

    private void OnEnable()
    {
        interactAction.performed += OnInteractPerformed;
        moveAction.Enable();
        interactAction.Enable();
    }

    private void OnDisable()
    {
        interactAction.performed -= OnInteractPerformed;
        moveAction.Disable();
        interactAction.Disable();
        touchDirection = Vector2.zero;
    }

    private void OnDestroy()
    {
        moveAction.Dispose();
        interactAction.Dispose();
    }

    // Called by the on-screen joystick.
    public void SetTouchDirection(Vector2 direction)
    {
        touchDirection = direction;
    }

    // Called by the on-screen Interact button (and by the keyboard action above).
    public void PressInteract()
    {
        InteractPressed?.Invoke();
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        PressInteract();
    }
}
