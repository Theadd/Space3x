﻿using System.Collections.Generic;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor.Drawers.NonSerialized;
using Space3x.InspectorAttributes.Editor.Extensions;
using Space3x.InspectorAttributes.Editor.VisualElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(AllowExtendedAttributes), useForChildren: false)]
    public class AllowExtendedAttributesDecorator : Decorator<AutoDecorator, AllowExtendedAttributes>
    {
        public PropertyAttributeController Controller;

        protected override bool RedrawOnAnyValueChange => true;

        private bool m_IsReady;

        private void PopulateNonSerializedProperties()
        {
            var parentElement = Container.hierarchy.parent;
            var allFields = new Dictionary<string, VisualElement>();
            foreach (var child in parentElement.hierarchy.Children())
            {
                if (child is PropertyField childField)
                {
                    var childProp = childField.GetSerializedProperty();
                    if (childProp != null)
                        allFields.Add(childProp.name, childField);
                }
            }
            VisualElement previousField = null;
            for (var i = 0; i < Controller.Properties.Values.Count; i++)
            {
                var prop = Controller.Properties.Values[i];
                if (prop is SerializedPropertyNode serializedNode)
                {
                    if (allFields.TryGetValue(serializedNode.Name, out VisualElement targetField))
                    {
                        previousField = targetField;
                        serializedNode.Field = targetField;
                    }
                    else
                        Debug.LogWarning($"No PropertyField found for {serializedNode.Name}.");
                }
                else if (prop is NonSerializedPropertyNode nonSerializedNode)
                {
                    var bindableField = new BindablePropertyField();
                    nonSerializedNode.Field = bindableField;
                    bindableField.BindProperty(nonSerializedNode, applyCustomDrawers: true);
                    previousField.AddAfter(bindableField);
                    bindableField.AttachDecoratorDrawers();
                    previousField = bindableField;
                }
            }
        }
        
        public override void OnAttachedAndReady(VisualElement element)
        {
            Controller = PropertyAttributeController.GetInstance(Property);
        }

        public override void OnUpdate()
        {
            if (!m_IsReady)
            {
                m_IsReady = true;
                PopulateNonSerializedProperties();
            }
        }

        public override void OnReset(bool disposing = false)
        {
            if (disposing)
                if (Controller != null)
                    PropertyAttributeController.RemoveFromCache(Controller);

            Controller = null;
            base.OnReset(disposing);
        }
    }
}