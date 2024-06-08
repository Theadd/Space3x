using System;
using System.Collections.Generic;
using System.Reflection;
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
    }
    
    public class VTypeMember : IProperty, IPropertyFlags
    {
        // public string Name;
        public FieldInfo RuntimeField;
        public PropertyInfo RuntimeProperty;
        public MethodInfo PropertyGetter;
        public MethodInfo PropertySetter;
        public Type FieldType;
        public List<CustomAttributeData> CustomAttributes;
        public List<PropertyAttribute> PropertyAttributes;
        
        public VTypeFlags Flags { get; set; }
        public string Name { get; set; }
        public string PropertyPath => string.IsNullOrEmpty(ParentPath) || string.IsNullOrEmpty(Name)
            ? (ParentPath ?? "") + (Name ?? "")
            : ParentPath + "." + Name;
        public string ParentPath { get; set; }
        
        private List<Type> m_DecoratorDrawers;
        private Type m_PropertyDrawer;

        public List<Type> DecoratorDrawers => m_DecoratorDrawers ?? (Rebuild() ? m_DecoratorDrawers : null);
        public Type PropertyDrawer => m_DecoratorDrawers == null ? (Rebuild() ? m_PropertyDrawer : null) : m_PropertyDrawer;

        public bool IsHidden => (Flags | VTypeFlags.HideInInspector) == Flags ||
                                !((Flags | VTypeFlags.Serializable) == Flags ||
                                  (Flags | VTypeFlags.ShowInInspector) == Flags);
        public bool IsSerializable => (Flags | VTypeFlags.Serializable) == Flags;

        private bool Rebuild()
        {
            // PropertyHandlerCache phc;
            
            m_DecoratorDrawers = new List<Type>(PropertyAttributes.Count);
            for (var i = 0; i < PropertyAttributes.Count; i++)
            {
                var drawer = CachedDrawers.GetCustomDrawer(PropertyAttributes[i].GetType());
                if (drawer == null)
                {
                    Debug.LogWarning($"CachedDrawers.GetCustomDrawer({PropertyAttributes[i].GetType().Name}) is null.");
                    m_DecoratorDrawers.Add(null);
                    continue;
                }
                if (typeof(DecoratorDrawer).IsAssignableFrom(drawer) && (
                        !(RuntimeField != null)
                        || !IsArrayOrList(FieldType)
                        /* || propertyType.IsArrayOrList() */))
                {
                    m_DecoratorDrawers.Add(drawer);
                }
                else if (typeof(PropertyDrawer).IsAssignableFrom(drawer))
                {
                    m_DecoratorDrawers.Add(null);
                    if (IsArrayOrList(FieldType) && !PropertyAttributes[i].applyToCollection)   // TODO: Not really accurate.
                        continue;
                    m_PropertyDrawer = drawer;
                }
            }

            m_PropertyDrawer ??= CachedDrawers.GetCustomDrawer(FieldType);

            return true;
        }
        
        private static bool IsArrayOrList(Type listType) => 
            listType.IsArray || listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof (List<>);
    }
}