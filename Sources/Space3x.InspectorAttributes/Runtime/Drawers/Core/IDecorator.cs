using System;
using UnityEngine.Internal;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
{
    [ExcludeFromDocs]
    public interface IDecorator : IDrawer
    {
        GhostDecorator GhostContainer { get; set; }
        
        /// <summary>
        /// Ensures that the container is properly attached and positioned.
        /// </summary>
        /// <param name="onProperlyAttachedCallback"></param>
        /// <returns>Whether the container was properly attached or not.</returns>
        bool EnsureContainerIsProperlyAttached(Action onProperlyAttachedCallback = null);

        bool HasValidContainer();

        void OnAttachedAndReady(VisualElement element);

        void ProperlyRemoveFromHierarchy();
    }
}