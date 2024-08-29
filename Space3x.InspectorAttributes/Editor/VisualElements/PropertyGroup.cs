using Space3x.Attributes.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.VisualElements
{
    [UxmlElement]
    [HideInInspector]
    public partial class PropertyGroup : VisualElement, ILayoutElement
    {
        [UxmlAttribute]
        public string GroupName { get; set; } = string.Empty;
        
        public GroupType Type { get; set; }
        
        public PropertyGroup() => AddToClassList($"ui3x-property-group");
        
        public virtual bool GroupContains(VisualElement element) => element.parent == contentContainer || element.parent == this;
    }
}
