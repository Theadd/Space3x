using System;
using Space3x.Attributes.Types;
using Space3x.UiToolkit.Types;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(AllowExtendedAttributes), useForChildren: false)]
    public class AllowExtendedAttributesDecorator : Space3x.InspectorAttributes.AllowExtendedAttributesDecorator
    {
        private FieldFactoryExtender m_FieldFactory;

        protected override void Extend()
        {
            // if (Field.ClassListContains(UssConstants.UssAttributesExtended)) 
            //     DebugLog.Warning($"<color=#FF0000FF><b>Field: {Field.name} already has {UssConstants.UssAttributesExtended} " +
            //                      $"class list item! ThisHash: {this.GetHashCode()}</b></color>");
#if UNITY_6000_0_OR_NEWER
            // if ((Application.isPlaying && GhostContainer.panel is IRuntimePanel) || Field is BindablePropertyField) return;
            if ((Application.isPlaying && GhostContainer.panel?.contextType == ContextType.Player) || Field is BindablePropertyField) return;
#else
            if ((Application.isPlaying && (GhostContainer.panel is IPanel containerPanel && containerPanel.contextType == ContextType.Player)) 
                || Field is BindablePropertyField) return;
#endif
            var parentElement = Container.hierarchy.parent;
            // Debug.Log($"<color=#000000FF><b><u>@AllowExtendedAttributes Extend(): TH: {GetHashCode()} {Property.PropertyPath}</u></b></color> :: {Field?.AsString()}");
            if (parentElement.ClassListContains(UssConstants.UssFactoryPopulated))
            {
                // parentElement.LogThis($"(INFO) [VD!] <color=#000000FF><b>Populated already by factory!</b></color> TH: {GetHashCode()} {Property.PropertyPath}");
                return;
            }

            m_FieldFactory ??= new FieldFactoryExtender((PropertyAttributeController)Property.GetController());
            if (Field is PropertyField propertyField)
                m_FieldFactory.PropertyFieldOrigin = propertyField;
            else if (Field is BindablePropertyField bindableField)
                m_FieldFactory.PropertyFieldOrigin = bindableField;
            else
                throw new NotImplementedException();
            m_FieldFactory.Rebuild(parentElement);
        }

        public override void OnAttachedAndReady(VisualElement element)
        {
            base.OnAttachedAndReady(element);
#if SPACE3X_DEBUG
            // element.Add(new Button(() => OnClick()) { text = "1", tooltip = "DEBUG ME!", style = { fontSize = 8 } });
            element.Add(new Button(() => OnClick()) { text = element.panel?.contextType.ToString(), tooltip = element.panel?.contextType.ToString(), style = { fontSize = 8 } });
            element.Add(new Button(OnClickAddDevTooltips) { text = "2", tooltip = "Dev Tooltips", style = { fontSize = 8 } });
            element.SetVisible(true);
            element.style.flexWrap = Wrap.Wrap;
            element.style.left = -16;
            element.style.position = Position.Absolute;
            element.style.maxWidth = 4;
            element.style.flexDirection = FlexDirection.Row;
#endif
        }

        private void OnClickAddDevTooltips()
        {
            foreach (var element in Container.hierarchy.parent.GetChildrenFields())
            {
                element.tooltip = element.AsString();
                DebugLog.Info(element.tooltip);
            }
        }

        private void OnClick()
        {
            Debug.Log("STOP");
        }
    }
}
