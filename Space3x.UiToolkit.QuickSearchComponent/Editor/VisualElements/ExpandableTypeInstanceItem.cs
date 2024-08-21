using System;
using Space3x.InspectorAttributes.Editor;
using Space3x.InspectorAttributes.Editor.Drawers;
using Space3x.InspectorAttributes.Editor.Extensions;
using Space3x.InspectorAttributes.Editor.VisualElements;
using Space3x.UiToolkit.Types;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.QuickSearchComponent.Editor.VisualElements
{
    [UxmlElement(uxmlName: "ui3x.ExpandableTypeInstanceItem")]
    [HideInInspector]
    public partial class ExpandableTypeInstanceItem : BindableElement, IExpandablePropertyContent
    {
        // public IDrawer CollectionDrawer { get; set; }
        public VisualElement Content { get; set; }
        public VisualElement ContentContainer { get; set; }
        public bool IsExpanded { get; set; }
        public TypeInstanceField SelectorField { get; private set; }
        public IPropertyNode Property { get; protected set; }
        public IPropertyNode CollectionProperty { get; protected set; }
        public int PropertyIndex { get; protected set; }
        public VisualElement Container { get; set; }
        public Action<TypeField, VisualElement, ShowWindowMode> OnShowPopup;
        // public VisualElement VisualTarget => Field;
        // public PropertyField Field => m_Field ??= Container.GetClosestParentOfType<PropertyField, InspectorElement>();
        //
        // private PropertyField m_Field;

        // public MarkerDecoratorsCache DecoratorsCache => 
        //     m_DecoratorsCache ??= UngroupedMarkerDecorators.GetInstance(
        //         Field?.GetParentPropertyField()?.GetSerializedProperty()?.GetHashCode() 
        //         ?? Property.serializedObject.GetHashCode(),
        //         Property.serializedObject.GetHashCode());
        
        public MarkerDecoratorsCache DecoratorsCache => 
            m_DecoratorsCache ??= UngroupedMarkerDecorators.GetInstance(Property.GetSerializedObject(), ContentContainer.panel);
        
        // public MarkerDecoratorsCache DecoratorsCache => 
        //     m_DecoratorsCache ??= UngroupedMarkerDecorators.GetInstance(
        //         Property.serializedObject.GetHashCode(),
        //         Property.serializedObject.GetHashCode());

        private MarkerDecoratorsCache m_DecoratorsCache;


        public ExpandableTypeInstanceItem()
        {
            CreateItemContainerGUI();
        }
        
        public void BindProperty(IPropertyNode collectionProperty, int propertyIndex)
        {
            SelectorField?.Unbind();
            m_DecoratorsCache?.ClearCache();
            m_DecoratorsCache = null;
            CollectionProperty = collectionProperty;
            PropertyIndex = propertyIndex;
            Property = collectionProperty.GetArrayElementAtIndex(propertyIndex);
            IsExpanded = Property.IsExpanded();
            Debug.Log($"pIndex: {propertyIndex}, " +
                      $"isExpanded: {IsExpanded}, " +
                      $"PropertyPath: {Property.PropertyPath}, " +
                      $"CollectionPath: {CollectionProperty.PropertyPath}, " +
                      $"Collection.isExpanded: {CollectionProperty.IsExpanded()}");
            ((IExpandablePropertyContent) this).RebuildExpandablePropertyContentGUI(OnAttachContentToPanel);
            // Foldout.text = Property.displayName;
            // SetValue(GetTypeFromSerializedPropertyValue(Property));
            // TypeUndoRedoController.Bind(Property, this); // TODO
        }
        
        private void CreateItemContainerGUI()
        {
            Container = this;   //new BindableElement() { viewDataKey = "vdk-item-container" };
            // Context.WithExtension<TrackChangesOnEx, ITrackChangesOn, BindableElement>((BindableElement) Container, out var success);
            // if (!success) 
            //     OnUpdate();

            ContentContainer = new VisualElement() { name = "ContentContainer" };
            SelectorField = CreateSelectorFieldGUI();
            Container.Add(SelectorField);
            Container.Add(ContentContainer);
        }

        // viewDataKey = "vdk-items-ct-" + Property.serializedObject.targetObject.GetInstanceID() + "-" + Property.propertyPath
        public virtual VisualElement CreateContentGUI()
        {
            if (Property.HasSerializedProperty())
            {
                return new PropertyField(Property.GetSerializedProperty())
                {
                    viewDataKey =
                        $"vdk-{CollectionProperty.GetSerializedObject().targetObject.GetInstanceID()}-{CollectionProperty.PropertyPath}-ipf-{PropertyIndex}",
                }.WithClasses("ui3x-no-toggle");
            }
            else
            {
                return new BindablePropertyField(Property)
                {
                    viewDataKey =
                        $"vdk-{CollectionProperty.GetSerializedObject().targetObject.GetInstanceID()}-{CollectionProperty.PropertyPath}-ipf-{PropertyIndex}",
                }.WithClasses("ui3x-no-toggle");
            }
            // new PropertyField(Property) { viewDataKey = "vdk-item-content--" + PropertyIndex }.WithClasses("ui3x-no-toggle");
        }

        private TypeInstanceField CreateSelectorFieldGUI() => (TypeInstanceField)
            (new TypeInstanceField() { OnShowPopup = OnShowItemPopup, ExpandablePropertyContent = this })
                .WithClasses(BaseField<int>.alignedFieldUssClassName);
        
        
        
        private void OnAttachContentToPanel()
        {
            if (SelectorField is TypeInstanceField instanceField)
            {
                Property.SetExpanded(true);
                ContentContainer.SetVisible(true);
                // ContentContainer.SetVisible(false);
                Content.MarkDirtyRepaint();
                EditorApplication.delayCall += (EditorApplication.CallbackFunction) (() =>
                {
                    if (UngroupedMarkerDecorators.GetInstance((PropertyField) Content) is MarkerDecoratorsCache altDecoratorsCache)
                        altDecoratorsCache.DisableAutoGroupingOnActiveSelection(disable: true);
                    DecoratorsCache.DisableAutoGroupingOnActiveSelection(disable: true);
                    UngroupedMarkerDecorators.SetAutoDisableGroupingWhenCreatingCachesInGroup(Property.GetSerializedObject(), ContentContainer.panel, false);
                    Content.MarkDirtyRepaint();

                    EditorApplication.delayCall += (EditorApplication.CallbackFunction) (() =>
                    {
                        ContentContainer.SetVisible(true);
                        // ContentContainer.SetVisible(false);
                        BindPropertyToTypeField(SelectorField);

                        // instanceField.Foldout.value = IsExpanded;
                        instanceField.SetFoldoutValue(IsExpanded);
                        Content.MarkDirtyRepaint();
                    });
                });
                
            }
            else
                throw new NotImplementedException("Missing code path");
        }
        
        private void BindPropertyToTypeField(TypeInstanceField instanceField)
        {
            // var delayedContentHashCode = ((PropertyField) Content).GetSerializedProperty()?.GetHashCode() ?? Property.GetHashCode();
            // var altDecoratorsCache = UngroupedMarkerDecorators.GetInstance(delayedContentHashCode);
            if (Property.HasSerializedProperty() && Property.GetSerializedProperty() is SerializedProperty serializedProperty)
            {
                var altDecoratorsCache = UngroupedMarkerDecorators.GetInstance((PropertyField)Content, serializedProperty);
                altDecoratorsCache.DisableAutoGroupingOnActiveSelection(disable: true);
                instanceField.BindProperty((SerializedProperty)CollectionProperty.GetSerializedProperty(), PropertyIndex);
                instanceField.BindPropertyToContent();
                if (Content is PropertyField propertyField)
                {
                    Debug.Log(
                        $"// TODO: RebuildChildDecoratorDrawersIfNecessary is correctly dealing with collection items?");
                    propertyField.RebuildChildDecoratorDrawersIfNecessary(serializedProperty);
                    altDecoratorsCache.RebuildAll();
                    DecoratorsCache.RebuildAll();
                    altDecoratorsCache.DisableAutoGroupingOnActiveSelection(disable: false);
                    DecoratorsCache.DisableAutoGroupingOnActiveSelection(disable: false);
                    altDecoratorsCache.HandlePendingDecorators();
                    DecoratorsCache.HandlePendingDecorators();
                }
            }
            else
            {
                Debug.LogException(new NotImplementedException("QuickSearch is not fully implemented for non-serialized properties yet."));
            }
        }

        public void OnShowItemPopup(TypeField target, VisualElement selectorField, ShowWindowMode mode)
        {
            Debug.Log($"OnShowItemPopup!");
            OnShowPopup.Invoke(target, selectorField, mode);
        }
    }
}
