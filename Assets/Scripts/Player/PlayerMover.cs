using UnityEngine;

// Moves the player with physics: every physics step it pushes the Rigidbody2D (AddForce)
// towards the velocity the player asked for. Walls stop the body through normal collisions.
[RequireComponent(typeof(Rigidbody2D), typeof(PlayerInputReader))]
public class PlayerMover : MonoBehaviour
{
    [SerializeField] private float maxSpeed = 4f;

    [SerializeField, Tooltip("How fast the player reaches the wanted speed. Higher = snappier.")]
    private float responsiveness = 20f;

    private Rigidbody2D body;
    private PlayerInputReader input;
    private GameSession session;

    // The last direction the player moved in. The interaction sensor looks this way.
    public Vector2 Facing { get; private set; } = Vector2.down;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        input = GetComponent<PlayerInputReader>();
        session = GameSession.Instance;

        body.bodyType = RigidbodyType2D.Dynamic;
        body.gravityScale = 0f;
        body.freezeRotation = true;
    }

    private void FixedUpdate()
    {
        Vector2 direction = session.IsPaused ? Vector2.zero : input.Move;
        if (direction.sqrMagnitude > 0.01f)
        {
            Facing = direction.normalized;
        }

        // F = m * a, where "a" closes the gap between the current and the wanted velocity.
        Vector2 wantedVelocity = direction * maxSpeed;
        Vector2 velocityGap = wantedVelocity - body.linearVelocity;
        float gain = Mathf.Min(responsiveness, 1f / Time.fixedDeltaTime);
        body.AddForce(velocityGap * gain * body.mass);
    }
}
