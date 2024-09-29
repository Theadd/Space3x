using Unity.Properties;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
{
    [UxmlElement]
    public partial class ViewLoader : BindableElement
    {
        [System.NonSerialized]
        private ScriptableObject m_View;
        
        [CreateProperty]
        [UxmlAttribute]
        public ScriptableObject View
        {
            get => m_View;
            set
            {
                if (m_View == value) return;
                m_View = value;
                if (!Application.isPlaying)
                {
                    m_IsAutoReloadReady = true;
                    AutoReload();
                }
            }
        }

        [System.NonSerialized]
        private UIViewContainer m_ViewContainer;
        
        [System.NonSerialized]
        private bool m_AutoReload;
        
        [System.NonSerialized]
        private bool m_IsAutoReloadReady = false;
        
        [System.NonSerialized]
        private bool m_AutoReloaded = false;
        
        [System.NonSerialized]
        private CustomSampler sampler;

        public ViewLoader()
        {
            /* // Reload();
             * Reload() is automatically called by deserialization of the View property, this would reload the UI twice.
             */
            sampler = CustomSampler.Create("ViewLoader.Reload");
            m_AutoReload = !Application.isPlaying;
            if (Application.isPlaying)
                Add(new Button(OnButtonClick) { text = "LOAD!" });
        }

        private void OnButtonClick()
        {
            Reload();
        }
        
        [EventInterest(typeof(AttachToPanelEvent))]
        protected override void HandleEventBubbleUp(EventBase ev)
        {
            if (ev is not AttachToPanelEvent) return;
            AutoReload();
        }

        private void AutoReload()
        {
            if (!m_AutoReload || panel == null || !m_IsAutoReloadReady || m_AutoReloaded) return;
            Reload();
            m_AutoReloaded = true;
        }

        private void Reload()
        {
            // Debug.Log($"[PM!] ViewLoader.Reload() START");
            sampler.Begin();
            Clear();
            if (m_ViewContainer != null)
                Object.Destroy(m_ViewContainer);
            if (m_View == null) return;
            m_ViewContainer = ScriptableObject.CreateInstance<UIViewContainer>();
            m_ViewContainer.SetView(m_View);
            
            var controller = PropertyAttributeController.GetInstance((UnityEngine.Object)m_ViewContainer);
            var property = controller.GetProperty("uiView");
            
            var bindableField = BindablePropertyField.Create(this);
            Add(bindableField);
            bindableField.BindProperty(property, false);
            bindableField.Resolve(showInInspector: true);
            sampler.End();
            // Debug.Log($"[PM!] ViewLoader.Reload() END; IsRuntimeUI = {property.IsRuntimeUI()}");
        }
    }
}
