using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Space3x.InspectorAttributes.Editor.Drawers;
using Space3x.InspectorAttributes.Editor.Drawers.NonSerialized;
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
        public static readonly string listViewBoundFieldProperty = "unity-list-view-property-field-bound";
        public VisualElement Field;
        public IProperty Property;
        public VisualElement DecoratorDrawersContainer => m_DecoratorDrawersContainer ??= CreateDecoratorDrawersContainer();
        private VisualElement m_DecoratorDrawersContainer;
        private PropertyAttributeController m_Controller;
        private Type m_PropertyDrawerOnCollectionItems;
        private PropertyAttribute m_PropertyDrawerOnCollectionItemsAttribute;
        private FieldInfo m_RuntimeField;   // Only set and used for creating instances of property drawers on collection items.

        public BindablePropertyField()
        {
            if (DecoratorDrawersContainer != null)
            {
                this.WithClasses(ussClassName, "unity-property-field");
            }
        }

        private VisualElement CreateDecoratorDrawersContainer()
        {
            var element = new VisualElement();
            element.WithClasses(decoratorDrawersContainerClassName);
            this.Insert(0, element);
            return element;
        }

        public void BindProperty(IProperty property)
        {
            if (!(property is NonSerializedPropertyNode nonSerializedPropertyNode))
                throw new ArgumentException("Invalid IProperty.");
            nonSerializedPropertyNode.Field = this;
            Property = nonSerializedPropertyNode;
            m_Controller = PropertyAttributeController.GetInstance(Property);
            if (m_Controller == null)
                throw new ArgumentException("Unexpected, value for PropertyAttributeController is null.");
            var vType = m_Controller.AnnotatedType.GetValue(Property.Name);
            if (vType == null)
                throw new ArgumentException("Unexpected value.");

            PropertyDrawer drawer = null;
            var propertyDrawer = vType.PropertyDrawer;
            if (propertyDrawer != null && typeof(IDrawer).IsAssignableFrom(propertyDrawer))
            {
                try
                {
                    drawer = (PropertyDrawer)Activator.CreateInstance(propertyDrawer);
                    drawer.SetAttribute(vType.PropertyDrawerAttribute);
                    drawer.SetFieldInfo(vType.RuntimeField);
                    drawer.SetPreferredLabel(Property.DisplayName());
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
            
            if (drawer == null)
            {
                BindToBuiltInField();
            }
            else
            {
                var customDrawer = drawer as ICreateDrawerOnPropertyNode;
                var field = customDrawer.CreatePropertyNodeGUI(Property);
                if (Field != null && Field != field)
                    Field.RemoveFromHierarchy();
                Field = field;
                Add(Field);
            }

            if (Field != null)
                Field.tooltip = vType.Tooltip;
        }

        public void AttachDecoratorDrawers()
        {
            DecoratorDrawersContainer.Clear();
            var vType = m_Controller.AnnotatedType.GetValue(Property.Name);
            if (vType == null)
                throw new ArgumentException("Unexpected value.");
            for (var i = 0; i < vType.DecoratorDrawers.Count; i++)
            {
                DecoratorDrawer drawer = null;
                var decorator = vType.DecoratorDrawers[i];
                if (decorator != null)
                {
                    try
                    {
                        drawer = (DecoratorDrawer)Activator.CreateInstance(decorator);
                        drawer.SetAttribute(vType.PropertyAttributes[i]);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        drawer = null;
                    }
                    if (drawer != null)
                    {
                        var decoratorElement = drawer.CreatePropertyGUI();
                        if (decoratorElement == null)
                        {
                            Debug.LogError("Custom DecoratorDrawer is not compatible with UI Toolkit. " + drawer.GetType().Name);
                            continue;
                        }

                        DecoratorDrawersContainer.Add(decoratorElement);
                    }
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
                        // TODO: CreatePropertyNodeGUI on Property item instead of the Property array itself.
                        return ((ICreateDrawerOnPropertyNode)drawer).CreatePropertyNodeGUI(Property);
                    };
                }

                field = list switch
                {
                    int[] _ => ConfigureListView<int[], VisualElement, int>
                        (Field as ListView, () => new ListView(), itemFactory ?? (() => new IntegerField())),
                    _ => null
                };
            }
            else 
            {
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
                    // int[] _ => ConfigureListView<int[], IntegerField, int>(Field as ListView, () => new ListView(), () => new IntegerField()),
                    _ => throw new NotImplementedException($"{propertyInfo.FieldType} not yet implemented in {nameof(BindablePropertyField)}.{nameof(BindToBuiltInField)}().")
                };
            }
            Field = field;
            if (!isAdded)
                this.Add(Field);
        }
        
        private TField ConfigureField<TField, TValue>(
            TField field,
            // SerializedProperty property,
            Func<TField> factory)
            where TField : BaseField<TValue>
        {
            if ((object) field == null)
            {
                field = (TField) factory().WithClasses(BaseField<TValue>.alignedFieldUssClassName);
                field.RegisterValueChangedCallback<TValue>((evt => 
                    this.OnFieldValueChanged((EventBase) evt)));
                this.dataSource = new BindableDataSource<TValue>(m_Controller.DeclaringObject, Property.Name);
            }
            // string str = this.label ?? property.localizedDisplayName;
            // field.bindingPath = property.propertyPath;
            // field.SetProperty(BaseField<TValue>.serializedPropertyCopyName, (object) property.Copy());
            // field.name = "unity-input-" + property.propertyPath;
            // field.label = str;
            field.SetBinding(nameof(BaseField<TValue>.value), new DataBinding
            {
                dataSourcePath = new PropertyPath(nameof(BindableDataSource<TValue>.Value)),
                bindingMode = BindingMode.TwoWay
            });
            // PropertyField.ConfigureFieldStyles<TField, TValue>(field);
            return field;
        }

        private void OnFieldValueChanged(EventBase evt)
        {
            Debug.Log("  IN OnFieldValueChanged(ev);");
        }
        
        private VisualElement ConfigureListView<TValue, TItemField, TItemValue>(
            ListView listView,
            Func<ListView> factory,
            Func<TItemField> itemFactory)
            where TValue : IList, IList<TItemValue>
            where TItemField : VisualElement
            // where TItemField : BaseField<TItemValue>
        {
            if (listView == null)
            {
                listView = factory();
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
                if (element is TItemField itemField)
                {
                    itemField.dataSource = new BindableArrayDataSource<TValue, TItemValue>(
                        m_Controller.DeclaringObject, Property.Name, i);
                    itemField.SetBinding(nameof(BaseField<TItemValue>.value), new DataBinding
                    {
                        dataSourcePath = new PropertyPath(nameof(BindableArrayDataSource<TValue, TItemValue>.Value)),
                        bindingMode = BindingMode.TwoWay
                    });
                    if (element is BaseField<TItemValue> baseField)
                    {
                        baseField.label = "Element " + i;
                    }
                }
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
            this.dataSource = new BindableDataSource<TValue>(m_Controller.DeclaringObject, Property.Name);
            var str = listViewNamePrefix + Property.PropertyPath;
            listView.headerTitle = Property.DisplayName();
            // listView.userData = (object) serializedProperty;
            // listView.bindingPath = property.propertyPath;
            listView.viewDataKey = str;
            listView.name = str;
            listView.itemsSource = ((BindableDataSource<TValue>)this.dataSource).Value;
            // listView.SetProperty((PropertyName) listViewBoundFieldProperty, (object) this);
            // listView.SetBinding(nameof(BaseField<TValue>.value), new DataBinding
            // {
            //     dataSourcePath = new PropertyPath(nameof(BindableDataSource<TValue>.Value)),
            //     bindingMode = BindingMode.TwoWay
            // });
            return (VisualElement) listView;
        }
    }
}
