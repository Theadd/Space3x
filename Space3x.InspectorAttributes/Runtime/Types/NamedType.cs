using System;
using Space3x.InspectorAttributes.Utilities;
using UnityEngine;

namespace Space3x.InspectorAttributes.Types
{
    [Serializable]
    public struct NamedType : IEquatable<NamedType>
    {
        [SerializeField]
        public string TypeName;

        [NonSerialized]
        private Type m_Value;
        
        public Type Value
        {
            get => TypeName == string.Empty ? null : m_Value ??= Parse(TypeName);
            set => TypeName = (m_Value = value)?.FullTypeName() ?? string.Empty;
        }

        public NamedType(Type type)
        {
            TypeName = Stringify(type);
            m_Value = type;
        }
        
        public NamedType(string typeName = "") : this()
        {
            TypeName = string.IsNullOrEmpty(typeName) ? string.Empty : typeName;
            m_Value = null;
        }
        
        public bool Equals(NamedType other) => TypeName == other.TypeName;
        public bool Equals(ref NamedType other) => TypeName != string.Empty && TypeName == other.TypeName;
        
        public bool Equals(Type other) => TypeName != string.Empty && TypeName == Stringify(other);

        public override bool Equals(object obj) =>
            obj is NamedType th
                ? Equals(ref th)
                : obj is Type t && Stringify(t) == TypeName;

        public override int GetHashCode() => TypeName.GetHashCode();

        public override string ToString() => Value?.FullTypeName();
        
        /* Static methods & operators */
        private static Type Parse(string typeName) => TypeUtilityExtensions.GetType(typeName);

        private static string Stringify(Type type) => type?.FullTypeName() ?? string.Empty;
        
        public static explicit operator NamedType(string typeName) => new(typeName);
        
        public static explicit operator NamedType(Type type) => new(type);

        public static explicit operator Type(NamedType sType) => sType.Value;

        public static bool operator ==(NamedType left, NamedType right) => left.Equals(ref right);

        public static bool operator !=(NamedType left, NamedType right) => !left.Equals(ref right);
    }
}
