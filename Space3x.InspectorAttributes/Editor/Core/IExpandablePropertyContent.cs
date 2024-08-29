using System;
using UnityEditor.UIElements;
using UnityEngine.Internal;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    [ExcludeFromDocs]
    public interface IExpandablePropertyContent
    {
        VisualElement Content { get; set; }
        VisualElement ContentContainer { get; set; }
        public bool IsExpanded { get; set; }
        public MarkerDecoratorsCache DecoratorsCache { get; }

        VisualElement CreateContentGUI();
        
        public virtual void RebuildExpandablePropertyContentGUI(Action onContentAttached = null)
        {
            ContentContainer ??= new VisualElement() { name = "ContentContainer" };
            if (Content != null)
            {
                Content.Unbind();
                ContentContainer.Clear();
            }
            Content = CreateContentGUI();
            if (onContentAttached != null) 
                Content.RegisterCallbackOnce<AttachToPanelEvent>(_ => onContentAttached.Invoke());
            /* TODO: FIXME: Uncomment: ContentContainer.SetVisible(false); */
            ContentContainer.Add(Content);
        }
    }
}
