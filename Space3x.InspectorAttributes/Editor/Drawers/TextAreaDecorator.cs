using System.Linq;
using Space3x.InspectorAttributes.Editor.Extensions;
using Space3x.InspectorAttributes.Editor.VisualElements;
using Space3x.UiToolkit.Types;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(TextAreaAttribute), useForChildren: true)]
    public class TextAreaDecorator : Decorator<AutoDecorator, TextAreaAttribute>, IAttributeExtensionContext<TextAreaAttribute>
    {
        public override TextAreaAttribute Target => (TextAreaAttribute) attribute;
        
        public override void OnUpdate()
        {
            VisualTarget.GetChildrenFields().OfType<TextField>().ForEach(field =>
            {
                field.multiline = true;
                field.verticalScrollerVisibility = ScrollerVisibility.Auto;
                field.style.minHeight = Target.minLines * 20;
                field.style.maxHeight = Target.maxLines * 20;
                field.style.height = new StyleLength(StyleKeyword.Auto);
                field.WithClasses(UssConstants.UssTextArea);
            });
        }
    }
}
