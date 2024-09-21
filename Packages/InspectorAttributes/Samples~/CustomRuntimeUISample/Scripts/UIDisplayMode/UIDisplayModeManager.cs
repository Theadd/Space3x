using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public static class UIDisplayModeManager
{
    public enum UIDisplayComponentType
    {
        None = 0,
        RenderTextureUI = 1,
        DockedUI = 2,
        NoRuntimeStylesUI = 3,
        RawUIWithoutAnnotations = 4,
        [HideInInspector]
        RenderTextureTarget = -1,
    }

    private static Dictionary<UIDisplayComponentType, UIDisplayModeComponent> m_Components = new Dictionary<UIDisplayComponentType, UIDisplayModeComponent>();
    // private static bool m_DockedDisplayMode = false;
    public static UIDisplayComponentType ActiveDisplayMode { get; private set; } = UIDisplayComponentType.RenderTextureUI;

    public static bool DockedDisplayMode
    {
        get => ActiveDisplayMode == UIDisplayComponentType.DockedUI;
        set
        {
            if (DockedDisplayMode == value) return;
            ActiveDisplayMode = value ? UIDisplayComponentType.DockedUI : UIDisplayComponentType.RenderTextureUI;
            RefreshDisplayMode();
        }
    }
    
    public static void SetActiveDisplayMode(UIDisplayComponentType mode)
    {
        ActiveDisplayMode = mode == UIDisplayComponentType.RenderTextureTarget
            ? UIDisplayComponentType.RenderTextureUI
            : mode;
        RefreshDisplayMode();
    }

    private static void RefreshDisplayMode()
    {
        foreach (var (componentType, component) in m_Components)
        {
            if (component != null)
                component.SetDisplayModeActive(ActiveDisplayMode == componentType || (componentType == UIDisplayComponentType.RenderTextureTarget && ActiveDisplayMode == UIDisplayComponentType.RenderTextureUI)); 
        }
    }

    public static void Add(UIDisplayModeComponent component, UIDisplayComponentType componentType) => m_Components[componentType] = component;
    
    public static void Remove(UIDisplayModeComponent component, UIDisplayComponentType componentType)
    {
        if (m_Components[componentType] == component)
            m_Components[componentType] = null;
    }
}
