using Space3x.UiToolkit.QuickSearchComponent.Editor.VisualElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class QuickSearchUtilityWindow : EditorWindow
{
    [SerializeField]
    private StyleSheet m_StyleSheet;
        
    private QuickSearchElement m_Content;
    
    [MenuItem("Tools/QuickSearchUtilityWindow")]
    public static void ShowExample()
    {
        QuickSearchUtilityWindow wnd = GetWindow<QuickSearchUtilityWindow>();
        // wnd.titleContent = new GUIContent("QuickSearchUtilityWindow");
    }

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;
        
        if (root != null && m_StyleSheet != null)
        {
            root.styleSheets.Add(m_StyleSheet);
        }
        
        m_Content = new QuickSearchElement()
        {
            style = { flexGrow = 1f }
        };
        root.Add(m_Content);
    }
}
