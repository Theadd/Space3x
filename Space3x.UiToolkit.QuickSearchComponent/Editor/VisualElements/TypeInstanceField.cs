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
//        public Foldout ContentFoldout;
        public Foldout ContentFoldout => Content.Children().FirstOrDefault() as Foldout;
//        public Toggle ContentFoldoutToggle;

        private bool m_IgnoreValueChangesInFoldout;

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
            Debug.Log("OnFoldoutToggle: " + ev.newValue);
            if (ContentFoldout != null) ContentFoldout.value = ev.newValue;
            Container.SetVisible(ev.newValue && Property.managedReferenceValue != null);
            Property.isExpanded = ev.newValue;
//
//            if (ContentFoldout != null && !m_IgnoreValueChangesInFoldout)
//                ContentFoldout.value = ev.newValue;
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
        
        [Obsolete("Use BindPropertyToContent instead.")]
        protected virtual void SetPropertyContent(bool unbind = false)
        {
//            BindContentFoldout();
            try
            {
                Content.BindProperty(Property);
            }
            catch (Exception e)
            {
                Debug.LogError("<color=#FF7F00FF>" + e.ToString() + "</color>");
            }
//            BindContentFoldout();   // TODO: remove
            /* HI THERE */
            
//            try
//            {
//                Debug.LogWarning($"[TypeInstanceField] PropertyField (Content) Children: {Content.hierarchy.childCount}; " 
//                                 + $"{(Content.hierarchy.childCount == 1 ? Content.hierarchy[0].ToString() : "None")}");
//            }
//            catch (Exception e)
//            {
//                Debug.LogError(e.ToString());
//            }
//            try
//            {
//                if (Content.hierarchy.ElementAt(0)?.hierarchy.ElementAt(0) is Toggle toggleElement)
//                    toggleElement.SetVisible(false);
//            }
//            catch (Exception e)
//            {
//                Debug.LogError(e.ToString());
//            }
//
//            var allBindingInfos = Content.GetBindingInfos().ToList();
//            try
//            {
//                Debug.Log("HERE !! =================> " + allBindingInfos.Count);
////                if (unbind)
////                {
////                    Content.Unbind();
////                    Content.BindProperty(Property);
////                }
////                else
//                {
//                    Content.BindProperty(Property);
//                }
////                if (Content?.hierarchy.childCount != 0) // MOD
////                    Content.Unbind();   // MOD
////                
////                if (Content?.hierarchy.childCount == 0) // MOD
////                    Content.BindProperty(Property);
//                // TODO: FIXME: REMOVE: ActiveEditorTracker.sharedTracker.ForceRebuild();
//            }
//            catch (Exception e)
//            {
//                Debug.LogError(e.ToString());
//            }
//
//            try
//            {
//                var (contentFoldout, _) = Content.AsChildren();
//                ContentFoldout = contentFoldout as Foldout;
//                if (ContentFoldout == null) 
//                    Debug.LogError("<color=#ff0000ff>[TypeInstanceField] ContentFoldout IS NULL!!</color>");
//                ContentFoldoutToggle = ContentFoldout.hierarchy.ElementAt(0) as Toggle;
//                ContentFoldoutToggle.SetVisible(false);
//                if (ContentFoldout.value != Foldout.value) Foldout.value = ContentFoldout.value;
//            }
//            catch (Exception e)
//            {
//                Debug.LogError(e.ToString());
//            }
        }

