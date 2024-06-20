using System;
using System.Collections.Generic;
using System.Linq;
using Space3x.InspectorAttributes.Editor.Drawers;
using Space3x.InspectorAttributes.Editor.Extensions;
using Space3x.UiToolkit.Types;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.QuickSearchComponent.Editor.VisualElements
{
    public partial class TypeInstanceField
    {
        public Toggle FoldoutToggle => Foldout?.hierarchy.ElementAt(0) as Toggle;
        public VisualElement FoldoutHeader => FoldoutToggle?.hierarchy.ElementAt(0);
        public Foldout ContentFoldout => Content.Children().FirstOrDefault() as Foldout;

        private void ApplyFoldoutStyles()
        {
            style.marginLeft = 0f;
            style.overflow = new StyleEnum<Overflow>(Overflow.Visible);
            Foldout.style.overflow = new StyleEnum<Overflow>(Overflow.Visible);
            FoldoutToggle.style.marginRight = 0f;
            FoldoutHeader.style.marginLeft = 0f;
            FoldoutHeader.WithClasses(BaseField<bool>.alignedFieldUssClassName, "unity-base-field", "unity-base-text-field");
            var (icon, foldoutLabel, (textField, _)) = FoldoutHeader.AsChildren();
            
            foldoutLabel.WithClasses("unity-base-field__label", "unity-base-text-field__label");
            textField.WithClasses("unity-base-text-field__input", "unity-base-text-field__input--single-line", "ui3x-text-link")
                .WithClasses(false, "unity-base-field__inspector-field")
                .style.flexShrink = 0f;
            textField.style.paddingRight = 0f;
            
            var (textInput, button, _) = textField.AsChildren();
            var (textElement, _) = textInput.AsChildren();

            button.WithClasses(false, "unity-text-element", "unity-button")
                .WithClasses("unity-object-field__selector");
            textInput.WithClasses(false, "unity-base-text-field__input", "unity-base-text-field__input--single-line");
            textElement.style.unityTextAlign = TextAnchor.MiddleCenter;

            Foldout.RegisterValueChangedCallback(OnFoldoutToggle);
            textElement.AddManipulator(new Clickable(() => OnShowPopup?.Invoke(this, TextField, ShowWindowMode.Popup)));
        }

        private void OnFoldoutToggle(ChangeEvent<bool> ev) =>
            SetFoldoutValue(ev.newValue);

        public void SetFoldoutValue(bool isOpen)
        {
            if (Foldout.value != isOpen)
                Foldout.value = isOpen;
            else
            {
                if (ContentFoldout != null) ContentFoldout.value = isOpen;
                Container.SetVisible(isOpen && Property.managedReferenceValue != null);
                Property.isExpanded = isOpen;
                ExpandablePropertyContent.IsExpanded = isOpen;
            }
            
        }

        public void BindPropertyToContent()
        {
            try
            {
                Content.BindProperty(Property);
            }
            catch (Exception e)
            {
                Debug.LogError("<color=#FF7F00FF>" + e.ToString() + "</color>");
            }
        }
    }

    [UxmlElement(uxmlName: "ui3x.TypeInstanceField")]
    [HideInInspector]
    public partial class TypeInstanceField : TypeField
    {
        public Foldout Foldout;
        public VisualElement Container => ExpandablePropertyContent.ContentContainer;
        public PropertyField Content => (PropertyField) ExpandablePropertyContent.Content;
        public IExpandablePropertyContent ExpandablePropertyContent;
        
        private const string UndoGroupName = "Selected Object Type Change";
        private bool m_ShowLabel;
        
        public TypeInstanceField() : this(true) {}
        
        public TypeInstanceField(bool showLabel, string initialLabel = "Type Field") : base(
            visualInput: new Foldout() { text = initialLabel, toggleOnLabelClick = true, value = false },
            initialLabel: null)
        {
            m_ShowLabel = showLabel;
        }
        
        protected override void OnAttachToPanel(AttachToPanelEvent evt)
        {
            Add(CreateSearchableGUI());
        }
        
        protected override VisualElement CreateSearchableGUI()
        {
            var headerContent = base.CreateSearchableGUI();
            if (VisualInput is Foldout)
                Foldout = (Foldout) VisualInput;
            else
                Foldout = new Foldout() { text = "Foldout", toggleOnLabelClick = true };
            Foldout.AddToClassList(BaseField<int>.alignedFieldUssClassName);
            FoldoutHeader.Add(headerContent);
            ApplyFoldoutStyles();
            
            return Foldout;
        }

        public override void Unbind()
        {
            CollectionProperty = null;
            PropertyIndex = -1;
            Property = null;
            Content?.Unbind();
        }

        public override void BindProperty(SerializedProperty property, int propertyIndex = -1)
        {
            CollectionProperty = propertyIndex >= 0 ? property : null;
            PropertyIndex = propertyIndex;
            Property = propertyIndex == -1 ? property : property.GetArrayElementAtIndex(propertyIndex);
            Foldout.text = Property.displayName;

            SetValue(GetTypeFromSerializedPropertyValue(Property));

            TypeUndoRedoController.Bind(Property, this);
        }

        protected override Type GetTypeFromSerializedPropertyValue(SerializedProperty property)
        {
            if (Property.managedReferenceValue == null) return null;
            return property.GetUnderlyingValue()?.GetType();
        }
        
        protected override void OnValueChange(IEnumerable<Type> newValues)
        {
            var enumerable = newValues.ToList();
            Type newValue = enumerable.Any() ? enumerable.First() : null;
            
            ExpandablePropertyContent.DecoratorsCache.ClearCache();
            ExpandablePropertyContent.DecoratorsCache.DisableAutoGroupingOnActiveSelection(disable: true);
            Property.serializedObject.Update();
            var wasExpanded = Property.isExpanded;

            TypeUndoRedoController.RecordObject(Property.serializedObject.targetObject, UndoGroupName);
            
            if (Property.managedReferenceValue != null && newValue == null)
                SetPropertyValue(null);
            else if (Property.managedReferenceValue == null && newValue != null)
                SetPropertyValue(null, newValue?.GetConstructor(Type.EmptyTypes)?.Invoke(null) ?? null);
            else
                SetPropertyValue(null, newValue?.GetConstructor(Type.EmptyTypes)?.Invoke(null) ?? null);

            SetValue(newValue);
            Property.isExpanded = false;
            Property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            TypeUndoRedoController.AddValue(Undo.GetCurrentGroup(), Property);
            Undo.FlushUndoRecordObjects();
            Undo.IncrementCurrentGroup();

            BindPropertyToContent();
            Content.MarkDirtyRepaint();
            
            Container.SetVisible(Property.managedReferenceValue != null);

            EditorApplication.delayCall += (EditorApplication.CallbackFunction) (() =>
            {
                if (wasExpanded)
                {
                    Property.isExpanded = true;
                    if (ContentFoldout != null) ContentFoldout.value = true;
                }
            });
        }

        protected override void SetPropertyValue(Type newValue, object newValueInstance = null) =>
            Property.managedReferenceValue = newValueInstance;

        public override void OnUndoRedoCallback(in UndoRedoInfo undo)
        {
            if (!undo.undoName.Equals(UndoGroupName))
                throw new Exception("Invalid call to TypeInstanceField.OnUndoRedoCallback({`" + undo.undoName + "`})");
            
            try
            {
                Property.serializedObject.Update();
                var t = GetTypeFromSerializedPropertyValue(Property);
                if (t != CurrentType) SetValue(t);
                BindPropertyToContent();
            }
            catch (Exception)
            {
                Debug.LogError("Ignored exception @ " + this);
            }
        }
    }
}
