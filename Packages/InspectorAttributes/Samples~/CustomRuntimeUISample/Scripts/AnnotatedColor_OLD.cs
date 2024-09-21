// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Runtime.CompilerServices;
// using JetBrains.Annotations;
// using Space3x.Attributes.Types;
// using Space3x.InspectorAttributes;
// using Space3x.Properties.Types;
// using Space3x.UiToolkit.Types;
// using UnityEngine;
// using UnityEngine.Assertions;
// using UnityEngine.UIElements;
//
//
// // public interface IComponent
// // {
// //     // VisualElement Field { get; }
// //     // IPropertyNode Property { get; }
// //     IDrawer Drawer { get; }
// // }
//
// // public interface IComponent<TValue> : IComponent
// // {
// //     TValue Values { get; set; }
// // }
//
// // public class GradientDataComponent
// // {
// //     public Color32 color;
// //     public int index;
// //     public IPropertyNode property;
// //
// //     public GradientDataComponent(Color32 color, int index, IPropertyNode property)
// //     {
// //         this.color = color;
// //         this.index = index;
// //         this.property = property;
// //     }
// // }
//
// // public class GradientSliderComponent : IComponent
// // {
// //     public IDrawer Drawer { get; set; }
// //     // public GradientDataComponent Values { get; set; }
// //     public Color32 color;
// //     public int index;
// //     public IPropertyNode property;
// //     
// //     public GradientSliderComponent(Color32 color, int index, IPropertyNode property)
// //     {
// //         this.color = color;
// //         this.index = index;
// //         this.property = property;
// //     }
// // }
// //
// // public static class InlineDecorators
// // {
// //     public static TElement ThrowIfNull<TElement>(this TElement element, [CallerMemberName] string callerName = "") => 
// //         element ?? throw new ArgumentNullException(callerName);
// //
// //     public static void GradientSliderDecorator(IComponent component)
// //     {
// //         if (component is not GradientSliderComponent data)
// //             throw new ArgumentException(nameof(component));
// //         
// //         var sliderIntField = data.Drawer.Field
// //             .WithClasses(true, "ui3x-gradient-slider", "unity-inspector-element", "unity-inspector-main-container")
// //             .Q<SliderInt>(className: "unity-slider-int");
// //         var label = sliderIntField.ThrowIfNull().Q<Label>(className: "unity-label");
// //         var sliderInput = sliderIntField.Q(className: "unity-slider-int__input");
// //
// //         Action<Color32> updateGradient = (Color32 color) =>
// //         {
// //             Debug.Log("UPDATE GRADIENT WITH COLOR: " + color);
// //             sliderInput.style.backgroundImage =
// //                 new StyleBackground(TextureUtility.CreateGradientTexture2DOnColorComponent(color, data.index));
// //         };
// //         updateGradient(data.color);
// //         
// //         // sliderIntField.RegisterValueChangedCallback((ChangeEvent<int> ev) =>
// //         // {
// //         //     
// //         // });
// //         
// //         // TODO: UNCOMMENT: sliderIntField.TrackPropertyValue(data.property, node => updateGradient((Color32)node.GetValue()));
// //         if (label == null) return;
// //         label.TrackPropertyValue(data.property, node => updateGradient((Color32)node.GetValue()));
// //         label.style.minWidth = new StyleLength(StyleKeyword.Null);
// //         label.style.width = new StyleLength(StyleKeyword.Null);
// //     }
// // }
//
// // public partial class AnnotatedColor
// // {
// //     private void ConvertToGradientSlider(IDrawer drawer, IPropertyNode property)
// //     {
// //         // Assert.IsNotNull(drawer?.Field);
// //         // Assert.IsNotNull(property);
// //         Debug.Log("IN ConvertToGradientSlider!!!!!!!!!!!!!!!!!!!!!!!!");
// //         if (drawer.ThrowIfNull().Field.ThrowIfNull().ClassListContains("ui3x-gradient-slider")) return;
// //         var targetProperty = property.ThrowIfNull().GetProperty(nameof(m_Value));
// //         InlineDecorators.GradientSliderDecorator(
// //             new GradientSliderComponent((Color32)(Color)targetProperty.GetValue(),
// //                 Array.IndexOf(new[] { "r", "g", "b", "a" }, property.Name), targetProperty) { Drawer = drawer });
// //     }
// // }
//
// // [Serializable]
// public class AnnotatedColor
// {
//     [AllowExtendedAttributes]
//     [BeginRow]
//     
//         [BeginColumn]
//             [Button(nameof(Clickable), Text = " ")]
//         [EndColumn]
//     
//         [BeginColumn]
//             [BeginRow(ProportionalSize = false)]
//
//                 [GradientSlider(0, 255, MinColor = nameof(GetLowValueColor), MaxColor = nameof(GetHighValueColor))]
//                 public int r;
//
//                 // [VirtualDecorator(OnUpdate = nameof(ConvertToGradientSlider))]
//                 [GradientSlider(0, 255, MinColor = nameof(GetLowValueColor), MaxColor = nameof(GetHighValueColor))]
//                 public int g;
//
//                 [GradientSlider(0, 255, MinColor = nameof(GetLowValueColor), MaxColor = nameof(GetHighValueColor))]
//                 public int b;
//             [EndRow]
//         [EndColumn]
//             
//         [BeginColumn]
//             [BeginRow]
//                 [GradientSlider(0, 255, MinColor = nameof(GetLowValueColor), MaxColor = nameof(GetHighValueColor))]
//                 public int a;
//             [EndRow]
//         [EndColumn]
//     
//     [EndRow]
//     
//     [ShowInInspector]
//     [Visible(false)]
//     // private Color32 m_Value;
//     public Color m_Value;
//     
//     [HideInInspector]
//     public Color32 Value
//     {
//         get => m_Value;
//         set
//         {
//             r = value.r;
//             g = value.g;
//             b = value.b;
//             a = value.a;
//             m_Value = new Color32((byte)r, (byte)g, (byte)b, (byte)a);
//         }
//     }
//
//     [TrackChangesOn(nameof(r))]
//     [TrackChangesOn(nameof(g))]
//     [TrackChangesOn(nameof(b))]
//     [TrackChangesOn(nameof(a))]
//     [UsedImplicitly]
//     public void Update(IPropertyNode changedPropertyNode)
//     {
//         // Enumerable.Range(0, 65)
//         //     .Select(num => $"{num}: " + string.Join(", ", Enumerable.Range(7, 32)
//         //         .Where(mod => "rgbaRGBA".Select(c => ((uint)c - num) % mod).Distinct().Count() == 8))).ToList();
//         // "rgbaRGBA".Select(c => ((uint)c - 1) % 10).ToList();
//         // changedPropertyNode.GetProperty(nameof(m_Value)).SetValue((Color)new Color32((byte)r, (byte)g, (byte)b, (byte)a));
//         m_Value = new Color32((byte)r, (byte)g, (byte)b, (byte)a);
//         Debug.Log($"UPDATE: " + (m_Value).ToString() + " changedPropertyNode: " + changedPropertyNode.PropertyPath);
//     }
//
//     private static readonly Color[] ColorMasks = new[]
//     {
//         Color.green - Color.black,      // G
//         Color.red - Color.black,        // R
//         Color.magenta,                  // g
//         Color.cyan,                     // r
//         Color.black,                    // A
//         Color.blue - Color.black,       // B
//         Color.white - Color.black,      // a
//         Color.white - Color.blue + Color.black,    // b
//     };
//
//     public Color GetLowValueColor(IPropertyNode property) => m_Value * ColorMasks[((uint)property.Name[0] - 1) % 10];
//     
//     public Color GetHighValueColor(IPropertyNode property) => m_Value * ColorMasks[((uint)property.Name[0] - 1) % 10] + ColorMasks[((uint)property.Name.ToUpperInvariant()[0] - 1) % 10];
//     
//     public AnnotatedColor() => Value = Color.clear;
//
//     public AnnotatedColor(Color32 color) => Value = color;
//
//     public AnnotatedColor(Color color) => Value = color;
//
//     public AnnotatedColor(string hexString) => Value = ColorUtility.TryParseHtmlString(hexString, out Color color) ? color : Color.clear;
//
//     public override string ToString() => ColorUtility.ToHtmlStringRGBA(Value);
//
//     public void Clickable() => Debug.Log(this.ToString() + " => " + Value.ToString());
//     
//     public static explicit operator AnnotatedColor(string hexString) => new(hexString);
//         
//     public static explicit operator AnnotatedColor(Color color) => new(color);
//     
//     public static explicit operator AnnotatedColor(Color32 color) => new(color);
//
//     public static explicit operator Color(AnnotatedColor color) => color.Value;
//     
//     public static explicit operator Color32(AnnotatedColor color) => color.Value;
// }
