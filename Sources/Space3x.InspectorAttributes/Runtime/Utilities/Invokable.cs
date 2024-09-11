using System.Reflection;

namespace Space3x.InspectorAttributes
{
    /// <summary>
    /// Using reflection, returns the result of a member invocation on an object.
    /// Where the member can be a method, a property or a field on that object.
    /// <seealso cref="Unity.VisualScripting.ReflectionInvoker"/>
    /// </summary>
    /// <typeparam name="TIn">The type of the parameter passed in to the callable member when invoking a method.</typeparam>
    /// <typeparam name="TOut">The type of the value returned from the callable member.</typeparam>
    public class Invokable<TIn, TOut>
    {
        /// <summary>
        /// The member to invoke, can be a method, a property or a field.
        /// </summary>
        public MemberInfo CallableMember { get; set; }
        
        /// <summary>
        /// The object to invoke the member on.
        /// </summary>
        public object TargetObject { get; set; }


        public object[] Parameters { get; set; } = null;

        /// <summary>
        /// Invokes the callable member on the target object.
        /// Silently falls back to Invoke() if the member is a method that does
        /// not take a single parameter. This also applies to any subsequent call.
        /// </summary>
        /// <param name="parameter">The parameter passed in to the callable member when invoking a method.</param>
        /// <returns>The value returned from the member.</returns>
        public TOut Invoke(TIn parameter)
        {
            if (m_ForceEmptyArgs)
                return Invoke();

            try
            {
                return (TOut) ReflectionUtility.GetMemberInfoValue(CallableMember, TargetObject, new object[] { parameter });
            }
            catch (TargetParameterCountException)
            {
                m_ForceEmptyArgs = true;
                return Invoke();
            }
        }

        /// <summary>
        /// Invokes the callable member on the target object.
        /// </summary>
        /// <returns>The value returned from the member.</returns>
        public TOut Invoke() => 
            (TOut)ReflectionUtility.GetMemberInfoValue(CallableMember, TargetObject, EmptyArgs);
        
        public TOut InvokeWith(object[] methodParameters) => 
            (TOut) ReflectionUtility.GetMemberInfoValue(CallableMember, TargetObject, methodParameters);

        #region Irrelevant Stuff (to increase performance in some cases)
        // ReSharper disable once StaticMemberInGenericType
        private static readonly object[] EmptyArgs;
        // Forces subsequent calls to Invoke() to use EmptyArgs.
        private bool m_ForceEmptyArgs = false;
        // Force a single initialization call for all invokables.
        static Invokable() => EmptyArgs = new object[] {};
        #endregion
    }
}
