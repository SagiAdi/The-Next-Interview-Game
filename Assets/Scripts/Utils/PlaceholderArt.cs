using UnityEngine;

// Simple white shapes made at runtime (no image files needed). Objects tint them with their color.
// The two static fields are only a cache, so each shape is created once.
public static class PlaceholderArt
{
    private static Sprite square;
    private static Sprite circle;

    public static Sprite Square
    {
        get
        {
            if (square == null)
            {
                var texture = new Texture2D(4, 4);
                var pixels = new Color[16];
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = Color.white;
                }

                texture.SetPixels(pixels);
                texture.filterMode = FilterMode.Point;
                texture.Apply();
                square = Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            }

            return square;
        }
    }

    public static Sprite Circle
    {
        get
        {
            if (circle == null)
            {
                const int size = 64;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                texture.filterMode = FilterMode.Bilinear;

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

                texture.SetPixels32(pixels);
                texture.Apply();

                // 64 pixels per unit: the circle is exactly 1 world unit wide, like the square.
                circle = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            }

            return circle;
        }
    }
}
