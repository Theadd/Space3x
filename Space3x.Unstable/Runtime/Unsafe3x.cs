using System;
using System.Globalization;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;

namespace Space3x.Unstable
{
    // ReSharper disable once InconsistentNaming
    public static class Unsafe3x
    {
        public static readonly BindingFlags PublicStaticFlags = BindingFlags.Public 
                                                                | BindingFlags.NonPublic
                                                                | BindingFlags.Static 
                                                                | BindingFlags.CreateInstance 
                                                                | BindingFlags.DoNotWrapExceptions;

        /// <summary>
        /// The same as <see cref="Unsafe3x.As{T}(object)"/>.
        /// </summary>
        /// <param name="input"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T CastObject<T>(object input) => (T) input;

        /// <summary>
        /// Returns an object of the specified type and whose value is equivalent to the specified object.
        /// </summary>
        /// <param name="input">An object</param>
        /// <typeparam name="T">The specified type</typeparam>
        /// <returns></returns>
        public static T ConvertObject<T>(object input) => (T) Convert.ChangeType(input, typeof(T));

        /// <summary>
        /// Casts the input object to the specified type T, where T is a reference type.
        /// </summary>
        /// <param name="from">The object to be converted.</param>
        /// <typeparam name="T">The type to be converted to.</typeparam>
        /// <returns>The converted object.</returns>
        public static T As<T>(object from) where T : class => (T) from;
        
        /// <summary>
        /// Creates an instance of the specified target type using the provided value.
        /// </summary>
        /// <typeparam name="TIn">The type of the input value.</typeparam>
        /// <param name="targetType">The target type to create an instance of.</param>
        /// <param name="value">The value to use when creating the instance.</param>
        /// <returns>The created instance of the target type.</returns>
        public static object CreateWrapper<TIn>(Type targetType, TIn value)
        {
            var method = typeof(Unsafe3x).GetMethod(
                    "CreateInstance", 
                    BindingFlags.Static | BindingFlags.Public)!
                .MakeGenericMethod(typeof(TIn), targetType);
            return method.Invoke(null, PublicStaticFlags, null, new object[] { value }, CultureInfo.InvariantCulture);
        }
        
        /// <summary>
        /// Creates an instance of the specified target type using the provided value.
        /// </summary>
        /// <param name="from"></param>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <returns></returns>
        [UsedImplicitly]
        public static TOut CreateInstance<TIn, TOut>(object from) => (TOut)Activator.CreateInstance(typeof(TOut), (TIn) from);
    }
}
