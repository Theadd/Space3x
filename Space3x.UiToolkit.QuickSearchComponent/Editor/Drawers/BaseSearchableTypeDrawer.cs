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
    public abstract class BaseSearchableTypeDrawer<TAttribute> : 
            Drawer<TAttribute>, 
            IAttributeExtensionContext<TAttribute>,
            IExpandablePropertyContent
        where TAttribute : BaseSearchableTypeAttribute
    {
//        // Flag: Has Dispose already been called?
//        private bool m_Disposed = false;
        private int m_SelectedIndex;
        private bool m_IsTypeValue;
        private Type m_ElementType;
        
        // public override TAttribute Target => (TAttribute) attribute;
        // public SerializedProperty Property { get; private set; }
        
        protected QuickSearchPopup Popup { get; set; }
        protected QuickSearchElement PopupContent { get; set; }
        protected VisualElement SelectorField { get; private set; }
        // protected VisualElement Container { get; private set; }
        public VisualElement Content { get; set; }
        public VisualElement ContentContainer { get; set; }
        protected ListView ListViewElement { get; set; }
        protected virtual List<Type> GetAllTypes() => Target.GetAllTypes();
        protected virtual void OnReload() => Target.ReloadCache();

        protected override VisualElement OnCreatePropertyGUI(SerializedProperty property)
        {
            Property = property;
            Debug.Log($"<color=#FF0099FF>IN OnCreatePropertyGUI, propertyPath: {property.propertyPath}, property.name: {property.name}, " +
                      $"property.hash: {property.GetHashCode()}, serializedObject.hash: {property.serializedObject.GetHashCode()}</color>");
            m_ElementType = GetUnderlyingElementType(property);
            m_IsTypeValue = IsTypeValue(m_ElementType);
            // Debug.Log($"IsTypeValue: {m_IsTypeValue}, name: {property.name}, propertyType: {property.propertyType}, isArray: {property.isArray}, property.GetUnderlyingType(): {property.GetUnderlyingType()}");
            Validate();
            if (attribute.applyToCollection)
            {
                if (!Property.isArray) Debug.LogError($"Collection property {Property.name} is not an array", property.serializedObject.targetObject);
//                Container = CreatePropertyCollectionGUI(property);
//                return Container;
                return CreatePropertyCollectionGUI(property);
            }
//            Container = CreateContainerGUI();
//            RepaintContainerGUI();
////            if (GetAllTypes().Count > 0)
////                OnValueChange(GetAllTypes().ElementAtOrDefault(m_SelectedIndex));
//            
//            // Container.RegisterCallback<SerializedPropertyChangeEvent>(OnPropertyChanged);
//            return Container;
            return CreateContainerGUI();
        }

//        // TODO: Remove
//        private void OnPropertyChanged(SerializedPropertyChangeEvent ev)
//        {
//            try
//            {
//                Debug.LogWarning($"[BaseSearchableTypeDrawer] OnPropertyChanged: {ev.changedProperty.boxedValue}");
//            }
//            catch (Exception)
//            {
//                Debug.Log("Ignored exception");
//            }
//        }

        /*
         * <voffset=0em><line-height=0px><b>RangeDrawer</b> <br><voffset=0em><align="right"><alpha=#7F>(in UnityEditor)<alpha=#FF>
         */
        public virtual VisualElement CreatePropertyCollectionGUI(SerializedProperty property)
        {
            ListViewElement = CreateListView();
            ListViewElement.BindProperty(property);
            ListViewElement.headerTitle = property.displayName;

            return ListViewElement;
        }
        
        protected virtual ListView CreateListView()
        {
            // TODO: Remove
            if (!m_IsTypeValue) 
                Debug.LogError("TODO: makeItem = () => ... new TypeInstanceField() { OnShowPopup = OnShowPopup, Container = ContentContainer, Content = (PropertyField) Content };");
            
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
                // onAdd = (BaseListView list) => Debug.Log($"onAdd {list}"),
                headerTitle = "Elements",
                makeItem = () =>
                {
                    return m_IsTypeValue 
                        ? new TypeField(showLabel: false) { OnShowPopup = OnShowPopup } 
                        : new TypeInstanceField() { OnShowPopup = OnShowPopup };
                },
                bindItem = (element, i) => {
                    if (element is TypeInstanceField instanceField)
                    {
                        instanceField.Unbind();
                        instanceField.BindProperty(Property, i);
                    }
                    else if (element is TypeField field)
                    {
                        field.Unbind();
                        field.BindProperty(Property, i);
                    }
                }
            };
            return listView;
        }

        private void RepaintContainerGUI()
        {
            Debug.Log($"<color=#FF00FFCC>IN REPAINT: Container.childCount: {Container.hierarchy.childCount}</color>");
            Container.Clear();
            m_SelectorFieldAttached = false;
            m_ContentAttached = false;
            
            ((IExpandablePropertyContent) this).RebuildExpandablePropertyContentGUI(OnAttachContentToPanel);

            m_InitialContentHashCode = m_IsTypeValue 
                ? Property.GetHashCode() 
                : ((PropertyField) Content)?.GetSerializedProperty()?.GetHashCode() ?? 0;
            
            SelectorField = AddSelectorFieldEventListeners(
                ApplySelectorFieldVisuals(
                    CreateSelectorFieldGUI()));
            Container.Add(SelectorField);
            
//            Container.Add(Content);
            Container.Add(ContentContainer);
        }
        
        protected virtual void Validate()
        {
            if (!m_IsTypeValue && Property.propertyType != SerializedPropertyType.ManagedReference)
                throw new Exception(nameof(TAttribute) 
                                    + " can be used only with a ManagedReference. Add a [SerializedReference] attribute"
                                    + " to the property you want to use this attribute on.");
        }

        private static bool IsTypeValue(Type type)
        {
            if (type == null) return false;
            
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
            Container = new BindableElement();

            Context.WithExtension<TrackChangesOnEx, ITrackChangesOn, BindableElement>((BindableElement) Container, out var success);
            if (!success) 
                OnUpdate();
//            Debug.LogWarning("// TODO: ITrackChangesEx");
//            if (Target is ITrackChangesEx targetWithTrackChanges)
//            {
//                var trackedPropertyName = targetWithTrackChanges.TrackChangesOn;
//                if (trackedPropertyName != string.Empty)
//                {
//                    var trackedProperty = Property.serializedObject.FindProperty(trackedPropertyName);
//                    if (trackedProperty != null)
//                    {
//                        element.Unbind();
//                        element.TrackPropertyValue(trackedProperty, OnTrackedPropertyChanged);
//                        element.BindProperty(trackedProperty);
//                    }
//                    else
//                        Debug.LogWarning($"Could not find property {trackedPropertyName} on {Property.serializedObject.targetObject}");
//                }
//            }

            return Container;
        }

        public virtual VisualElement CreateContentGUI() => m_IsTypeValue ? new VisualElement() : new PropertyField(Property);

        protected virtual VisualElement CreateSelectorFieldGUI() =>
            m_IsTypeValue 
                ? new TypeField(showLabel: true, initialLabel: Property.displayName) { OnShowPopup = OnShowPopup }
                : new TypeInstanceField() { OnShowPopup = OnShowPopup, ExpandablePropertyContent = this };

        protected virtual void OnShowPopup(TypeField target, VisualElement selectorField, ShowWindowMode mode)
        {
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
//            if (selectorField is BindableElement bindableField)
//            {
//                bindableField.RegisterCallback<ChangeEvent<Type[]>>(OnValueChangeCallback);
//            }
            
            return selectorField;
        }

        private IVisualElementScheduledItem m_BindTask;
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

        private int m_InitialContentHashCode;
        private int m_AttachedContentHashCode;
        private int m_DelayedContentHashCode;

        private void OnContainerFullyAttached()
        {
            if (SelectorField is TypeInstanceField)
            {
                Debug.Log($"... @BaseSearchableTypeDrawer.OnContainerFullyAttached: {Content.AsString()}");
                m_AttachedContentHashCode = ((PropertyField) Content)?.GetSerializedProperty()?.GetHashCode() ?? 0;
                Debug.Log($"    ///////////////////// ContentHashCodes: Initial = {m_InitialContentHashCode}, Attached = {m_AttachedContentHashCode}, Property = {Property.GetHashCode()}");
                if (m_AttachedContentHashCode != 0)
                {
                    var altDecoratorsCache = UngroupedMarkerDecorators.GetInstance(m_AttachedContentHashCode);
                    altDecoratorsCache.DisableAutoGroupingOnActiveSelection(disable: true);
                    Debug.Log("                     ///////////////////// AutoGrouping DISABLED");
                }
                DecoratorsCache.DisableAutoGroupingOnActiveSelection(disable: true);  // MOD_ME
            }
            m_BindTask = Container.schedule.Execute(() => BindPropertyToTypeField(SelectorField));
            m_BindTask.ExecuteLater(1);
        }
        
        private void BindPropertyToTypeField(VisualElement selectorField)
        {
            Debug.Log("                                       #2 @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
            m_BindTask?.Pause();
            Debug.Log("                                       #3 @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
            if (selectorField is TypeInstanceField instanceField)
            {
//                var altInstanceIdA = ((PropertyField) Content).GetSerializedProperty().GetHashCode();
                m_DelayedContentHashCode = ((PropertyField) Content).GetSerializedProperty().GetHashCode();
                Debug.Log(Container.CreateRichTextReport("RichTextReport A"));
                Debug.Log($"    ///////////////////// ContentHashCodes: Initial = {m_InitialContentHashCode}, " +
                          $"Attached = {m_AttachedContentHashCode}, Delayed = {m_DelayedContentHashCode}, " +
                          $"Property = {Property.GetHashCode()}");
                var altDecoratorsCache = UngroupedMarkerDecorators.GetInstance(m_DelayedContentHashCode);
                altDecoratorsCache.DisableAutoGroupingOnActiveSelection(disable: true);
                Debug.Log("                                       #4 @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                instanceField.BindProperty(Property, -1);
                Debug.Log(Container.CreateRichTextReport("RichTextReport B"));
                Debug.Log("                                       #5 @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                altDecoratorsCache.TryRebuildAll();
                Debug.Log(Container.CreateRichTextReport("RichTextReport C"));
                DecoratorsCache.TryRebuildAll();  // TODO: remove redundant call
                Debug.Log(Container.CreateRichTextReport("RichTextReport D"));
                Debug.Log("                                       #6 @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
//                DecoratorsCache.TryRebuildAndLinkAll();
                altDecoratorsCache.DisableAutoGroupingOnActiveSelection(disable: false);
                DecoratorsCache.DisableAutoGroupingOnActiveSelection(disable: false);
                Debug.Log(Container.CreateRichTextReport("RichTextReport E"));
                Debug.Log($"  ... AFTER @BaseSearchableTypeDrawer.BindPropertyToTypeField: {selectorField.AsString()}");
                DecoratorsCache.HandlePendingDecorators();
                Debug.Log(Container.CreateRichTextReport("RichTextReport F"));
                altDecoratorsCache.HandlePendingDecorators();
                Debug.Log(Container.CreateRichTextReport("RichTextReport G"));
//                var altInstanceIdB = ((PropertyField) Content).GetSerializedProperty().GetHashCode();
//                Debug.Log($"<color=#ffffffff><b>  .. ALTERNATIVE DECORATORS CACHE INSTANCE ID A: {altInstanceIdA} .. </b></color>");
//                Debug.Log($"<color=#ffffffff><b>  .. ALTERNATIVE DECORATORS CACHE INSTANCE ID B: {altInstanceIdB} .. </b></color>");
            } 
            else if (selectorField is TypeField typeField)
            {
                typeField.BindProperty(Property, -1);
            }
            else
                throw new NotImplementedException("Missing code path");
        }

        private IVisualElementScheduledItem m_DelayedUpdateTask;
        
        public void ExecuteDelayedUpdate()
        {
//            Debug.Log(".. .. .. @BaseSearchableTypeDrawer.ExecuteDelayedUpdate");
            m_DelayedUpdateTask = Container.schedule.Execute(() =>
            {
                m_DelayedUpdateTask?.Pause();
                Debug.Log(".. .. .. @BaseSearchableTypeDrawer.ExecuteDelayedUpdate INNER");
                OnUpdate();
                m_DelayedUpdateTask = null;
                Debug.Log(".. .. .. @BaseSearchableTypeDrawer.ExecuteDelayedUpdate DONE");
            });
            m_DelayedUpdateTask.ExecuteLater(1);
        }

//        private void OnValueChangeCallback(ChangeEvent<IEnumerable<Type>> e) => OnValueChange(e.newValue);
//
//        protected virtual void OnValueChange(IEnumerable<Type> newValues)
//        {
//            foreach (var newValue in newValues)
//            {
//                Property.SetUnderlyingValue(Unsafe3x.CreateWrapper<Type>(Property.GetUnderlyingType(), newValue));
//                ((TextField)SelectorField).value = newValue.Name;
//                Property.serializedObject.ApplyModifiedProperties();
//            }
////            Property.managedReferenceValue = typeof(LayerDrawer);
////            Property.serializedObject.ApplyModifiedProperties();
//        }

        public override void OnUpdate()
        {
            OnReload();
            RepaintContainerGUI();
        }

//        protected virtual void OnTrackedPropertyChanged(SerializedProperty trackedProperty)
//        {
//            OnReload();
//            RepaintContainerGUI();
//        }
        
        public override void OnReset(bool disposing = false)
        {
            if (!disposing)
            {
                // ...
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
//                if (Marker?.IsUsed == true || Marker?.ClassListContains(GroupMarker.UssUsedClassName) == true)
//                {
//                    if (!Target.IsOpen)
//                        Marker.GetClosestParentOfType<PropertyGroupField>().RemoveFromHierarchy();
//                }
//                RemoveGroupMarker();
//                LinkedMarkerDecorator = null;
            }
            
            base.OnReset(disposing);
        }
        
//        protected virtual void Dispose(bool disposing)
//        {
//            if (m_Disposed) return;
//            if (disposing)
//            {
////                if (SelectorField is BindableElement bindableField)
////                    bindableField.UnregisterCallback<ChangeEvent<Type[]>>(OnValueChangeCallback);
//            }
//            m_Disposed = true;
//        }
    }
}
