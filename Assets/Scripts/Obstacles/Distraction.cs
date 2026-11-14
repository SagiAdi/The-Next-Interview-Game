using UnityEngine;

// A distraction (for example a phone notification) that bounces around the city.
// When it hits something that can be distracted, that thing loses time.
// The harder the hit, the more time is lost.
[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class Distraction : MonoBehaviour
{
    private Rigidbody2D body;
    private DistractionType type;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    // Called once, right after the distraction is spawned.
    public void Launch(DistractionType distractionType)
    {
        type = distractionType;
        transform.localScale = Vector3.one * type.size;
        GetComponent<SpriteRenderer>().color = type.color;
        body.mass = type.mass;

        // An impulse changes the velocity at once: speed = impulse / mass.
        Vector2 direction = Random.insideUnitCircle.normalized;
        body.AddForce(direction * type.cruiseSpeed * body.mass, ForceMode2D.Impulse);
        body.AddTorque(type.spinTorque, ForceMode2D.Impulse);
    }

    private void FixedUpdate()
    {
        if (type == null)
        {
            return;
        }

        // Keep a steady cruising speed: push when too slow, brake when too fast.
        Vector2 velocity = body.linearVelocity;
        float speed = velocity.magnitude;
        if (speed < 0.05f)
        {
            Vector2 direction = Random.insideUnitCircle.normalized;
            body.AddForce(direction * type.cruiseSpeed * body.mass, ForceMode2D.Impulse);
            return;
        }

        float speedGap = type.cruiseSpeed - speed;
        body.AddForce(velocity / speed * speedGap * body.mass);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (type == null || !collision.gameObject.TryGetComponent(out IDistractable target))
        {
            return;
        }

        // The Collision2D tells how hard the two bodies hit each other.
        float impactSpeed = collision.relativeVelocity.magnitude;
        float seconds = Mathf.Clamp(
            impactSpeed * type.secondsPerImpactSpeed, type.minPenaltySeconds, type.maxPenaltySeconds);
        target.LoseFocus(seconds);

        // Bounce away from the contact point, so the distraction does not stick to the player.
        Vector2 contactPoint = collision.GetContact(0).point;
        Vector2 away = ((Vector2)transform.position - contactPoint).normalized;
        body.AddForce(away * type.cruiseSpeed * body.mass, ForceMode2D.Impulse);
    }
}
