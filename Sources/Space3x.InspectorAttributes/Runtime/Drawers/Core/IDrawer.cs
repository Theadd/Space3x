using System;
using Space3x.Properties.Types;
using UnityEngine;
using UnityEngine.Internal;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
{
    [ExcludeFromDocs]
    public interface IDrawer : IDisposable
    {
        public static StyleSheet DefaultStyleSheet  = Resources.Load<StyleSheet>("Space3x.InspectorAttributes.Stylesheet");
        
        public IPropertyNode Property { get; }
        
        public VisualElement Container { get; }
        
        public VisualElement Field { get; }
        
        /// <summary>
        /// The VisualElement that the effects of the decorator or property drawer should be applied to.
        /// </summary>
        public VisualElement VisualTarget { get; }
        
        /// <summary>
        /// The closest parent InspectorElement related to this decorator or property drawer.
        /// </summary>
        public BindableElement InspectorElement => (BindableElement)Field?.GetCommonClosestParentEventHandler();
        
// #if UNITY_EDITOR
//         public BindableElement InspectorElement => (BindableElement)Field?.GetClosestParentOfType<UnityEditor.UIElements.InspectorElement>() 
//                                                    ?? Field?.GetClosestParentOfType<TemplateContainer>();
// #else
//         public BindableElement InspectorElement => (BindableElement)Field?.GetClosestParentOfType<TemplateContainer>();
// #endif
        
        public MarkerDecoratorsCache DecoratorsCache { get; }

        /// <summary>
        /// Override this method to perform any custom logic when the decorator or property drawer needs to be updated.
        /// </summary>
        public void OnUpdate();

        void OnReset(bool disposing = false);

        public void AddDefaultStyles(VisualElement target)
        {
            // TODO: FIXME: Next line is a workaround to prevent our stylesheets to be added to UI Builder interface.
            target = InspectorElement;
            if ((target ??= InspectorElement)?.styleSheets.Contains(DefaultStyleSheet) == false)
                target.styleSheets.Add(DefaultStyleSheet);
        }
    }
    
    public interface IDrawer<TAttribute> : IDrawer
    {
        public TAttribute Target { get; }
    }
}
