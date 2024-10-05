using Space3x.Attributes.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
{
    // [CustomPropertyDrawer(typeof(AllowExtendedAttributes), useForChildren: false)]
    public class AllowExtendedAttributesDecorator : Decorator<AutoDecorator, AllowExtendedAttributes>
    {
        protected override bool UpdateOnAnyValueChange => true;

        private bool m_IsReady;
        
        protected virtual void Extend() { }

        public override void OnAttachedAndReady(VisualElement element)
        {
            DecoratorsCache.DisableAutoGroupingOnActiveSelection(disable: true);
        }

        public override void OnUpdate()
        {
            if (m_IsReady) return;
            m_IsReady = true;
            Extend();
            DecoratorsCache.RebuildAll();
            DecoratorsCache.DisableAutoGroupingOnActiveSelection(disable: false);
            DecoratorsCache.HandlePendingDecorators();
        }

        public override void OnReset(bool disposing = false)
        {
            m_IsReady = false;
            if (disposing && Property?.IsValid() == false) 
                DecoratorsCache?.Dispose();
            base.OnReset(disposing);
        }
    }
}
