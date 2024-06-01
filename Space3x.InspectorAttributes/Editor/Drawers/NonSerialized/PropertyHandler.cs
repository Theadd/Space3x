// using System;
// using System.Reflection;
// using UnityEditor;
// using UnityEngine;
//
// namespace Space3x.InspectorAttributes.Editor.Drawers.NonSerialized
// {
//     /// <summary>
//     /// Original source by <a href="https://forum.unity.com/threads/locate-custompropertydrawer-from-serializedobject.462405/#post-3003847">CDF</a> (Unity Forum). 
//     /// </summary>
//     public class PropertyHandler {
//  
//         private static MethodInfo getHandler;
//         private static object[] getHandlerParams;
//  
//         private object handler;
//         private Type type;
//  
//         private PropertyInfo propertyDrawerInfo;
//         private MethodInfo guiHandler;
//         private object[] guiParams;
//  
//         public PropertyDrawer propertyDrawer {
//  
//             get { return propertyDrawerInfo.GetValue(handler, null) as PropertyDrawer; }
//         }
//  
//         static PropertyHandler() {
//  
//             getHandler = Type
//                 .GetType("UnityEditor.ScriptAttributeUtility, UnityEditor")
//                 .GetMethod("GetHandler", BindingFlags.NonPublic | BindingFlags.Static);
//             getHandlerParams = new object[1];
//         }
//  
//         private PropertyHandler(object handler) {
//  
//             this.handler = handler;
//  
//             type = handler.GetType();
//             propertyDrawerInfo = type.GetProperty("propertyDrawer", BindingFlags.NonPublic | BindingFlags.Instance);
//             guiHandler = type.GetMethod("OnGUI", BindingFlags.Public | BindingFlags.Instance);
//             guiParams = new object[4];
//         }
//  
//         public bool OnGUI(Rect position, SerializedProperty property, GUIContent label, bool includeChildren) {
//  
//             guiParams[0] = position;
//             guiParams[1] = property;
//             guiParams[2] = label;
//             guiParams[3] = includeChildren;
//  
//             return (bool)guiHandler.Invoke(handler, guiParams);
//         }
//  
//         public static PropertyHandler GetHandler(SerializedProperty property) {
//  
//             getHandlerParams[0] = property;
//  
//             return new PropertyHandler(getHandler.Invoke(null, getHandlerParams));
//         }
//     }
// }