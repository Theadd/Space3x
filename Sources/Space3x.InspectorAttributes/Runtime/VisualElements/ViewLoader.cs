using Space3x.UiToolkit.Types;
using Unity.Properties;
using UnityEngine;
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
                Reload();
            }
        }

        [System.NonSerialized]
        private UIViewContainer m_ViewContainer;

        public ViewLoader()
        {
            /* // Reload();
             * Reload() is automatically called by deserialization of the View property, this would reload the UI twice.
             */
        }
        
        private void Reload()
        {
            Debug.LogError($"[VD!] IS FIRST CALL TO ViewLoader.Reload()!!??!??!?!?!?!!??!?!?!?!?!?!!?");
            Clear();
            if (m_ViewContainer != null)
                Object.Destroy(m_ViewContainer);
            if (m_View == null) return;
            m_ViewContainer = ScriptableObject.CreateInstance<UIViewContainer>();
            m_ViewContainer.SetView(m_View);
            
            var controller = PropertyAttributeController.GetInstance((UnityEngine.Object)m_ViewContainer);
            var property = controller.GetProperty("uiView");
            
            var bindableField = new BindablePropertyField();
            Add(bindableField);
            bindableField.BindProperty(property, false);
            bindableField.Resolve(showInInspector: true);
        }
    }
}
