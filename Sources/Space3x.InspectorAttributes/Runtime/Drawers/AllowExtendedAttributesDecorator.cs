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
            Debug.Log("[AEA!] END OnUpdate()");
        }

        public override void OnReset(bool disposing = false)
        {
            if (!disposing) 
                m_IsReady = false;
            // else
            //     if (Property is IControlledProperty propertyWithSerializedObject && propertyWithSerializedObject.Controller != null)
            //         PropertyAttributeController.RemoveFromCache((PropertyAttributeController)propertyWithSerializedObject.Controller);

            base.OnReset(disposing);
        }
    }
}
