using System;
using System.Collections.Generic;
using System.Linq;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor;
using Space3x.InspectorAttributes.Editor.Drawers;
using Space3x.InspectorAttributes.Editor.Extensions;
using Space3x.UiToolkit.QuickSearchComponent.Editor.Extensions;
using Space3x.UiToolkit.QuickSearchComponent.Editor.VisualElements;
using Space3x.UiToolkit.Types;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.VisualScripting;

namespace Space3x.UiToolkit.QuickSearchComponent.Editor.Drawers
{
    // [CanEditMultipleObjects]
    public abstract class BaseSearchableTypeDrawer<TAttribute> : 
            Drawer<TAttribute>, 
            IAttributeExtensionContext<TAttribute>,
            IExpandablePropertyContent
        where TAttribute : BaseSearchableTypeAttribute
    {
        private int m_SelectedIndex;
        private bool m_IsTypeValue;
        private Type m_ElementType;
        
        protected QuickSearchPopup Popup { get; set; }
        protected QuickSearchElement PopupContent { get; set; }
        protected VisualElement SelectorField { get; private set; }
        public VisualElement Content { get; set; }
        public VisualElement ContentContainer { get; set; }
        public bool IsExpanded { get; set; }
        protected ListView ListViewElement { get; set; }
        protected virtual List<Type> GetAllTypes() => Target.GetAllTypes();
        protected virtual void OnReload() => Target.ReloadCache();

        protected override VisualElement OnCreatePropertyGUI(IProperty property)
        {
            if (!(property.GetSerializedProperty() is SerializedProperty serializedProperty))
                return null;
            m_ElementType = GetUnderlyingElementType(serializedProperty);
            m_IsTypeValue = IsTypeValue(m_ElementType);
            Validate();
            if (attribute.applyToCollection)
            {
                if (!Property.IsArray()) 
                    Debug.LogError($"Collection property {Property.Name} is not an array", serializedProperty.serializedObject.targetObject);
                return CreatePropertyCollectionGUI(serializedProperty);
            }

            UngroupedMarkerDecorators.SetAutoDisableGroupingWhenCreatingCachesInGroup(Property.GetSerializedObject().GetHashCode(), true);
            IsExpanded = Property.IsExpanded();
            Debug.Log($"isExpanded: {IsExpanded}, " +
                      $"PropertyPath: {Property.PropertyPath}, " +
                      $"PropertyHash: {Property.GetHashCode()}, " +
                      $"SerializedObjectHash: {Property.GetSerializedObject().GetHashCode()}");
            if (!m_IsTypeValue)
                Property.SetExpanded(false);

            return CreateContainerGUI();
        }

        public virtual VisualElement CreatePropertyCollectionGUI(SerializedProperty property)
        {
            ListViewElement = CreateListView();
            ListViewElement.BindProperty(property);
            ListViewElement.headerTitle = property.displayName;

            return ListViewElement;
        }
        
        protected virtual ListView CreateListView()
        {
            // TODO
            var serializedProperty = Property.GetSerializedProperty();
            var listView = new ListView
            {
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                showFoldoutHeader = true,
                reorderable = true,
                showBorder = true,
                showAddRemoveFooter = true,
                showBoundCollectionSize = true,
                showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
                allowAdd = true,
                allowRemove = true,
                reorderMode = ListViewReorderMode.Animated,
                headerTitle = "Elements",
                makeItem = () =>
                {
                    return m_IsTypeValue
                        ? new TypeField(showLabel: false) { OnShowPopup = OnShowPopup }
                        : new ExpandableTypeInstanceItem() { OnShowPopup = OnShowPopup };
                },
                bindItem = (element, i) => {

                    if (element is ExpandableTypeInstanceItem instanceItem)
                    {
                        instanceItem.BindProperty(serializedProperty, i);
                    }
                    else if (element is TypeField field)
                    {
                        field.Unbind();
                        field.BindProperty(serializedProperty, i);
                    }
                },
                viewDataKey = $"vdk-{Property.GetSerializedObject().targetObject.GetInstanceID()}-{Property.PropertyPath}-lv"
            };
            return listView;
        }

        private void RepaintContainerGUI()
        {
            Container.Clear();
            m_SelectorFieldAttached = false;
            m_ContentAttached = false;
            
            ((IExpandablePropertyContent) this).RebuildExpandablePropertyContentGUI(OnAttachContentToPanel);

            SelectorField = AddSelectorFieldEventListeners(
                ApplySelectorFieldVisuals(
                    CreateSelectorFieldGUI()));
            
            Container.Add(SelectorField);
            Container.Add(ContentContainer);
        }
        
        protected virtual void Validate()
        {
            // TODO
            if (!m_IsTypeValue && (Property.GetSerializedProperty().propertyType != SerializedPropertyType.ManagedReference && !attribute.applyToCollection))
                throw new Exception(nameof(TAttribute) 
                                    + " can be used only with a ManagedReference. Add a [SerializedReference] attribute"
                                    + " to the property you want to use this attribute on.");
        }

        private static bool IsTypeValue(Type type)
        {
            if (type == null)
                return false;
            
            foreach (var ct in type.GetConstructors())
            {
                if (ct.GetParameters().Length != 1) continue;
                if (ct.GetParameters().Any(param => param.ParameterType == typeof(Type)))
                    return true;
            }

            return false;
        }
        
        private static Type GetUnderlyingElementType(SerializedProperty property)
        {
            var type = property.GetUnderlyingType();
            if (property.isArray) type = type.GetElementType() ?? type.GetGenericArguments().FirstOrDefault();
            return type;
        }
        
        public IAttributeExtensionContext<TAttribute> Context => this;
        
        protected virtual VisualElement CreateContainerGUI()
        {
            Container = new BindableElement() { viewDataKey = "vdk-ct-" + Property.GetSerializedObject().targetObject.GetInstanceID() + "-" + Property.PropertyPath };
            Context.WithExtension<TrackChangesOnEx, ITrackChangesOn, BindableElement>((BindableElement) Container, out var success);
            if (!success) 
                OnUpdate();

            return Container;
        }

        public virtual VisualElement CreateContentGUI() => m_IsTypeValue 
            ? new VisualElement() 
            : (new PropertyField(Property.GetSerializedProperty()) { viewDataKey = "vdk-content" }).WithClasses("ui3x-no-toggle");

        protected virtual VisualElement CreateSelectorFieldGUI() =>
            m_IsTypeValue
                ? new TypeField(showLabel: true, initialLabel: Property.DisplayName()) { OnShowPopup = OnShowPopup }
                : new TypeInstanceField() { OnShowPopup = OnShowPopup, ExpandablePropertyContent = this };

        public void OnShowPopup(TypeField target, VisualElement selectorField, ShowWindowMode mode)
        {
            Debug.Log($"OnShowPopup!");
            PopupContent ??= new QuickSearchElement() { };
//            var swatch = new System.Diagnostics.Stopwatch();
//            swatch.Start();
            PopupContent.DataSource = GetAllTypes().ToArray();
//            swatch.Stop();
//            Debug.Log($"OnShowPopup (PopupContent.DataSource = GetAllTypes().ToArray()): {swatch.ElapsedMilliseconds}ms");
//            var sType = GetSelectedType();
//            if (sType == null)
//                PopupContent.SetValueWithoutNotify(new List<Type>() {});
//            else
//                PopupContent.SetValueWithoutNotify(new List<Type>() { sType });
            Popup ??= (new QuickSearchPopup() { }).WithContent(PopupContent);
//            Popup.Show(SelectorField);
            Popup.WithSearchable(target).Show(selectorField, mode);
        }

        protected virtual VisualElement ApplySelectorFieldVisuals(VisualElement selectorField)
        {
            selectorField.EnableInClassList(BaseField<int>.alignedFieldUssClassName, true);
            return selectorField;
        }
        
        protected virtual VisualElement AddSelectorFieldEventListeners(VisualElement selectorField)
        {
            selectorField.RegisterCallbackOnce<AttachToPanelEvent>(OnAttachSelectorFieldToPanel);
            return selectorField;
        }

        private bool m_SelectorFieldAttached = false;
        private bool m_ContentAttached = false;
        
        private void OnAttachSelectorFieldToPanel(AttachToPanelEvent evt)
        {
            m_SelectorFieldAttached = true;
            if (m_SelectorFieldAttached && m_ContentAttached)
                OnContainerFullyAttached();
        }

        private void OnAttachContentToPanel()
        {
            m_ContentAttached = true;
            if (m_SelectorFieldAttached && m_ContentAttached)
                OnContainerFullyAttached();
        }

        private int m_AttachedContentHashCode;
        private int m_DelayedContentHashCode;

        private void OnContainerFullyAttached()
        {
            if (SelectorField is TypeInstanceField instanceField)
            {
                Property.SetExpanded(true);
                // ContentContainer.SetVisible(true);
                ContentContainer.SetVisible(false);
                Content.MarkDirtyRepaint();
                EditorApplication.delayCall += (EditorApplication.CallbackFunction) (() =>
                {
                    m_AttachedContentHashCode = ((PropertyField) Content)?.GetSerializedProperty()?.GetHashCode() ?? 0;
                    if (m_AttachedContentHashCode != 0)
                    {
                        var altDecoratorsCache = UngroupedMarkerDecorators.GetInstance(m_AttachedContentHashCode);
                        altDecoratorsCache.DisableAutoGroupingOnActiveSelection(disable: true);
                    }
                    DecoratorsCache.DisableAutoGroupingOnActiveSelection(disable: true);
                    UngroupedMarkerDecorators.SetAutoDisableGroupingWhenCreatingCachesInGroup(Property.GetSerializedObject().GetHashCode(), false);
                    Content.MarkDirtyRepaint();

                    EditorApplication.delayCall += (EditorApplication.CallbackFunction) (() =>
                    {
                        // ContentContainer.SetVisible(true);
                        ContentContainer.SetVisible(false);
                        BindPropertyToTypeField(SelectorField);

                        // Property.isExpanded = IsExpanded;
                        // if (!IsExpanded)
                        //     ContentContainer.SetVisible(false);
                        // else
                        //     ContentContainer.SetVisible(instanceField.Property.managedReferenceValue != null);
                        Debug.Log($"2: instanceField.Foldout.value = {IsExpanded};");
                        // instanceField.Foldout.value = IsExpanded;
                        instanceField.SetFoldoutValue(IsExpanded);
                        Content.MarkDirtyRepaint();
                    });
                });
                
            }
            else if (SelectorField is TypeField typeField)
            {
                typeField.BindProperty(Property.GetSerializedProperty(), -1);
                UngroupedMarkerDecorators.SetAutoDisableGroupingWhenCreatingCachesInGroup(Property.GetSerializedObject().GetHashCode(), false);
            }
            else
                throw new NotImplementedException("Missing code path");
        }
        
        private void BindPropertyToTypeField(VisualElement selectorField)
        {
            if (selectorField is TypeInstanceField instanceField)
            {
                m_DelayedContentHashCode = ((PropertyField) Content).GetSerializedProperty().GetHashCode();
                var altDecoratorsCache = UngroupedMarkerDecorators.GetInstance(m_DelayedContentHashCode);
                altDecoratorsCache.DisableAutoGroupingOnActiveSelection(disable: true);
                instanceField.BindProperty(Property.GetSerializedProperty(), -1);
                if (Content is PropertyField propertyField)
                {
                    propertyField.RebuildChildDecoratorDrawersIfNecessary(Property.GetSerializedProperty());
                    altDecoratorsCache.TryRebuildAll();
                    DecoratorsCache.TryRebuildAll();  // TODO: remove redundant call
                    altDecoratorsCache.DisableAutoGroupingOnActiveSelection(disable: false);
                    DecoratorsCache.DisableAutoGroupingOnActiveSelection(disable: false);
                    altDecoratorsCache.HandlePendingDecorators();
                    DecoratorsCache.HandlePendingDecorators();
                }
            }
        }

        // private IVisualElementScheduledItem m_DelayedUpdateTask;
        //
        // public void ExecuteDelayedUpdate()
        // {
        //     m_DelayedUpdateTask = Container.schedule.Execute(() =>
        //     {
        //         m_DelayedUpdateTask?.Pause();
        //         OnUpdate();
        //         m_DelayedUpdateTask = null;
        //     });
        //     m_DelayedUpdateTask.ExecuteLater(1);
        // }

        public override void OnUpdate()
        {
            OnReload();
            RepaintContainerGUI();
        }

        public override void OnReset(bool disposing = false)
        {
            if (!disposing)
            {
                Container?.LogThis("// TODO: SOFT DRAWER RESET " + GetType().Name + "(NOT DISPOSING)");
            }
            else
            {
                Container?.LogThis("RESET " + GetType().Name + "(DISPOSING)");
                Content?.Unbind();
                Content?.RemoveFromHierarchy();
                ContentContainer?.RemoveFromHierarchy();
                switch (SelectorField)
                {
                    case TypeInstanceField instanceField:
                        instanceField.Unbind();
                        break;
                    case TypeField typeField:
                        typeField.Unbind();
                        break;
                }
                Container?.Clear();
                Content = null;
                ContentContainer = null;
                SelectorField = null;
                Container?.RemoveFromHierarchy();
                Container = null;
            }
            
            base.OnReset(disposing);
        }
    }
}
