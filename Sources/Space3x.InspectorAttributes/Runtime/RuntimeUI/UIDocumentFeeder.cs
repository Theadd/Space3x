using System.Collections.Generic;
using Space3x.Attributes.Types;
using Space3x.Properties.Types;
using Space3x.UiToolkit.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
{
    internal interface IUIDocumentFeeder
    {
        protected UIDocument Document { get; set; }

        internal sealed void Render(IPropertyNode propertyNode, VisualElement element)
        {
            // var controller = PropertyAttributeController.GetInstance((UnityEngine.Object)this);
            // var property = controller.GetProperty(propertyNode.PropertyPath);
            // var ingredientProperty = controller.GetProperty("");
            Debug.Log($"property is null? {(propertyNode == null)}");
            var bindableField = new BindablePropertyField();
            element.Add(bindableField);
            // bindableField.BindProperty(propertyNode, true);
            bindableField.BindProperty(propertyNode, false);
            bindableField.WithClasses(UssConstants.UssShowInInspector);
            // bindableField.AttachDecoratorDrawers();
        }
        
        internal virtual void Reload()
        {
            PropertyAttributeController.ClearCache();
            UngroupedMarkerDecorators.ClearCache();
        }
    }
    
    // [ExecuteAlways]
    [RequireComponent(typeof(UIDocument))]
    public sealed class UIDocumentFeeder : MonoBehaviour, IUIDocumentFeeder
    {
        [AllowExtendedAttributes]
        [Button(nameof(PrintInstanceIds))]
        public ScriptableUIView uiView;
        private UIDocument m_Document;
        private static List<int> s_InstanceIds = new List<int>();

        public void PrintInstanceIds()
        {
            Debug.Log("PrintInstanceIds() => " + string.Join(", ", s_InstanceIds));
        }

        private void OnEnable() => m_Document = GetComponent<UIDocument>();

        private void OnDisable()
        {
            ((IUIDocumentFeeder)this).Reload();
        }

        private void Awake()
        {
            ((IUIDocumentFeeder)this).Reload();
            s_InstanceIds.Add(this.GetInstanceID());
        }

        // [TrackChangesOn(nameof(uiView))]
        public void OnRenderView(IPropertyNode uiViewProperty)
        {
            Debug.Log($"<color=#000000FF><b><u>@UIDocumentFeeder OnRenderView(): {string.Join(", ", s_InstanceIds)}</u></b></color>");
            if (uiView == null) return;
            var container = m_Document.rootVisualElement.Q(uiView.viewName);
            ((IUIDocumentFeeder)this).Render(uiViewProperty, container);
        }
        
        private void Start()
        {
            ((IUIDocumentFeeder)this).Reload();
            Debug.Log($"<color=#000000FF><b><u>@UIDocumentFeeder Start(): {string.Join(", ", s_InstanceIds)}</u></b></color>");
            var controller = PropertyAttributeController.GetInstance((UnityEngine.Object)this);
            var property = controller.GetProperty(nameof(uiView));
            OnRenderView(property);
        }

        static UIDocumentFeeder()
        {
            Debug.Log($"<color=#000000FF><b><u>@UIDocumentFeeder STATIC constructor: {string.Join(", ", s_InstanceIds)}</u></b></color>");
        }

        UIDocument IUIDocumentFeeder.Document
        {
            get => m_Document;
            set => m_Document = value;
        }
    }
}
