using System;
using Space3x.Properties.Types;
using Space3x.UiToolkit.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
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
