using System;
using UnityEngine;

namespace Space3x.Attributes.Types
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class TrackChangesOnAttribute : PropertyAttribute, ITrackChangesOnEx<TrackChangesOnAttribute>
    {
        public string PropertyName { get; }
        
        public TrackChangesOnAttribute(string propertyName) => PropertyName = propertyName;
    }
}
