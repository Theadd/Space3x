using UnityEngine;

namespace Space3x.UiToolkit.Types
{
    public static class PixelUtility
    {
        public static Texture2D CreateGradientTexture2D(Color startColor, Color endColor, int width = 100)
        {
            var tex = new Texture2D(width,1);
            for (var i = 0; i < width; i++)
                tex.SetPixel(i, 0, Color.Lerp(startColor, endColor, (1.0f / (float)(width - 1)) * (float)i));
            tex.Apply();
            return tex;
        }
        
        public static Texture2D CreateHSVGradientTexture2D(Color startColor, Color endColor, int width = 360)
        {
            Color.RGBToHSV(startColor, out var h0, out var s0, out var v0);
            Color.RGBToHSV(endColor, out var h1, out var s1, out var v1);
            var tex = new Texture2D(width,1);
            for (var i = 0; i < width; i++)
                tex.SetPixel(i, 0, Color.HSVToRGB(
                    Mathf.Lerp(h0, h1, 1.0f / (width - 1) * i), 
                    Mathf.Lerp(s0, s1, 1.0f / (width - 1) * i), 
                    Mathf.Lerp(v0, v1, 1.0f / (width - 1) * i)));
            tex.Apply();
            return tex;
        }
    }
}
