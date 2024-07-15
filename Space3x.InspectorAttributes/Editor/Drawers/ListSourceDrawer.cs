using System;
using System.Collections.Generic;
using Space3x.Attributes.Types;
using Space3x.Attributes.Types.DeveloperNotes;
using Space3x.InspectorAttributes.Editor.Utilities;
using Space3x.InspectorAttributes.Editor.VisualElements;
using UnityEditor;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    [Draft]
    [CustomPropertyDrawer(typeof(ListSourceAttribute), useForChildren: false)]
    public class ListSourceDrawer : Drawer<ListSourceAttribute>
    {
        private bool m_IsReady = false;
        private Invokable<List<string>, List<string>> m_Invokable = null;
        private VariableListView m_ListView = null;
        
        public override ListSourceAttribute Target => (ListSourceAttribute) attribute;
        
        protected override VisualElement OnCreatePropertyGUI(IPropertyNode property)
        {
            m_ListView = new VariableListView()
            {
                Text = Target.Text ?? preferredLabel,
                style =
                {
                    maxHeight = 230f
                }
            };
            m_ListView.BindProperty<List<string>>(property, VariableListView.ValueProperty);
            m_ListView.RegisterCallback<AttachToPanelEvent>(OnAttachContentToPanel);
            m_ListView.RegisterValueChangedCallback(OnValueChanged);
            return m_ListView;
        }

        private void OnValueChanged(ChangeEvent<List<string>> ev)
        {
            if (Property.HasSerializedProperty() &&
                Property.GetSerializedProperty() is SerializedProperty serializedProperty)
            {
                if (!serializedProperty.isArray)
                {
                    DebugLog.Warning($"Property {serializedProperty.name} is not an array.");
                }
                else
                {
                    serializedProperty.serializedObject.Update();
                    serializedProperty.ClearArray();
                    for (var i = 0; i < ev.newValue.Count; i++)
                    {
                        serializedProperty.InsertArrayElementAtIndex(i);
                        serializedProperty.GetArrayElementAtIndex(i).stringValue = ev.newValue[i];
                    }
                    serializedProperty.serializedObject.ApplyModifiedProperties();
                }
            }
            else
            {
                throw new NotImplementedException("Not implemented yet for non-serialized properties.");
            }
        }

        private void OnAttachContentToPanel(AttachToPanelEvent evt) => OnUpdate();

        public override void OnUpdate()
        {
            if (string.IsNullOrEmpty(Target.MemberName)) return;
            if (!m_IsReady)
            {
                m_IsReady = true;
                if (Property.TryCreateInvokable<List<string>, List<string>>(Target.MemberName, out var invokable))
                    m_Invokable = invokable;
                else
                {
                    DebugLog.Warning($"Failed to create invokable for {Target.MemberName} in {nameof(ListSourceDrawer)}.");
                    return;
                }
            }
            var res = m_Invokable?.Invoke();
            if (res != null)
            {
                var shouldUpdate = res.Count != m_ListView.AvailableItems.Count || res.Count == 0;
                if (!shouldUpdate)
                    for (var i = 0; i < res.Count; i++)
                        if (res[i] != m_ListView.AvailableItems[i])
                        {
                            shouldUpdate = true;
                            break;
                        }
                if (shouldUpdate) 
                    m_ListView.AvailableItems = new List<string>(res);
            }
            m_ListView.value = GetSelectedItemsFromPropertyValue();
        }

        private List<string> GetSelectedItemsFromPropertyValue()
        {
            if (Property.HasSerializedProperty() &&
                Property.GetSerializedProperty() is SerializedProperty serializedProperty)
            {
                if (!serializedProperty.isArray)
                {
                    DebugLog.Warning($"Property {serializedProperty.name} is not an array.");
                    return new List<string>();
                }
                else
                {
                    var list = new List<string>();
                    for (var i = 0; i < serializedProperty.arraySize; i++)
                    {
                        list.Add(serializedProperty.GetArrayElementAtIndex(i).stringValue);
                    }
                    return list;
                }
            }

            throw new NotImplementedException("Not implemented yet for non-serialized properties.");
        }
    }
}
