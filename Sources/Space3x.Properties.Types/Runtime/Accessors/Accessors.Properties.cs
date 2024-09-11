using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Space3x.Properties.Types
{
    public static partial class Accessors
    {
        /// <summary>
        /// Returns a compiled lambda of a property get accessor using generics.
        /// </summary>
        public static Func<TObject, TProperty> Getter<TObject, TProperty>(string propertyName)
        {
            ParameterExpression paramExpression = Expression.Parameter(typeof(TObject), "value");
            Expression propertyGetterExpression = Expression.Property(paramExpression, propertyName);
            return Expression.Lambda<Func<TObject, TProperty>>
                    (propertyGetterExpression, paramExpression)
                    .Compile();
        }

        /// <summary>
        /// Returns a compiled lambda of a property set accessor using generics.
        /// </summary>
        public static Action<TObject, TProperty> Setter<TObject, TProperty>(string propertyName)
        {
            ParameterExpression paramExpression = Expression.Parameter(typeof(TObject));
            ParameterExpression paramExpression2 = Expression.Parameter(typeof(TProperty), propertyName);
            MemberExpression propertyGetterExpression = Expression.Property(paramExpression, propertyName);
            return Expression.Lambda<Action<TObject, TProperty>>
                (Expression.Assign(propertyGetterExpression, paramExpression2), paramExpression, paramExpression2)
                .Compile();
        }
        
        /// <summary>
        /// Returns a compiled lambda of a non-generic property get accessor.
        /// </summary>
        public static Func<object, object> Getter(Type type, PropertyInfo property)
        {
            var propertyName = property.Name;
            var parmExpression = Expression.Parameter(typeof(object), "it");
            var castExpression = Expression.Convert(parmExpression, type);
            var propertyExpression = Expression.Convert(Expression.Property(castExpression, propertyName), typeof(object));
            var lambdaExpression = Expression.Lambda(typeof(Func<,>).MakeGenericType(typeof(object), typeof(object)), propertyExpression, parmExpression);
            return lambdaExpression.Compile() as Func<object, object>;
        }
        
        /// <summary>
        /// Returns a compiled lambda of a non-generic property set accessor.
        /// </summary>
        public static Action<object, object> Setter(PropertyInfo propertyInfo)
        {
            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            UnaryExpression instanceExpression = Expression.Convert(instance, propertyInfo.DeclaringType);
            ParameterExpression propertyValue = Expression.Parameter(typeof(object), "propertyValue");
            UnaryExpression propertyValueExpression = Expression.Convert(propertyValue, propertyInfo.PropertyType);
            var body = Expression.Assign(Expression.Property(instanceExpression, propertyInfo.Name), propertyValueExpression);
            return Expression.Lambda<Action<object, object>>(body, instance, propertyValue).Compile();
        }
    }
}
