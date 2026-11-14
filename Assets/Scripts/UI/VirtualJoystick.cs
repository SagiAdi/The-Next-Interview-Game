using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Round on-screen stick for phones. Touch inside the circle and drag: the knob follows the finger.
// It only reports the direction through an event. It does not know who listens.
public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private static readonly Color BaseColor = new Color(1f, 1f, 1f, 0.25f);
    private static readonly Color KnobColor = new Color(1f, 1f, 1f, 0.85f);

    private RectTransform baseRect;
    private RectTransform knob;

    // Direction and strength: (0,0) in the middle, length 1 at the edge.
    public event System.Action<Vector2> DirectionChanged;

    private void Awake()
    {
        baseRect = (RectTransform)transform;
        knob = transform.Find("Knob") as RectTransform;

        Image baseImage = GetComponent<Image>();
        baseImage.sprite = PlaceholderArt.Circle;
        baseImage.color = BaseColor;

        Image knobImage = knob.GetComponent<Image>();
        knobImage.sprite = PlaceholderArt.Circle;
        knobImage.color = KnobColor;
        knobImage.raycastTarget = false;
    }

    private void OnDisable()
    {
        Release();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Finger position relative to the middle of the circle, in canvas units.
        // (The local point is measured from the pivot, so the center of the rectangle is subtracted.)
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            baseRect, eventData.position, eventData.pressEventCamera, out Vector2 local);
        local -= baseRect.rect.center;

        float radius = baseRect.sizeDelta.x * 0.5f;
        Vector2 direction = Vector2.ClampMagnitude(local / radius, 1f);

        // The knob moves inside the circle, so the player sees where the stick points.
        float travel = radius - knob.sizeDelta.x * 0.5f;
        knob.anchoredPosition = direction * travel;
        DirectionChanged?.Invoke(direction);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Release();
    }

    private void Release()
    {
        if (knob != null)
        {
            knob.anchoredPosition = Vector2.zero;
        }

        DirectionChanged?.Invoke(Vector2.zero);
    }
}
