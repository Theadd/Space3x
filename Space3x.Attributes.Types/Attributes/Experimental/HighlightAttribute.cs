using UnityEngine;

namespace Space3x.Attributes.Types
{
    public class HighlightAttribute : PropertyAttribute
    {
        public HighlightAttribute(bool applyToCollection = false) : base(applyToCollection: applyToCollection) { }
    }
}
