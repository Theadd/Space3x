using System;

namespace Space3x.Attributes.Types.DeveloperNotes
{
    /// <summary>
    /// Types with this attribute are marked as drafts and not production ready.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class DraftAttribute : Attribute { }
}