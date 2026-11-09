using UnityEngine;

// Creates one white square at runtime. Objects tint it with SpriteRenderer.color.
public static class PlaceholderArt
{
    static Sprite square;

    public static Sprite Square
    {
        get
        {
            if (square == null)
            {
                var tex = new Texture2D(4, 4);
                var pixels = new Color[16];
                for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
                tex.SetPixels(pixels);
                tex.filterMode = FilterMode.Point;
                tex.Apply();
                square = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            }
            return square;
        }
    }
}
