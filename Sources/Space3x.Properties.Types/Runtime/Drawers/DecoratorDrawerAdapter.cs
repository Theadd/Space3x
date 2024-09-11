#if UNITY_EDITOR || RUNTIME_UITOOLKIT_DRAWERS
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.Properties.Types
{
#if UNITY_EDITOR
    public abstract class DecoratorDrawerAdapter : UnityEditor.DecoratorDrawer { }
#else
     public abstract class DecoratorDrawerAdapter
     {
          public PropertyAttribute attribute;
          
          public virtual VisualElement CreatePropertyGUI() => (VisualElement) null;
     }
#endif
}
#endif
