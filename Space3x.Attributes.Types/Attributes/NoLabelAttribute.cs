using System;
using System.Collections.Generic;

namespace Space3x.Attributes.Types
{
    /// <summary>
    /// Hides the label of the property field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class NoLabelAttribute : ClassesAttribute
    {
        private new bool Enabled { get; set; } = true;

        private new List<string> ClassNames { get; set; }

        public NoLabelAttribute(bool applyToChildren = false) : base(true, applyToChildren ? "ui3x-no-label-all" : "ui3x-no-label") { }
    }
}
