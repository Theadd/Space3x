using System;
using UnityEngine;

namespace Space3x.Attributes.Types
{
    /// <summary>
    /// Displays a string property as an editable Rich Text Label on double-click.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class EditableRichTextAttribute : PropertyAttribute
    {
        public EditableRichTextAttribute() : base(applyToCollection: false) { }
    }
}
