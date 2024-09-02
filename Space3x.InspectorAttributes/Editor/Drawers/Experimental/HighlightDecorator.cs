using Space3x.Attributes.Types;
using UnityEditor;
using Space3x.InspectorAttributes.Editor.VisualElements;
using Space3x.UiToolkit.Types;
using UnityEngine;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(HighlightAttribute), useForChildren: true)]
    public class HighlightDecorator : Decorator<AutoDecorator, HighlightAttribute>
    {
        public override HighlightAttribute Target => (HighlightAttribute) attribute;
        
        public override void OnUpdate()
        {
            var element = VisualTarget;
            if (element != null)
            {
                element.style.borderTopWidth = 4f;
                element.style.borderRightWidth = 4f;
                element.style.borderBottomWidth = 4f;
                element.style.borderLeftWidth = 4f;
                element.style.borderTopColor = Color.green;
                element.style.borderRightColor = Color.green;
                element.style.borderBottomColor = Color.green;
                element.style.borderLeftColor = Color.green;
                element.WithClasses("HIGHLIGHTER");
            }
        }
    }
}
