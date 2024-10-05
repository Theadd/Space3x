using Space3x.Attributes.Types.DeveloperNotes;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
{
    internal static class HierarchyExtensions
    {
        public static ITreeRenderer GetTreeRenderer(this VisualElement self) =>
            self switch
            {
                BindablePropertyField binPF => (binPF.Field as ITreeRenderer)?.GetTreeRenderer()
                                               ?? binPF.GetParentOrExpectedParent()?.GetTreeRenderer(),
                ITreeRenderer renderer => renderer.GetTreeRenderer(),
                IOffscreenContainer offsContainer => offsContainer.Renderer,
                GhostDecorator ghostDecorator => ghostDecorator.parent != null
                    ? ghostDecorator.parent.GetTreeRenderer()
                    : BindablePropertyField.s_ExpectedDecoratorParent?.GetTreeRenderer(),
                _ => self?.parent?.GetTreeRenderer()
            };

        public static VisualElement GetTreeContainer(this VisualElement self) => 
            self?.GetTreeRenderer()?.contentContainer;

        [Experimental(Text = "TreeNode implementation is not being propagated already. The internal access modifier should be public when fully implemented.")]
        internal static ITreeRenderer GetParentTreeRenderer(this ITreeRenderer self) =>
            self?.TreeNode?.parent?.Value?.Target;
        
        [Experimental(Text = "TreeNode implementation is not being propagated already. The internal access modifier should be public when fully implemented.")]
        internal static Context GetContext(this ITreeRenderer self) => self?.TreeNode?.Value;
        
        public static OffscreenEventHandler GetEventHandler(this VisualElement self) => self?.GetTreeContainer() as OffscreenEventHandler;
        
        public static IPanel GetLogicalPanel(this VisualElement self)
        {
            if (self?.panel != null) return self.panel;

            var renderer = self?.GetTreeRenderer() as VisualElement;
            if (renderer == null) return null;
            if (renderer == self)
            {
                while (renderer != null && renderer is ITreeRenderer) renderer = renderer.parent;

                if (renderer is BindablePropertyField bpf)
                    return bpf.GetParentOrExpectedParent()?.GetLogicalPanel();

                return renderer?.panel;
            }
            
            return renderer.GetLogicalPanel();
        }

        /// <summary>
        /// Avoid using it extensively, as It could be noticeable on deeply complex UI hierarchies if many elements
        /// rely on it all at once. Once the propagation of TreeNode objects is fully implemented on offscreen renderers,
        /// the performance impact will be negligible.
        /// </summary>
        public static bool IsPhysicallyAndLogicallyDetachedFromPanel(this VisualElement self) =>
            self?.panel == null && self?.GetLogicalPanel() == null;
    }
}