//        private void BindContentFoldout()
//        {
//            Debug.Log(" . . . . . BindContentFoldout . . . . . 0");
//            ContentFoldout = null;
//            ContentFoldoutToggle = null;
//            Content.UnregisterCallback<GeometryChangedEvent>(OnContentGeometryChanged);
//            if (Content.hierarchy.childCount > 0)
//            {
//                Debug.Log(" . . . . . BindContentFoldout . . . . . 1");
//                var (contentFoldout, _) = Content.AsChildren();
//                ContentFoldout = contentFoldout as Foldout;
//                if (ContentFoldout != null)
//                {
//                    Debug.Log(" . . . . . BindContentFoldout . . . . . 2");
//                    if (ContentFoldout.hierarchy.childCount > 0)
//                        ContentFoldoutToggle = ContentFoldout.hierarchy.ElementAt(0) as Toggle;
//                    ContentFoldoutToggle?.SetVisible(false);
//                    if (ContentFoldout.value != Foldout.value)
//                    {
//                        // m_IgnoreValueChangesInFoldout = true;
//                        Foldout.value = ContentFoldout.value;
//                        // m_IgnoreValueChangesInFoldout = false;
//                    }
//                }
//            }
//            if (ContentFoldout == null)
//            {
//                Debug.Log(" . . . . . BindContentFoldout . . . . . 3");
//                Content.RegisterCallback<GeometryChangedEvent>(OnContentGeometryChanged);
//            }
//        }
//
//        private void OnContentGeometryChanged(GeometryChangedEvent evt)
//        {
//            Debug.Log(" . . . . . BindContentFoldout . . . . . CALLBACK");
//            BindContentFoldout();
//        }
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
//            UngroupedMarkerDecorators.DisableAutoGroupingOnActiveSelection(disable: true);
            CollectionProperty = propertyIndex >= 0 ? property : null;
            PropertyIndex = propertyIndex;
            Property = propertyIndex == -1 ? property : property.GetArrayElementAtIndex(propertyIndex);
            Foldout.text = Property.displayName;
//            ExpandablePropertyContent.IsExpanded = Property.isExpanded;
//            Debug.Log($"<b>BINDING... wasExpanded: {ExpandablePropertyContent.IsExpanded}</b>");
//            Container.SetVisible(false);
//            Property.isExpanded = true;
            // ((IBindable) this).BindProperty(Property);
            SetValue(GetTypeFromSerializedPropertyValue(Property));

//            var allBindingInfos = Content.GetBindingInfos().ToList();
//            Debug.Log("<b>allBindingInfos.Count: " + allBindingInfos.Count + " !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!</b>");
            
//            Content.Unbind(); // MOD
    //            if (Property.managedReferenceValue != null)     // TODO: Always bind even when no value.
    //                SetPropertyContent(unbind: true);
//            else
//                Content.Unbind();
//            allBindingInfos = Content.GetBindingInfos().ToList();
//            Debug.Log("<b>(SECOND) allBindingInfos.Count: " + allBindingInfos.Count + " !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!</b>");
            // MOD
//            UngroupedMarkerDecorators.TryRebuildAndLinkAll(); // MOD_ME
//            UngroupedMarkerDecorators.TryRebuildAll();
//            UngroupedMarkerDecorators.DisableAutoGroupingOnActiveSelection(disable: false);
//            UngroupedMarkerDecorators.TryRebuildAndLinkAll();
            
//            Container.SetVisible(Property.managedReferenceValue != null);
//            if (wasExpanded)
//            {
//                EditorApplication.delayCall += (EditorApplication.CallbackFunction) (() =>
//                {
//                    Property.isExpanded = true;
//                    ContentFoldout.value = true;
//                });
//            }

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

            Debug.Log(".. .. .. REBUILD on CHANGE #0");
            ExpandablePropertyContent.DecoratorsCache.ClearCache();
            ExpandablePropertyContent.DecoratorsCache.DisableAutoGroupingOnActiveSelection(disable: true);
            Property.serializedObject.Update();
            var wasExpanded = Property.isExpanded;
