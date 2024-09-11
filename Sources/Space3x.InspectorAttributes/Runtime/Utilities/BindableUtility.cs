using System;
using Space3x.Properties.Types;
using UnityEngine.Internal;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
{
    internal interface IBindableUtility
    {
        void AutoNotifyValueChangedOnNonSerialized(VisualElement element, IPropertyNode property);

        void RegisterValueChangedCallback(VisualElement element, Action callback);

        void SetValueWithoutNotify(VisualElement element, object value);
    }
    
    [ExcludeFromDocs]
    internal static class BindableUtility
    {
        private static IBindableUtility s_Implementation;

        internal static void RegisterImplementationProvider(IBindableUtility provider) => s_Implementation = provider;

        public static void AutoNotifyValueChangedOnNonSerialized(VisualElement element, IPropertyNode property) =>
            s_Implementation?.AutoNotifyValueChangedOnNonSerialized(element, property);

        public static void RegisterValueChangedCallback(VisualElement element, Action callback) =>
            s_Implementation?.RegisterValueChangedCallback(element, callback);

        public static void SetValueWithoutNotify(VisualElement element, object value) =>
            s_Implementation?.SetValueWithoutNotify(element, value);
    }
}
