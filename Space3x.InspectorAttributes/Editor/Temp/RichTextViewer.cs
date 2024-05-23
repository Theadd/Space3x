using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor
{
    [InitializeOnLoad]
    public class RichTextViewer : EditorWindow
    {
        [SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;

        private static List<string> s_Titles = new List<string>();
        private static List<string> s_Texts = new List<string>();

        private ListView m_List;
        private Label m_Content;
        private VisualElement m_Container;

        public static void AddText(string title, string text)
        {
            s_Titles.Add(title);
            s_Texts.Add(text);
        }
        
        public static int Count() => s_Titles.Count;
        
        #region EditorWindow stuff
        static RichTextViewer() => EditorApplication.update += Startup;
        static void Startup()
        {
            EditorApplication.update -= Startup;
            ShowWindow();
        }

        private static void ShowWindow()
        {
            var window = GetWindow<RichTextViewer>();
            window.titleContent = new GUIContent("SampleEditor2024");
            window.Show();
        }
    
        [MenuItem("Tools/RichTextViewer")]
        public static void ShowExample()
        {
            RichTextViewer wnd = GetWindow<RichTextViewer>();
            wnd.titleContent = new GUIContent("RichTextViewer");
        }
        #endregion
        
        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Instantiate UXML
            m_Container = m_VisualTreeAsset.Instantiate();

            m_List = m_Container.Q<ListView>("RTViewerList");
            m_Content = m_Container.Q<Label>("RTViewerContent");
            m_List.makeItem = () => new Label();
            m_List.bindItem = (element, i) => (element as Label).text = s_Titles[i];
            m_List.itemsSource = s_Titles;

            m_List.selectionChanged -= OnSelectionChanged;
            m_List.selectionChanged += OnSelectionChanged;

            root.Add(m_Container);
        }

        private void OnSelectionChanged(IEnumerable<object> obj)
        {
            m_Content.text = s_Texts[m_List.selectedIndex];
        }
    }
}
