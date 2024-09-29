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

        // private static int s_Counter = 0;

        // public FieldFactoryBuilder Rebuild(VisualElement container = null)
        // {
        //     s_Counter++;
        //     Debug.Log($"[VD!] FieldFactoryBuilder Rebuild; {GetHashCode()}; COUNTER!: {s_Counter}");
        //     var self = RebuildInternal(container);
        //     s_Counter--;
        //     if (s_Counter == 0)
        //     {
        //         // Debug.Log($"[VD!] OffscreenEventHandler.RaiseAndResetOnFullyRendered(); {GetHashCode()} ");
        //         // OffscreenEventHandler.RaiseAndResetOnFullyRendered();
        //         // Debug.Log($"[VD!] <b>DONE!!</b> OffscreenEventHandler.RaiseAndResetOnFullyRendered(); {GetHashCode()}");
        //     }
        //
        //     Debug.Log($"[VD!] <b>DONE!!</b> FieldFactoryBuilder Rebuild; {GetHashCode()} COUNTER: {s_Counter}");
        //     return self;
        // }
        
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
            var bindableField = BindablePropertyField.Create(Container);
            // bindableField.WithClasses(
            //     propertyNode is SerializedPropertyNodeBase || propertyNode.ShowInInspector(), 
            //     UssConstants.UssShowInInspector);
            if (propertyNode is not InvokablePropertyNodeBase && !IsReadOnlyEnabled && propertyNode.IsReadOnly())
                bindableField.SetEnabled(false);
            bindableField.AddTo(Container);
            BindableFields.Add(bindableField);
            bindableField.BindProperty(propertyNode, applyCustomDrawers: true);
            // TODO: UNCOMMENT
            // bindableField.TrackPropertyValue(propertyNode, changedProperty =>
            // {
            //     try
            //     {
            //         BindableUtility.SetValueWithoutNotify(bindableField.Field, changedProperty.GetValue());
            //     }
            //     catch (Exception e)
            //     {
            //         Debug.LogException(e);
            //     }
            // });
            
            // Container.Add(bindableField);
            // bindableField.AttachDecoratorDrawers();
            bindableField.Resolve(attachDecorators: true,
                showInInspector: propertyNode is SerializedPropertyNodeBase || propertyNode.ShowInInspector());
            // if (Container is IOffscreenEventHandler handler) 
            //     handler.RaiseOnPropertyAddedEvent();
            return bindableField;
        }
    }
}
