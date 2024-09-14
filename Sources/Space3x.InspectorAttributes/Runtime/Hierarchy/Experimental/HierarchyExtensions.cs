using Space3x.UiToolkit.Types;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
{
    internal static class HierarchyExtensions
    {
        public static ITreeRenderer GetTreeRenderer(this VisualElement self) =>
            self switch
            {
                BindablePropertyField binPF => (binPF.Field as ITreeRenderer)?.GetTreeRenderer(),
                ITreeRenderer renderer => renderer.GetTreeRenderer(),
                IOffscreenContainer offsContainer => offsContainer.Renderer,
                _ => self?.parent?.GetTreeRenderer()
            };

        public static VisualElement GetTreeContainer(this VisualElement self) =>
            self?.GetTreeRenderer()?.contentContainer;

        public static ITreeRenderer GetParentTreeRenderer(this ITreeRenderer self) =>
            self?.TreeNode?.parent?.Value?.Target;
        
        public static Context GetContext(this ITreeRenderer self) => self?.TreeNode?.Value;
        
        public static OffscreenEventHandler GetEventHandler(this VisualElement self) => self?.GetTreeContainer() as OffscreenEventHandler;

        public static IPanel GetLogicalPanel(this VisualElement self) =>
            self?.panel ?? (self?.GetTreeRenderer() as VisualElement)?.GetLogicalPanel();

        public static bool IsPhysicallyAndLogicallyDetachedFromPanel(this VisualElement self) =>
            self?.panel == null && self?.GetLogicalPanel() == null;
    }

    /* RESOLVERS */
    public static class Resolvers
    {
        public static BindablePropertyField Resolve(this BindablePropertyField self, bool attachDecorators = false, bool showInInspector = false)
        {
            var handler = self.GetEventHandler() as OffscreenEventHandler;
            /*
             * At this point, a suitable VisualElement field or, a custom PropertyDrawer or, a ITreeRenderer (for
             * nested object properties) has been added to the hierarchy, including any nested children properties.
             */
            handler?.RaiseAndResetOnAttachToPanelEventOnce();

            if (attachDecorators)
                self.AttachDecoratorDrawers();
            
            handler?.RaiseAndResetOnAttachToPanelEventOnce();
            
            handler?.RaiseAndResetOnGeometryChangedEventOnce();
            
            if (showInInspector)
                self.EnableInClassList(UssConstants.UssShowInInspector, true);

            handler?.RaiseOnPropertyAddedEvent();   // Also handles .RaiseAndResetDelayedCalls() upon completion.
            
            return self;
        }
    }
}