//            Debug.Log(".. .. .. BEGIN REBUILD");
//            ExpandablePropertyContent.RebuildExpandablePropertyContentGUI();
//            Debug.Log(".. .. .. END REBUILD");
            
            TypeUndoRedoController.RecordObject(Property.serializedObject.targetObject, UndoGroupName);

            if (PropertyIndex == -1)
            {
                // BEGIN EDIT
                if (Property.managedReferenceValue != null && newValue == null)
                {
                    // MODX: Content.Unbind();
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
                    // MODX: Content.Unbind();
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
            Property.isExpanded = false;
            Property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            TypeUndoRedoController.AddValue(Undo.GetCurrentGroup(), Property);
            Undo.FlushUndoRecordObjects();
            Undo.IncrementCurrentGroup();

            // BEGIN EDIT
            Debug.Log(".. .. .. REBUILD on CHANGE #A");
//            Property.isExpanded = false;
//            Property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
//            Property.isExpanded = true;
            Debug.Log(".. .. .. REBUILD on CHANGE #A1");
            SetPropertyContent();
            // MODX: Content.Unbind();
            Content.MarkDirtyRepaint();
            Debug.Log(".. .. .. REBUILD on CHANGE #A2");

//            if (newValue != null)
//                Property.isExpanded = true;
            Container.SetVisible(Property.managedReferenceValue != null);
//            Property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            
            Debug.Log(".. .. .. REBUILD on CHANGE #A3 sss");
            
            EditorApplication.delayCall += (EditorApplication.CallbackFunction) (() =>
            {
                if (wasExpanded)
                {
                    Property.isExpanded = true;
                    if (ContentFoldout != null) ContentFoldout.value = true;
                }
                /*Debug.Log(".. .. .. REBUILD on CHANGE #B");
                // MODX: Content.Unbind();
                SetPropertyContent();   // Content.BindProperty(Property);
                Content.MarkDirtyRepaint();
                Debug.Log(".. .. .. REBUILD on CHANGE #B2");
                
                RebuildChildDecoratorDrawersIfNecessary(Content, Property);
                
                ExpandablePropertyContent.DecoratorsCache.TryRebuildAll();  // TODO: remove redundant call
//                UngroupedMarkerDecorators.TryRebuildAndLinkAll();
                ExpandablePropertyContent.DecoratorsCache.DisableAutoGroupingOnActiveSelection(disable: false);
                Debug.Log(".. .. .. REBUILD on CHANGE #B3");
                
                
//                EditorApplication.delayCall += (EditorApplication.CallbackFunction) (() =>
//                {
//                    Debug.Log(".. .. .. REBUILD on CHANGE #C");
//                    Content.Unbind();
//                    SetPropertyContent();   // Content.BindProperty(Property);
//                    Content.MarkDirtyRepaint();
//                    Debug.Log(".. .. .. REBUILD on CHANGE #C2");
//                    
//                });*/
            });

            // END EDIT


            /*
            Debug.Log(".. .. .. REBUILD on CHANGE #1");
            SetPropertyContent();
            Debug.Log(".. .. .. REBUILD on CHANGE #2");
//            ExpandablePropertyContent.CallResetForContentAsPropertyField((SerializedProperty) null);
//            ExpandablePropertyContent.CallResetForContentAsPropertyField(Property.Copy());
            Content.Unbind();
            Debug.Log(".. .. .. REBUILD on CHANGE 3");
            Content.MarkDirtyRepaint();
            Debug.Log(".. .. .. REBUILD on CHANGE 4");
//            EditorApplication.delayCall += (EditorApplication.CallbackFunction) (() =>
//            {
                Debug.Log(".. .. .. REBUILD on CHANGE #5");
//                UngroupedMarkerDecorators.ClearCache();
                ExpandablePropertyContent.CallResetForContentAsPropertyField(Property);
                Debug.Log(".. .. .. REBUILD on CHANGE #6");
                Content.MarkDirtyRepaint();
                Debug.Log(".. .. .. REBUILD on CHANGE #7");
//            });
            Debug.Log(".. .. .. REBUILD on CHANGE #8");

            if (newValue != null)
                Property.isExpanded = true;

//            Debug.Log(".. .. .. Delayed REBUILD CALLED");
            EditorApplication.delayCall += (EditorApplication.CallbackFunction) (() =>
            {
                Debug.Log(".. .. .. Delayed REBUILD #0");
//                UngroupedMarkerDecorators.ClearCache();
                Content.Unbind();
                Debug.Log(".. .. .. Delayed REBUILD #1");
                Content.BindProperty(Property);
                Debug.Log(".. .. .. Delayed REBUILD #1.2???");
                ExpandablePropertyContent.CallResetForContentAsPropertyField(Property);
//                SetPropertyContent();
                Debug.Log(".. .. .. Delayed REBUILD #2");
                Content.MarkDirtyRepaint();
            });
            Debug.Log(".. .. .. Delayed REBUILD CALLED");
            */

//            Debug.Log(".. .. .. BEGIN REBUILD");
////            ExpandablePropertyContent.RebuildExpandablePropertyContentGUI(() => SetPropertyContent());
//            ExpandablePropertyContent.ReloadPropertyContentGUI(() =>
//            {
//                SetPropertyContent();
//                Content.MarkDirtyRepaint();
//                Debug.Log(".. .. .. END REBUILD --- INNER");
//            });
//            Debug.Log(".. .. .. END REBUILD");

//            ((IDrawer) ExpandablePropertyContent).ForceRebuild();
//            ExpandablePropertyContent.CallResetForContentAsPropertyField(Property);

//            ExpandablePropertyContent.ExecuteDelayedUpdate();
//            SetPropertyContent();
//            UngroupedMarkerDecorators.TryRebuildAll();  // TODO: remove redundant call
//            UngroupedMarkerDecorators.TryRebuildAndLinkAll();
//            UngroupedMarkerDecorators.DisableAutoGroupingOnActiveSelection(disable: false);
        }

        private void RebuildChildDecoratorDrawersIfNecessary(PropertyField parentField, SerializedProperty parentProperty)
        {
            var property = parentProperty.Copy();
            var parentDepth = property.depth;
            var visitedNodes = new HashSet<long>();
            Debug.Log($"!! Rebuilding decorators !! hasChildren: {property.hasChildren}, " +
                      $"isExpanded: {property.isExpanded}, hasVisibleChildren: {property.hasVisibleChildren}, " +
                      $"depth: {property.depth}");
            
            SerializedProperty endProperty = property.GetEndProperty();
            var allChildFields = parentField.GetChildren<PropertyField>();
            var childFieldsByPath = new Dictionary<string, PropertyField>();
            foreach (var childField in allChildFields)
            {
                var childProperty = childField.GetSerializedProperty();
                if (childProperty != null)
                    childFieldsByPath.Add(childProperty.propertyPath, childField);
            }
            bool visitChild;
            do
            {
                // default is false so we don't enumerate each character of each string,
                visitChild = false;
                
                if (property.propertyType == SerializedPropertyType.ManagedReference)
                {
                    long refId = property.managedReferenceId;
                    if (visitedNodes.Add(refId))
                        visitChild = true; // First time seeing node, so visit it
                }
                
                var childProperty = property.Copy();
                if (childFieldsByPath.TryGetValue(childProperty.propertyPath, out var childField))
                {
                    Debug.Log($"Found child: {childProperty.propertyPath}");
                    var prevNestingLevel = childField.GetDrawNestingLevel();
                    childField.SetDrawNestingLevel(0);
                    childProperty.AssignToPropertyField(childField);
                    childField.SetDrawNestingLevel(prevNestingLevel);
                }
                else
                {
                    Debug.Log($"Not found: {childProperty.propertyPath}");
                }
                // TODO: NextVisible => Next
            } while (property.NextVisible(visitChild) && !SerializedProperty.EqualContents(property, endProperty));
            endProperty = (SerializedProperty) null;
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
