using Space3x.Attributes.Types;
using UnityEngine;

namespace Space3x.InspectorAttributes
{
    // [CreateAssetMenu(fileName = "FILENAME", menuName = "MENUNAME", order = 0)]
    public abstract class ScriptableUIView : ScriptableObject
    {
        [AllowExtendedAttributes]
        [Tooltip("Each UIView asset should have an unique name.")]
        [VisibleOn(nameof(IsPlaying), Visible = false)]
        public string viewName = "ui-view";

        protected bool IsPlaying => Application.IsPlaying(this);
    }
}
