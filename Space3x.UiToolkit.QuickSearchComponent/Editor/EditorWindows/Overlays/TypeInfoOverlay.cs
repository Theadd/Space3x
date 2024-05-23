using System;
using Space3x.UiToolkit.QuickSearchComponent.Editor;
using Space3x.UiToolkit.QuickSearchComponent.Editor.VisualElements;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

[Overlay(id = "DisplayTypeInfoOverlay", displayName = "Type Info", defaultDockZone = DockZone.BottomToolbar, defaultDockPosition = DockPosition.Bottom, defaultLayout = Layout.HorizontalToolbar, maxHeight = 190f, minHeight = 0f)]
class TypeInfoOverlay : Overlay, ICreateHorizontalToolbar
{
    public QuickSearchWindow ActiveWindow { get; private set; }
    public Color BackgroundColor { get; set; } = new Color32(25, 25, 25, 255);
    private TypeInfoHorizontalToolbar m_OverlayToolbar;
    private TextElement Info { get; set; }
    private readonly TypeRewriter m_Rewriter = new();
    private string m_Text = string.Empty;
    
    public TypeInfoOverlay(QuickSearchWindow win)
    {
        ActiveWindow = win;
        defaultSize = new Vector2(500f, 44f);
    }

    public override VisualElement CreatePanelContent()
    {
        return (Info = new Label(m_Text));
    }

    // Setup horizontal layout to only have CustomToolbarItem visual element.
    public OverlayToolbar CreateHorizontalToolbarContent()
    {
        var horizontalOverlayToolbar = new OverlayToolbar();
        m_OverlayToolbar = new TypeInfoHorizontalToolbar(this);
        horizontalOverlayToolbar.Add(m_OverlayToolbar);

        return horizontalOverlayToolbar;
    }

    public void SetValue(Type type)
    {
        SetInfo(type == null 
            ? string.Empty 
            : TypeRewriter.NJoin("\n", new[]
            {
                m_Rewriter.SetTarget(type).GetFormattedType(),
                TypeRewriter.Description(type)
            }));
    }

    public void SetInfo(string text = null)
    {
        if (text == null) text = m_Text;
        m_Text = text;
        if (Info != null) Info.text = text;
        if (m_OverlayToolbar?.Info != null) m_OverlayToolbar.Info.text = text;
    }
    
    
    [EditorToolbarElement("TypeInfoHorizontalToolbar", typeof(QuickSearchWindow))]
    public class TypeInfoHorizontalToolbar : OverlayToolbar
    {
        public TextElement Info { get; private set; }
        private readonly TypeInfoOverlay m_Overlay;

        public TypeInfoHorizontalToolbar(TypeInfoOverlay overlay)
        {
            m_Overlay = overlay;
            
            Info = new TextLine()
            {
                text = m_Overlay.m_Text,
                style =
                {
                    flexGrow = 1f,
                    flexShrink = 0f,
                    backgroundColor = new StyleColor(m_Overlay.BackgroundColor),
                }
            };
            Info.RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            Add(Info);
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            VisualElement element = Info;
            for (var i = 15; i > 0; i--)
            {
                element = element.hierarchy.parent;
                if (element == null) break;
                element.style.flexGrow = 1f;
                if (element.name == "unity-content-container")
                {
                    element.style.backgroundColor = new StyleColor(m_Overlay.BackgroundColor);
                    break;
                }
                else if (element.name == "unity-overlay")
                {
                    element.style.flexBasis = 16f;
                }
            }
        }
    }
}


