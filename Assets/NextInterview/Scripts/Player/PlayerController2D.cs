using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Top-down movement and interaction.
// Keys are read with Unity's Input System (InputAction): arrows and WASD to move, E or Space to interact.
// Walls are checked with rectangle math on the Transforms, and interaction picks the closest object by distance.
public class PlayerController2D : MonoBehaviour
{
    public const string Version = "inputaction v4 (serialized fields)";

    // True while a panel (puzzle, job board) is open. Stops movement.
    public static bool Frozen;

    [SerializeField] private float speed = 4f;

    [SerializeField, Tooltip("How close (in units) the player must be to use something.")]
    private float interactRadius = 1.3f;

    [SerializeField, Tooltip("Print diagnostic messages to the Console. Turn off once everything works.")]
    private bool debugLogs = true;

    BoxCollider2D feet;
    SpriteAnimator anim;
    readonly List<BoxCollider2D> walls = new List<BoxCollider2D>();
    float insideCheckTimer;

    InputAction moveAction;
    InputAction interactAction;

    void Awake()
    {
        feet = GetComponent<BoxCollider2D>();
        anim = GetComponent<SpriteAnimator>();

        // The physics body must not move the player by itself.
        Rigidbody2D body = GetComponent<Rigidbody2D>();
        if (body != null) body.bodyType = RigidbodyType2D.Kinematic;

        CreateInputActions();
    }

    void OnEnable()
    {
        moveAction?.Enable();
        interactAction?.Enable();
    }

    void OnDisable()
    {
        moveAction?.Disable();
        interactAction?.Disable();
    }

    void OnDestroy()
    {
        moveAction?.Dispose();
        interactAction?.Dispose();
    }

    void CreateInputActions()
    {
        // Movement: a 2D vector made from four keys (arrows or WASD)
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

        // Interact: E or Space
        interactAction = new InputAction("Interact", InputActionType.Button);
        interactAction.AddBinding("<Keyboard>/e");
        interactAction.AddBinding("<Keyboard>/space");
    }

    void Start()
    {
        if (debugLogs)
            Debug.Log("[Player] version: " + Version + ". Walls found: " + CountWalls()
                      + ". Player position: " + (Vector2)transform.position);
    }

    void Update()
    {
        if (Frozen)
        {
            if (anim != null) anim.Drive(Vector2.zero);
            return;
        }

        if (interactAction.WasPressedThisFrame())
        {
            if (debugLogs)
            {
                Interactable near = FindNearest();
                Debug.Log("[Player] E pressed. Nearby: " + (near != null ? near.name : "nothing within " + interactRadius));
            }
            TryInteract();
        }

        // Keyboard (InputAction) and the on-screen joystick both move the player.
        Vector2 input = moveAction.ReadValue<Vector2>() + VirtualJoystick.Value;
        input = Vector2.ClampMagnitude(input, 1f);
        Vector2 step = input * speed * Time.deltaTime;

        // Move one axis at a time, so the player slides along a wall instead of getting stuck.
        Vector2 pos = transform.position;
        if (FindBlockingWall(pos + new Vector2(step.x, 0f)) == null) pos.x += step.x;
        if (FindBlockingWall(pos + new Vector2(0f, step.y)) == null) pos.y += step.y;
        transform.position = new Vector3(pos.x, pos.y, transform.position.z);

        if (anim != null) anim.Drive(input * speed);

        // Diagnostics: report if the player is standing inside a wall (twice per second).
        if (debugLogs)
        {
            insideCheckTimer -= Time.deltaTime;
            if (insideCheckTimer <= 0f)
            {
                insideCheckTimer = 0.5f;
                BoxCollider2D inside = FindBlockingWall(pos);
                if (inside != null)
                    Debug.Log("[Player] standing INSIDE wall '" + inside.name + "' at " + pos);
            }
        }
    }

    // Solid walls are box colliders that are not triggers (the player's own feet box is skipped).
    int CountWalls()
    {
        walls.Clear();
        foreach (BoxCollider2D box in FindObjectsByType<BoxCollider2D>(FindObjectsSortMode.None))
        {
            if (box != feet && !box.isTrigger) walls.Add(box);
        }
        return walls.Count;
    }

    // Returns the first solid wall that the player's feet box would overlap at "position", or null.
    // Sizes and positions are read from the Transforms, because the physics engine's copy was unreliable here.
    BoxCollider2D FindBlockingWall(Vector2 position)
    {
        if (walls.Count == 0) CountWalls();

        Vector2 scale = transform.lossyScale;
        Vector2 center = position + (Vector2)transform.TransformVector(feet.offset);
        Vector2 half = Vector2.Scale(feet.size, scale) * 0.5f - Vector2.one * 0.01f;

        foreach (BoxCollider2D wall in walls)
        {
            Transform wt = wall.transform;
            Vector2 wallCenter = wt.TransformPoint(wall.offset);
            Vector2 wallHalf = Vector2.Scale(wall.size, wt.lossyScale) * 0.5f;

            if (Mathf.Abs(center.x - wallCenter.x) < half.x + wallHalf.x &&
                Mathf.Abs(center.y - wallCenter.y) < half.y + wallHalf.y)
            {
                return wall;
            }
        }
        return null;
    }

    // Returns the closest Interactable within interactRadius, or null.
    public Interactable FindNearest()
    {
        Interactable best = null;
        float bestDistance = interactRadius;

        foreach (Interactable item in FindObjectsByType<Interactable>(FindObjectsSortMode.None))
        {
            float distance = Vector2.Distance(transform.position, item.transform.position);
            if (distance <= bestDistance)
            {
                bestDistance = distance;
                best = item;
            }
        }
        return best;
    }

    public void TryInteract()
    {
        if (Frozen) return;

        Interactable target = FindNearest();
        if (target != null) target.Interact(this);
    }
}
