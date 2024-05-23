﻿using Space3x.Attributes.Types;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(LayerAttribute))]
    public class LayerDrawer : Drawer<LayerAttribute>
    {
        protected override VisualElement OnCreatePropertyGUI(SerializedProperty property)
        {
            var field = new LayerField() { label = property.displayName };
            field.AddToClassList(BaseField<int>.alignedFieldUssClassName);
            field.BindProperty(property);
            return field;
        }
    }
}
