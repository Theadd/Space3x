using System;
using System.Reflection;

namespace Space3x.UiToolkit.SlicedText
{
    internal static partial class R
    {
        public static readonly BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;
        public static readonly BindingFlags NonPublicInstance = BindingFlags.NonPublic | BindingFlags.Instance;
        public static readonly BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static;
        public static readonly BindingFlags NonPublicStatic = BindingFlags.NonPublic | BindingFlags.Static;

        /// <summary>
        ///     Searches for the specified method whose parameters match the specified argument types.
        /// </summary>
        /// <param name="methodName">A string containing the name of the method to get.</param>
        /// <param name="bindingFlags">A bitmask comprised of one or more BindingFlags.</param>
        /// <param name="parameterTypes">
        ///     (Optional) An array of method's parameter types, i.e.: `new Type[] { typeof(string), typeof(int) };`
        ///     for a method with two parameters, string and int.
        /// </param>
        /// <typeparam name="T">The Type that contains the method.</typeparam>
        /// <returns>
        ///     A MethodInfo object representing the method that matches the specified requirements, if found. Otherwise,
        ///     **null**.
        /// </returns>
        public static MethodInfo GetMethod<T>(string methodName, BindingFlags bindingFlags,
            Type[] parameterTypes = null)
        {
            return typeof(T).GetMethod(methodName, bindingFlags, null, parameterTypes ?? new Type[] { }, null);
        }

        public static FieldInfo GetField<T>(string fieldName, BindingFlags bindingFlags)
        {
            return typeof(T).GetField(fieldName, bindingFlags);
        }
    }
}
