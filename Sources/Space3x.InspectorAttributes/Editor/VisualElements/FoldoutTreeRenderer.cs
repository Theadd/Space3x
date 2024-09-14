using Space3x.Properties.Types;
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
        
        ITreeRenderer ITreeRenderer.GetTreeRenderer() => this;
        
        public NTree<Context> TreeNode { get; set; }

        public override VisualElement contentContainer => Container ?? base.contentContainer;

        public new void Add(VisualElement child) => contentContainer.Add(child);

        // public virtual IOffscreenEventHandler GetOffscreenEventHandler() => null;
    }
}
