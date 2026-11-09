using UnityEngine;

// Gives a visual object a plain square until you assign real art (for example AI-generated art).
[RequireComponent(typeof(SpriteRenderer))]
public class Placeholder : MonoBehaviour
{
    void Awake()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr.sprite == null) sr.sprite = PlaceholderArt.Square;
    }
}
