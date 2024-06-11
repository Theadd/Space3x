﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Space3x.Attributes.Types;
using UnityEditor;
using UnityEngine;

namespace Space3x.InspectorAttributes.Editor.Drawers.NonSerialized
{
    [Flags]
    public enum VTypeFlags
    {
        None = 0,
        HideInInspector = 1,
        ShowInInspector = 2,
        Serializable = 4,
        NonReorderable = 8,
    }
    
    public class VTypeMember : IProperty, IPropertyFlags
    {
        // public string Name;
        public FieldInfo RuntimeField;
        // public PropertyInfo RuntimeProperty;
        // public MethodInfo PropertyGetter;
        // public MethodInfo PropertySetter;
        public Type FieldType;
        // public List<CustomAttributeData> CustomAttributes;
        public List<PropertyAttribute> PropertyAttributes;
        
        public VTypeFlags Flags { get; set; }
        public string Name { get; set; }
        public string PropertyPath => string.IsNullOrEmpty(ParentPath) || string.IsNullOrEmpty(Name)
            ? (ParentPath ?? "") + (Name ?? "")
            : ParentPath + "." + Name;
        public string ParentPath { get; set; }

        private List<Type> m_DecoratorDrawers;
        private Type m_PropertyDrawer;
        private Type m_PropertyDrawerOnCollectionItems;
        private string m_Tooltip = null;

        public List<Type> DecoratorDrawers => m_DecoratorDrawers ?? (Rebuild() ? m_DecoratorDrawers : null);
        public Type PropertyDrawer => m_DecoratorDrawers == null ? (Rebuild() ? m_PropertyDrawer : null) : m_PropertyDrawer;
        public Type PropertyDrawerOnCollectionItems => m_DecoratorDrawers == null ? (Rebuild() ? m_PropertyDrawerOnCollectionItems : null) : m_PropertyDrawerOnCollectionItems;
        public PropertyAttribute PropertyDrawerAttribute { get; private set; } = null;
        public PropertyAttribute PropertyDrawerOnCollectionItemsAttribute { get; private set; } = null;
        public string Tooltip => m_Tooltip ??= GetTooltipText();

        public bool IsHidden => (Flags | VTypeFlags.HideInInspector) == Flags ||
                                !((Flags | VTypeFlags.Serializable) == Flags ||
                                  (Flags | VTypeFlags.ShowInInspector) == Flags);
        public bool IsSerializable => (Flags | VTypeFlags.Serializable) == Flags;
        
        public bool IsNonReorderable => (Flags | VTypeFlags.NonReorderable) == Flags;

        private string GetTooltipText()
        {
            foreach (var propertyAttribute in PropertyAttributes)
                if (propertyAttribute is TooltipAttribute attr)
                    return attr.tooltip ?? "";

            return "";
        }
        
        private bool Rebuild()
        {
            var isArray = IsArrayOrList(FieldType);
            m_DecoratorDrawers = new List<Type>(PropertyAttributes.Count);
            m_PropertyDrawerOnCollectionItems = null;
            for (var i = 0; i < PropertyAttributes.Count; i++)
            {
                var attr = PropertyAttributes[i];
                // var drawer = CachedDrawers.GetCustomDrawer(attr.GetType());
                // TODO: ContextMenuItemAttribute, @see: PropertyHandler.HandleAttribute
                var drawer = attr switch
                {
                    TooltipAttribute _ => null,
                    // HideInInspector _ => null,
                    NonReorderableAttribute _ => null,
                    ShowInInspectorAttribute _ => null,
                    _ => CachedDrawers.GetCustomDrawer(attr.GetType())
                };
                if (drawer == null)
                {
                    Debug.LogWarning($"CachedDrawers.GetCustomDrawer({attr.GetType().Name}) is null in {RuntimeField?.Name}.");
                    m_DecoratorDrawers.Add(null);
                    continue;
                }
                // if (typeof (DecoratorDrawer).IsAssignableFrom(forPropertyAndType) && (!(field != (FieldInfo) null) || !field.FieldType.IsArrayOrList() || propertyType.IsArrayOrList()))
                if (typeof(DecoratorDrawer).IsAssignableFrom(drawer)) /* && (
                        !(RuntimeField != null)
                        || !isArray 
                        || (isArray && attr.applyToCollection)*/
                        /* || propertyType.IsArrayOrList()) */
                {
                    m_DecoratorDrawers.Add(drawer);
                }
                // if (typeof (PropertyDrawer).IsAssignableFrom(forPropertyAndType))
                else if (typeof(PropertyDrawer).IsAssignableFrom(drawer))
                {
                    m_DecoratorDrawers.Add(null);
                    // if (propertyType != (System.Type) null && propertyType.IsArrayOrList() && !attribute.applyToCollection)
                    //     return;
                    if (isArray && !attr.applyToCollection)
                    {
                        m_PropertyDrawerOnCollectionItems = drawer;
                        PropertyDrawerOnCollectionItemsAttribute = attr;
                        continue;
                    }
                    m_PropertyDrawer = drawer;
                    PropertyDrawerAttribute = attr;
                }
                else
                {
                    m_DecoratorDrawers.Add(null);
                }
            }

            m_PropertyDrawer ??= CachedDrawers.GetCustomDrawer(FieldType);

            return true;
        }
        
        private static bool IsArrayOrList(Type listType) => 
            listType.IsArray || listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof (List<>);
    }
}