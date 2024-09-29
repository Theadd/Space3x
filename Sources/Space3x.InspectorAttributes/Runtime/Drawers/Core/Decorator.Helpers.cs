using System;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
{
    public abstract partial class Decorator<T, TAttribute>
    {
        private bool m_Ready;
        private bool m_Removed = false;
        private bool m_TotallyRemoved = false;
        private bool m_Added = false;
        private bool m_OnUpdateCalled = false;
        private bool m_DetachingItself;

        /// <inheritdoc/>
        public bool EnsureContainerIsProperlyAttached(Action onProperlyAttachedCallback = null)
        {
            if (!HasValidContainer())
            {
                m_Ready = false;
                Container.RemoveFromHierarchy();
                // Container.WithClasses("ui3x-detached");
                ProperlyAddContainerBeforeField();
                onProperlyAttachedCallback?.Invoke();
                return false;
            }
            else
                onProperlyAttachedCallback?.Invoke();

            return true;
        }

        private int GetPositionOfGhostDecoratorFromTheEnd()
        {
            var parent = GhostContainer.hierarchy.parent;
            if (parent == null)
                return -1;
            var index = parent.hierarchy.IndexOf(GhostContainer);
            var maxIndex = parent.hierarchy.childCount - 1;
            var sum = 0;
            for (var i = index + 1; i <= maxIndex; i++)
            {
                if (parent.hierarchy.ElementAt(i) is GhostDecorator)
                    sum++;
            }

            return sum;
        }

        private VisualElement GetContainerOfPreviousDecorator()
        {
            var parent = GhostContainer.hierarchy.parent;
            if (parent == null)
                return null;
            var index = parent.hierarchy.IndexOf(GhostContainer) - 1;
            if (index < 0)
                return null;

            var previous = parent.hierarchy.ElementAt(index) as VisualElement;
            if (previous is not GhostDecorator previousGhostDecorator)
                return null;

            return previousGhostDecorator.DecoratorContainer;
        }

        private void ProperlyAddContainerBeforeField()
        {
            var previousContainer = GetContainerOfPreviousDecorator();
            if (previousContainer != null)
            {
                previousContainer.AddAfter(Container);
                return;
            }

            var posFromEnd = GetPositionOfGhostDecoratorFromTheEnd();
            var element = (VisualElement)Field;
            if (posFromEnd > 0)
            {
                for (var i = 0; i < posFromEnd; i++)
                {
                    var previous = element.GetPreviousSibling();
                    if (previous == null)
                        break;
                    if (previous is AutoDecorator)
                    {
                        if (previous.GetPreviousSibling() is GroupMarker aux)
                            previous = aux;
                        element = previous;
                    }
                }
            }

            element.AddBefore(Container);
        }

        public virtual bool HasValidContainer() =>
            Container != null
            && Field != null
#if UNITY_EDITOR
            && Container.GetNextSiblingOfType<UnityEditor.UIElements.PropertyField, BindablePropertyField>() == Field;
#else
            && Container.GetNextSiblingOfType<BindablePropertyField>() == Field;
#endif
    }
}
