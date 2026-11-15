using UnityEngine;

// Gives an object a plain shape until real art is assigned to its SpriteRenderer.
[RequireComponent(typeof(SpriteRenderer))]
public class Placeholder : MonoBehaviour
{
    [SerializeField, Tooltip("On = circle, off = square.")]
    private bool round;

    private void Awake()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer.sprite == null)
        {
            spriteRenderer.sprite = round ? PlaceholderArt.Circle : PlaceholderArt.Square;
        }
    }
}
