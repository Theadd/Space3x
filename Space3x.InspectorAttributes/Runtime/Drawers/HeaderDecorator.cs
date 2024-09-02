using Space3x.Properties.Types;
using Space3x.UiToolkit.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
#if UNITY_EDITOR
    [UnityEditor.CustomPropertyDrawer(typeof(HeaderAttribute), false)]
#endif
    [CustomRuntimeDrawer(typeof(HeaderAttribute), false)]
    public class HeaderDecorator : Decorator<BlockDecorator, HeaderAttribute>, IAttributeExtensionContext<HeaderAttribute>
    {
        public override HeaderAttribute Target => (HeaderAttribute) attribute;

        protected override void OnCreatePropertyGUI(VisualElement container)
        {
            container.Add(new Label(Target.header ?? "").WithClasses("ui3x-header"));
        }
    }
}
