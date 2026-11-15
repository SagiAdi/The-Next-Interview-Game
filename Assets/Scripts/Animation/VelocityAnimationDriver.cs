using UnityEngine;

// Tells the sprite animator how fast the physics body is moving, so the walk cycle matches the movement.
[RequireComponent(typeof(Rigidbody2D), typeof(SpriteAnimator))]
public class VelocityAnimationDriver : MonoBehaviour
{
    private Rigidbody2D body;
    private SpriteAnimator animator;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<SpriteAnimator>();
    }

    private void Update()
    {
        animator.Drive(body.linearVelocity);
    }
}
