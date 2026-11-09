using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Builds simple UI (panels, text, buttons, input fields) from code,
// so the game needs no hand-made UI prefabs.
public static class UIBuilder
{
    static Font DefaultFont => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    public static Canvas CreateCanvas(string name, int sortOrder)
    {
        var go = new GameObject(name);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortOrder;

        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        go.AddComponent<GraphicRaycaster>();
        EnsureEventSystem();
        return canvas;
    }

    static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;
        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<StandaloneInputModule>();
    }

    public static RectTransform CreatePanel(Transform parent, Color color, Vector2 size, Vector2 pos, string name = "Panel")
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.AddComponent<Image>().color = color;
        return Place(go, parent, pos, size);
    }

    public static Text CreateText(Transform parent, string content, int fontSize, Vector2 pos, Vector2 box,
                                  TextAnchor align = TextAnchor.MiddleCenter)
    {
        var go = new GameObject("Text", typeof(RectTransform));
        var text = go.AddComponent<Text>();
        text.font = DefaultFont;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = align;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.text = content;
        Place(go, parent, pos, box);
        return text;
    }

    public static Button CreateButton(Transform parent, string label, Vector2 pos, Vector2 size, UnityAction onClick)
    {
        var go = new GameObject("Button", typeof(RectTransform));
        var image = go.AddComponent<Image>();
        image.color = new Color(0.2f, 0.45f, 0.9f);
        var button = go.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);
        Place(go, parent, pos, size);
        CreateText(go.transform, label, 36, Vector2.zero, size);
        return button;
    }

    public static InputField CreateInputField(Transform parent, Vector2 pos, Vector2 size)
    {
        var go = new GameObject("AnswerField", typeof(RectTransform));
        var image = go.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.95f);
        var field = go.AddComponent<InputField>();
        field.targetGraphic = image;
        field.lineType = InputField.LineType.MultiLineNewline;
        Place(go, parent, pos, size);

        var text = CreateText(go.transform, "", 34, Vector2.zero, size - new Vector2(40f, 40f), TextAnchor.UpperLeft);
        text.color = Color.black;
        field.textComponent = text;
        return field;
    }

    static RectTransform Place(GameObject go, Transform parent, Vector2 pos, Vector2 size)
    {
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return rt;
    }
}
