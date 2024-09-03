using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor;
using Space3x.Properties.Types;
using Space3x.UiToolkit.Types;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
{
    public partial class BindablePropertyField
    {
        private VisualElement ConfigureListView<TItemValue>(
                Func<VisualElement> itemFactory,
                ListView listView,
                Func<ListView> factory = null)
            //where TValue : IList, IList<TItemValue>
        {
            DebugLog.Info($"[USK3] [lV!] [BindablePropertyField] ConfigureListView + IList({typeof(TItemValue).Name}): {Property.PropertyPath}");
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
                var propertyAtIndex = Property.GetArrayElementAtIndex(i);
                Debug.LogWarning("[PAC!] [lV!] bindItem " + i + " @ " + propertyAtIndex.PropertyPath + " -- <b><u>IS UNRELIABLE</u>: " + propertyAtIndex.IsUnreliable() + "</b>");
                if (((BindablePropertyField)element).Property?.Equals(propertyAtIndex) ?? false)
                {
                    // (RETURN STATEMENT COMMENTED) SO-NO-
                    Debug.LogWarning("[PAC!] [lV!] <b>SKIPPING</b>bindItem " + i + " @ " + propertyAtIndex.PropertyPath + " VALUE: " + propertyAtIndex.GetValue());
                    return;
                }
                DebugLog.Error($" [lV!]  =====>>>>  .BindProperty WITH CUSTOM DRAWERS: {propertyAtIndex.PropertyPath}; #{i}; {propertyAtIndex.GetValue()}");
                // Needs more testing in order to see in which cases an early return is preferred. Currently I've only
                // detected one drawer (IngredientDrawerUIE example from "Compatibility - Custom PropertyDrawers" docs page)
                // that benefits from such lousy workaround while moving items in a non-serialized list. Not even a
                // fully working fix but at least allows to move items around.
                if (m_HasCustomDrawerOnCollectionItems && ((BindablePropertyField)element).Property != null) return;
                
                ((BindablePropertyField)element).BindProperty(propertyAtIndex, true);
                // EDIT: added AttachDecoratorDrawers call
                ((BindablePropertyField)element).AttachDecoratorDrawers();
                element.EnableInClassList("ARRAY_ELEMENT_" + i, true);
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
                Debug.Log("OnAdd @ [lV!] " + Property.PropertyPath + " -- <b><u>IS UNRELIABLE</u>: " + Property.IsUnreliable() + "</b>");
                List<TItemValue> newList = new List<TItemValue>(list.itemsSource?.Cast<TItemValue>() ?? new TItemValue[] {});
                if (default(TItemValue) != null)
                    newList.Add(default);
                else
                {
                    UnityEngine.Object itemToClone = null;
                    try
                    {
                        if (typeof(ScriptableObject).IsAssignableFrom(typeof(TItemValue)))
                        {
                            newList.Add((TItemValue)(object)ScriptableObject.CreateInstance(typeof(TItemValue)));
                        }
                        else if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(TItemValue)) && newList.Count > 0)
                        {
                            itemToClone = (UnityEngine.Object)(object)newList.LastOrDefault(e => e != null);
                            newList.Add((TItemValue)(object)UnityEngine.Object.Instantiate(itemToClone));
                        }
                        else
                        {
                            newList.Add(Activator.CreateInstance<TItemValue>());
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
                ((DataSourceBinding)this.dataSource).Value = Property.IsArray()
                    ? newList.ToArray()
                    : newList.ToList();
                // Debug.LogWarning($"[NOTIC3] [onAdd] Callback ON {property.PropertyPath} => {ev.newValue?.ToString()}");
                // TODO: Uncomment. (KEEP COMMENTED)
                // list.itemsSource = (IList)((DataSourceBinding)this.dataSource).Value;
                // (BindingId) nameof (itemsSource);
            };
            listView.onRemove = list =>
            {
                Debug.Log("OnRemove @ [lV!] " + Property.PropertyPath + " -- <b><u>IS UNRELIABLE</u>: " + Property.IsUnreliable() + "</b>");
                List<TItemValue> newList = new List<TItemValue>(list.itemsSource.Cast<TItemValue>());
                var indices = new List<int>(list.selectedIndices);
                if (indices.Count == 0)
                    indices.Add(newList.Count - 1);
                newList = newList.Where((item, i) => !indices.Contains(i)).ToList();
                ((DataSourceBinding)this.dataSource).Value = Property.IsArray()
                    ? newList.ToArray()
                    : newList.ToList();
                // TODO: Uncomment. (KEEP COMMENTED)
                // list.itemsSource = (IList)((DataSourceBinding)this.dataSource).Value;
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
                if (!Equals(Property, node))
                {
                    DebugLog.Error($"  <b>[PAC!] [lV!] listView.TrackPropertyValue NOT EQUALS! {Property.PropertyPath} != {node.PropertyPath}</b>");
                    return;
                }
                Debug.LogWarning("[PAC!] [lV!] IN LISTVIEW TrackPropertyValue CALLBACK FOR " + Property.PropertyPath + " :: lvTHash: " + listView.GetHashCode());
                // listView.itemsSource = (IList)((DataSourceBinding)this.dataSource).Value;
                listView.itemsSource = (IList)node.GetValue();
            });
            
            listView.viewController.itemsSourceChanged += () =>
            {
                Debug.LogWarning("[PAC!] [lV!] itemsSourceChanged @ " + Property.PropertyPath + " :: lvTHash: " + listView.GetHashCode());
                Debug.LogWarning($"  _______ [lV!] <color=#0000FFFF>listView.itemsSource</color>: _________ ");
                IList list = (IList)listView.itemsSource;
                for (var i = 0; i < list.Count; i++)
                {
                    var o = list[i];
                    Debug.LogWarning($"    [lV!]  {i} => {o?.ToString()}");
                }
                Debug.LogWarning($"  _______ [lV!] <color=#0000FFFF>(IList)Property.GetValue()</color>: _________ ");
                IList listB = (IList)Property.GetValue();
                for (var i = 0; i < listB.Count; i++)
                {
                    var o = listB[i];
                    Debug.LogWarning($"    [lV!]  {i} => {o?.ToString()}");
                }
            };

            listView.viewController.itemIndexChanged += (srcIndex, dstIndex) =>
            {
                Debug.LogWarning("[PAC!] [lV!] itemIndexChanged @ " + Property.PropertyPath + " :: " + srcIndex + " => " + dstIndex + " :: lvTHash: " + listView.GetHashCode());
                IList list = (IList)Property.GetValue();
                for (var i = 0; i < list.Count; i++)
                {
                    var o = list[i];
                    Debug.LogWarning($"    [lV!]  {i} => {o?.ToString()}");
                }

                for (var i = Math.Min(srcIndex, dstIndex); i <= Math.Max(srcIndex, dstIndex); i++)
                {
                    DebugLog.Info($"<color=#FFFF00FF>// TODO: Uncomment - [lV!] .NotifyValueChanged(Property.GetArrayElementAtIndex(i));</color>");
                    // TODO: Uncomment
                    (Property as INonSerializedPropertyNode)?.NotifyValueChanged(Property.GetArrayElementAtIndex(i));
                }
                
                /* PROPAGATING CHANGES */
                DebugLog.Info($"<color=#FFFF00FF>/* PROPAGATING CHANGES */</color>");
                List<TItemValue> newList = new List<TItemValue>(listView.itemsSource.Cast<TItemValue>());
                ((DataSourceBinding)this.dataSource).Value = Property.IsArray()
                    ? newList.ToArray()
                    : newList.ToList();
                /* --- */
                
            };
            // BindableUtility.AutoNotifyValueChangedOnNonSerialized(itemElement, Property)
            Debug.LogWarning("[PAC!] [lV!] LISTVIEW CONFIGURED FOR " + Property.PropertyPath + " :: lvTHash: " + listView.GetHashCode());
            
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