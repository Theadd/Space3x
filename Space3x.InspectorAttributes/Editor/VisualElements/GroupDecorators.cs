using Space3x.InspectorAttributes.Editor.Extensions;
using Space3x.UiToolkit.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.VisualElements
{
    [UxmlElement]
    [HideInInspector]
    public abstract partial class GroupDecorator : AutoDecorator
    {
        public new static readonly string UssClassName = "ui3x-group-decorator";
        public GroupDecorator() => this.WithClasses(GroupDecorator.UssClassName);
    }
    
    [UxmlElement]
    [HideInInspector]
    public partial class BeginRowGroup : GroupDecorator, IElementBlock
    {
        public new static readonly string UssClassName = "ui3x-row-begin";
        public BeginRowGroup() => this.WithClasses(BeginRowGroup.UssClassName).SetVisible(true);
    }
    
    [UxmlElement]
    [HideInInspector]
    public partial class EndRowGroup : GroupDecorator
    {
        public new static readonly string UssClassName = "ui3x-row-end";
        public EndRowGroup() => this.WithClasses(EndRowGroup.UssClassName).SetVisible(false);
    }
    
    [UxmlElement]
    [HideInInspector]
    public partial class BeginColumnGroup : GroupDecorator, IElementBlock
    {
        public new static readonly string UssClassName = "ui3x-column-begin";
        public BeginColumnGroup() => this.WithClasses(BeginColumnGroup.UssClassName).SetVisible(true);
    }
    
    [UxmlElement]
    [HideInInspector]
    public partial class EndColumnGroup : GroupDecorator
    {
        public new static readonly string UssClassName = "ui3x-column-end";
        public EndColumnGroup() => this.WithClasses(EndColumnGroup.UssClassName).SetVisible(false);
    }
}
