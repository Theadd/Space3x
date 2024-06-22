using System;
using UnityEngine;

namespace Space3x.Attributes.Types
{
    public enum GroupType { None, Row, Column }
    
    [AttributeUsage(AttributeTargets.Class 
                    | AttributeTargets.Method 
                    | AttributeTargets.Property 
                    | AttributeTargets.Field,
        AllowMultiple = true, Inherited = true)]
    public class GroupMarkerAttribute : PropertyAttribute
    {
        public string Text { get; set; }
        
        public GroupType Type { get; set; }
        
        /// <summary>
        /// Whether this attribute is for the opening marker or the closing marker.
        /// </summary>
        public bool IsOpen { get; set; }

        /// <summary>
        /// By default, elements within the group will be sized proportionally to the size of their content.
        /// Set to false in order to make all elements the same size regardless of their content.
        /// </summary>
        public bool ProportionalSize { get; set; } = true;

        /// <summary>
        /// Group serialized members within this attribute marker to it's corresponding closing attribute.
        /// </summary>
        public GroupMarkerAttribute(GroupType groupType) => this.Type = groupType;
    }
}
