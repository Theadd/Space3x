#if UNITY_EDITOR || RUNTIME_UITOOLKIT_DRAWERS
using System.Reflection;
using UnityEngine;

namespace Space3x.Properties.Types
{
#if UNITY_EDITOR
     public abstract class PropertyDrawerAdapter : UnityEditor.PropertyDrawer { }
#else
     public abstract class PropertyDrawerAdapter
     {
          public PropertyAttribute attribute;

          public FieldInfo fieldInfo;

          public string preferredLabel;
     }
#endif
}
#endif
