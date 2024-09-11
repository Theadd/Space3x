using System;
using UnityEngine;

namespace Space3x.Properties.Types
{
    public static class PropertyExtensions
    {
        public static UnityEngine.Object GetTargetObject(this IPropertyNode self)
        {
            if (self is not IControlledProperty property)
                return null;

            try
            {
                if (!property.Controller.IsRuntimeUI)
                    return 
#if UNITY_EDITOR
                        (property.Controller.SerializedObject as UnityEditor.SerializedObject)?.targetObject is UnityEngine.Object target ? target :
#endif
                            property.Controller.TargetObject;

                return property.Controller.TargetObject;
            }
            catch (NullReferenceException)
            {
                // Can happen when switching from Debug mode to Normal mode in the inspector after editing some properties.
                Debug.LogWarning("NullReferenceException when trying to get property: " + self.PropertyPath + Environment.NewLine
                                 + "Can happen when switching from Debug mode to Normal mode in the inspector after editing some properties.");
            }

            return null;
        }

        public static bool IsRuntimeUI(this IPropertyNode self) =>
            self is IControlledProperty node && (node.Controller?.IsRuntimeUI ?? false);
    }
}
