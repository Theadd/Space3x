using System;
using System.Runtime.CompilerServices;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor.Drawers;
using Space3x.UiToolkit.Types;
using UnityEngine;
using UnityEngine.Internal;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.VisualElements
{
    public interface IEditorDecorator { }
    
    public interface IElementBlock { }
    
    public interface ILayoutElement { }

    [UxmlElement]
    [HideInInspector]
    public partial class AutoDecorator : BindableElement, IEditorDecorator
    {
        protected AutoDecorator(string className, bool visible = true) =>
            this.WithClasses(UssConstants.UssDecorator, className, visible ? "" : UssConstants.UssHidden);

        public AutoDecorator() => this.WithClasses(UssConstants.UssDecorator, UssConstants.UssHidden);
    }

    [UxmlElement]
    [HideInInspector]
    public partial class BlockDecorator : AutoDecorator, IElementBlock
    {
        public BlockDecorator() : base(UssConstants.UssBlockDecorator) { }
    }
    
    [UxmlElement]
    [HideInInspector]
    public partial class GhostDecorator : AutoDecorator
    {
        public IDecorator TargetDecorator { get; set; }
        
        public VisualElement DecoratorContainer => TargetDecorator.Container;
        
        public GhostDecorator() : base(UssConstants.UssGhostDecorator) { }
    }
    
    [ExcludeFromDocs]
    [UxmlElement]
    [HideInInspector]
    public partial class GroupMarker : VisualElement
    {
        public GroupType Type { get; set; }
        
        [UxmlAttribute]
        public string GroupName { get; set; }
        
        public VisualElement Origin { get; set; }
        
        public IGroupMarkerDecorator MarkerDecorator { get; set; }

        public GroupMarker LinkedMarker { get; set; } = null;
        
        /// <summary>
        /// Whether this marker is used as the beginning of a group or as it's ending tag.
        /// </summary>
        [UxmlAttribute]
        public bool IsOpen { get; set; }
        
        public bool IsUsed { get; set; } = false;

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
            this.WithClasses(UssConstants.UssUsedGroupMarker);
            other.WithClasses(UssConstants.UssUsedGroupMarker);
            otherDecorator.LinkedMarkerDecorator = thisDecorator;
            thisDecorator.LinkedMarkerDecorator = otherDecorator;
        }

        public GroupMarker() => 
            this
                .WithClasses(UssConstants.UssGroupMarker, "ui3x-id-" + RuntimeHelpers.GetHashCode(this));
    }
}
