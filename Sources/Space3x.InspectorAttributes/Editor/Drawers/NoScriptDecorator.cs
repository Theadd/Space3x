using System.Linq;
using Space3x.Attributes.Types;
using UnityEditor;
using Space3x.InspectorAttributes.Editor.VisualElements;
using Space3x.InspectorAttributes.Editor.Extensions;
using UnityEditor.UIElements;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(NoScriptAttribute), useForChildren: true)]
    public class NoScriptDecorator : Decorator<AutoDecorator, NoScriptAttribute>, IAttributeExtensionContext<NoScriptAttribute>
    {
        public override void OnUpdate()
        {
            if (Field != null)
            {
                var root = Field.GetClosestParentOfType<InspectorElement>();
                var topPropertyField = root?.GetChildren<PropertyField>().FirstOrDefault();
                if (topPropertyField != null && topPropertyField.bindingPath == "m_Script")
                    topPropertyField.SetVisible(false);
            }
        }
    }
}
