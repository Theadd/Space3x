using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor.Drawers;
using Space3x.InspectorAttributes.Editor.Extensions;
using Space3x.InspectorAttributes.Editor.FieldFactories;
using Space3x.InspectorAttributes.Editor.Utilities;
using Space3x.InspectorAttributes.Types;
using Space3x.Properties.Types;
using Space3x.Properties.Types.Editor;
using Space3x.UiToolkit.Types;
using Unity.Properties;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.VisualElements
{
    /// <summary>
    /// Like <see cref="PropertyField"/> but for non-serialized properties (<see cref="IPropertyNode"/>).
    /// </summary>
    [UxmlElement]
    public partial class BindablePropertyField : VisualElement, IBindable
    {
        public static readonly string ussClassName = "ui3x-bindable-property-field";
        public static readonly string decoratorDrawersContainerClassName = "unity-decorator-drawers-container";
        public static readonly string listViewNamePrefix = "unity-list-";
        public VisualElement Field;
        public IPropertyNode Property;
        public VisualElement DecoratorDrawersContainer => m_DecoratorDrawersContainer ??= CreateDecoratorDrawersContainer();
        private VisualElement m_DecoratorDrawersContainer;
        private PropertyAttributeController m_Controller;
        // private Type m_PropertyDrawerOnCollectionItems = null;
        // private PropertyAttribute m_PropertyDrawerOnCollectionItemsAttribute;
        // private FieldInfo m_RuntimeField;   // Only set and used for creating instances of property drawers on collection items.
        // private Dictionary<VisualElement, ICreateDrawerOnPropertyNode> m_ElementDrawers;
        private FieldFactoryBuilder m_FieldFactoryBuilder;
        
        // IBindable interface
        public IBinding binding { get; set; }
        public string bindingPath { get; set; }
        
        public BindablePropertyField()
        {
            if (DecoratorDrawersContainer != null)
                this.WithClasses(ussClassName, "unity-property-field");
        }

        /// <summary>
        /// Creates a new <see cref="BindablePropertyField"/> bound to the given <paramref name="property"/>.
        /// </summary>
        /// <param name="property">A non-serialized <see cref="IPropertyNode"/>.</param>
        /// <param name="applyCustomDrawers">Whether to create all property and decorator drawers annotated on the property.</param>
        public BindablePropertyField(IPropertyNode property, bool applyCustomDrawers = false) : this() => BindProperty(property, applyCustomDrawers);

        private VisualElement CreateDecoratorDrawersContainer()
        {
            var element = new VisualElement();
            element.WithClasses(decoratorDrawersContainerClassName);
            this.Insert(0, element);
            return element;
        }

        /// <summary>
        /// Binds the given <paramref name="property"/> to this <see cref="BindablePropertyField"/>,
        /// optionally creating all decorator drawers annotated on the property.
        /// </summary>
        /// <param name="property">A non-serialized <see cref="IPropertyNode"/>.</param>
        /// <param name="applyCustomDrawers">Whether to create all property and decorator drawers annotated on the property.</param>
        public void BindProperty(IPropertyNode property, bool applyCustomDrawers = false)
        {
            // if (!(property is INonSerializedPropertyNode nonSerializedPropertyNode))
            //     throw new ArgumentException($"Invalid IPropertyNode, it must be a non serialized property in order to bind it to a {nameof(BindablePropertyField)}, for serialized properties, just use {nameof(PropertyField)} instead.");
            // Property = nonSerializedPropertyNode;
            Property = property;
            bindingPath = Property.PropertyPath;
            // m_Controller = PropertyAttributeController.GetInstance(Property);
            m_Controller = Property.GetController();
            if (m_Controller == null)
                throw new ArgumentException("Unexpected, value for PropertyAttributeController is null.");
            var vType = m_Controller.AnnotatedType.GetValue(Property.Name);
            if (vType == null && applyCustomDrawers)
            {
                if (property.IsArrayOrListElement())
                {
                    var collectionProperty = property.GetParentProperty();
                    vType = collectionProperty.GetController().AnnotatedType.GetValue(collectionProperty.Name);
                }
                else
                {
                    DebugLog.Error(new ArgumentException("Unexpected value.").ToString());
                    Field?.RemoveFromHierarchy();
                    Field = new Label("Unexpected value.");
                    Add(Field);
                    return;
                }
            }

            PropertyDrawer drawer = null;
            if (applyCustomDrawers)
            {
                var propertyDrawer = property.IsArrayOrListElement() ? vType.PropertyDrawerOnCollectionItems : vType.PropertyDrawer;
                if (propertyDrawer != null)
                {
                    if (typeof(ICreatePropertyNodeGUI).IsAssignableFrom(propertyDrawer))
                    {
                        try
                        {
                            drawer = DrawerExtensions.CreatePropertyDrawer(
                                propertyDrawer,
                                property.IsArrayOrListElement() ? vType.PropertyDrawerOnCollectionItemsAttribute : vType.PropertyDrawerAttribute, 
                                vType.RuntimeField, 
                                Property.DisplayName());
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                            drawer = null;
                        }
                    }
                    // else
                    // {
                    //     // TODO: merge if and else if they're still equivalent
                    //     try
                    //     {
                    //         drawer = DrawerExtensions.CreatePropertyDrawer(
                    //             propertyDrawer,
                    //             property.IsArrayOrListElement() ? vType.PropertyDrawerOnCollectionItemsAttribute : vType.PropertyDrawerAttribute, 
                    //             vType.RuntimeField, 
                    //             Property.DisplayName());
                    //     }
                    //     catch (Exception e)
                    //     {
                    //         Debug.LogException(e);
                    //         drawer = null;
                    //     }
                    // }
                }
            }

            VisualElement field = null;
            if (drawer == null)
            {
                field = Property.IsInvokable() ? BindToInvokablePropertyNode() : BindToBuiltInField();
            }
            else
            {
                if (drawer is ICreatePropertyNodeGUI customDrawer)
                {
                    field = customDrawer.CreatePropertyNodeGUI(Property);
                    // Debug.LogError("<b>BindableUtility.AutoNotifyValueChangedOnNonSerialized(field, Property);</b>");
                    BindableUtility.AutoNotifyValueChangedOnNonSerialized(field, Property);
                }
                else
                {
                    // TODO: For IMGUI drawers, wrap them in a custom drawer with an IMGUIContainer element.
                    DebugLog.Error("// TODO: For IMGUI drawers, wrap them in a custom drawer with an IMGUIContainer element.");
                }
            }
            Field?.RemoveFromHierarchy();
            Field = field;
            if (Field != null)
            {
                // TODO: Remove next line
                // Field.WithClasses(false, UssConstants.UssAligned);
                Add(Field);
                Field.tooltip = vType?.Tooltip ?? "";
            }
        }

        /// <summary>
        /// Instantiates and attaches all decorator drawers annotated on the property.
        /// </summary>
        public void AttachDecoratorDrawers()
        {
            DecoratorDrawersContainer.Clear();
            var vType = m_Controller.AnnotatedType.GetValue(Property.Name);
            if (vType == null)
            {
                if (Property.IsArrayOrListElement())
                {
                    var collectionProperty = Property.GetParentProperty();
                    vType = collectionProperty.GetController().AnnotatedType.GetValue(collectionProperty.Name);
                }
                else
                {
                    DebugLog.Error(new ArgumentException("Unexpected value.").ToString());
                    return;
                }
            }
            for (var i = 0; i < vType.DecoratorDrawers.Count; i++)
            {
                var decorator = vType.DecoratorDrawers[i];
                if (decorator == null) continue;
                try
                {
                    if (Property.IsArrayOrListElement() && vType.PropertyAttributes[i].applyToCollection) continue;
                    var drawer = DrawerExtensions.CreateDecoratorDrawer(decorator, vType.PropertyAttributes[i]);
                    if (drawer.CreatePropertyGUI() is VisualElement decoratorElement)
                        DecoratorDrawersContainer.Add(decoratorElement);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        /// <summary>
        /// Removes the <see cref="BindablePropertyField"/> from the VisualElement's hierarchy, properly removing
        /// all it's decorator drawers.
        /// </summary>
        public void ProperlyRemoveFromHierarchy()
        {
            for (var i = DecoratorDrawersContainer.hierarchy.childCount - 1; i >= 0; i--)
            {
                if (DecoratorDrawersContainer.hierarchy[i] is GhostDecorator ghostDecorator)
                {
                    ghostDecorator.TargetDecorator.ProperlyRemoveFromHierarchy();
                }
            }
            DecoratorDrawersContainer.Clear();
            try
            {
                RemoveFromHierarchy();
            }
            catch (Exception e)
            {
                DebugLog.Error(e.ToString());
            }
        }

        private VisualElement BindToInvokablePropertyNode()
        {
            VisualElement field = null;
            field = ConfigureInvokableField<InvokableField, string>(Field as InvokableField, () => new InvokableField(label: Property.DisplayName())
            {
                Property = Property
            });
            
            return field;
        }

        private bool TryGetAssignedPropertyValueAndType(out object propertyValue, out Type propertyType)
        {
            var propertyInfo = m_Controller.DeclaringObject.GetType().GetField(Property.Name, 
                BindingFlags.Instance 
                | BindingFlags.Static
                | BindingFlags.NonPublic
                | BindingFlags.Public);
            if (propertyInfo == null)
            {
                propertyValue = null;
                propertyType = null;
                return false;
            }
            propertyValue = propertyInfo.GetValue(m_Controller.DeclaringObject);
            propertyType = propertyValue?.GetType() ?? propertyInfo.FieldType;
            return true;
        }
        
        private VisualElement BindToBuiltInField()
        {
            VisualElement field = null;
            var isCollectionElement = Property.IsArrayOrListElement();
            var isCollection = Property.IsArrayOrList();
            // The declared underlying element type for collections and collections elements, or the declared property type for non-collections.
            Type declaredPropertyType = isCollectionElement
                ? Property.GetParentProperty().GetUnderlyingElementType()
                : isCollection
                    ? Property.GetUnderlyingElementType()
                    : Property.GetUnderlyingType();
            if (!TryGetAssignedPropertyValueAndType(out object propertyValue, out Type propertyType))
            {
                propertyValue = Property.GetValue();
                propertyType = propertyValue?.GetType();
            }

            if (isCollection)
            {
                Func<VisualElement> itemFactory = () => new BindablePropertyField().WithClasses(UssConstants.UssShowInInspector);
                field = declaredPropertyType switch
                {
                    _ when declaredPropertyType == typeof(int) => ConfigureListView<int>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(uint) => ConfigureListView<uint>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(long) => ConfigureListView<long>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(ulong) => ConfigureListView<ulong>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(double) => ConfigureListView<double>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(float) => ConfigureListView<float>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(bool) => ConfigureListView<bool>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(string) => ConfigureListView<string>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(Color) => ConfigureListView<Color>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(Vector2) => ConfigureListView<Vector2>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(Vector3) => ConfigureListView<Vector3>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(Vector4) => ConfigureListView<Vector4>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(Rect) => ConfigureListView<Rect>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(Bounds) => ConfigureListView<Bounds>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(Vector2Int) => ConfigureListView<Vector2Int>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(Vector3Int) => ConfigureListView<Vector3Int>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(RectInt) => ConfigureListView<RectInt>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(BoundsInt) => ConfigureListView<BoundsInt>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(AnimationCurve) => ConfigureListView<AnimationCurve>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(Gradient) => ConfigureListView<Gradient>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(Hash128) => ConfigureListView<Hash128>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(NamedType) => ConfigureListView<NamedType>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(SerializableType) => ConfigureListView<SerializableType>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(UnityEngine.Object) => ConfigureListView<UnityEngine.Object>(itemFactory, Field as ListView),
                    _ when Property.HasChildren() => ConfigureDynamicListView(itemFactory, Field as ListView),
                    _ => FieldNotImplemented()
                };
            }
            else
            {
                DebugLog.Info($"[USK3] [BindablePropertyField] BindToBuiltInField ON ({declaredPropertyType?.Name}): {Property.PropertyPath}");
                field = declaredPropertyType switch
                {
                    not null when declaredPropertyType == typeof(long) => ConfigureField<LongField, long>(Field as LongField, () => new LongField(label: Property.DisplayName())),
                    not null when declaredPropertyType == typeof(ulong) => ConfigureField<UnsignedLongField, ulong>(Field as UnsignedLongField, () => new UnsignedLongField(label: Property.DisplayName())),
                    not null when declaredPropertyType == typeof(uint) => ConfigureField<UnsignedIntegerField, uint>(Field as UnsignedIntegerField, () => new UnsignedIntegerField(label: Property.DisplayName())),
                    not null when declaredPropertyType == typeof(int) => ConfigureField<IntegerField, int>(Field as IntegerField, () => new IntegerField(label: Property.DisplayName())),
                    not null when declaredPropertyType == typeof(double) => ConfigureField<DoubleField, double>(Field as DoubleField, () => new DoubleField(label: Property.DisplayName())),
                    not null when declaredPropertyType == typeof(float) => ConfigureField<FloatField, float>(Field as FloatField, () => new FloatField(label: Property.DisplayName())),
                    not null when declaredPropertyType == typeof(bool) => ConfigureField<Toggle, bool>(Field as Toggle, () => new Toggle(label: Property.DisplayName())),
                    not null when declaredPropertyType == typeof(string) => ConfigureField<TextField, string>(Field as TextField, () => new TextField(label: Property.DisplayName())),
                    not null when declaredPropertyType == typeof(Color) => ConfigureField<ColorField, Color>(Field as ColorField, () => new ColorField(label: Property.DisplayName())),
                    not null when declaredPropertyType == typeof(Vector2) => ConfigureField<Vector2Field, Vector2>(Field as Vector2Field, () => new Vector2Field(label: Property.DisplayName())),
                    not null when declaredPropertyType == typeof(Vector3) => ConfigureField<Vector3Field, Vector3>(Field as Vector3Field, () => new Vector3Field(label: Property.DisplayName())),
                    not null when declaredPropertyType == typeof(Vector4) => ConfigureField<Vector4Field, Vector4>(Field as Vector4Field, () => new Vector4Field(label: Property.DisplayName())),
                    not null when declaredPropertyType == typeof(Rect) => ConfigureField<RectField, Rect>(Field as RectField, () => new RectField(label: Property.DisplayName())),
                    not null when declaredPropertyType == typeof(Bounds) => ConfigureField<BoundsField, Bounds>(Field as BoundsField, () => new BoundsField(label: Property.DisplayName())),
                    not null when declaredPropertyType == typeof(Vector2Int) => ConfigureField<Vector2IntField, Vector2Int>(Field as Vector2IntField, () => new Vector2IntField(label: Property.DisplayName())),
                    not null when declaredPropertyType == typeof(Vector3Int) => ConfigureField<Vector3IntField, Vector3Int>(Field as Vector3IntField, () => new Vector3IntField(label: Property.DisplayName())),
                    not null when declaredPropertyType == typeof(RectInt) => ConfigureField<RectIntField, RectInt>(Field as RectIntField, () => new RectIntField(label: Property.DisplayName())),
                    not null when declaredPropertyType == typeof(BoundsInt) => ConfigureField<BoundsIntField, BoundsInt>(Field as BoundsIntField, () => new BoundsIntField(label: Property.DisplayName())),
                    not null when declaredPropertyType == typeof(AnimationCurve) => ConfigureField<CurveField, AnimationCurve>(Field as CurveField, () => new CurveField(label: Property.DisplayName())),
                    not null when declaredPropertyType == typeof(Gradient) => ConfigureField<GradientField, Gradient>(Field as GradientField, () => new GradientField(label: Property.DisplayName())),
                    not null when declaredPropertyType == typeof(Hash128) => ConfigureField<Hash128Field, Hash128>(Field as Hash128Field, () => new Hash128Field(label: Property.DisplayName())),
                    not null when typeof(UnityEngine.Object).IsAssignableFrom(declaredPropertyType) => ConfigureObjectField<ObjectField, UnityEngine.Object>(Field as ObjectField, () => new ObjectField(label: Property.DisplayName())
                    {
                        objectType = declaredPropertyType,
                        allowSceneObjects = true
                    }),
                    not null when Property.HasChildren() => ConfigureChildrenFields(propertyType ?? declaredPropertyType),
                    _ => FieldNotImplemented()
                };
            }
            return field;
        }

        private VisualElement FieldNotImplemented() => new Label(Property.DisplayName() + " - [NOT IMPLEMENTED]");
        
        private TField ConfigureInvokableField<TField, TValue>(
            TField field,
            Func<TField> factory)
            where TField : BaseField<TValue>
        {
            // TODO: This is a draft.
            if ((object) field == null)
            {
                field = (TField)factory().WithClasses(BaseField<TValue>.alignedFieldUssClassName);
                if (Property is IInvokablePropertyNode invokableProperty)
                {
                    if (field is BaseField<string> invokableField)
                    {
                        invokableProperty.ValueChanged += p => invokableField.value = p.Value?.ToString() ?? string.Empty;
                    }
                }
            }

            return field;
        }

        private long m_ValueHash = 0;
        
        private VisualElement ConfigureChildrenFields(Type propertyType)
        {
            DebugLog.Info($"[USK3] [BindablePropertyField] ConfigureChildrenFields FOR propertyType {propertyType.Name}: {Property.PropertyPath}");
            var field = new Foldout()
            {
                text = Property.DisplayName(),
                value = true
            };
            var controller = PropertyAttributeController.GetOrCreateInstance(Property, propertyType, true);
            m_FieldFactoryBuilder = new FieldFactoryBuilder(controller);
            m_FieldFactoryBuilder.Rebuild(field.contentContainer);
            m_ValueHash = Property.GetUnderlyingValue()?.GetHashCode() ?? 0;
            this.TrackPropertyValue(Property, OnTrackedPropertyValueChanged);

            return field;
        }

        private void OnTrackedPropertyValueChanged(IPropertyNode trackedNode)
        {
            if (!Equals(Property, trackedNode))
            {
                Debug.Log($"[PAC!] <u>[ConfigureChildrenFields] <b>NOT EQUALS!</b> OnTrackedPropertyValueChanged</u>: {Property.PropertyPath} != {trackedNode.PropertyPath}");
                // return;
            }
            var node = Property;
            object nodeValue = null;
            try
            {
                nodeValue = node.GetUnderlyingValue();
            }
            catch (ArgumentOutOfRangeException)
            {
                m_FieldFactoryBuilder.Clear();
                return;
            }
            if ((nodeValue?.GetHashCode() ?? 0) == m_ValueHash)
                return;
            m_ValueHash = nodeValue?.GetHashCode() ?? 0;
            m_FieldFactoryBuilder.Clear();
            if (nodeValue == null) return;
            var controller = PropertyAttributeController.GetOrCreateInstance(node, nodeValue.GetType());
            m_FieldFactoryBuilder = new FieldFactoryBuilder(controller)
            {
                EnableReadOnly = m_FieldFactoryBuilder.EnableReadOnly
            };
            m_FieldFactoryBuilder.Rebuild(((Foldout)Field).contentContainer);
        }
        
        private TField ConfigureField<TField, TValue>(
            TField field,
            Func<TField> factory)
            where TField : BaseField<TValue>
        {
            if ((object) field == null)
            {
                field = (TField) factory().WithClasses(BaseField<TValue>.alignedFieldUssClassName);
                field.RegisterValueChangedCallback<TValue>((ev =>
                {
                    if (this.dataSource is DataSourceBinding bindableSource)
                        if (RuntimeHelpers.Equals((TValue)bindableSource.Value, ev.newValue) && !RuntimeHelpers.Equals(ev.previousValue, ev.newValue))
                            bindableSource.NotifyValueChanged();
                }));
                this.dataSource = new DataSourceBinding(Property);
            }
            field.SetBinding(nameof(BaseField<TValue>.value), new DataBinding
            {
                
                dataSourcePath = new PropertyPath(nameof(DataSourceBinding.Value)),
                bindingMode = BindingMode.TwoWay
            });
            
            return field;
        }
        
        private TField ConfigureObjectField<TField, TValue>(
            TField field,
            Func<TField> factory)
            where TValue : UnityEngine.Object
            where TField : BaseField<TValue>
        {
            if ((object) field == null)
            {
                field = (TField) factory().WithClasses(BaseField<TValue>.alignedFieldUssClassName);
                field.RegisterValueChangedCallback<TValue>((ev =>
                {
                    if (this.dataSource is DataSourceObjectBinding bindableSource)
                        if (RuntimeHelpers.Equals((TValue)bindableSource.Value, ev.newValue) && !RuntimeHelpers.Equals(ev.previousValue, ev.newValue))
                            bindableSource.NotifyValueChanged();
                }));
                this.dataSource = new DataSourceObjectBinding(Property);
            }
            field.SetBinding(nameof(BaseField<TValue>.value), new DataBinding
            {
                dataSourcePath = new PropertyPath(nameof(DataSourceObjectBinding.Value)),
                bindingMode = BindingMode.TwoWay
            });
            return field;
        }

        private VisualElement ConfigureListView<TItemValue>(
            Func<VisualElement> itemFactory,
            ListView listView,
            Func<ListView> factory = null)
            //where TValue : IList, IList<TItemValue>
        {
            DebugLog.Info($"[USK3] [BindablePropertyField] ConfigureListView + IList({typeof(TItemValue).Name}): {Property.PropertyPath}");
            if (listView == null)
            {
                if (factory != null)
                    listView = factory();
                else
                    listView = new ListView();
                listView.showBorder = true;
                listView.selectionType = SelectionType.Multiple;
                listView.showAddRemoveFooter = true;
                listView.showBoundCollectionSize = true;
                listView.showFoldoutHeader = true;
                listView.reorderable = !Property.IsNonReorderable();
                listView.reorderMode = ListViewReorderMode.Animated;
                listView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
                listView.showAlternatingRowBackgrounds = AlternatingRowBackground.None;
            }

            listView.makeItem = () => itemFactory()
                .WithClasses(BaseField<TItemValue>.alignedFieldUssClassName);
            listView.bindItem = (element, i) =>
            {
                Debug.LogWarning("[PAC!] bindItem " + i + " @ " + Property.PropertyPath);
                var propertyAtIndex = Property.GetArrayElementAtIndex(i);
                if (((BindablePropertyField)element).Property?.Equals(propertyAtIndex) ?? false)
                {
                    Debug.LogWarning("[PAC!] <b>SKIPPING</b>bindItem " + i + " @ " + Property.PropertyPath);
                    return;
                }
                ((BindablePropertyField)element).BindProperty(propertyAtIndex, true);
                // element.dataSource = new DataSourceBinding(Property, i);
                // element.SetBinding(nameof(BaseField<TItemValue>.value), new DataBinding
                // {
                //     dataSourcePath = new PropertyPath(nameof(DataSourceBinding.Value)),
                //     bindingMode = BindingMode.TwoWay
                // });
                // if (element is BaseField<TItemValue> baseField)
                //     baseField.label = "Element " + i;
                // if (m_ElementDrawers?.TryGetValue(element, out ICreateDrawerOnPropertyNode elementDrawer) ?? false)
                //     elementDrawer.SetPropertyNode(Property.GetArrayElementAtIndex(i));
                //     // TODO: BindableUtility.AutoNotifyValueChangedOnNonSerialized(itemElement, Property); m_ElementDrawers
            };
            listView.onAdd = list =>
            {
                Debug.Log("OnAdd @ " + Property.PropertyPath);
                List<TItemValue> newList = new List<TItemValue>(list.itemsSource?.Cast<TItemValue>() ?? new TItemValue[] {});
                newList.Add(default);
                ((DataSourceBinding)this.dataSource).Value = Property.IsArray()
                    ? newList.ToArray()
                    : newList.ToList();
                // Debug.LogWarning($"[NOTIC3] [onAdd] Callback ON {property.PropertyPath} => {ev.newValue?.ToString()}");
                // TODO: Uncomment.
                list.itemsSource = (IList)((DataSourceBinding)this.dataSource).Value;
                // (BindingId) nameof (itemsSource);
            };
            listView.onRemove = list =>
            {
                Debug.Log("OnRemove @ " + Property.PropertyPath);
                List<TItemValue> newList = new List<TItemValue>(list.itemsSource.Cast<TItemValue>());
                var indices = new List<int>(list.selectedIndices);
                if (indices.Count == 0)
                    indices.Add(newList.Count - 1);
                newList = newList.Where((item, i) => !indices.Contains(i)).ToList();
                ((DataSourceBinding)this.dataSource).Value = Property.IsArray()
                    ? newList.ToArray()
                    : newList.ToList();
                // TODO: Uncomment.
                list.itemsSource = (IList)((DataSourceBinding)this.dataSource).Value;
            };
            this.dataSource = new DataSourceBinding(Property);
            var str = listViewNamePrefix + Property.PropertyPath;
            listView.headerTitle = Property.DisplayName();
            listView.viewDataKey = str;
            listView.name = str;
            
            listView.itemsSource = (IList)((DataSourceBinding)this.dataSource).Value;
            listView.SetBinding(nameof(BaseVerticalCollectionView.itemsSource), new DataBinding
            {
                dataSourcePath = new PropertyPath(nameof(DataSourceBinding.Value)),
                bindingMode = BindingMode.TwoWay
            });
            
            listView.TrackPropertyValue(Property, node =>
            {
                if (!Equals(Property, node)) return;
                Debug.LogWarning("[PAC!] IN LISTVIEW TrackPropertyValue CALLBACK FOR " + Property.PropertyPath + " :: lvTHash: " + listView.GetHashCode());
                // listView.itemsSource = (IList)((DataSourceBinding)this.dataSource).Value;
                listView.itemsSource = (IList)node.GetValue();
            });
            
            listView.viewController.itemsSourceChanged += () =>
            {
                Debug.LogWarning("[PAC!] itemsSourceChanged @ " + Property.PropertyPath + " :: lvTHash: " + listView.GetHashCode());
            };

            listView.viewController.itemIndexChanged += (srcIndex, dstIndex) =>
            {
                Debug.LogWarning("[PAC!] itemIndexChanged @ " + Property.PropertyPath + " :: " + srcIndex + " => " + dstIndex + " :: lvTHash: " + listView.GetHashCode());
                IList list = (IList)Property.GetValue();
                for (var i = 0; i < list.Count; i++)
                {
                    var o = list[i];
                    Debug.LogWarning($"    {i} => {o?.ToString()}");
                }

                for (var i = Math.Min(srcIndex, dstIndex); i <= Math.Max(srcIndex, dstIndex); i++)
                {
                    (Property as INonSerializedPropertyNode)?.NotifyValueChanged(Property.GetArrayElementAtIndex(i));
                }
            };
            // BindableUtility.AutoNotifyValueChangedOnNonSerialized(itemElement, Property)
            Debug.LogWarning("[PAC!] LISTVIEW CONFIGURED FOR " + Property.PropertyPath + " :: lvTHash: " + listView.GetHashCode());
            
            return (VisualElement) listView;
        }
        
        private static MethodInfo s_ConfigureListViewMethod = null;
        private const BindingFlags PublicStaticFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.CreateInstance | BindingFlags.DoNotWrapExceptions;

        private VisualElement ConfigureDynamicListView(
            Func<VisualElement> itemFactory,
            ListView listView,
            Func<ListView> factory = null)
        {
            var itemType = Property.GetUnderlyingElementType();
            DebugLog.Info($"[USK3] [BindablePropertyField] ConfigureDynamicListView + IList({itemType?.Name}): {Property.PropertyPath}");
            s_ConfigureListViewMethod ??= typeof(BindablePropertyField).GetMethod("ConfigureListView",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var method = s_ConfigureListViewMethod!.MakeGenericMethod(itemType);
            return (VisualElement) method.Invoke(this, PublicStaticFlags, null, new object[] { itemFactory, listView, factory },
                CultureInfo.InvariantCulture);
        }
    }
}
