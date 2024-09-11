using UnityEngine;

namespace Space3x.Attributes.Types
{
    public class HighlightAttribute : PropertyAttribute
    {
        public HighlightAttribute(bool applyToCollection = false) 
#if UNITY_2023_3_OR_NEWER
            : base(applyToCollection: applyToCollection)
#endif
        { }
    }
}
