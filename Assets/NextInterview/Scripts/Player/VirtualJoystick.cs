using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Round on-screen stick for phones. Touch inside the circle and drag:
// the knob follows the finger, and the direction is sent to the player.
public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    // Direction and strength, from (0,0) in the middle to length 1 at the edge.
    public static Vector2 Value { get; private set; }

    RectTransform baseRect;
    RectTransform knob;

    // Start (not Awake): the HUD creates the knob after this component is added.
    void Start()
    {
        baseRect = (RectTransform)transform;
        knob = transform.Find("Knob") as RectTransform;

        // Move the stick to the bottom-left corner of the screen, so it is never cut off
        baseRect.anchorMin = Vector2.zero;
        baseRect.anchorMax = Vector2.zero;
        baseRect.anchoredPosition = new Vector2(30f + baseRect.sizeDelta.x * 0.5f, 30f + baseRect.sizeDelta.y * 0.5f);

        // Round base
        Image baseImage = GetComponent<Image>();
        if (baseImage != null)
        {
            baseImage.sprite = RoundArt.Circle;
            baseImage.color = new Color(1f, 1f, 1f, 0.25f);
        }

        // Round knob that the player can see clearly
        if (knob != null)
        {
            Image knobImage = knob.GetComponent<Image>();
            if (knobImage != null)
            {
                knobImage.sprite = RoundArt.Circle;
                knobImage.color = new Color(1f, 1f, 1f, 0.85f);
                knobImage.raycastTarget = false;
            }
        }

        Release();
    }

    public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);

    public void OnDrag(PointerEventData eventData)
    {
        if (baseRect == null) return;

        // Finger position relative to the middle of the circle, in canvas units
        RectTransformUtility.ScreenPointToLocalPointInRectangle(baseRect, eventData.position, eventData.pressEventCamera, out Vector2 local);

        float radius = baseRect.sizeDelta.x * 0.5f;
        Vector2 direction = Vector2.ClampMagnitude(local / radius, 1f);
        Value = direction;

        // The knob moves inside the circle, so the player can see where the stick points
        if (knob != null)
        {
            float travel = radius - knob.sizeDelta.x * 0.5f;
            knob.anchoredPosition = direction * travel;
        }
    }

    public void OnPointerUp(PointerEventData eventData) => Release();

    void OnDisable() => Release();

    void Release()
    {
        Value = Vector2.zero;
        if (knob != null) knob.anchoredPosition = Vector2.zero;
    }

    // Makes a round white sprite at runtime (no image files needed).
    static class RoundArt
    {
        static Sprite circle;

        public static Sprite Circle
        {
            get
            {
                if (circle == null)
                {
                    const int size = 64;
                    var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                    tex.filterMode = FilterMode.Bilinear;

                    Vector2 center = new Vector2(size / 2f, size / 2f);
                    float radius = size / 2f - 1f;
                    var pixels = new Color32[size * size];
                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                            byte alpha = (byte)(Mathf.Clamp01(radius - distance + 0.5f) * 255f);
                            pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                        }
                    }
                    tex.SetPixels32(pixels);
                    tex.Apply();

                    circle = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
                }
                return circle;
            }
        }
    }
}
