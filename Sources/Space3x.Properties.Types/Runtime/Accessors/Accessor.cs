using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Space3x.Properties.Types
{
    public interface IAccessor
    {
        public Accessor TypeAccessor { get; }

        public object Value { get; set; }
        
        public abstract T GetValue<T>(object instance);
        public object GetValue(object instance) => GetValue<object>(instance);
        public abstract void SetValue<T>(object instance, T value);
        public void SetValue(object instance, object value) => SetValue<object>(instance, value);
        
        public object this[string memberName]
        {
            get => TypeAccessor[memberName];
            set => TypeAccessor[memberName] = value;
        }
    }

    public class Accessor
    {
        /// <summary>
        /// Notice it's an static non-readonly field, therefore shared on all Accessor types.
        /// </summary>
        [PublicAPI]
        public static BindingFlags DefaultFlags = BindingFlags.Instance | BindingFlags.Public | 
                                                 BindingFlags.Static | BindingFlags.NonPublic;

        public readonly Dictionary<string, IAccessor> Members = new Dictionary<string, IAccessor>();
        public readonly TypeInfo Info;
        
        protected Accessor(Type target) => Info = target.GetTypeInfo();

        public object this[string memberName]
        {
            get => Members.TryGetValue(memberName, out IAccessor instance)
                ? instance.Value
                : GetMember(memberName, DefaultFlags)?.Value;
            set
            {
                if (!Members.TryGetValue(memberName, out IAccessor instance))
                    instance = GetMember(memberName, DefaultFlags);
                instance?.SetValue(null, value);
            }
        }
        
        public IAccessor GetMember(string memberName, BindingFlags bindingFlags)
        {
            var member = Info.GetMember(memberName, DefaultFlags).FirstOrDefault()
                         ?? Info.GetMember($"<{memberName}>k__BackingField", DefaultFlags).FirstOrDefault();
            if (member == null) return null;
            IAccessor instance = member switch
            {
                FieldInfo field => new FieldAccessor(field) { TypeAccessor = this },
                PropertyInfo property => new PropertyAccessor(property) { TypeAccessor = this },
                _ => null
            };
            if (instance != null)
                Members[memberName] = instance;
            
            return instance;
        }
        
        public static Accessor Create<TType>() => new Accessor(typeof(TType));
    }
    
    
    
    
    public class ObjectAccessor
    {
        public readonly Accessor TypeAccessor;
        public readonly object Target;
        
        internal ObjectAccessor(Accessor typeAccessor, object target)
        {
            TypeAccessor = typeAccessor;
            Target = target;
        }

        public object this[string memberName]
        {
            get => TypeAccessor.Members.TryGetValue(memberName, out IAccessor instance)
                ? instance.GetValue(Target)
                : TypeAccessor.GetMember(memberName)?.GetValue(Target);
            set
            {
                if (!TypeAccessor.Members.TryGetValue(memberName, out IAccessor instance))
                    instance = TypeAccessor.GetMember(memberName);
                instance?.SetValue(Target, value);
            }
        }
    }
}
