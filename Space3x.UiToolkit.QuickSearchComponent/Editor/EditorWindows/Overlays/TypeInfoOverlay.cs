using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Space3x.Documentation;
using Space3x.InspectorAttributes.Editor.Extensions;
using Space3x.UiToolkit.QuickSearchComponent.Editor.VisualElements;
using Space3x.UiToolkit.Types;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.QuickSearchComponent.Editor
{
    [Overlay(id = "DisplayTypeInfoOverlay", displayName = "Type Info", defaultDockZone = DockZone.BottomToolbar, defaultDockPosition = DockPosition.Bottom, defaultLayout = Layout.HorizontalToolbar, maxHeight = 190f, minHeight = 0f)]
    class TypeInfoOverlay : Overlay, ICreateHorizontalToolbar
    {
        public QuickSearchWindow ActiveWindow { get; private set; }
        public Color BackgroundColor { get; set; } = new Color32(25, 25, 25, 255);
        private TypeInfoHorizontalToolbar m_OverlayToolbar;
        private TextElement Info { get; set; }
        private readonly TypeRewriter m_Rewriter = new();
        private string m_Text = string.Empty;
        private Type m_CurrentType = null;
        private List<string> ReloadableAssemblyNames => m_ReloadableAssemblyNames ??= XmlDocumentationGenerator
            .GetAllGenerationSources()
            .Select(Path.GetFileNameWithoutExtension)
            .ToList();
        private List<string> m_ReloadableAssemblyNames = null;
    
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
            SetCurrentType(type);
            SetInfo(type == null 
                ? string.Empty 
                : TypeRewriter.NJoin("\n", new[]
                {
                    m_Rewriter.SetTarget(type).GetFormattedType(),
                    PrettyRichText(TypeRewriter.Description(type))
                }));
        }

        public void SetInfo(string text = null)
        {
            if (text == null) text = m_Text;
            m_Text = text;
            if (Info != null) Info.text = text;
            if (m_OverlayToolbar?.Info != null) m_OverlayToolbar.Info.text = text;
        }

        private void SetCurrentType(Type type)
        {
            m_CurrentType = type;
            var isReloadable = ReloadableAssemblyNames.Contains(type.Assembly.GetName().Name);
            m_OverlayToolbar?.GenDocsButton.SetVisible(isReloadable);
        }

        private string PrettyRichText(string text)
        {
            return text?.Replace("    ", "")
                .Replace("\r\n\r\n", "<br><br>")
                .Replace("\n\n", "<br><br>")
                .Replace("\r\r", "<br><br>")
                .Replace("\r\n", "")
                .Replace("\n", "")
                .Replace("\r", "");
        }
    
        public async void OnClickGenDocsButton()
        {
            if (m_CurrentType != null)
            {
                m_OverlayToolbar?.GenDocsButton.SetEnabled(false);
                var assemblyName = m_CurrentType.Assembly.GetName().Name;
                var sourceFilename = assemblyName + ".csproj";
                Debug.Log($"Assembly: {assemblyName}");
                foreach (var source in XmlDocumentationGenerator.GetAllGenerationSources())
                {
                    if (Path.GetFileName(source) == sourceFilename)
                    {
                        await XmlDocumentationGenerator.Generate(source);
                        DocumentationExtensions.RemoveAssemblyFromCache(m_CurrentType.Assembly);
                        SetValue(m_CurrentType);
                        Debug.Log($"done: {assemblyName}");
                        break;
                    }
                }
                m_OverlayToolbar?.GenDocsButton.SetEnabled(true);
            }
        }
    
        [EditorToolbarElement("TypeInfoHorizontalToolbar", typeof(QuickSearchWindow))]
        public class TypeInfoHorizontalToolbar : OverlayToolbar
        {
            public TextElement Info { get; private set; }
            public Button GenDocsButton { get; private set; }
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
                GenDocsButton = new Button(OnClickGenDocs)
                {
                    text = "RELOAD",
                    style =
                    {
                        position = Position.Absolute,
                        bottom = 1f,
                        right = 1f,
                        marginRight = 0f,
                        marginBottom = 0f,
                        borderBottomColor = new StyleColor(new Color32(0, 0, 0, 0)),
                        borderTopColor = new StyleColor(new Color32(0, 0, 0, 0)),
                        borderLeftColor = new StyleColor(new Color32(0, 0, 0, 0)),
                        borderRightColor = new StyleColor(new Color32(0, 0, 0, 0)),
                        borderBottomLeftRadius = 1f,
                        borderBottomRightRadius = 1f,
                        borderTopLeftRadius = 1f,
                        borderTopRightRadius = 1f,
                        backgroundColor = new StyleColor(new Color32(128, 128, 128, 35)),
                        paddingLeft = 4f,
                        paddingRight = 4f,
                        unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold),
                        fontSize = 8f,
                        color = new StyleColor(new Color32(255, 128, 0, 192)),
                        letterSpacing = 4f
                    }
                };
                GenDocsButton.RegisterCallback<PointerEnterEvent>(_ =>
                {
                    GenDocsButton.style.backgroundColor = new StyleColor(new Color32(128, 128, 128, 85));
                    GenDocsButton.style.color = new StyleColor(new Color32(255, 128, 0, 225));
                });
                GenDocsButton.RegisterCallback<PointerLeaveEvent>(_ =>
                {
                    GenDocsButton.style.backgroundColor = new StyleColor(new Color32(128, 128, 128, 35));
                    GenDocsButton.style.color = new StyleColor(new Color32(255, 128, 0, 192));
                });
                Add(GenDocsButton);
            }

            private void OnClickGenDocs() => m_Overlay.OnClickGenDocsButton();

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
}
