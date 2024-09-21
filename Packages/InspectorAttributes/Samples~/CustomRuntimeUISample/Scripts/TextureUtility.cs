using UnityEngine;

public static class TextureUtility
{
    public static Texture2D CreateFromGradient (Gradient grad, int width = 32, int height = 1) {
        var gradTex = new Texture2D(width, height, TextureFormat.ARGB32, false);
        gradTex.filterMode = FilterMode.Bilinear;
        float inv = 1f / (width - 1);
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                var t = x * inv;
                Color col = grad.Evaluate(t);
                gradTex.SetPixel(x, y, col);
            }
        }
        gradTex.Apply();
        return gradTex;
    }
    
    public static Texture2D CreateSinglePixelTexture2D(Color color)
    {
        var tex = new Texture2D(1,1);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return tex;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="color"></param>
    /// <param name="index">The index of the color component, where R = 0, G = 1 and B = 2.</param>
    public static Texture2D CreateGradientTexture2DOnColorComponent(Color32 color, int index)
    {
        var tex = new Texture2D(256,1);
        for (int i = 0; i <= 255; i++)
        {
            color[index] = (byte)i;
            tex.SetPixel(i, 0, color);
        }
        tex.Apply();
        return tex;
    }
    
    public static Texture2D CreateHueGradient(float s = 1.0f, float v = 1.0f, int width = 360)
    {
        var tex = new Texture2D(width,1);
        for (var i = 0; i < width; i++)
            tex.SetPixel(i, 0, Color.HSVToRGB(1.0f / ((float)(width - 1)) * (float)i, s, v));
        tex.Apply();
        return tex;
    }
    
    public static Texture2D CreateGradientTexture2D(Color startColor, Color endColor, int width = 100)
    {
        var tex = new Texture2D(width,1);
        for (var i = 0; i < width; i++)
            tex.SetPixel(i, 0, Color.Lerp(startColor, endColor, (1.0f / (float)(width - 1)) * (float)i));
        tex.Apply();
        return tex;
    }
    
    public static Gradient CreateAlphaGradient(Color color)
    {
        var gradient = new Gradient();

        // Blend color from red at 0% to blue at 100%
        var colors = new GradientColorKey[2];
        colors[0] = new GradientColorKey(color, 0.0f);
        colors[1] = new GradientColorKey(color, 1.0f);

        // Blend alpha from opaque at 0% to transparent at 100%
        var alphas = new GradientAlphaKey[2];
        alphas[0] = new GradientAlphaKey(0.0f, 0.0f);
        alphas[1] = new GradientAlphaKey(1.0f, 1.0f);

        gradient.SetKeys(colors, alphas);

        // What's the color at the relative time 0.25 (25%) ?
        // Debug.Log(gradient.Evaluate(0.25f));
        return gradient;
    }
}
