using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor.Drawers.NonSerialized;
using Space3x.InspectorAttributes.Editor.Extensions;
using Space3x.InspectorAttributes.Editor.VisualElements;
using Space3x.UiToolkit.Types;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(AllowAttributesOnNonSerializedAttribute), useForChildren: false)]
    public class AllowAttributesOnNonSerializedDecorator : Decorator<AutoDecorator, AllowAttributesOnNonSerializedAttribute>
    {
        public PropertyAttributeController Controller;

        protected override bool RedrawOnAnyValueChange => true;

        public int UpdateCount = 0;

        private Button m_Button;

        // private bool m_IsReady;

        public override void OnAttachedAndReady(VisualElement element)
        {
            // if (m_IsReady) return;
            GhostContainer.WithClasses("dev-ui3x-on-ready");
            Container.WithClasses("dev-ui3x-on-ready");
            Field.WithClasses("dev-ui3x-on-ready");
            ((IDrawer)this).InspectorElement.WithClasses("dev-ui3x-on-ready");
            
            Controller = PropertyAttributeController.GetInstance(this);
            element.WithClasses($"{this.GetType().Name}--{this.GetHashCode()}");
            Debug.Log("PropertyAttributeController DONE! OnAttachedAndReady " + element.AsString());
            if (m_Button == null)
            {
                m_Button = new Button(OnClick) { text = "Breakpoint! #" + UpdateCount };
                element.Add(m_Button);
            }
            for (var i = 0; i < Controller.AnnotatedType.Values.Count; i++)
            {
                var annotatedType = Controller.AnnotatedType.Values[i];
                if (!annotatedType.IsSerializable && !annotatedType.IsHidden)
                {
                    Debug.Log($"[CUSTOM] {annotatedType.Name} {annotatedType.FieldType}");
                    
                }
            }

            // m_IsReady = true;
        }

        private void OnClick()
        {
            GhostContainer.WithClasses("dev-ui3x-on-click");
            Container.WithClasses("dev-ui3x-on-click");
            Field.WithClasses("dev-ui3x-on-click");
            ((IDrawer)this).InspectorElement.WithClasses("dev-ui3x-on-click");

            if (!EnsureContainerIsProperlyAttached(OnContainerDidRebuild))
            {
                Debug.LogError($"<color=FF0000FF>EnsureContainerIsProperlyAttached</color> - Container was NOT properly attached. " + Container?.AsString());
            }

            Debug.Log("<color=#ff0000ff>Trigger debugger breakpoint!</color>");
        }

        private void OnContainerDidRebuild()
        {
            Debug.LogError($"<color=00FF00FF>OnContainerDidRebuild</color> " + Container.AsString());
        }

        public override void OnUpdate()
        {
            UpdateCount++;
            
            if (m_Button != null)
            {
                m_Button.text = "Breakpoint! #" + UpdateCount;
                m_Button.MarkDirtyRepaint();
            }
            
            GhostContainer.WithClasses("dev-ui3x-on-update");
            Container.WithClasses("dev-ui3x-on-update");
            Field.WithClasses("dev-ui3x-on-update");
            ((IDrawer)this).InspectorElement.WithClasses("dev-ui3x-on-update");
            Debug.Log("EVERYTHING SUCCEEDED!");
            Container.SetVisible(true);
        }

        public override void OnReset(bool disposing = false)
        {
            if (disposing)
            {
                if (Controller != null)
                    PropertyAttributeController.RemoveFromCache(Controller);
            }

            m_Button = null;
            Controller = null;
            base.OnReset(disposing);
        }
    }
}
