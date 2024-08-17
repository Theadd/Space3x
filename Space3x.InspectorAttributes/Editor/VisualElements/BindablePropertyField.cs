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
using Space3x.InspectorAttributes.Editor.Utilities;
using Space3x.InspectorAttributes.Types;
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
    public partial class BindablePropertyField : VisualElement
    {
        public static readonly string ussClassName = "ui3x-bindable-property-field";
        public static readonly string decoratorDrawersContainerClassName = "unity-decorator-drawers-container";
        public static readonly string listViewNamePrefix = "unity-list-";
        public VisualElement Field;
        public IPropertyNode Property;
        public VisualElement DecoratorDrawersContainer => m_DecoratorDrawersContainer ??= CreateDecoratorDrawersContainer();
        private VisualElement m_DecoratorDrawersContainer;
        private PropertyAttributeController m_Controller;
        private Type m_PropertyDrawerOnCollectionItems = null;
        private PropertyAttribute m_PropertyDrawerOnCollectionItemsAttribute;
        private FieldInfo m_RuntimeField;   // Only set and used for creating instances of property drawers on collection items.
        private Dictionary<VisualElement, ICreateDrawerOnPropertyNode> m_ElementDrawers;
        private BindablePropertyFieldFactory m_BindablePropertyFieldFactory;

        /// <summary>
        /// Path of the target property to be bound (Read Only).
        /// </summary>
        public string BindingPath => Property?.PropertyPath ?? string.Empty;
        
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
            // m_Controller = PropertyAttributeController.GetInstance(Property);
            m_Controller = Property.GetController();
            if (m_Controller == null)
                throw new ArgumentException("Unexpected, value for PropertyAttributeController is null.");
            var vType = m_Controller.AnnotatedType.GetValue(Property.Name);
            if (vType == null && applyCustomDrawers)
            {
                DebugLog.Error(new ArgumentException("Unexpected value.").ToString());
                Field?.RemoveFromHierarchy();
                Field = new Label("WTF!");
                Add(Field);
                return;
            }

            PropertyDrawer drawer = null;
            if (applyCustomDrawers)
            {
                var propertyDrawer = vType.PropertyDrawer;
                if (propertyDrawer != null && typeof(IDrawer).IsAssignableFrom(propertyDrawer))
                {
                    try
                    {
                        drawer = DrawerExtensions.CreatePropertyDrawer(
                            propertyDrawer,
                            vType.PropertyDrawerAttribute, 
                            vType.RuntimeField, 
                            Property.DisplayName());
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        drawer = null;
                    }
                }

                m_PropertyDrawerOnCollectionItems = vType.PropertyDrawerOnCollectionItems;
                if (m_PropertyDrawerOnCollectionItems != null &&
                    typeof(IDrawer).IsAssignableFrom(m_PropertyDrawerOnCollectionItems))
                {
                    m_PropertyDrawerOnCollectionItemsAttribute = vType.PropertyDrawerOnCollectionItemsAttribute;
                    m_RuntimeField = vType.RuntimeField;
                }
                else
                    m_PropertyDrawerOnCollectionItems = null;
            }

            VisualElement field = null;
            if (drawer == null)
            {
                field = Property.IsInvokable() ? BindToInvokablePropertyNode() : BindToBuiltInField();
            }
            else
            {
                if (drawer is ICreateDrawerOnPropertyNode customDrawer)
                {
                    field = customDrawer.CreatePropertyNodeGUI(Property);
                    (m_ElementDrawers ??= new Dictionary<VisualElement, ICreateDrawerOnPropertyNode>())[field] = customDrawer;
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
                DebugLog.Error(new ArgumentException("Unexpected value.").ToString());
                return;
            }
            for (var i = 0; i < vType.DecoratorDrawers.Count; i++)
            {
                var decorator = vType.DecoratorDrawers[i];
                if (decorator == null) continue;
                try
                {
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
        
        private VisualElement BindToBuiltInField()
        {
            var propertyInfo = m_Controller.DeclaringObject.GetType().GetField(Property.Name, 
                BindingFlags.Instance 
                | BindingFlags.Static
                | BindingFlags.NonPublic
                | BindingFlags.Public);
            VisualElement field = null;
            object propertyValue = null;
            var isCollectionItem = false;
            if (propertyInfo == null && Property.IsArrayOrListElement())
            {
                propertyValue = Property.GetUnderlyingValue();
                isCollectionItem = true;
            }
            else
                propertyValue = propertyInfo.GetValue(m_Controller.DeclaringObject);
            if (propertyValue is IList list)
            {
                Func<VisualElement> itemFactory = null;
                if (m_PropertyDrawerOnCollectionItems != null)
                {
                    itemFactory = () =>
                    {
                        PropertyDrawer drawer = DrawerExtensions.CreatePropertyDrawer(
                            m_PropertyDrawerOnCollectionItems, 
                            m_PropertyDrawerOnCollectionItemsAttribute,
                            m_RuntimeField,
                            Property.DisplayName());
                        VisualElement itemElement = ((ICreateDrawerOnPropertyNode)drawer).CreatePropertyNodeGUI(Property);
                        (m_ElementDrawers ??= new Dictionary<VisualElement, ICreateDrawerOnPropertyNode>())[itemElement] = (ICreateDrawerOnPropertyNode)drawer;
                        return itemElement;
                    };
                    field = list switch
                    {
                        IList<NamedType> _ => ConfigureListView<NamedType>(itemFactory, Field as ListView),
                        IList<SerializableType> _ => ConfigureListView<SerializableType>(itemFactory, Field as ListView),
                        IList<UnityEngine.Object> _ => ConfigureListView<UnityEngine.Object>(itemFactory, Field as ListView),
                        not null when Property.HasChildren() => ConfigureDynamicListView(itemFactory, Field as ListView),
                        _ => null
                    };
                    if (field != null) return field;
                }
                field = list switch
                {
                    IList<int> _ => ConfigureListView<int>(itemFactory ?? (() => new IntegerField()), Field as ListView),
                    IList<uint> _ => ConfigureListView<uint>(itemFactory ?? (() => new UnsignedIntegerField()), Field as ListView),
                    IList<long> _ => ConfigureListView<long>(itemFactory ?? (() => new LongField()), Field as ListView),
                    IList<ulong> _ => ConfigureListView<ulong>(itemFactory ?? (() => new UnsignedLongField()), Field as ListView),
                    IList<double> _ => ConfigureListView<double>(itemFactory ?? (() => new DoubleField()), Field as ListView),
                    IList<float> _ => ConfigureListView<float>(itemFactory ?? (() => new FloatField()), Field as ListView),
                    IList<bool> _ => ConfigureListView<bool>(itemFactory ?? (() => new Toggle()), Field as ListView),
                    IList<string> _ => ConfigureListView<string>(itemFactory ?? (() => new TextField()), Field as ListView),
                    IList<Color> _ => ConfigureListView<Color>(itemFactory ?? (() => new ColorField()), Field as ListView),
                    IList<Vector2> _ => ConfigureListView<Vector2>(itemFactory ?? (() => new Vector2Field()), Field as ListView),
                    IList<Vector3> _ => ConfigureListView<Vector3>(itemFactory ?? (() => new Vector3Field()), Field as ListView),
                    IList<Vector4> _ => ConfigureListView<Vector4>(itemFactory ?? (() => new Vector4Field()), Field as ListView),
                    IList<Rect> _ => ConfigureListView<Rect>(itemFactory ?? (() => new RectField()), Field as ListView),
                    IList<Bounds> _ => ConfigureListView<Bounds>(itemFactory ?? (() => new BoundsField()), Field as ListView),
                    IList<Vector2Int> _ => ConfigureListView<Vector2Int>(itemFactory ?? (() => new Vector2IntField()), Field as ListView),
                    IList<Vector3Int> _ => ConfigureListView<Vector3Int>(itemFactory ?? (() => new Vector3IntField()), Field as ListView),
                    IList<RectInt> _ => ConfigureListView<RectInt>(itemFactory ?? (() => new RectIntField()), Field as ListView),
                    IList<BoundsInt> _ => ConfigureListView<BoundsInt>(itemFactory ?? (() => new BoundsIntField()), Field as ListView),
                    IList<AnimationCurve> _ => ConfigureListView<AnimationCurve>(itemFactory ?? (() => new CurveField()), Field as ListView),
                    IList<Gradient> _ => ConfigureListView<Gradient>(itemFactory ?? (() => new GradientField()), Field as ListView),
                    IList<Hash128> _ => ConfigureListView<Hash128>(itemFactory ?? (() => new Hash128Field()), Field as ListView),
                    // IList<NamedType> _ => ConfigureListView<NamedType>(itemFactory ?? (() => new Hash128Field()), Field as ListView),
                    // TODO: IList<UnityEngine.Object>
                    _ => FieldNotImplemented()
                };
            }
            else
            {
                var propertyType = propertyValue?.GetType() ?? (isCollectionItem ? Property.GetParentProperty().GetUnderlyingElementType() : propertyInfo.FieldType);
                field = propertyType switch
                {
                    not null when propertyType == typeof(long) => ConfigureField<LongField, long>(Field as LongField, () => new LongField(label: Property.DisplayName())),
                    not null when propertyType == typeof(ulong) => ConfigureField<UnsignedLongField, ulong>(Field as UnsignedLongField, () => new UnsignedLongField(label: Property.DisplayName())),
                    not null when propertyType == typeof(uint) => ConfigureField<UnsignedIntegerField, uint>(Field as UnsignedIntegerField, () => new UnsignedIntegerField(label: Property.DisplayName())),
                    not null when propertyType == typeof(int) => ConfigureField<IntegerField, int>(Field as IntegerField, () => new IntegerField(label: Property.DisplayName())),
                    not null when propertyType == typeof(double) => ConfigureField<DoubleField, double>(Field as DoubleField, () => new DoubleField(label: Property.DisplayName())),
                    not null when propertyType == typeof(float) => ConfigureField<FloatField, float>(Field as FloatField, () => new FloatField(label: Property.DisplayName())),
                    not null when propertyType == typeof(bool) => ConfigureField<Toggle, bool>(Field as Toggle, () => new Toggle(label: Property.DisplayName())),
                    not null when propertyType == typeof(string) => ConfigureField<TextField, string>(Field as TextField, () => new TextField(label: Property.DisplayName())),
                    not null when propertyType == typeof(Color) => ConfigureField<ColorField, Color>(Field as ColorField, () => new ColorField(label: Property.DisplayName())),
                    not null when propertyType == typeof(Vector2) => ConfigureField<Vector2Field, Vector2>(Field as Vector2Field, () => new Vector2Field(label: Property.DisplayName())),
                    not null when propertyType == typeof(Vector3) => ConfigureField<Vector3Field, Vector3>(Field as Vector3Field, () => new Vector3Field(label: Property.DisplayName())),
                    not null when propertyType == typeof(Vector4) => ConfigureField<Vector4Field, Vector4>(Field as Vector4Field, () => new Vector4Field(label: Property.DisplayName())),
                    not null when propertyType == typeof(Rect) => ConfigureField<RectField, Rect>(Field as RectField, () => new RectField(label: Property.DisplayName())),
                    not null when propertyType == typeof(Bounds) => ConfigureField<BoundsField, Bounds>(Field as BoundsField, () => new BoundsField(label: Property.DisplayName())),
                    not null when propertyType == typeof(Vector2Int) => ConfigureField<Vector2IntField, Vector2Int>(Field as Vector2IntField, () => new Vector2IntField(label: Property.DisplayName())),
                    not null when propertyType == typeof(Vector3Int) => ConfigureField<Vector3IntField, Vector3Int>(Field as Vector3IntField, () => new Vector3IntField(label: Property.DisplayName())),
                    not null when propertyType == typeof(RectInt) => ConfigureField<RectIntField, RectInt>(Field as RectIntField, () => new RectIntField(label: Property.DisplayName())),
                    not null when propertyType == typeof(BoundsInt) => ConfigureField<BoundsIntField, BoundsInt>(Field as BoundsIntField, () => new BoundsIntField(label: Property.DisplayName())),
                    not null when propertyType == typeof(AnimationCurve) => ConfigureField<CurveField, AnimationCurve>(Field as CurveField, () => new CurveField(label: Property.DisplayName())),
                    not null when propertyType == typeof(Gradient) => ConfigureField<GradientField, Gradient>(Field as GradientField, () => new GradientField(label: Property.DisplayName())),
                    not null when propertyType == typeof(Hash128) => ConfigureField<Hash128Field, Hash128>(Field as Hash128Field, () => new Hash128Field(label: Property.DisplayName())),
                    not null when typeof(UnityEngine.Object).IsAssignableFrom(propertyType) => ConfigureObjectField<ObjectField, UnityEngine.Object>(Field as ObjectField, () => new ObjectField(label: Property.DisplayName())
                    {
                        objectType = isCollectionItem ? Property.GetParentProperty().GetUnderlyingElementType() : propertyInfo.FieldType,
                        allowSceneObjects = true
                    }),
                    not null when Property.HasChildren() => ConfigureChildrenFields(propertyType),
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
            var field = new Foldout()
            {
                text = Property.DisplayName(),
                value = true
            };
            var controller = PropertyAttributeController.GetOrCreateInstance(Property, propertyType, true);
            m_BindablePropertyFieldFactory = BindablePropertyFieldFactory.Create(controller);
            m_BindablePropertyFieldFactory.Rebuild(field.contentContainer);
            m_ValueHash = Property.GetUnderlyingValue()?.GetHashCode() ?? 0;
            this.TrackPropertyValue(Property, OnTrackedPropertyValueChanged);

            return field;
        }

        private void OnTrackedPropertyValueChanged(IPropertyNode trackedNode)
        {
            var node = Property;
            object nodeValue = null;
            try
            {
                nodeValue = node.GetUnderlyingValue();
            }
            catch (ArgumentOutOfRangeException)
            {
                m_BindablePropertyFieldFactory.Clear();
                return;
            }
            if ((nodeValue?.GetHashCode() ?? 0) == m_ValueHash)
                return;
            m_ValueHash = nodeValue?.GetHashCode() ?? 0;
            m_BindablePropertyFieldFactory.Clear();
            if (nodeValue == null) return;
            var controller = PropertyAttributeController.GetOrCreateInstance(node, nodeValue.GetType());
            m_BindablePropertyFieldFactory = BindablePropertyFieldFactory.Create(controller);
            m_BindablePropertyFieldFactory.Rebuild(((Foldout)Field).contentContainer);
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
                Debug.Log("bindItem " + i + " @ " + Property.PropertyPath);
                element.dataSource = new DataSourceBinding(Property, i);
                element.SetBinding(nameof(BaseField<TItemValue>.value), new DataBinding
                {
                    dataSourcePath = new PropertyPath(nameof(DataSourceBinding.Value)),
                    bindingMode = BindingMode.TwoWay
                });
                if (element is BaseField<TItemValue> baseField)
                    baseField.label = "Element " + i;
                if (m_ElementDrawers?.TryGetValue(element, out ICreateDrawerOnPropertyNode elementDrawer) ?? false)
                    elementDrawer.SetPropertyNode(Property.GetArrayElementAtIndex(i));
                    // TODO: BindableUtility.AutoNotifyValueChangedOnNonSerialized(itemElement, Property); m_ElementDrawers
            };
            listView.onAdd = list =>
            {
                Debug.Log("OnAdd @ " + Property.PropertyPath);
                List<TItemValue> newList = new List<TItemValue>(list.itemsSource.Cast<TItemValue>());
                newList.Add(default);
                // ((DataSourceBinding)this.dataSource).Value = list.itemsSource is Array
                //     ? (TValue)((IList<TItemValue>) newList.ToArray())
                //     : (TValue)((IList<TItemValue>) newList.ToList());
                ((DataSourceBinding)this.dataSource).Value = Property.IsArray()
                    ? newList.ToArray()
                    : newList.ToList();
                list.itemsSource = (IList)((DataSourceBinding)this.dataSource).Value;
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
                list.itemsSource = (IList)((DataSourceBinding)this.dataSource).Value;
            };
            this.dataSource = new DataSourceBinding(Property);
            var str = listViewNamePrefix + Property.PropertyPath;
            listView.headerTitle = Property.DisplayName();
            listView.viewDataKey = str;
            listView.name = str;
            listView.itemsSource = (IList)((DataSourceBinding)this.dataSource).Value;
            
            return (VisualElement) listView;
        }
        
        private static MethodInfo ConfigureListViewMethod = null;
        private static readonly BindingFlags PublicStaticFlags = BindingFlags.Public 
                                                                | BindingFlags.NonPublic
                                                                | BindingFlags.Static 
                                                                | BindingFlags.CreateInstance 
                                                                | BindingFlags.DoNotWrapExceptions;

        private VisualElement ConfigureDynamicListView(
            Func<VisualElement> itemFactory,
            ListView listView,
            Func<ListView> factory = null)
        {
            var itemType = Property.GetUnderlyingElementType();
            Debug.Log($"Item Type: {itemType?.Name}");

            ConfigureListViewMethod ??= typeof(BindablePropertyField).GetMethod("ConfigureListView",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var method = ConfigureListViewMethod!.MakeGenericMethod(itemType);
            return (VisualElement) method.Invoke(this, PublicStaticFlags, null, new object[] { itemFactory, listView, factory },
                CultureInfo.InvariantCulture);
            
            // var listType = typeof(List<>).MakeGenericType(itemType);
            // object emptyList = Activator.CreateInstance(listType);
            // dynamic dynList = emptyList;
            // Debug.Log($"Dynamic Types: \n{itemType}\n{listType}\n{emptyList}\n{dynList}");
            //
            // return ConfigureDynamicListViewWrapper(dynList, itemFactory, listView, factory);
        }
        
        // private VisualElement ConfigureDynamicListViewWrapper<TValue>(
        //     TValue emptyList,
        //     Func<VisualElement> itemFactory,
        //     ListView listView,
        //     Func<ListView> factory = null)
        //     where TValue : IList //, IList<TItemValue>
        //     // where TItemValue : class
        // {
        //     emptyList.Add(default);
        //     object defaultItem = emptyList[0];
        //     dynamic dynItem = defaultItem;
        //     Debug.Log($"Dynamic Types 1: \n{typeof(TValue)}\n{dynItem}\n{defaultItem}");
        //     return ConfigureDynamicListViewWrapper2(emptyList, dynItem, itemFactory, listView, factory);
        // }
        //
        // private VisualElement ConfigureDynamicListViewWrapper2<TValue, TItemValue>(
        //     TValue emptyList,
        //     TItemValue defaultItem,
        //     Func<VisualElement> itemFactory,
        //     ListView listView,
        //     Func<ListView> factory = null)
        //     where TValue : IList, IList<TItemValue>
        // // where TItemValue : class
        // {
        //     Debug.Log($"Dynamic Types 2: \n{typeof(TValue)}\n{typeof(TItemValue)}\n{defaultItem}");
        //     return null; // ConfigureCustomListView<TItemValue>(itemFactory, listView, factory);
        // }
        //
        // private VisualElement ConfigureCustomListView<TValue, TItemValue>(
        //         TValue emptyList,
        //     Func<VisualElement> itemFactory,
        //     ListView listView,
        //     Func<ListView> factory = null)
        //     //where TValue : IList, IList<TItemValue>
        // {
        //     if (listView == null)
        //     {
        //         if (factory != null)
        //             listView = factory();
        //         else
        //             listView = new ListView();
        //         listView.showBorder = true;
        //         listView.selectionType = SelectionType.Multiple;
        //         listView.showAddRemoveFooter = true;
        //         listView.showBoundCollectionSize = true;
        //         listView.showFoldoutHeader = true;
        //         listView.reorderable = !Property.IsNonReorderable();
        //         listView.reorderMode = ListViewReorderMode.Animated;
        //         listView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
        //         listView.showAlternatingRowBackgrounds = AlternatingRowBackground.None;
        //     }
        //
        //     listView.makeItem = () => itemFactory()
        //         .WithClasses(BaseField<TItemValue>.alignedFieldUssClassName);
        //     listView.bindItem = (element, i) =>
        //     {
        //         Debug.Log("bindItem " + i);
        //         element.dataSource = new DataSourceBinding(Property, i);
        //         element.SetBinding(nameof(BaseField<TItemValue>.value), new DataBinding
        //         {
        //             dataSourcePath = new PropertyPath(nameof(DataSourceBinding.Value)),
        //             bindingMode = BindingMode.TwoWay
        //         });
        //         if (element is BaseField<TItemValue> baseField)
        //             baseField.label = "Element " + i;
        //         if (m_ElementDrawers?.TryGetValue(element, out ICreateDrawerOnPropertyNode elementDrawer) ?? false)
        //             elementDrawer.SetPropertyNode(Property.GetArrayElementAtIndex(i));
        //             // TODO: BindableUtility.AutoNotifyValueChangedOnNonSerialized(itemElement, Property); m_ElementDrawers
        //     };
        //     listView.onAdd = list =>
        //     {
        //         Debug.Log("OnAdd");
        //         List<TItemValue> newList = new List<TItemValue>(list.itemsSource.Cast<TItemValue>());
        //         newList.Add(default);
        //         // ((DataSourceBinding)this.dataSource).Value = list.itemsSource is Array
        //         //     ? (TValue)((IList<TItemValue>) newList.ToArray())
        //         //     : (TValue)((IList<TItemValue>) newList.ToList());
        //         ((DataSourceBinding)this.dataSource).Value = Property.IsArray()
        //             ? newList.ToArray()
        //             : newList.ToList();
        //         list.itemsSource = (IList)((DataSourceBinding)this.dataSource).Value;
        //     };
        //     listView.onRemove = list =>
        //     {
        //         Debug.Log("OnRemove");
        //         List<TItemValue> newList = new List<TItemValue>(list.itemsSource.Cast<TItemValue>());
        //         var indices = new List<int>(list.selectedIndices);
        //         if (indices.Count == 0)
        //             indices.Add(newList.Count - 1);
        //         newList = newList.Where((item, i) => !indices.Contains(i)).ToList();
        //         ((DataSourceBinding)this.dataSource).Value = Property.IsArray()
        //             ? newList.ToArray()
        //             : newList.ToList();
        //         list.itemsSource = (IList)((DataSourceBinding)this.dataSource).Value;
        //     };
        //     this.dataSource = new DataSourceBinding(Property);
        //     var str = listViewNamePrefix + Property.PropertyPath;
        //     listView.headerTitle = Property.DisplayName();
        //     listView.viewDataKey = str;
        //     listView.name = str;
        //     listView.itemsSource = (IList)((DataSourceBinding)this.dataSource).Value;
        //     
        //     return (VisualElement) listView;
        // }
    }
}
