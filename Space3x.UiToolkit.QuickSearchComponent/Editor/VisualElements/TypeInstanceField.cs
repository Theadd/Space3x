using System;
using System.Collections.Generic;
using System.Linq;
using Space3x.InspectorAttributes.Editor.Drawers;
using Space3x.InspectorAttributes.Editor.Extensions;
using Space3x.InspectorAttributes.Utilities;
using Space3x.UiToolkit.QuickSearchComponent.Editor.VisualElements;
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
        public Foldout ContentFoldout;
        public Toggle ContentFoldoutToggle;

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
            // ((TextField)textField)
            
            var (textInput, button, _) = textField.AsChildren();
            var (textElement, _) = textInput.AsChildren();

            button.WithClasses(false, "unity-text-element", "unity-button")
                .WithClasses("unity-object-field__selector");
            textInput.WithClasses(false, "unity-base-text-field__input", "unity-base-text-field__input--single-line");
            textElement.style.unityTextAlign = TextAnchor.MiddleCenter;

            Foldout.RegisterValueChangedCallback(OnFoldoutToggle);
            textElement.AddManipulator(new Clickable(() => OnShowPopup?.Invoke(this, TextField, ShowWindowMode.Popup)));
        }

        private void OnFoldoutToggle(ChangeEvent<bool> ev)
        {
            if (ContentFoldout != null)
                ContentFoldout.value = ev.newValue;
        }

        protected virtual void SetPropertyContent(bool unbind = false)
        {
            try
            {
                Debug.LogWarning($"[TypeInstanceField] PropertyField (Content) Children: {Content.hierarchy.childCount}; " 
                                 + $"{(Content.hierarchy.childCount == 1 ? Content.hierarchy[0].ToString() : "None")}");
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
            try
            {
                if (Content.hierarchy.ElementAt(0)?.hierarchy.ElementAt(0) is Toggle toggleElement)
                    toggleElement.SetVisible(false);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }

            var allBindingInfos = Content.GetBindingInfos().ToList();
            try
            {
                Debug.Log("HERE !! =================> " + allBindingInfos.Count);
//                if (unbind)
//                {
//                    Content.Unbind();
//                    Content.BindProperty(Property);
//                }
//                else
                {
                    Content.BindProperty(Property);
                }
//                if (Content?.hierarchy.childCount != 0) // MOD
//                    Content.Unbind();   // MOD
//                
//                if (Content?.hierarchy.childCount == 0) // MOD
//                    Content.BindProperty(Property);
                // TODO: FIXME: REMOVE: ActiveEditorTracker.sharedTracker.ForceRebuild();
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }

            try
            {
                var (contentFoldout, _) = Content.AsChildren();
                ContentFoldout = contentFoldout as Foldout;
                if (ContentFoldout != null && ContentFoldout.hierarchy.childCount > 0)
                {
                    ContentFoldoutToggle = ContentFoldout.hierarchy.ElementAt(0) as Toggle;
                    if (ContentFoldoutToggle != null)
                    {
                        ContentFoldoutToggle.SetVisible(false);
                        if (ContentFoldout.value != Foldout.value) Foldout.value = ContentFoldout.value;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }
    }
    
    
    
    [UxmlElement(uxmlName: "ui3x.TypeInstanceField")]
    [HideInInspector]
    public partial class TypeInstanceField : TypeField
    {
        public Foldout Foldout;
        public VisualElement Container;
        public PropertyField Content;
        private const string UndoGroupName = "Selected Object Type Change";

        private bool m_ShowLabel;

        public TypeInstanceField() : this(true) {}
        
        public TypeInstanceField(bool showLabel, string initialLabel = "Type Field") : base(
            visualInput: new Foldout() { text = initialLabel, toggleOnLabelClick = true, value = false },
            initialLabel: null)
        {
            // Container = new VisualElement() { };     // EDIT
            m_ShowLabel = showLabel;
        }
        
        protected override void OnAttachToPanel(AttachToPanelEvent evt)
        {
//            Debug.LogWarning($"[TypeInstanceField.OnAttachToPanel] PropertyField (Content) Children: {Content.hierarchy.childCount}; " +
//                             $"{(Content.hierarchy.childCount == 1 ? Content.hierarchy[0].ToString() : "None")}");
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
//            var toggle = Foldout.hierarchy.ElementAt(0);
//            var foldoutHeaderContainer = toggle.hierarchy.ElementAt(0);
//            foldoutHeaderContainer.Add(headerContent);
            // Foldout.Add(Container);  // EDIT
            return Foldout;
        }

        public override void Unbind()
        {
//            base.Unbind();
            CollectionProperty = null;
            PropertyIndex = -1;
            Property = null;
            // Container.Clear();   // EDIT
            Content.Unbind();   // EDIT
        }

        public override void BindProperty(SerializedProperty property, int propertyIndex = -1)
        {
            Debug.Log("<b>BINDING... 23</b>");
//            UngroupedMarkerDecorators.DisableAutoGroupingOnActiveSelection(disable: true);
            CollectionProperty = propertyIndex >= 0 ? property : null;
            PropertyIndex = propertyIndex;
            Property = propertyIndex == -1 ? property : property.GetArrayElementAtIndex(propertyIndex);
            Foldout.text = Property.displayName;
            // ((IBindable) this).BindProperty(Property);
            SetValue(GetTypeFromSerializedPropertyValue(Property));

            var allBindingInfos = Content.GetBindingInfos().ToList();
            Debug.Log("<b>allBindingInfos.Count: " + allBindingInfos.Count + " !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!</b>");
            
            Content.Unbind(); // MOD
            if (Property.managedReferenceValue != null) 
                SetPropertyContent(unbind: true);
//            else
//                Content.Unbind();
            allBindingInfos = Content.GetBindingInfos().ToList();
            Debug.Log("<b>(SECOND) allBindingInfos.Count: " + allBindingInfos.Count + " !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!</b>");
            // MOD
//            UngroupedMarkerDecorators.TryRebuildAndLinkAll(); // MOD_ME
//            UngroupedMarkerDecorators.TryRebuildAll();
//            UngroupedMarkerDecorators.DisableAutoGroupingOnActiveSelection(disable: false);
//            UngroupedMarkerDecorators.TryRebuildAndLinkAll();
            
            Container.SetVisible(Property.managedReferenceValue != null);
            TypeUndoRedoController.Bind(Property, this);
        }
        
//        public virtual void BindProperty(SerializedProperty property, int propertyIndex = -1)
//        {
//            CollectionProperty = propertyIndex >= 0 ? property : null;
//            PropertyIndex = propertyIndex;
//            Property = propertyIndex == -1 ? property : property.GetArrayElementAtIndex(propertyIndex);
//            label = m_ShowLabel ? Property?.displayName ?? (string.IsNullOrEmpty(label) ? "Type Field" : label) : null;
//            SetValue(GetTypeFromSerializedPropertyValue(Property));
//            ((VisualElement) this).Unbind();
//            ((IBindable) this).BindProperty(Property);
//            TypeUndoRedoController.Bind(Property, this);
////            UpdateLabel();
//            MarkDirtyRepaint();
//        }
        
        protected override Type GetTypeFromSerializedPropertyValue(SerializedProperty property)
        {
            if (Property.managedReferenceValue == null) return null;
            Type t = property.GetUnderlyingValue()?.GetType();
            // Debug.Log("!GetTypeFromSerializedPropertyValue: " + t?.FullTypeName());
            return t;
        }
        
        protected override void OnValueChange(IEnumerable<Type> newValues)
        {
            var enumerable = newValues.ToList();
            Type newValue = enumerable.Any() ? enumerable.First() : null;

            UngroupedMarkerDecorators.ClearCache();
            UngroupedMarkerDecorators.DisableAutoGroupingOnActiveSelection(disable: true);
            Property.serializedObject.Update();
            TypeUndoRedoController.RecordObject(Property.serializedObject.targetObject, UndoGroupName);

            if (PropertyIndex == -1)
            {
                // BEGIN EDIT
                if (Property.managedReferenceValue != null && newValue == null)
                {
                    Content.Unbind();
                    SetPropertyValue(null);
                    // Property.managedReferenceValue = null;
//                    Property.serializedObject.ApplyModifiedProperties();
                }
                else if (Property.managedReferenceValue == null && newValue != null)
                {
                    SetPropertyValue(null, newValue?.GetConstructor(Type.EmptyTypes)?.Invoke(null) ?? null);
                    // Property.managedReferenceValue = newValue?.GetConstructor(Type.EmptyTypes)?.Invoke(null) ?? null;
//                    Property.serializedObject.ApplyModifiedProperties();
//                    SetPropertyContent();
                }
                else
                // END EDIT
                {
                    Debug.Log($"OnValueChange: {newValue?.FullTypeName()}");
                    Content.Unbind();
                    SetPropertyValue(null, newValue?.GetConstructor(Type.EmptyTypes)?.Invoke(null) ?? null);
                    // Property.managedReferenceValue = newValue?.GetConstructor(Type.EmptyTypes)?.Invoke(null) ?? null;
//                    Property.serializedObject.ApplyModifiedProperties();
//                    SetPropertyContent();
                }
            }
            else
                throw new NotImplementedException("TODO: HERE!");
                // CollectionProperty.GetArrayElementAtIndex(PropertyIndex).boxedValue = newValue?.GetConstructor(Type.EmptyTypes)?.Invoke(null);
            SetValue(newValue);
            Property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            TypeUndoRedoController.AddValue(Undo.GetCurrentGroup(), Property);
            Undo.FlushUndoRecordObjects();
            Undo.IncrementCurrentGroup();
            SetPropertyContent();
            UngroupedMarkerDecorators.TryRebuildAll();
            UngroupedMarkerDecorators.TryRebuildAndLinkAll();
            UngroupedMarkerDecorators.DisableAutoGroupingOnActiveSelection(disable: false);
        }
        
        protected override void SetPropertyValue(Type newValue, object newValueInstance = null)
        {
            if (PropertyIndex == -1)
                Property.managedReferenceValue = newValueInstance;
            else
                throw new NotImplementedException("TODO: HERE!");
        }
        
        public override void OnUndoRedoCallback(in UndoRedoInfo undo)
        {
            if (!undo.undoName.Equals(UndoGroupName))
                throw new Exception("Invalid call to TypeInstanceField.OnUndoRedoCallback({`" + undo.undoName + "`})");
            
            try
            {
                Property.serializedObject.Update();
                Debug.Log($"[TypeInstanceField] OnUndoRedoCallback: {PropertyIndex} {Property.GetUnderlyingValue()}");
                // Content.Unbind();
                var t = GetTypeFromSerializedPropertyValue(Property);
                if (t != CurrentType) SetValue(t);
                SetPropertyContent();
            }
            catch (Exception)
            {
                Debug.LogError("Ignored exception @ " + this);
            }
        }
        
//        protected override void OnValueChange(IEnumerable<Type> newValues)
//        {
//            var enumerable = newValues.ToList();
//            Type newValue = enumerable.Any() ? enumerable.First() : null;
//
//            if (PropertyIndex == -1)
//            {
//                // BEGIN EDIT
//                if (Property.managedReferenceValue != null && newValue == null)
//                {
//                    Content.Unbind();
//                    Property.managedReferenceValue = null;
//                    Property.serializedObject.ApplyModifiedProperties();
//                }
//                else if (Property.managedReferenceValue == null && newValue != null)
//                {
//                    Property.managedReferenceValue = newValue?.GetConstructor(Type.EmptyTypes)?.Invoke(null) ?? null;
//                    Property.serializedObject.ApplyModifiedProperties();
//                    SetPropertyContent();
//                }
//                else
//                // END EDIT
//                {
//                    Debug.Log($"OnValueChange: {newValue?.FullTypeName()}");
//                    Content.Unbind();
//                    Property.managedReferenceValue = newValue?.GetConstructor(Type.EmptyTypes)?.Invoke(null) ?? null;
//                    Property.serializedObject.ApplyModifiedProperties();
//                    SetPropertyContent();
//                }
//            }
//            else
//                throw new NotImplementedException("TODO: HERE!");
//                // CollectionProperty.GetArrayElementAtIndex(PropertyIndex).boxedValue = newValue?.GetConstructor(Type.EmptyTypes)?.Invoke(null);
//            SetValue(newValue);
//        }
    }
}
