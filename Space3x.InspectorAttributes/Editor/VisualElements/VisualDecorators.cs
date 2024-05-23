using System;
using System.Runtime.CompilerServices;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor.Drawers;
using Space3x.InspectorAttributes.Editor.Extensions;
using Space3x.UiToolkit.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.VisualElements
{
    public interface IEditorDecorator { }
    
    public interface IElementBlock { }
    
//    [UxmlElement]
//    public partial class DetachedDecorators : VisualElement
//    {
//        public VisualElement Origin { get; set; }
//        public PropertyField RelatedField { get; set; }
//        public DetachedDecorators() => AddToClassList($"ui3x-detached-decorators");
//    }

    [UxmlElement]
    [HideInInspector]
    public partial class AutoDecorator : VisualElement, IEditorDecorator
    {
        public static readonly string UssClassName = "ui3x-auto-decorator";

        public AutoDecorator() =>
            this
                .WithClasses(AutoDecorator.UssClassName)
                .SetVisible(false);
    }

    [UxmlElement]
    [HideInInspector]
    public partial class BlockDecorator : AutoDecorator, IElementBlock
    {
        public new static readonly string UssClassName = "ui3x-block-decorator";
        public BlockDecorator() => this.WithClasses(BlockDecorator.UssClassName).SetVisible(true);
    }
    
    [UxmlElement]
    [HideInInspector]
    public partial class GhostDecorator : AutoDecorator
    {
        public new static readonly string UssClassName = "ui3x-ghost-decorator";
        
        public IDecorator TargetDecorator { get; set; }
        
        public VisualElement DecoratorContainer => TargetDecorator.Container;
        
        public GhostDecorator() => this.WithClasses(GhostDecorator.UssClassName).SetVisible(true);
    }
    
    [UxmlElement]
    [HideInInspector]
    public partial class GhostDecoratorBlock : GhostDecorator, IElementBlock
    {
        public new static readonly string UssClassName = "ui3x-ghost-decorator-block";
        public GhostDecoratorBlock() => this.WithClasses(GhostDecoratorBlock.UssClassName).SetVisible(true);
    }
    
//    [UxmlElement]
//    public partial class VisualElementReference : VisualElement
//    {
//        public VisualElement Reference { get; set; }
//
//        public override VisualElement contentContainer => Reference ?? this;
//        
//        public VisualElementReference() => AddToClassList($"ui3x-reference");
//
//        public new void Add(VisualElement child) => Reference.Add(child);
//    }
    
    [UxmlElement]
    [HideInInspector]
    public partial class GroupMarker : VisualElement
    {
        public static readonly string UssClassName = "ui3x-group-marker";
        public static readonly string UssUsedClassName = UssClassName + "__used";
        
        public GroupType Type { get; set; }
        
        [UxmlAttribute]
        public string GroupName { get; set; }
        
//        public AutoDecorator Origin { get; set; }
        public VisualElement Origin { get; set; }
        
        // public GroupMarkerDecorator<AutoDecorator, GroupMarkerAttribute> MarkerDecorator { get; set; }
        public IGroupMarkerDecorator MarkerDecorator { get; set; }

        public GroupMarker LinkedMarker { get; set; } = null;
        
        /// <summary>
        /// Whether this marker is used as the beginning of a group or as it's ending tag.
        /// </summary>
        [UxmlAttribute]
        public bool IsOpen { get; set; }
        
        public bool IsUsed { get; set; } = false;

//        public void LinkTo<TDecorator, TGroupAttribute>(GroupMarker other)
//            where TDecorator : AutoDecorator, new()
//            where TGroupAttribute : GroupMarkerAttribute
        public void LinkTo(GroupMarker other)
        {
            if (MarkerDecorator is not IGroupMarkerDecorator thisDecorator)
                throw new Exception("Marker decorator not set.");
            if (other.MarkerDecorator is not IGroupMarkerDecorator otherDecorator)
                throw new Exception("Other Marker decorator not set.");
            if (LinkedMarker != null || other.LinkedMarker != null)
                if (thisDecorator.LinkedMarkerDecorator != otherDecorator)
                    throw new Exception("Linked marker already set.");

            LinkedMarker = other;
            other.LinkedMarker = this;
            IsUsed = true;
            other.IsUsed = true;
            this.WithClasses(GroupMarker.UssUsedClassName);
            other.WithClasses(GroupMarker.UssUsedClassName);
            otherDecorator.LinkedMarkerDecorator = thisDecorator;
            thisDecorator.LinkedMarkerDecorator = otherDecorator;
        }

        public GroupMarker() => 
            this
                .WithClasses(UssClassName, "ui3x-id-" + RuntimeHelpers.GetHashCode(this));
    }
}
