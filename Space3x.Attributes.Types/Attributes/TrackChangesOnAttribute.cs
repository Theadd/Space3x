using System;
using UnityEngine;

namespace Space3x.Attributes.Types
{
    /// <summary>
    /// Annotate a method with this attribute to call it automatically when the value of the specified property changes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class TrackChangesOnAttribute : PropertyAttribute, ITrackChangesOnEx<TrackChangesOnAttribute>
    {
        /// <summary>
        /// The name of the property to track value changes on.
        /// </summary>
        public string PropertyName { get; }
        
        /// <summary>
        /// Annotate a method with this attribute to call it automatically when the value of the specified property changes.
        /// </summary>
        /// <param name="propertyName">The name of the property to track value changes on.</param>
        public TrackChangesOnAttribute(string propertyName) => PropertyName = propertyName;
    }
}
