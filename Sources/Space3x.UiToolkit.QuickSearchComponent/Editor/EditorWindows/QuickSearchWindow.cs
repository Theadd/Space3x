using System;
using System.Collections.Generic;
using System.Linq;
using Space3x.Properties.Types;
using Space3x.UiToolkit.QuickSearchComponent.Editor.VisualElements;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.QuickSearchComponent.Editor
{
    [EditorWindowTitle(icon = "d_SearchWindow", title = "Quick Type Search")]
    public class QuickSearchWindow : EditorWindow, ISupportsOverlays
    {
        [SerializeField]
        protected List<StyleSheet> StyleSheets = new List<StyleSheet>();

        public QuickSearchPopup WindowContent;

        public static QuickSearchPopup WorkingWindowContent { get; set; }

        private bool m_ShowOverlay = true;
        private TypeInfoOverlay m_Overlay;
        
        // @see: public void ShowNotification(GUIContent notification, double fadeoutWait)

        public static void Show(QuickSearchPopup windowContent, ShowWindowMode mode = ShowWindowMode.Utility)
        {
            if (EditorWindow.HasOpenInstances<QuickSearchWindow>())
            {
                var prevWindow = GetWindow<QuickSearchWindow>();
                prevWindow.Close();
            }
            QuickSearchWindow window = GetWindow<QuickSearchWindow>();
            // window.titleContent = new GUIContent("Quick Type Search");
            window.WindowContent = windowContent;
            WorkingWindowContent = windowContent;
            window.WindowContent.WindowContainer = window;
            switch (mode)
            {
                case ShowWindowMode.Utility:
                    window.ShowUtility();
                    break;
                case ShowWindowMode.Modal:
                    window.ShowModal();
                    break;
                case ShowWindowMode.Popup:
                    window.ShowPopup();
                    break;
                case ShowWindowMode.AuxWindow:
                    window.ShowAuxWindow();
                    break;
                case ShowWindowMode.NormalWindow:
                    window.Show();
                    break;
                case ShowWindowMode.ModalUtility:
                    window.ShowModalUtility();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
        
            if (StyleSheets != null && StyleSheets.Count > 0)
            {
                foreach (var styleSheet in StyleSheets)
                {
                    if (!root.styleSheets.Contains(styleSheet))
                        root.styleSheets.Add(styleSheet);
                }
            }
            m_KeepTryingTask = root.schedule.Execute(KeepTrying).Every(10);
            m_KeepTryingTask.ExecuteLater(1);
        }

        private IVisualElementScheduledItem m_KeepTryingTask;
#if !UNITY_6000_0_OR_NEWER
        private static Accessor s_OverlayAccessor;
        private static IAccessor s_Overlays;
#endif
        
        protected virtual void KeepTrying()
        {
            WindowContent ??= WorkingWindowContent;
            
            if (WindowContent != null)
            {
                m_KeepTryingTask.Pause();
                if (m_ShowOverlay)
                    overlayCanvas.Add(m_Overlay);
                else
                    overlayCanvas.Remove(m_Overlay);
                WorkingWindowContent = null;
                WindowContent.OnOpen();
                WindowContent.PopupContent.RegisterValueChangedCallback(OnSelectionChanged);
                if (m_ShowOverlay)
                    m_Overlay.SetValue(WindowContent.PopupContent.value?.FirstOrDefault());
#if UNITY_6000_0_OR_NEWER
                foreach (var overlay in overlayCanvas.overlays)
#else
                // private List<Overlay> m_Overlays = new List<Overlay>();
                s_OverlayAccessor ??= Accessor.Create<OverlayCanvas>();
                s_Overlays ??= s_OverlayAccessor.GetMember("m_Overlays");
                var overlays = s_Overlays.GetValue<List<Overlay>>(overlayCanvas);
                foreach (var overlay in overlays)
#endif
                    if (overlay.GetType().Name == "OverlayMenu")
                        overlay.displayed = false;
            }
        }

        private void OnSelectionChanged(ChangeEvent<IEnumerable<Type>> ev)
        {
            Type selectedType = ev.newValue.FirstOrDefault();
            m_Overlay.SetValue(selectedType);
        }

        private void OnEnable() => m_Overlay = new TypeInfoOverlay(this);

        private void OnDisable()
        {
            WindowContent?.PopupContent.UnregisterValueChangedCallback(OnSelectionChanged);
            WindowContent?.OnClose();
        }
    }
}
