using System;
using System.Collections.Generic;

namespace Space3x.Attributes.Types
{
    /// <summary>
    /// Aligns VisualElements within a group to the left on column mode and to the top on row mode.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AlignItemsStartAttribute : ClassesAttribute
    {
        private new bool Enabled { get; set; } = true;

        private new List<string> ClassNames { get; set; }

        public AlignItemsStartAttribute() : base(true, "ui3x-align-items-flex-start") { }
    }
}
