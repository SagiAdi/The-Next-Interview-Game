using System;
using UnityEngine;

// The player's "eyes": finds the Interactable in front of the player.
// A circle is cast forward (Physics2D.CircleCast), and then a ray (Physics2D.Raycast)
// checks that no wall stands between the player and the thing that was found.
[RequireComponent(typeof(PlayerMover))]
public class InteractionSensor : MonoBehaviour
{
    [SerializeField, Tooltip("Radius of the circle that is cast forward.")]
    private float castRadius = 0.7f;

    [SerializeField, Tooltip("How far forward the circle travels.")]
    private float reach = 0.8f;

    private readonly RaycastHit2D[] castHits = new RaycastHit2D[16];
    private readonly RaycastHit2D[] rayHits = new RaycastHit2D[8];
    private ContactFilter2D everything;
    private ContactFilter2D solidOnly;
    private PlayerMover mover;

    public event Action<Interactable> TargetChanged;

    // The thing the player would use right now, or null.
    public Interactable Target { get; private set; }

    private void Awake()
    {
        mover = GetComponent<PlayerMover>();

        everything = new ContactFilter2D();
        everything.useTriggers = true;

        solidOnly = new ContactFilter2D();
        solidOnly.useTriggers = false;
    }

    private void FixedUpdate()
    {
        Interactable found = FindTarget();

        // ReferenceEquals: a destroyed target must also count as a change.
        if (ReferenceEquals(found, Target))
        {
            return;
        }

        Target = found;
        TargetChanged?.Invoke(Target);
    }

    private Interactable FindTarget()
    {
        Vector2 origin = transform.position;
        int count = Physics2D.CircleCast(origin, castRadius, mover.Facing, everything, castHits, reach);

        Interactable best = null;
        float bestDistance = float.MaxValue;
        for (int i = 0; i < count; i++)
        {
            Collider2D hitCollider = castHits[i].collider;
            Interactable candidate = hitCollider.GetComponent<Interactable>();
            if (candidate == null || castHits[i].distance >= bestDistance)
            {
                continue;
            }

            if (IsBehindWall(origin, hitCollider))
            {
                continue;
            }

            best = candidate;
            bestDistance = castHits[i].distance;
        }

        return best;
    }

    // True when a static solid collider (a wall) lies on the straight line to the target.
    private bool IsBehindWall(Vector2 origin, Collider2D target)
    {
        Vector2 toTarget = target.ClosestPoint(origin) - origin;
        float distance = toTarget.magnitude;
        if (distance < 0.01f)
        {
            return false;
        }

        int count = Physics2D.Raycast(origin, toTarget / distance, solidOnly, rayHits, distance);
        for (int i = 0; i < count; i++)
        {
            // Walls have no Rigidbody2D. The player and the distractions do, so they never block the view.
            if (rayHits[i].collider.attachedRigidbody == null)
            {
                return true;
            }
        }

        return false;
    }
}
