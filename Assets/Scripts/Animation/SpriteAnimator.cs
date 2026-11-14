using UnityEngine;

// Plays a walk cycle from a sprite sheet.
// "frames" holds 16 sprites: 4 directions (0 = down, 1 = left, 2 = right, 3 = up), each with 4 frames.
// The setup wizard (Tools > Next Interview > Create Everything) fills this in.
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAnimator : MonoBehaviour
{
    private const int Directions = 4;
    private const int FramesPerDirection = 4;

    [SerializeField, Tooltip("16 sprites: 4 directions (down, left, right, up) x 4 frames.")]
    private Sprite[] frames;

    [SerializeField] private float framesPerSecond = 8f;

    [SerializeField, Tooltip("On = keep animating even when standing still (used for NPCs).")]
    private bool autoPlay;

    private SpriteRenderer spriteRenderer;
    private int direction;
    private int frame;
    private float timer;
    private bool moving;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        Show();
    }

    // Called every frame with the character's current velocity.
    public void Drive(Vector2 velocity)
    {
        moving = velocity.sqrMagnitude > 0.05f;
        if (!moving)
        {
            return;
        }

        if (Mathf.Abs(velocity.x) > Mathf.Abs(velocity.y))
        {
            direction = velocity.x < 0f ? 1 : 2;
        }
        else
        {
            direction = velocity.y < 0f ? 0 : 3;
        }
    }

    private void Update()
    {
        if (moving || autoPlay)
        {
            timer += Time.deltaTime;
            if (timer >= 1f / framesPerSecond)
            {
                timer -= 1f / framesPerSecond;
                frame = (frame + 1) % FramesPerDirection;
            }
        }
        else
        {
            // Standing still: show the neutral pose of the current direction.
            frame = 0;
            timer = 0f;
        }

        Show();
    }

    private void Show()
    {
        if (frames == null || frames.Length < Directions * FramesPerDirection)
        {
            return;
        }

        spriteRenderer.sprite = frames[direction * FramesPerDirection + frame];
    }
}
