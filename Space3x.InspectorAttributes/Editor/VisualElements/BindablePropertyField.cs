using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor.Drawers;
using Space3x.InspectorAttributes.Editor.Extensions;
using Space3x.UiToolkit.Types;
using Unity.Properties;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.VisualElements
{
    [UxmlElement]
    public partial class BindablePropertyField : VisualElement
    {
        public static readonly string ussClassName = "ui3x-bindable-property-field";
        public static readonly string decoratorDrawersContainerClassName = "unity-decorator-drawers-container";
        public static readonly string listViewNamePrefix = "unity-list-";
        public VisualElement Field;
        public IProperty Property;
        public VisualElement DecoratorDrawersContainer => m_DecoratorDrawersContainer ??= CreateDecoratorDrawersContainer();
        private VisualElement m_DecoratorDrawersContainer;
        private PropertyAttributeController m_Controller;
        private Type m_PropertyDrawerOnCollectionItems = null;
        private PropertyAttribute m_PropertyDrawerOnCollectionItemsAttribute;
        private FieldInfo m_RuntimeField;   // Only set and used for creating instances of property drawers on collection items.

        public BindablePropertyField()
        {
            if (DecoratorDrawersContainer != null)
                this.WithClasses(ussClassName, "unity-property-field");
        }

        public BindablePropertyField(IProperty property) : this() => BindProperty(property);

        private VisualElement CreateDecoratorDrawersContainer()
        {
            var element = new VisualElement();
            element.WithClasses(decoratorDrawersContainerClassName);
            this.Insert(0, element);
            return element;
        }

        public void BindProperty(IProperty property, bool applyCustomDrawers = false)
        {
            if (!(property is INonSerializedPropertyNode nonSerializedPropertyNode))
                throw new ArgumentException($"Invalid IProperty, it must be a non serialized property in order to bind it to a {nameof(BindablePropertyField)}, for serialized properties, just use {nameof(PropertyField)} instead.");
            Property = nonSerializedPropertyNode;
            m_Controller = PropertyAttributeController.GetInstance(Property);
            if (m_Controller == null)
                throw new ArgumentException("Unexpected, value for PropertyAttributeController is null.");
            var vType = m_Controller.AnnotatedType.GetValue(Property.Name);
            if (vType == null)
            {
                DebugLog.Error(new ArgumentException("Unexpected value.").ToString());
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
                        drawer = PropertyDrawerExtensions.CreatePropertyDrawer(
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

            if (drawer == null)
                BindToBuiltInField();
            else
            {
                if (drawer is ICreateDrawerOnPropertyNode customDrawer)
                {
                    var field = customDrawer.CreatePropertyNodeGUI(Property);
                    if (Field != null && Field != field)
                        Field.RemoveFromHierarchy();
                    Field = field;
                    Add(Field);
                }
            }

            if (Field != null)
                Field.tooltip = vType.Tooltip;
        }

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
                    var drawer = PropertyDrawerExtensions.CreateDecoratorDrawer(decorator, vType.PropertyAttributes[i]);
                    if (drawer.CreatePropertyGUI() is VisualElement decoratorElement)
                        DecoratorDrawersContainer.Add(decoratorElement);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
        
        private void BindToBuiltInField()
        {
            var propertyInfo = m_Controller.DeclaringObject.GetType().GetField(Property.Name, 
                BindingFlags.Instance 
                | BindingFlags.Static
                | BindingFlags.NonPublic
                | BindingFlags.Public);
            var isAdded = Field != null;
            VisualElement field = null;
            var propertyValue = propertyInfo.GetValue(m_Controller.DeclaringObject);
            if (propertyValue is IList list)
            {
                Func<VisualElement> itemFactory = null;
                if (m_PropertyDrawerOnCollectionItems != null)
                {
                    itemFactory = () =>
                    {
                        PropertyDrawer drawer = null;
                        drawer = (PropertyDrawer)Activator.CreateInstance(m_PropertyDrawerOnCollectionItems);
                        drawer.SetAttribute(m_PropertyDrawerOnCollectionItemsAttribute);
                        drawer.SetFieldInfo(m_RuntimeField);
                        drawer.SetPreferredLabel(Property.DisplayName());

                        return ((ICreateDrawerOnPropertyNode)drawer).CreatePropertyNodeGUI(Property);
                    };
                }
                field = list switch
                {
                    int[] _ => ConfigureListView<int[], int>(itemFactory ?? (() => new IntegerField()), Field as ListView),
                    uint[] _ => ConfigureListView<uint[], uint>(itemFactory ?? (() => new UnsignedIntegerField()), Field as ListView),
                    long[] _ => ConfigureListView<long[], long>(itemFactory ?? (() => new LongField()), Field as ListView),
                    ulong[] _ => ConfigureListView<ulong[], ulong>(itemFactory ?? (() => new UnsignedLongField()), Field as ListView),
                    double[] _ => ConfigureListView<double[], double>(itemFactory ?? (() => new DoubleField()), Field as ListView),
                    float[] _ => ConfigureListView<float[], float>(itemFactory ?? (() => new FloatField()), Field as ListView),
                    bool[] _ => ConfigureListView<bool[], bool>(itemFactory ?? (() => new Toggle()), Field as ListView),
                    string[] _ => ConfigureListView<string[], string>(itemFactory ?? (() => new TextField()), Field as ListView),
                    Color[] _ => ConfigureListView<Color[], Color>(itemFactory ?? (() => new ColorField()), Field as ListView),
                    Vector2[] _ => ConfigureListView<Vector2[], Vector2>(itemFactory ?? (() => new Vector2Field()), Field as ListView),
                    Vector3[] _ => ConfigureListView<Vector3[], Vector3>(itemFactory ?? (() => new Vector3Field()), Field as ListView),
                    Vector4[] _ => ConfigureListView<Vector4[], Vector4>(itemFactory ?? (() => new Vector4Field()), Field as ListView),
                    Rect[] _ => ConfigureListView<Rect[], Rect>(itemFactory ?? (() => new RectField()), Field as ListView),
                    Bounds[] _ => ConfigureListView<Bounds[], Bounds>(itemFactory ?? (() => new BoundsField()), Field as ListView),
                    Vector2Int[] _ => ConfigureListView<Vector2Int[], Vector2Int>(itemFactory ?? (() => new Vector2IntField()), Field as ListView),
                    Vector3Int[] _ => ConfigureListView<Vector3Int[], Vector3Int>(itemFactory ?? (() => new Vector3IntField()), Field as ListView),
                    RectInt[] _ => ConfigureListView<RectInt[], RectInt>(itemFactory ?? (() => new RectIntField()), Field as ListView),
                    BoundsInt[] _ => ConfigureListView<BoundsInt[], BoundsInt>(itemFactory ?? (() => new BoundsIntField()), Field as ListView),
                    AnimationCurve[] _ => ConfigureListView<AnimationCurve[], AnimationCurve>(itemFactory ?? (() => new CurveField()), Field as ListView),
                    Gradient[] _ => ConfigureListView<Gradient[], Gradient>(itemFactory ?? (() => new GradientField()), Field as ListView),
                    Hash128[] _ => ConfigureListView<Hash128[], Hash128>(itemFactory ?? (() => new Hash128Field()), Field as ListView),
                    _ => throw new NotImplementedException($"{propertyInfo.FieldType} not yet implemented in {nameof(BindablePropertyField)}.{nameof(BindToBuiltInField)}().")
                };
            }
            else
            {
                var propertyType = propertyValue?.GetType() ?? propertyInfo.FieldType;
                if (propertyValue == null)
                {
                    if (typeof(UnityEngine.Object).IsAssignableFrom(propertyType))
                        field = ConfigureField<ObjectField, UnityEngine.Object>(Field as ObjectField,
                            () => new ObjectField(label: Property.DisplayName())
                            {
                                objectType = propertyInfo.FieldType,
                                allowSceneObjects = true
                            });
                    else
                        field = new Label("Not implemented: " + propertyType.Name);
                }
                else 
                    field = propertyValue switch
                    {
                        long _ => ConfigureField<LongField, long>(Field as LongField, () => new LongField(label: Property.DisplayName())),
                        ulong _ => ConfigureField<UnsignedLongField, ulong>(Field as UnsignedLongField, () => new UnsignedLongField(label: Property.DisplayName())),
                        uint _ => ConfigureField<UnsignedIntegerField, uint>(Field as UnsignedIntegerField, () => new UnsignedIntegerField(label: Property.DisplayName())),
                        int _ => ConfigureField<IntegerField, int>(Field as IntegerField, () => new IntegerField(label: Property.DisplayName())),
                        double _ => ConfigureField<DoubleField, double>(Field as DoubleField, () => new DoubleField(label: Property.DisplayName())),
                        float _ => ConfigureField<FloatField, float>(Field as FloatField, () => new FloatField(label: Property.DisplayName())),
                        bool _ => ConfigureField<Toggle, bool>(Field as Toggle, () => new Toggle(label: Property.DisplayName())),
                        string _ => ConfigureField<TextField, string>(Field as TextField, () => new TextField(label: Property.DisplayName())),
                        Color _ => ConfigureField<ColorField, Color>(Field as ColorField, () => new ColorField(label: Property.DisplayName())),
                        Vector2 _ => ConfigureField<Vector2Field, Vector2>(Field as Vector2Field, () => new Vector2Field(label: Property.DisplayName())),
                        Vector3 _ => ConfigureField<Vector3Field, Vector3>(Field as Vector3Field, () => new Vector3Field(label: Property.DisplayName())),
                        Vector4 _ => ConfigureField<Vector4Field, Vector4>(Field as Vector4Field, () => new Vector4Field(label: Property.DisplayName())),
                        Rect _ => ConfigureField<RectField, Rect>(Field as RectField, () => new RectField(label: Property.DisplayName())),
                        Bounds _ => ConfigureField<BoundsField, Bounds>(Field as BoundsField, () => new BoundsField(label: Property.DisplayName())),
                        Vector2Int _ => ConfigureField<Vector2IntField, Vector2Int>(Field as Vector2IntField, () => new Vector2IntField(label: Property.DisplayName())),
                        Vector3Int _ => ConfigureField<Vector3IntField, Vector3Int>(Field as Vector3IntField, () => new Vector3IntField(label: Property.DisplayName())),
                        RectInt _ => ConfigureField<RectIntField, RectInt>(Field as RectIntField, () => new RectIntField(label: Property.DisplayName())),
                        BoundsInt _ => ConfigureField<BoundsIntField, BoundsInt>(Field as BoundsIntField, () => new BoundsIntField(label: Property.DisplayName())),
                        AnimationCurve _ => ConfigureField<CurveField, AnimationCurve>(Field as CurveField, () => new CurveField(label: Property.DisplayName())),
                        Gradient _ => ConfigureField<GradientField, Gradient>(Field as GradientField, () => new GradientField(label: Property.DisplayName())),
                        Hash128 _ => ConfigureField<Hash128Field, Hash128>(Field as Hash128Field, () => new Hash128Field(label: Property.DisplayName())),
                        UnityEngine.Object _ => ConfigureField<ObjectField, UnityEngine.Object>(Field as ObjectField, () => new ObjectField(label: Property.DisplayName())
                        {
                            objectType = propertyInfo.FieldType,
                            allowSceneObjects = true
                        }),
                        _ => throw new NotImplementedException($"{propertyInfo.FieldType} not yet implemented in {nameof(BindablePropertyField)}.{nameof(BindToBuiltInField)}().")
                    };
            }
            Field = field;
            if (!isAdded)
                this.Add(Field);
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
                    if (this.dataSource is BindableDataSource<TValue> bindableSource)
                        if (RuntimeHelpers.Equals(bindableSource.Value, ev.newValue) && !RuntimeHelpers.Equals(ev.previousValue, ev.newValue))
                            bindableSource.NotifyValueChanged();
                }));
                this.dataSource = new BindableDataSource<TValue>(Property);
            }
            field.SetBinding(nameof(BaseField<TValue>.value), new DataBinding
            {
                dataSourcePath = new PropertyPath(nameof(BindableDataSource<TValue>.Value)),
                bindingMode = BindingMode.TwoWay
            });
            return field;
        }

        private VisualElement ConfigureListView<TValue, TItemValue>(
            Func<VisualElement> itemFactory,
            ListView listView,
            Func<ListView> factory = null)
            where TValue : IList, IList<TItemValue>
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
                element.dataSource = new BindableDataSource<TItemValue>(Property, i);
                element.SetBinding(nameof(BaseField<TItemValue>.value), new DataBinding
                {
                    dataSourcePath = new PropertyPath(nameof(BindableDataSource<TItemValue>.Value)),
                    bindingMode = BindingMode.TwoWay
                });
                if (element is BaseField<TItemValue> baseField)
                    baseField.label = "Element " + i;
            };
            listView.onAdd = list =>
            {
                List<TItemValue> newList = new List<TItemValue>(list.itemsSource.Cast<TItemValue>());
                newList.Add(default);
                ((BindableDataSource<TValue>)this.dataSource).Value = list.itemsSource is Array
                    ? (TValue)((IList<TItemValue>) newList.ToArray())
                    : (TValue)((IList<TItemValue>) newList.ToList());
                list.itemsSource = ((BindableDataSource<TValue>)this.dataSource).Value;
            };
            listView.onRemove = list =>
            {
                List<TItemValue> newList = new List<TItemValue>(list.itemsSource.Cast<TItemValue>());
                var indices = new List<int>(list.selectedIndices);
                if (indices.Count == 0)
                    indices.Add(newList.Count - 1);
                newList = newList.Where((item, i) => !indices.Contains(i)).ToList();
                ((BindableDataSource<TValue>)this.dataSource).Value = list.itemsSource is Array
                    ? (TValue)((IList<TItemValue>) newList.ToArray())
                    : (TValue)((IList<TItemValue>) newList.ToList());
                list.itemsSource = ((BindableDataSource<TValue>)this.dataSource).Value;
            };
            this.dataSource = new BindableDataSource<TValue>(Property);
            var str = listViewNamePrefix + Property.PropertyPath;
            listView.headerTitle = Property.DisplayName();
            listView.viewDataKey = str;
            listView.name = str;
            listView.itemsSource = ((BindableDataSource<TValue>)this.dataSource).Value;
            
            return (VisualElement) listView;
        }
    }
}
