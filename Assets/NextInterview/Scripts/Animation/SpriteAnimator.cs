using UnityEngine;

// Plays a walk cycle from a sprite sheet.
// "frames" must hold 16 sprites in this order:
// 4 directions (0 = down, 1 = left, 2 = right, 3 = up), each with 4 frames.
// The setup wizard (Tools > Next Interview > Create Everything) fills this in for you.
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAnimator : MonoBehaviour
{
    [SerializeField, Tooltip("16 sprites: 4 directions (down, left, right, up) x 4 frames.")]
    private Sprite[] frames;

    [SerializeField] private float framesPerSecond = 8f;

    [SerializeField, Tooltip("On = keep animating even when standing still (used for NPCs).")]
    private bool autoPlay;

    SpriteRenderer sr;
    int direction;   // 0 down, 1 left, 2 right, 3 up
    int frame;       // 0..3 inside the walk cycle
    float timer;
    bool moving;

    // Used by the setup wizard to configure the animator.
    public void Setup(Sprite[] frames, bool autoPlay)
    {
        this.frames = frames;
        this.autoPlay = autoPlay;
    }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        Show();
    }

    // Called every frame with the character's current velocity.
    public void Drive(Vector2 velocity)
    {
        moving = velocity.sqrMagnitude > 0.01f;
        if (!moving) return;

        if (Mathf.Abs(velocity.x) > Mathf.Abs(velocity.y))
            direction = velocity.x < 0f ? 1 : 2;
        else
            direction = velocity.y < 0f ? 0 : 3;
    }

    void Update()
    {
        if (moving || autoPlay)
        {
            timer += Time.deltaTime;
            if (timer >= 1f / framesPerSecond)
            {
                timer -= 1f / framesPerSecond;
                frame = (frame + 1) % 4;
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

    void Show()
    {
        if (sr == null || frames == null || frames.Length < 16) return;
        sr.sprite = frames[direction * 4 + frame];
    }
}
