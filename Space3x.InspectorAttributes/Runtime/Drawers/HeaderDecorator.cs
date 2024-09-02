using Space3x.InspectorAttributes.Editor.VisualElements;
using Space3x.UiToolkit.Types;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(HeaderAttribute), useForChildren: false)]
    public class HeaderDecorator : Decorator<BlockDecorator, HeaderAttribute>, IAttributeExtensionContext<HeaderAttribute>
    {
        public override HeaderAttribute Target => (HeaderAttribute) attribute;

        protected override void OnCreatePropertyGUI(VisualElement container)
        {
            container.Add(new Label(Target.header ?? "").WithClasses("ui3x-header"));
        }
    }
}
