using JetBrains.Annotations;
using Space3x.Attributes.Types;
using Space3x.Properties.Types;
using UnityEngine;

// [Serializable]
public class AnnotatedColor
{
    [AllowExtendedAttributes]
    [BeginRow]
    
        [BeginColumn]
            [Button(nameof(Clickable), Text = " ")]
        [EndColumn]
    
        [BeginColumn]
            [BeginRow(ProportionalSize = false)]

                [GradientSlider(0, 255, MinColor = nameof(GetLowValueColor), MaxColor = nameof(GetHighValueColor))]
                public int r;

                [GradientSlider(0, 255, MinColor = nameof(GetLowValueColor), MaxColor = nameof(GetHighValueColor))]
                public int g;

                [GradientSlider(0, 255, MinColor = nameof(GetLowValueColor), MaxColor = nameof(GetHighValueColor))]
                public int b;
            [EndRow]
        [EndColumn]
            
        [BeginColumn]
            [BeginRow]
                [GradientSlider(0, 255, MinColor = nameof(GetLowValueColor), MaxColor = nameof(GetHighValueColor))]
                public int a;
            [EndRow]
        [EndColumn]
    
    [EndRow]
    
    [ShowInInspector]
    [Visible(false)]
    // private Color32 m_Value;
    private Color32 m_Value;
    
    [HideInInspector]
    public Color32 Value
    {
        get => m_Value;
        set
        {
            r = value.r;
            g = value.g;
            b = value.b;
            a = value.a;
            m_Value = new Color32((byte)r, (byte)g, (byte)b, (byte)a);
        }
    }

    [TrackChangesOn(nameof(r))]
    [TrackChangesOn(nameof(g))]
    [TrackChangesOn(nameof(b))]
    [TrackChangesOn(nameof(a))]
    [UsedImplicitly]
    public void Update(IPropertyNode changedPropertyNode)
    {
        // Enumerable.Range(0, 65)
        //     .Select(num => $"{num}: " + string.Join(", ", Enumerable.Range(7, 32)
        //         .Where(mod => "rgbaRGBA".Select(c => ((uint)c - num) % mod).Distinct().Count() == 8))).ToList();
        // "rgbaRGBA".Select(c => ((uint)c - 1) % 10).ToList();
        // changedPropertyNode.GetProperty(nameof(m_Value)).SetValue((Color)new Color32((byte)r, (byte)g, (byte)b, (byte)a));
        m_Value = new Color32((byte)r, (byte)g, (byte)b, (byte)a);
        Debug.Log($"UPDATE: " + (m_Value).ToString() + " changedPropertyNode: " + changedPropertyNode.PropertyPath);
    }

    private static readonly Color[] ColorMasks = new[]
    {
        Color.green - Color.black,      // G
        Color.red - Color.black,        // R
        Color.magenta,                  // g
        Color.cyan,                     // r
        Color.black,                    // A
        Color.blue - Color.black,       // B
        Color.white - Color.black,      // a
        Color.white - Color.blue + Color.black,    // b
    };

    public Color GetLowValueColor(IPropertyNode property) => m_Value * ColorMasks[((uint)property.Name[0] - 1) % 10];
    
    public Color GetHighValueColor(IPropertyNode property) => m_Value * ColorMasks[((uint)property.Name[0] - 1) % 10] + ColorMasks[((uint)property.Name.ToUpperInvariant()[0] - 1) % 10];
    
    public AnnotatedColor() => Value = Color.clear;

    public AnnotatedColor(Color32 color) => Value = color;

    public AnnotatedColor(Color color) => Value = color;

    public AnnotatedColor(string hexString) => Value = ColorUtility.TryParseHtmlString(hexString, out Color color) ? color : Color.clear;

    public override string ToString() => ColorUtility.ToHtmlStringRGBA(Value);

    public void Clickable() => Debug.Log(this.ToString() + " => " + Value.ToString());
    
    public static explicit operator AnnotatedColor(string hexString) => new(hexString);
        
    public static explicit operator AnnotatedColor(Color color) => new(color);
    
    public static explicit operator AnnotatedColor(Color32 color) => new(color);

    public static explicit operator Color(AnnotatedColor color) => color.Value;
    
    public static explicit operator Color32(AnnotatedColor color) => color.Value;
}
