using System.Collections.Generic;
using Space3x.InspectorAttributes.Editor.VisualElements;
using Space3x.UiToolkit.Types;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Utilities
{
    public class BindablePropertyFieldFactory
    {
        public PropertyAttributeController Controller { get; private set; }
        public VisualElement Container { get; set; }
        
        private List<BindablePropertyField> m_BindableFields = new List<BindablePropertyField>();

        public static BindablePropertyFieldFactory Create(PropertyAttributeController controller)
        {
            return new BindablePropertyFieldFactory()
            {
                Controller = controller
            };
        }

        public BindablePropertyFieldFactory Rebuild(VisualElement container = null)
        {
            container ??= Container;
            if (container == null) return this;
            Clear();
            Container = container;
            container.WithClasses(UssConstants.UssFactoryPopulated);
            
            for (var i = 0; i < Controller.Properties.Values.Count; i++)
            {
                var propertyNode = Controller.Properties.Values[i];
                if (string.IsNullOrEmpty(propertyNode.Name)) continue;
                // DebugLog.Info($"  <color=#FFFF00FF>REQUESTING {prop.Name} ON: {parentPath}</color> ({prop.PropertyPath})");
                
                if (propertyNode is InvokablePropertyNodeBase invokableNode && invokableNode.IncludeInInspector())
                {
                    var invokableField = new BindablePropertyField();
                    invokableField.WithClasses(invokableNode.ShowInInspector(), UssConstants.UssShowInInspector);
                    invokableField.BindProperty(invokableNode, applyCustomDrawers: true);
                    container.Add(invokableField);
                    invokableField.AttachDecoratorDrawers();
                    m_BindableFields.Add(invokableField);
                }
                else
                {
                    var showInInspector = propertyNode is SerializedPropertyNodeBase || propertyNode.ShowInInspector();
                    if (showInInspector || propertyNode.IncludeInInspector())
                    {
                        var bindableField = new BindablePropertyField();
                        bindableField.WithClasses(showInInspector, UssConstants.UssShowInInspector);
                        bindableField.BindProperty(propertyNode, applyCustomDrawers: true);
                        container.Add(bindableField);
                        bindableField.AttachDecoratorDrawers();
                        m_BindableFields.Add(bindableField);
                        if (propertyNode.HasChildren() && !propertyNode.IsArrayOrList())
                            bindableField.TrackPropertyValue(propertyNode, PropertyAttributeController.OnPropertyValueChanged);
                        // DebugLog.Info($"    <color=#66FF66FF>{nonSerializedNode.Name} SYNCED as <b>NON</b>-SERIALIZED ON: {parentPath}</color> ({nonSerializedNode.PropertyPath})");
                    }
                }
            }

            return this;
        }

        public BindablePropertyFieldFactory Clear()
        {
            RemoveAllBindableFields();
            Container?.Clear();
            Container?.WithClasses(false, UssConstants.UssFactoryPopulated);
            return this;
        }
        
        private void RemoveAllBindableFields()
        {
            for (var index = m_BindableFields.Count - 1; index >= 0; index--)
            {
                var bindableField = m_BindableFields[index];
                bindableField.ProperlyRemoveFromHierarchy();
            }

            m_BindableFields.Clear();
        }
    }
}