using System;

namespace Space3x.Properties.Types
{
    /// <summary>
    /// Another non-standard custom implementation of an EventHandler. It covers specific use cases not suitable for
    /// <see cref="UnityEngine.UIElements.EventBase"/>, mainly due to ("that awesome mechanism we all love so fucking
    /// much of spawning") internal access modifiers all around. The proper way would be using <see cref="EventHandler{TEventArgs}"/>
    /// on custom <see cref="System.EventArgs"/> with event pooling, but on second thought and with much regret, I'll
    /// stick with the old plain parameterless <see cref="System.Action"/>.
    /// </summary>
    public interface IOffscreenEventHandler
    {
        /// <summary>
        /// This event is fired when an entire <see cref="IPropertyNode"/> is added to the tree, including its custom
        /// property drawer, if any, all of its decorators, all its nested child properties, when appropriate, and is
        /// ready to process any following properties.
        /// </summary>
        public event Action onPropertyAdded;
        
        /// <summary>
        /// Fired when this container is assigned to a renderer (or reassigned).
        /// </summary>
        public event Action onParentChanged;

        /// <summary>
        /// It's fired after every single call to the <see cref="UnityEngine.UIElements.VisualElement.Add">Add</see>
        /// method on this offscreen container, whether it is for a property drawer, decorator, or else. 
        /// </summary>
        public event Action onElementAdded;
        
        /// <summary>
        /// It is intended to replace the OnGeometryChanged event when it is just being used to delay some
        /// code execution for later. Unlike other action events from this interface, any registered callback will be
        /// called only once.
        /// </summary>
        public event Action delayCall;

        void RaiseOnPropertyAddedEvent();
    }
}
