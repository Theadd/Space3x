using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Space3x.Attributes.Types
{
    /// <summary>
    /// Adds or removes the provided USS classes on the next blocking VisualElement (also known as "visual target").
    /// 
    /// This attribute generates a non-blocking element so any attributes placed before or above it will
    /// pass through this when applying their effect to their next visual target.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class ClassesAttribute : PropertyAttribute
    {
        /// <summary>
        /// Set to false in order to remove those classes from the VisualElement's class list instead of adding them.
        /// </summary>
        public bool Enabled { get; set; } = true;
        
        /// <summary>
        /// The list of classes to add or remove from the next visual target.
        /// </summary>
        public List<string> ClassNames { get; set; }
        
        /// <summary>
        /// Adds or removes the provided USS classes on the next blocking VisualElement (also known as "visual target").
        /// </summary>
        /// <param name="classNames">The list of classes to add or remove.</param>
        public ClassesAttribute(params string[] classNames) => ClassNames = classNames.ToList();
        
        /// <summary>
        /// Adds or removes the provided USS classes on the next blocking VisualElement (also known as "visual target").
        /// </summary>
        /// <param name="enabled">Set to false in order to remove those classes from the VisualElement's class list instead of adding them.</param>
        /// <param name="classNames">The list of classes to add or remove.</param>
        public ClassesAttribute(bool enabled, params string[] classNames)
        {
            Enabled = enabled;
            ClassNames = classNames.ToList();
        }
        
        protected ClassesAttribute() 
#if UNITY_2023_3_OR_NEWER
            : base(applyToCollection: false)
#endif
        { }
        
        protected ClassesAttribute(bool applyToCollection) 
#if UNITY_2023_3_OR_NEWER
            : base(applyToCollection: applyToCollection)
#endif
        { }
    }
}
