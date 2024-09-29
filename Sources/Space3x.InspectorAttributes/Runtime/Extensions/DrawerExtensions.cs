using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
{
    public static class DrawerExtensions
    {
        public static IPanel GetPanel(this IDrawer drawer) => drawer is IDecorator decorator
            ? decorator.GhostContainer.panel
            : drawer.Container.panel;
        
        public static IPanel GetLogicalPanel(this IDrawer self) =>
            self is IDecorator decorator
                ? decorator.GhostContainer?.GetLogicalPanel()
                : self.Container?.GetLogicalPanel();
    }
}
