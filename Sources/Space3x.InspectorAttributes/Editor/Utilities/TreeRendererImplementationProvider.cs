using Space3x.InspectorAttributes.Editor.VisualElements;
using Space3x.Properties.Types;
using UnityEditor;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor
{
    [InitializeOnLoad]
    internal class TreeRendererImplementationProvider : ITreeRendererUtility
    {
        static TreeRendererImplementationProvider() =>
            TreeRendererUtility.RegisterImplementationProvider(new TreeRendererImplementationProvider());
        
        public VisualElement Create(IPropertyNode property)
        {
            return new FoldoutTreeRenderer()
            {
                text = property.DisplayName(),
                value = true
            };
        }
    }
}
