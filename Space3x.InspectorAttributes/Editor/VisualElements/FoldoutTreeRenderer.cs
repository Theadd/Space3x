using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.VisualElements
{
    [UxmlElement]
    [HideInInspector]
    public partial class FoldoutTreeRenderer : Foldout, ITreeRenderer
    {
        public VisualElement Container { get; set; } = null;

        public virtual void Render(bool shouldRender) { }

        public override VisualElement contentContainer => Container ?? base.contentContainer;

        public new void Add(VisualElement child) => contentContainer.Add(child);
    }
}
