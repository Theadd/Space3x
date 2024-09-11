using System;
using Space3x.Attributes.Types.DeveloperNotes;
using UnityEngine;

namespace Space3x.Attributes.Types
{
    [ViolatesYAGNI]
    public abstract class Extension<T> : GenericExtension
    {
        public virtual bool TryApply<TValue, TContent>(IExtensionContext context, TContent content, out TValue outValue, TValue defaultValue)
            where TContent : T
        {
            outValue = defaultValue;
            return false;
        }

        public virtual bool TryApply<TTarget, TContent>(IExtensionContext context, TContent content, TTarget target)
            where TContent : T
            where TTarget : class => false;
        
        [HideInCallstack]
        protected bool Fail<TException>(TException exception)
            where TException : Exception, new()
        {
            Debug.LogException(exception);
            return false;
        }
    }
}
