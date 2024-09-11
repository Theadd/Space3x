#if UNITY_EDITOR || RUNTIME_UITOOLKIT_DRAWERS
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.Properties.Types
{
#if UNITY_EDITOR
     public abstract class PropertyDrawerAdapter : UnityEditor.PropertyDrawer
     {
          // public PropertyAttribute attribute;
          //
          // public FieldInfo fieldInfo;
          //
          // public string preferredLabel;
          //
          // public virtual VisualElement CreatePropertyGUI(UnityEditor.SerializedProperty property) => (VisualElement) null;
     }
#else
     public abstract class PropertyDrawerAdapter
     {
          [NonSerialized]
          public PropertyAttribute attribute;

          [NonSerialized]
          public FieldInfo fieldInfo;

          [NonSerialized]
          public string preferredLabel;

// #if UNITY_EDITOR
//           public virtual VisualElement CreatePropertyGUI(UnityEditor.SerializedProperty property) => (VisualElement) null;
// #endif
     }
#endif
}
#endif
