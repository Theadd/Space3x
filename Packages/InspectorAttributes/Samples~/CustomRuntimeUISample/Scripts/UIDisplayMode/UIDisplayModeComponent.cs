using Space3x.Attributes.Types;
using UnityEngine;
using UnityEngine.UIElements;

[ExecuteInEditMode]
public class UIDisplayModeComponent : MonoBehaviour
{
    [EnableOn(nameof(lockEdits), Enabled = false)]
    public UIDisplayModeManager.UIDisplayComponentType ComponentType =
        UIDisplayModeManager.UIDisplayComponentType.DockedUI;
    
    [Visible(false)]
    public bool lockEdits = true;
    
    [ContextMenu("Toggle Edit Lock")]
    public void ToggleEditLock() => lockEdits = !lockEdits;
    
    public void SetDisplayModeActive(bool display)
    {
        if (ComponentType != UIDisplayModeManager.UIDisplayComponentType.RenderTextureTarget)
        {
            var uiDocument = GetComponent<UIDocument>();
            if (uiDocument != null) uiDocument.enabled = display;
        }
        else
        {
            var uiRenderer = GetComponent<Renderer>();
            uiRenderer.enabled = display;
        }
    }

    private void OnEnable() => UIDisplayModeManager.Add(this, ComponentType);

    private void OnDisable() => UIDisplayModeManager.Remove(this, ComponentType);
}
