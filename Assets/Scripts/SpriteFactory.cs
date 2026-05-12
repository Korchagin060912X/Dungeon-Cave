using UnityEngine;

public static class SpriteFactory
{
    private static Sprite pixel;

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
}
