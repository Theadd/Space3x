using System;
using System.Collections.Generic;
using System.Reflection;
using Space3x.Attributes.Types;
using UnityEditor;
using UnityEngine;

namespace Space3x.InspectorAttributes.Editor
{
    public class VTypeMember : IPropertyNodeWithFlags
    {
        public FieldInfo RuntimeField;
        public MethodInfo RuntimeMethod;
        public Type FieldType;
        public List<PropertyAttribute> PropertyAttributes;
        
        public IPropertyNodeWithFlags Node => this;
        
        VTypeFlags IPropertyFlags.Flags => Flags;
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

        private string GetTooltipText()
        {
            foreach (var propertyAttribute in PropertyAttributes)
                if (propertyAttribute is TooltipAttribute attr)
                    return attr.tooltip ?? "";

            return "";
        }
        
        private bool Rebuild()
        {
            var isInvokableNode = RuntimeMethod != null;
            var isArray = !isInvokableNode && IsArrayOrList(FieldType);
            m_DecoratorDrawers = new List<Type>(PropertyAttributes.Count);
            m_PropertyDrawerOnCollectionItems = null;
            for (var i = 0; i < PropertyAttributes.Count; i++)
            {
                var attr = PropertyAttributes[i];
                // TODO: ContextMenuItemAttribute, @see: PropertyHandler.HandleAttribute
                var drawer = attr switch
                {
                    TooltipAttribute _ => null,
                    NonReorderableAttribute _ => null,
                    ShowInInspectorAttribute _ => null,
                    _ => CachedDrawers.GetCustomDrawer(attr.GetType())
                };
                if (drawer == null)
                {
                    m_DecoratorDrawers.Add(null);
                    continue;
                }
                if (typeof(DecoratorDrawer).IsAssignableFrom(drawer))
                    m_DecoratorDrawers.Add(drawer);
                else if (typeof(PropertyDrawer).IsAssignableFrom(drawer))
                {
                    m_DecoratorDrawers.Add(null);
                    if (isArray && !attr.applyToCollection)
                    {
                        if (m_PropertyDrawerOnCollectionItems == null)
                        {
                            m_PropertyDrawerOnCollectionItems = drawer;
                            PropertyDrawerOnCollectionItemsAttribute = attr;
                        }
                        continue;
                    }
                    if (m_PropertyDrawer == null)
                    {
                        m_PropertyDrawer = drawer;
                        PropertyDrawerAttribute = attr;
                    }
                }
                else
                    m_DecoratorDrawers.Add(null);
            }
            if (!isInvokableNode)
                m_PropertyDrawer ??= CachedDrawers.GetCustomDrawer(FieldType);

            return true;
        }
        
        private static bool IsArrayOrList(Type listType) => 
            listType.IsArray || listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof (List<>);

        public bool Equals(IPropertyNode other) => ReferenceEquals(this, other);
        
        public static VTypeMember CreateUnreliableCopy(VTypeMember other)
        {
            return new VTypeMember()
            {
                Name = other.Name,
                Flags = other.Flags.ToUnreliable(),
                ParentPath = other.ParentPath,
                m_DecoratorDrawers = other.m_DecoratorDrawers,
                m_PropertyDrawer = other.m_PropertyDrawer,
                m_PropertyDrawerOnCollectionItems = other.m_PropertyDrawerOnCollectionItems,
                m_Tooltip = other.m_Tooltip,
                RuntimeField = other.RuntimeField,
                RuntimeMethod = other.RuntimeMethod,
                FieldType = other.FieldType,
                PropertyAttributes = other.PropertyAttributes,
                PropertyDrawerAttribute = other.PropertyDrawerAttribute,
                PropertyDrawerOnCollectionItemsAttribute = other.PropertyDrawerOnCollectionItemsAttribute
            };
        }
    }
}
