using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Space3x.InspectorAttributes.Editor.Extensions;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
    public interface IDrawer : IDisposable
    {
        public static StyleSheet DefaultStyleSheet  = Resources.Load<StyleSheet>("Space3x.InspectorAttributes.Stylesheet");
        
        public SerializedProperty Property { get; }
        
        public VisualElement Container { get; }
        
        public PropertyField Field { get; }
        
        /// <summary>
        /// The VisualElement that the effects of the decorator or property drawer should be applied to.
        /// </summary>
        public VisualElement VisualTarget { get; }
        
        /// <summary>
        /// The closest parent InspectorElement related to this decorator or property drawer.
        /// </summary>
        public InspectorElement InspectorElement => Field?.GetClosestParentOfType<InspectorElement>();

        /// <summary>
        /// Override this method to perform any custom logic when the decorator or property drawer needs to be updated.
        /// </summary>
        public void OnUpdate();

        void OnReset(bool disposing = false);

        public void AddDefaultStyles()
        {
            var inspector = InspectorElement;
            if (inspector?.styleSheets.Contains(DefaultStyleSheet) == false)
                inspector.styleSheets.Add(DefaultStyleSheet);
        }
    }
    
    public interface IDrawer<TAttribute> : IDrawer
    {
        public TAttribute Target { get; }
    }
}
