using System;
using JetBrains.Annotations;
using Space3x.Attributes.Types;
using UnityEngine;

namespace Space3x.InspectorAttributes
{
    [UsedImplicitly]
    public class ConditionEx : Extension<IConditionEx>
    {
        public static bool TryCreateInvokable<TIn, TOut, TContent>(IExtensionContext context, TContent content, out Invokable<TIn, TOut> invokable)
            where TContent : ICondition
        {
            if (context is IAttributeExtensionContext drawer)
                if (!string.IsNullOrEmpty(content.Condition))
                {
                    if (drawer.Property.TryCreateInvokable<TIn, TOut>(content.Condition, out invokable, drawer: drawer as IDrawer)) 
                        return true;

                    Debug.LogError($"Could not find member {content.Condition} on {drawer.Property.PropertyPath}");
                }

            invokable = null;
            return false;
        }
        
        public override bool TryApply<TValue, TContent>(IExtensionContext context, TContent content, out TValue outValue, TValue defaultValue)
        {
            if (TryCreateInvokable<TValue, TValue, TContent>(context, content, out var invokable))
            {
                if (!((invokable.Parameters == null ? invokable.Invoke() : invokable.InvokeWith(invokable.Parameters)) is TValue value)) 
                    throw new Exception(nameof(ConditionEx) + " expects an out value to be of type " + typeof(TValue).Name + ".");
                
                outValue = value;
                return true;
            }

            outValue = defaultValue;
            return false;
        }
    }
}
