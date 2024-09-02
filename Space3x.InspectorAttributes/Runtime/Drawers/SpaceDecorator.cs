using Space3x.InspectorAttributes.Editor.VisualElements;
using Space3x.UiToolkit.Types;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(SpaceAttribute), useForChildren: false)]
    public class SpaceDecorator : Decorator<BlockDecorator, SpaceAttribute>, IAttributeExtensionContext<SpaceAttribute>
    {
        public override SpaceAttribute Target => (SpaceAttribute) attribute;

        protected override void OnCreatePropertyGUI(VisualElement container)
        {
            var spacer = new VisualElement().WithClasses("ui3x-space");
            spacer.style.height = Target.height;
            spacer.style.width = Target.height;
            container.style.flexGrow = 0f;
            container.style.flexShrink = 0f;
            spacer.style.flexGrow = 0f;
            spacer.style.flexGrow = 0f;
            container.Add(spacer);
        }
    }
}
