using Space3x.Attributes.Types;
using UnityEngine;

[ExecuteInEditMode]
public class UIDisplayModeSelector : MonoBehaviour
{
    [AllowExtendedAttributes]
    [NoScript]
    public UIDisplayModeManager.UIDisplayComponentType activeDisplayMode = UIDisplayModeManager.ActiveDisplayMode;

    [GradientSlider(0f, 1f, MinColor = "#FF0100", MaxColor = "#FF0001", Model = ColorModel.HSV, PixelsWidth = 36, ShowValue = true)]
    public float HSVHue = 0.25f;

    
    [TrackChangesOn(nameof(activeDisplayMode))]
    public void OnDisplayModeChanged()
    {
        if (activeDisplayMode != UIDisplayModeManager.ActiveDisplayMode)
            UIDisplayModeManager.SetActiveDisplayMode(activeDisplayMode);
    }
}
