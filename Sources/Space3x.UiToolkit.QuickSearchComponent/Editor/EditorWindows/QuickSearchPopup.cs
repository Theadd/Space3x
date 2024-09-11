using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.QuickSearchComponent.Editor.VisualElements
{
    public interface IQuickSearchable
    {
        void OnShow(QuickSearchElement element);
        void OnHide(QuickSearchElement element);
    }
    
    public enum ShowWindowMode
    {
        Popup,
        Utility,
        Modal,
        AuxWindow,
        ModalUtility,
        MainWindow,
        NormalWindow,
    }
    
    public class QuickSearchPopup : PopupWindowContent
    {
        public static StyleSheet StyleSheet = Resources.Load<StyleSheet>("DefaultQuickSearchElementStylesheet");

        private EditorWindow m_WindowContainer;

        public EditorWindow WindowContainer
        {
            get => m_WindowContainer ?? editorWindow;
            set => m_WindowContainer = value;
        }

        private QuickSearchElement m_Content;

        private IQuickSearchable m_Searchable;

        private Vector2 m_Size = new Vector2(300, 400);

        public QuickSearchPopup WithSize(Vector2 size)
        {
            m_Size = size;
            return this;
        }
        
        public QuickSearchPopup WithSearchable(IQuickSearchable searchable)
        {
            m_Searchable = searchable;
            return this;
        }
        
        public QuickSearchPopup WithContent(QuickSearchElement content)
        {
            m_Content = content;
            return this;
        }
        
        public QuickSearchElement PopupContent => m_Content;
        
        public override Vector2 GetWindowSize() => m_Size;

        public override void OnGUI(Rect rect)
        {
            // Intentionally left empty
        }

        public override void OnOpen()
        {
            var root = WindowContainer.rootVisualElement;
            if (StyleSheet != null)
            {
                if (!root.styleSheets.Contains(StyleSheet))
                    root.styleSheets.Add(StyleSheet);
            }
            m_Content ??= new QuickSearchElement() { };
            m_Content.style.flexGrow = 1f;
            root.Add(m_Content);
            m_Searchable?.OnShow(m_Content);
        }
            
        public override void OnClose()
        {
            WindowContainer = null;
            m_Searchable?.OnHide(m_Content);
        }

        public void ShowAsContext(ShowWindowMode mode = ShowWindowMode.Popup)
        {
            if (Event.current != null) 
                Show(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 0f, 0f), mode);
        }

        public void Show(VisualElement target, ShowWindowMode mode = ShowWindowMode.Popup) => Show(target.worldBound, mode);

        public void Show(Rect position, ShowWindowMode mode = ShowWindowMode.Popup)
        {
            switch (mode)
            {
                case ShowWindowMode.Popup:
                    UnityEditor.PopupWindow.Show(position, this.WithSize(new Vector2(Mathf.Max(position.width, m_Size.x), m_Size.y)));
                    break;
                case ShowWindowMode.Utility:
                case ShowWindowMode.Modal:
                case ShowWindowMode.AuxWindow:
                case ShowWindowMode.ModalUtility:
                case ShowWindowMode.MainWindow:
                case ShowWindowMode.NormalWindow:
                default:
                    QuickSearchWindow.Show(this, mode);
                    break;
            }
        }

        public void Close() => WindowContainer?.Close();
    }
}
