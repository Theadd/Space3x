#if SPACE3X_DEBUG
using System.Collections.Generic;
using System.Linq;
using Space3x.Properties.Types;
using UnityEngine;

namespace Space3x.InspectorAttributes.Editor.Utilities
{
    public static class DebuggingUtility
    {
        public static void ShowAllControllers()
        {
            var strControllers = new List<string>();
            var allKeys = PropertyAttributeController.GetAllInstanceKeys();
            var iKey = 0;
            foreach (var controller in PropertyAttributeController.GetAllInstances())
            {
                var key = allKeys[iKey];
                strControllers.Add($"KEY: " + key + "\n" + StringifyController(controller));
                iKey++;
            }

            Debug.Log(string.Join("\n\n", strControllers) + "\n\n");
        }

        private static string StringifyController(PropertyAttributeController controller)
        {
            var str = @$"IID: {controller.InstanceID}, ParentPath: ""{controller.ParentPath}"",
TargetObject.name: {controller.TargetObject?.name}, SO.target.IID: {controller.TargetObject?.GetInstanceID()},
TargetType: {controller.TargetType?.Name}, DeclaringType: {controller.DeclaringType?.Name},
Editor.target.InstanceID: {controller.Editor?.target?.GetInstanceID()}, Editor.serializedObject.target.InstanceID: {controller.Editor?.serializedObject?.targetObject?.GetInstanceID()},
DeclaringObject: {controller.DeclaringObject}
<b>PROPERTIES</b>";
            
            foreach (var pKey in controller.Properties.Keys)
            {
                if (pKey == string.Empty) continue;
                var pValue = controller.Properties.GetValue(pKey);
                var sPath = "<INVALID>";
                var pSerialized = pValue.GetSerializedProperty();
                if (pSerialized != null) sPath = pSerialized.propertyPath;
                
                str += $"\n\t'{pKey}' (@ '{pValue.ParentPath}')\n\t\tPropertyPath: '{pValue.PropertyPath}'"
                       + $"\n\t\tpropertyPath: '{sPath}'\n\t\tFlags: {(pValue as IPropertyFlags)?.Flags}" 
                       + $"\n\t\tPropertyType: {pValue.GetType().Name} + {string.Join(", ", pValue.GetType().GetInterfaces().Select(t => t.Name))}";
            }

            return str;
        }
    }
}
#endif