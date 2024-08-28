using System;
using Space3x.InspectorAttributes.Editor.Utilities;
using Space3x.InspectorAttributes.Editor.VisualElements;
using Space3x.UiToolkit.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.FieldFactories
{
    public class FieldFactoryBuilder : FieldFactoryBase
    {
        public FieldFactoryBuilder(PropertyAttributeController controller) => Controller = controller;

        public FieldFactoryBuilder Rebuild(VisualElement container = null)
        {
            container ??= Container;
            if (container == null) return this;
            Clear();
            Container = container;
            container.WithClasses(UssConstants.UssFactoryPopulated);
            
            for (var i = 0; i < Controller.Properties.Values.Count; i++)
            {
                var propertyNode = Controller.Properties.Values[i];
                if (string.IsNullOrEmpty(propertyNode.Name)
                    || (!propertyNode.IncludeInInspector() && !propertyNode.ShowInInspector()))
                    continue;
                
                if (propertyNode is InvokablePropertyNodeBase)
                {
                    AddField(propertyNode);
                }
                else
                {
                    var bindableField = AddField(propertyNode);
                    if (propertyNode.HasChildren() && !propertyNode.IsArrayOrList())
                        bindableField.TrackPropertyValue(propertyNode, PropertyAttributeController.OnPropertyValueChanged);
                }
            }

            return this;
        }

        private BindablePropertyField AddField(IPropertyNode propertyNode)
        {
            var bindableField = new BindablePropertyField();
            bindableField.WithClasses(
                propertyNode is SerializedPropertyNodeBase || propertyNode.ShowInInspector(), 
                UssConstants.UssShowInInspector);
            if (propertyNode is not InvokablePropertyNodeBase && !IsReadOnlyEnabled && propertyNode.IsReadOnly())
                bindableField.SetEnabled(false);
            bindableField.BindProperty(propertyNode, applyCustomDrawers: true);
            bindableField.TrackPropertyValue(propertyNode, changedProperty =>
            {
                // Debug.LogWarning($"[PAC!] [FieldFactoryBuilder] TRACKING PROPERTY CHANGED! ('{changedProperty.PropertyPath}')");
                // if (changedProperty is IBindablePropertyNode node && node.DataSource is IBindableDataSource dataSource)
                //     dataSource.IncreaseVersionNumber();

                try
                {
                    BindableUtility.SetValueWithoutNotify(bindableField.Field, changedProperty.GetValue());
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            });
            Container.Add(bindableField);
            bindableField.AttachDecoratorDrawers();
            BindableFields.Add(bindableField);
            return bindableField;
        }
    }
}
