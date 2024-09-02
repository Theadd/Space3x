using System;
using UnityEngine;

namespace Space3x.InspectorAttributes.Types
{
    [Serializable]
    public class SerializableType : IEquatable<SerializableType>
    {
        [SerializeField]
        private string m_TypeName;

        [NonSerialized]
        private Type m_Value;
        
        public string TypeName => m_TypeName ?? string.Empty;
        
        public Type Value
        {
            get => TypeName == string.Empty ? null : m_Value ??= Parse(m_TypeName);
            // set => m_TypeName = Stringify(m_Value = value);
            set => m_TypeName = (m_Value = value)?.FullTypeName() ?? string.Empty;
        }

        public SerializableType()
        {
            m_TypeName = string.Empty;
            m_Value = null;
        }

        public SerializableType(Type type)
        {
            m_TypeName = Stringify(type);
            m_Value = type;
        }
        
        public SerializableType(string typeName)
        {
            m_TypeName = typeName;
            m_Value = Value;
        }
        
        public bool Equals(SerializableType other) => TypeName != string.Empty && string.Equals(m_TypeName, other?.TypeName ?? string.Empty);
        
        public bool Equals(Type other) => TypeName != string.Empty && string.Equals(m_TypeName, Stringify(other));

        public override bool Equals(object obj) =>
            !ReferenceEquals(null, obj) && (obj is SerializableType th
                ? Equals(th)
                : obj is Type t && Stringify(t) == TypeName);

        public override int GetHashCode() => TypeName.GetHashCode();

        public override string ToString() => Value?.FullTypeName();
        
        /* Static methods & operators */
        private static Type Parse(string typeName) => TypeUtilityExtensions.GetType(typeName);

        private static string Stringify(Type type) => type?.FullTypeName() ?? string.Empty;

        //public static implicit operator Type(SerializableType sType) => sType.Value;
        
        public static explicit operator SerializableType(string typeName) => new SerializableType(typeName);
        
        public static explicit operator SerializableType(Type type) => new(type);

        public static explicit operator Type(SerializableType sType) => sType.Value;

        public static bool operator ==(SerializableType left, SerializableType right) => left?.Equals(right) ?? false;

        public static bool operator !=(SerializableType left, SerializableType right) => !(left?.Equals(right) ?? false);
    }
}
