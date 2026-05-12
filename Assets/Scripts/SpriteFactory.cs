using UnityEngine;

public static class SpriteFactory
{
    private static Sprite pixel;
    private static Sprite key;

    public static Sprite Pixel
    {
        get
        {
            if (pixel != null)
            {
                return pixel;
            }

            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            pixel = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            return pixel;
        }
    }

    public static Sprite Key
    {
        get
        {
            if (key != null)
            {
                return key;
            }

            Texture2D texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color clear = new Color(0f, 0f, 0f, 0f);
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    texture.SetPixel(x, y, clear);
                }
            }

            Color c = Color.white;
            // key head
            for (int y = 8; y <= 12; y++)
            {
                for (int x = 2; x <= 6; x++)
                {
                    texture.SetPixel(x, y, c);
                }
            }

            for (int y = 9; y <= 11; y++)
            {
                for (int x = 3; x <= 5; x++)
                {
                    texture.SetPixel(x, y, clear);
                }
            }

            // shaft
            for (int x = 6; x <= 13; x++)
            {
                texture.SetPixel(x, 10, c);
                texture.SetPixel(x, 9, c);
            }

            // teeth
            texture.SetPixel(11, 8, c);
            texture.SetPixel(12, 8, c);
            texture.SetPixel(13, 8, c);
            texture.SetPixel(13, 7, c);

            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.Apply();
            key = Sprite.Create(texture, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
            return key;
        }
    }
}
