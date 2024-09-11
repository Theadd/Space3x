using System;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine.UIElements;

namespace Space3x.Properties.Types
{
    public static partial class Accessors
    {
        [Obsolete]
        internal static Delegate CreateDelegateSetter(PropertyInfo propertyInfo)
        {
            ParameterExpression instance = Expression.Parameter(propertyInfo.ReflectedType, "instance");
            ParameterExpression propertyValue = Expression.Parameter(propertyInfo.PropertyType, "propertyValue");
            var body = Expression.Assign(Expression.Property(instance, propertyInfo.Name), propertyValue);
            return Expression.Lambda(body, instance, propertyValue).Compile();
        }

        // HAVING:
        // public class MyClass<T>
        // {
        //      public T MyProperty { get; set; }
        // }
        // var action = CreateSetter(propertyInfo);
        // action.Invoke(myClass, 10);

        public static IAccessor GetMember(this Accessor self, string memberName) => self.GetMember(memberName, Accessor.DefaultFlags);
    }
    
    public class FieldAccessor : IAccessor
    {
        public Accessor TypeAccessor { get; internal set; }
        public FieldInfo Info;

        public object Value
        {
            get => Info.GetValue(null);
            set => Info.SetValue(null, value);
        }

        public T GetValue<T>(object instance) => (T)Info.GetValue(instance);
        public void SetValue<T>(object instance, T value) => Info.SetValue(instance, value);

        internal FieldAccessor(FieldInfo info)
        {
            Info = info;
        }
    }

    public class PropertyAccessor : IAccessor
    {
        public Accessor TypeAccessor { get; internal set; }
        public PropertyInfo Info;
        // protected Func<object, object> Getter;
        // protected Action<object, object> Setter;

        public object Value
        {
            get => Info.GetValue(null);
            set => Info.SetValue(null, value);
        }

        public T GetValue<T>(object instance) => (T)Info.GetValue(instance);
        public void SetValue<T>(object instance, T value) => Info.SetValue(instance, value);

        internal PropertyAccessor(PropertyInfo info)
        {
            Info = info;
            // Getter = Accessors.Getter()
        }
    }
    












    // public class AccessorsAPI
    // {
    //     /*
    //      *  public class VisualElement : ...
    //      *  {
    //      *      private static uint s_NextId;
    //      *      private VisualElement m_PhysicalParent;
    //      *      private VisualElement m_LogicalParent;
    //      */
    //     
    //     public void BasicUsage()
    //     {
    //         // Get an field Accessor on the VisualElement type using plain reflection-
    //         var physicalParent = Accessor.Create<VisualElement>()["m_PhysicalParent"];
    //         // It could be declared as static, leveraging reusability, since it doesn't reference
    //         // any VisualElement instance.
    //         
    //         // We named it physicalParent since it's a direct accessor to the m_PhysicalParent private field
    //         // **BUT**, it can also be used for all other VisualElement Type's members just by naming them.
    //         var NextId = (uint)physicalParent
    //     }
    // }
    
    
    
    
    
    
    
    
    
    
    
    
    
    
}