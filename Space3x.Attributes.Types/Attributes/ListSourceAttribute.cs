using System;
using JetBrains.Annotations;
using Space3x.Attributes.Types.DeveloperNotes;
using UnityEngine;

namespace Space3x.Attributes.Types
{
    /// <summary>
    /// On <c>List{string}</c> properties, this attribute allows the user to select from a list of strings populated
    /// using reflection over the provided MemberName, while the value on the property itself only holds those selected
    /// values.
    /// </summary>
    [Experimental]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ListSourceAttribute : PropertyAttribute
    {
        /// <summary>
        /// Any method, field or property name of Type List{string} (or the Type of it's return value).
        /// </summary>
        public string MemberName { get; }

        /// <summary>
        /// Text to display as the Foldout label, which defaults to property's display name (preferredLabel). Set to
        /// null or empty to hide it.
        /// </summary>
        [UsedImplicitly]
        public string Text { get; }

        /// <summary>
        /// On <c>List{string}</c> properties, this attribute allows the user to select from a list of strings
        /// populated using reflection over the provided MemberName, while the value on the property itself only
        /// holds those selected values.
        /// </summary>
        /// <param name="memberName">Any method, field or property name of Type List{string} (or the Type of it's return value).</param>
        public ListSourceAttribute(string memberName) : base(applyToCollection: true) => MemberName = memberName;
    }
}
