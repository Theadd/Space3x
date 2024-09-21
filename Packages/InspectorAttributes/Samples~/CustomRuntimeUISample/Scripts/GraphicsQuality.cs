using JetBrains.Annotations;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes;
using Space3x.Properties.Types;
using UnityEngine;
using UnityEngine.UIElements;

public enum QualityLevel
{
    Low,
    Medium,
    High,
    VeryHigh
}

[CreateAssetMenu(fileName = "GraphicsQualitySettings", menuName = "UI Views/Graphics Quality Settings")]
public class GraphicsQuality : ScriptableObject
{
    [AllowExtendedAttributes]
    [Button(nameof(DebuggerBreak))]
    public bool verticalSync = true;
    
    [Header("UI Settings")]
    [System.NonSerialized]
    [ShowInInspector]
    private bool docked = false;
    [EnableOn(nameof(docked), Enabled = false)]
    [RangeSlider(0f, 5f, ShowValue = true)]
    [Tooltip("It actually changes the ambient intensity of the scene light, so no UI related.")]
    public float brightness = 2f;

    [Header("UI RenderTexture Settings")]
    [VisibleOn(nameof(InEditorPanel))]
    public Material renderTextureMaterial;
    
    public Color renderTextureColor = Color.white;
    [BeginRow(ProportionalSize = false)]
    [Aligned]
    public bool metallic = true;
    [Aligned]
    public bool smoothness = true;
    [EndRow]
    
    [Header("Skybox Settings")]
    [VisibleOn(nameof(InEditorPanel))]
    public Material skyboxMaterial;
    

    public Color skyboxTintColor = new Color32(43, 61, 79, 255);
    
    [BeginRow(ProportionalSize = false)]
        [Aligned]
        [RangeSlider(0f, 8f, ShowValue = true)]
        public float exposure = 1.0f;
        [Aligned]
        [RangeSlider(0f, 360f, ShowValue = true)]
        public float rotation = 0f;
    [EndRow]
    
    [Header("Shadow Settings")]
    public QualityLevel shadowQuality = QualityLevel.VeryHigh;
    [RangeSlider(0, 100)]
    public int shadowDistance = 80;
    [Header("Other Settings")]
    public Vector3 cameraDisplacement = Vector3.zero;
    
    [BeginRow(ProportionalSize = false)]
    [Aligned]
    public QualityLevel shaderQuality = QualityLevel.High;
    [Aligned]
    public bool depthOfField = true;
    [EndRow]
    
    [BeginRow(ProportionalSize = false)]
    [Aligned]
    public bool antiAliasing = true;
    [Aligned]
    public bool ambientOcclusion = true;
    [EndRow]

    [BeginRow]
    [Aligned]
    public bool bloomAndLensFlare = false;
    [Aligned]
    public bool motionBlur = false;
    [EndRow]
    
    [TrackChangesOn(nameof(skyboxTintColor))]
    [TrackChangesOn(nameof(exposure))]
    [TrackChangesOn(nameof(rotation))]
    [UsedImplicitly]
    private void OnUpdateSkybox()
    {
        if (skyboxMaterial == null) return;
        skyboxMaterial.SetFloat("_Exposure", exposure);
        skyboxMaterial.SetFloat("_Rotation", rotation);
        skyboxMaterial.SetColor("_Tint", skyboxTintColor);
        DynamicGI.UpdateEnvironment();
    }

    [TrackChangesOn(nameof(cameraDisplacement))]
    [UsedImplicitly]
    private void OnUpdateVector3() => Debug.Log($"On Update Vector3: {cameraDisplacement}");

    [TrackChangesOn(nameof(docked))]
    [UsedImplicitly]
    private void OnUpdateUIDisplayMode() => UIDisplayModeManager.DockedDisplayMode = docked;

    [TrackChangesOn(nameof(brightness))]
    [UsedImplicitly]
    private void OnUpdateBrightness() => RenderSettings.ambientIntensity = brightness;

    [TrackChangesOn(nameof(renderTextureColor))]
    [TrackChangesOn(nameof(metallic))]
    [TrackChangesOn(nameof(smoothness))]
    [UsedImplicitly]
    private void OnUpdateRenderTexture()
    {
        if (renderTextureMaterial == null) return;
        renderTextureMaterial.SetFloat("_Smoothness", smoothness ? .5f : 0f);
        renderTextureMaterial.SetFloat("_Metallic", metallic ? 1f : 0f);
        renderTextureMaterial.SetColor("_BaseColor", renderTextureColor);
    }

    private void DebuggerBreak(IPropertyNode property, IDrawer drawer)
    {
        var controller = property.GetController();
        Debug.Log(property.IsRuntimeUI());
    }
    
    private bool InEditorPanel(IDrawer drawer) => 
        drawer.GetLogicalPanel()?.contextType == ContextType.Editor;
}
