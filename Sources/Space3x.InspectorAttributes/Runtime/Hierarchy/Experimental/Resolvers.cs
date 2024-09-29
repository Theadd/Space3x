using System.Linq;
using Space3x.UiToolkit.Types;

namespace Space3x.InspectorAttributes
{
    public static class Resolvers
    {
        public static BindablePropertyField Resolve(this BindablePropertyField self, bool attachDecorators = false, bool showInInspector = false)
        {
            // var path = self?.Property?.PropertyPath ?? "--NULL--";
            var handler = self.GetEventHandler() as OffscreenEventHandler;
            /*
             * At this point, a suitable VisualElement field or, a custom PropertyDrawer or, a ITreeRenderer (for
             * nested object properties) has been added to the hierarchy, including any nested children properties.
             */
            handler?.RaiseAndResetOnAttachToPanelEventOnce();

            if (attachDecorators)
                self.AttachDecoratorDrawers();
            
            handler?.RaiseAndResetOnAttachToPanelEventOnce();

            var isEventHandlerOwner = self?.hierarchy.childCount > 0 && self.hierarchy.Children()
                .Any(c => c is ITreeRenderer renderer && (renderer.GetTreeRenderer()?.contentContainer == handler));
            
            if (isEventHandlerOwner)
                handler?.RaiseAndResetOnGeometryChangedEventOnce();
            
            if (showInInspector)
                self.EnableInClassList(UssConstants.UssShowInInspector, true);

            handler?.RaiseOnPropertyAddedEvent();   // Also handles .RaiseAndResetDelayedCalls() upon completion.
            
            return self;
        }
    }
}
