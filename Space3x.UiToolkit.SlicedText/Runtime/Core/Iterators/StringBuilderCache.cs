using System;
using System.Collections.Generic;
using System.Text;

namespace Space3x.UiToolkit.SlicedText.Iterators
{
    /// <summary>
    /// A simple object recycling system.
    /// </summary>
    /// <typeparam name="T">Type of the object to cache</typeparam>
    public abstract class ObjectCache<T> where T : class
    {
        private readonly Stack<T> m_Builders;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectCache{T}"/> class.
        /// </summary>
        protected ObjectCache()
        {
            m_Builders = new Stack<T>(4);
        }

        /// <summary>
        /// Clears this cache.
        /// </summary>
        public void Clear()
        {
            lock (m_Builders)
            {
                m_Builders.Clear();
            }
        }

        /// <summary>
        /// Gets a new instance.
        /// </summary>
        /// <returns></returns>
        public T Get()
        {
            lock (m_Builders)
            {
                if (m_Builders.Count > 0)
                {
                    return m_Builders.Pop();
                }
            }

            return NewInstance();
        }

        /// <summary>
        /// Releases the specified instance.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <exception cref="System.ArgumentNullException">if instance is null</exception>
        public void Release(T instance)
        {
            if (instance == null) return;
            
            Reset(instance);
            lock (m_Builders)
            {
                m_Builders.Push(instance);
            }
        }

        /// <summary>
        /// Creates a new instance of {T}
        /// </summary>
        /// <returns>A new instance of {T}</returns>
        protected abstract T NewInstance();

        /// <summary>
        /// Resets the specified instance when <see cref="Release"/> is called before storing back to this cache.
        /// </summary>
        /// <param name="instance">The instance.</param>
        protected abstract void Reset(T instance);
    }
    
    /// <summary>
    /// A default object cache that expect the type {T} to provide a parameter less constructor
    /// </summary>
    /// <typeparam name="T">The type of item to cache</typeparam>
    /// <seealso cref="Markdig.Helpers.ObjectCache{T}" />
    public abstract class DefaultObjectCache<T> : ObjectCache<T> where T : class, new()
    {
        protected override T NewInstance()
        {
            return new T();
        }
    }
    
    /// <summary>
    /// An implementation of <see cref="ObjectCache{T}"/> for <see cref="StringBuilder"/>
    /// </summary>
    /// <seealso cref="Markdig.Helpers.ObjectCache{StringBuilder}" />
    public class StringBuilderCache : DefaultObjectCache<StringBuilder>
    {
        /// <summary>
        /// A StringBuilder that can be used locally in a method body only.
        /// </summary>
        [ThreadStatic]
        private static StringBuilder s_Local;

        /// <summary>
        /// Provides a string builder that can only be used locally in a method. This StringBuilder MUST not be stored.
        /// </summary>
        /// <returns></returns>
        public static StringBuilder Local()
        {
            var sb = s_Local ?? (s_Local = new StringBuilder());
            if (sb.Length > 0)
            {
                sb.Length = 0;
            }
            return sb;
        }

        protected override void Reset(StringBuilder instance)
        {
            if (instance.Length > 0)
            {
                instance.Length = 0;
            }
        }
    }
}
