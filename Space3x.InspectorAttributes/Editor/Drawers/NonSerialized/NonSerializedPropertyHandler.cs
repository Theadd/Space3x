// using System;
// using System.Collections.Generic;
// using System.Reflection;
// using UnityEditor;
// using UnityEngine;
//
// namespace Space3x.InspectorAttributes.Editor.Drawers.NonSerialized
// {
//     internal class NonSerializedPropertyHandler
//     {
//         public void HandleAttribute(    /* CALLED for each PropertyAttribute ON a SerializedProperty (~=field)  */
//             SerializedProperty property,
//             PropertyAttribute attribute,
//             FieldInfo field,                /**/
//             System.Type propertyType)       /* For ManagedReference fields (class instances not inheriting from UnityEngine.Object), the Type of the instance assigned to the field. */
//         {                                      /*  
//         
//                 OR JUST: 
//                     System.Type propertyType ===>>> property.serializedObject.targetObject.GetType();
//         
//          */
//             if (attribute is TooltipAttribute)
//                 this.tooltip = (attribute as TooltipAttribute).tooltip;
//             else if (attribute is ContextMenuItemAttribute)
//             {
//                 if (this.contextMenuItems == null)
//                     this.contextMenuItems = new List<ContextMenuItemAttribute>();
//                 this.contextMenuItems.Add(attribute as ContextMenuItemAttribute);
//             }
//             else
//             {
//                 if (attribute.applyToCollection)
//                 {
//                     if (!property.isArray)
//                         return;
//                     if (!propertyType.IsArrayOrList())
//                         throw new NotSupportedException("Cannot apply attribute on a field of type " + propertyType.Name + ".\nPlease use this attribute on a collection.");
//                 }
//                 this.HandleDrawnType(property, attribute.GetType(), propertyType, field, attribute);
//             }
//         } 
//         /*
//          
//          
//          
//          
//          */
//         
//         public void HandleDrawnType(
//             SerializedProperty property,
//             System.Type drawnType,              /* attribute.GetType() */
//             System.Type propertyType,
//             FieldInfo field,
//             PropertyAttribute attribute)        /* Can be null */
//         {
//             System.Type forPropertyAndType = ScriptAttributeUtility.GetDrawerTypeForPropertyAndType(property, drawnType);
//             if (!(forPropertyAndType != (System.Type) null))
//                 return;
//             if (typeof (PropertyDrawer).IsAssignableFrom(forPropertyAndType))
//             {
//                 if (propertyType != (System.Type) null && propertyType.IsArrayOrList() && !attribute.applyToCollection)
//                     return;
//                 PropertyDrawer instance = (PropertyDrawer) Activator.CreateInstance(forPropertyAndType);
//                 instance.m_FieldInfo = field;
//                 instance.m_Attribute = attribute;
//                 if (this.m_PropertyDrawers == null)
//                     this.m_PropertyDrawers = new List<PropertyDrawer>();
//                 this.m_PropertyDrawers.Add(instance);
//             }
//             else if (typeof (DecoratorDrawer).IsAssignableFrom(forPropertyAndType) && (!(field != (FieldInfo) null) || !field.FieldType.IsArrayOrList() || propertyType.IsArrayOrList()))
//             {
//                 DecoratorDrawer instance = (DecoratorDrawer) Activator.CreateInstance(forPropertyAndType);
//                 instance.m_Attribute = attribute;
//                 if (this.m_DecoratorDrawers == null)
//                     this.m_DecoratorDrawers = new List<DecoratorDrawer>();
//                 this.m_DecoratorDrawers.Add(instance);
//             }
//         }
//     }
// }