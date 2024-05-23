//using System;
//using Space3x.InspectorAttributes.Editor.Drawers;
//using Space3x.InspectorAttributes.Editor.VisualElements;
//using Space3x.InspectorAttributes.Editor.Utilities;
//using Space3x.UiToolkit.Types;
//using UnityEngine;
//using UnityEngine.UIElements;
//
//namespace Space3x.InspectorAttributes.Editor.Extensions
//{
//    public static class DecoratorExtensions
//    {
////        [Obsolete]
////        public static bool TryGetConditionValue<T, TAttribute>(
////            this Decorator<T, TAttribute> self, 
////            string conditionPropertyName, 
////            out bool value) 
////            where T : VisualElement, new()
////            where TAttribute : PropertyAttribute
////        {
////            var conditionalProperty = ReflectionUtility.GetValidMemberInfo(conditionPropertyName, self.Property);
////            var memberInfoType = ReflectionUtility.GetMemberInfoType(conditionalProperty);
////            if (memberInfoType != null)
////            {
////                if (memberInfoType == typeof(bool))
////                {
////                    value = (bool)ReflectionUtility.GetMemberInfoValue(conditionalProperty, self.Property);
////                    return true;
////                }
////            }
////            value = false;
////            return false;
////        }
////        
////        [Obsolete]
////        public static bool TryGetPredicateFunction<T, TIn, TAttribute>(
////            this Decorator<T, TAttribute> self, 
////            string functionName, 
////            out Invokable<TIn, bool> predicate)
////            where T : VisualElement, new()
////            where TAttribute : PropertyAttribute
////        {
////            predicate = ReflectionUtility.CreateInvokable<TIn, bool>(functionName, self.Property); 
////            return predicate != null;
////        }
//        
////        private static bool TryGetDecoratorsContainerReference<T, TAttribute>(this Decorator<T, TAttribute> self, out VisualElementReference reference) 
////            where T : VisualElement, new()
////            where TAttribute : PropertyAttribute
////        {
////            reference = self.Container.parent.ElementAt(0) as VisualElementReference;
////            return reference != null;
////        }
//        
////        public static Decorator<T, TAttribute> Detach<T, TAttribute>(this Decorator<T, TAttribute> self)
////            where T : VisualElement, new()
////            where TAttribute : PropertyAttribute
////        {
////            self.Container.RemoveFromHierarchy();
////            self.Container.WithClasses("ui3x-detached");
////            self.Field.AddBefore(self.Container);
////            Debug.Log($"Detaching [{self.Container.GetType().Name} #{self.Container.name}] " +
////                      $"BEFORE {self.Field.name} (Field previous sibling: {self.Field.GetPreviousSibling().name}), " +
////                      $"Attribute: {self.Target.GetType().Name}; ELEMENT: {self.Container.name} ({self.Container.GetHashCode()})");
////            
////            return self;
////        }
//    }
//}
