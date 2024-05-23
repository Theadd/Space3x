using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Space3x.NavigateBack.Editor
{
    /// <summary>
    /// Provides a simple way to navigate back and forward in the editor's selection history
    /// of objects of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericNavigator<T> where T : class
    {
        /// <summary>
        /// Maximum number of items in the stack
        /// </summary>
        public int StackSize { get; set; } = 50;
        
        private Stack<WeakReference<T>> m_BackStack = new Stack<WeakReference<T>>();
        private Stack<WeakReference<T>> m_ForwardStack = new Stack<WeakReference<T>>();
        private T m_ActiveItem;
        
        /// <summary>
        /// Creates a new <see cref="GenericNavigator{T}"/>
        /// </summary>
        public GenericNavigator() => RegisterCallbacks(true);
        
        /// <summary>
        /// Cleans up
        /// </summary>
        ~GenericNavigator()
        {
            try
            {
                RegisterCallbacks(false);
                m_BackStack.Clear();
                m_ForwardStack.Clear();
                m_ActiveItem = null;
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /// <summary>
        /// Registers or unregisters the callback
        /// </summary>
        /// <param name="register"></param>
        private void RegisterCallbacks(bool register)
        {
            Selection.selectionChanged -= OnSelectionChanged;
            if (register)
                Selection.selectionChanged += OnSelectionChanged;
        }
        
        /// <summary>
        /// Callback when the selection changes
        /// </summary>
        private void OnSelectionChanged()
        {
            T activeItem = Selection.activeObject as T;
            if (activeItem == null || m_ActiveItem == activeItem) return;
            TryPeekFrom(ref m_BackStack, out T backPeek);
            
            if (m_ActiveItem != null && (backPeek == null || backPeek != m_ActiveItem))
                m_BackStack.Push(new WeakReference<T>(m_ActiveItem));
            
            m_ActiveItem = activeItem;
            if (m_BackStack.Count > StackSize + 10) 
                m_BackStack = new Stack<WeakReference<T>>(m_BackStack.Skip(m_BackStack.Count - StackSize));
            m_ForwardStack.Clear();
        }
        
        /// <summary>
        /// Try to peek from the stack
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool TryPeekFrom(ref Stack<WeakReference<T>> stack, out T value)
        {
            if (stack.Count > 0)
            {
                WeakReference<T> weakReference = stack.Peek();
                if (weakReference.TryGetTarget(out value))
                    return value != null;
            }
            value = null;
            return false;
        }

        /// <summary>
        /// Try to pop from the stack
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool TryPopTargetFrom(ref Stack<WeakReference<T>> stack, out T value)
        {
            value = null;
            while (stack.Count > 0)
            {
                WeakReference<T> weakReference = stack.Pop();
                if (weakReference.TryGetTarget(out T target) && target != null)
                {
                    value = target;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Navigates
        /// </summary>
        /// <param name="steps"></param>
        private void Navigate(int steps = -1)
        {
            switch (Math.Sign(steps))
            {
                case -1:
                    if (m_ActiveItem != null && TryPopTargetFrom(ref m_BackStack, out T target))
                    {
                        m_ForwardStack.Push(new WeakReference<T>(m_ActiveItem));
                        m_ActiveItem = target;
                    }
                    break;
                        
                case 1:
                    if (m_ActiveItem != null && TryPopTargetFrom(ref m_ForwardStack, out target))
                    {
                        m_BackStack.Push(new WeakReference<T>(m_ActiveItem));
                        m_ActiveItem = target;
                    }
                    break;
                default:
                    break;
            }
            Select(m_ActiveItem);
        }

        /// <summary>
        /// Selects
        /// </summary>
        /// <param name="target"></param>
        private void Select(T target)
        {
            Selection.activeObject = target as Object;
            if (!Selection.activeObject) return;
            EditorGUIUtility.PingObject(Selection.activeObject);
        }

        /// <summary>
        /// Navigate back
        /// </summary>
        public void Back() => Navigate(m_BackStack.Count > 0 ? -1 : 0);
        
        public bool CanGoBack() => m_BackStack.Count > 0;
        
        /// <summary>
        /// Navigate forward
        /// </summary>
        public void Forward() => Navigate(m_ForwardStack.Count > 0 ? 1 : 0);
        
        public bool CanGoForward() => m_ForwardStack.Count > 0;
    }
}
