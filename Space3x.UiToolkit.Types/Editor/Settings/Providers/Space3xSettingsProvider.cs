// using System.Collections.Generic;
// using UnityEditor;
// using UnityEngine;
// using UnityEngine.UIElements;
// using UnityEditor.UIElements;
//
// namespace Space3x.UiToolkit.Types.Settings.Providers
// {
//     // Create a new type of Settings Asset.
//     public class Space3xSettings : ScriptableObject
//     {
//         public const string SettingsPath = "Assets/Settings/Space3x/Settings.asset";
//
//         [SerializeField]
//         private int m_Number;
//
//         public static Space3xSettings GetOrCreateSettings()
//         {
//             var settings = AssetDatabase.LoadAssetAtPath<Space3xSettings>(SettingsPath);
//             if (settings == null)
//             {
//                 settings = ScriptableObject.CreateInstance<Space3xSettings>();
//                 AssetDatabase.CreateAsset(settings, SettingsPath);
//                 AssetDatabase.SaveAssets();
//             }
//             return settings;
//         }
//
//         public static SerializedObject GetSerializedSettings()
//         {
//             return new SerializedObject(GetOrCreateSettings());
//         }
//     }
//     
//     // Register a SettingsProvider using UIElements for the drawing framework:
//     public static class Space3xSettingsProviderRegister
//     {
//         [SettingsProvider]
//         public static SettingsProvider CreateMdDocsSettingsProvider()
//         {
//             // First parameter is the path in the Settings window.
//             // Second parameter is the scope of this setting: it only appears in the Settings window for the Project scope.
//             var provider = new SettingsProvider("Project/Space3x/Settings", SettingsScope.Project)
//             {
//                 label = "Space3x Settings",
//                 // activateHandler is called when the user clicks on the Settings item in the Settings window.
//                 activateHandler = (searchContext, rootElement) =>
//                 {
//                     var settings = Space3xSettings.GetSerializedSettings();
//                     var container = new VisualElement();
//                     container.AddToClassList("uitoolkit-settings-container");
//                     var title = new Label()
//                     {
//                         text = "Markdown Docs Generation"
//                     };
//                     title.AddToClassList("uitoolkit-settings-header");
//                     container.Add(title);
//                     var someButton = new Button(() => Debug.Log("Hey!")) { text = "Some Button" };
//                     container.Add(someButton);
//                     var inspectorElement = new InspectorElement(settings);
//                     container.Add(inspectorElement);
//
//                     rootElement.Add(container);
//                 },
//
//                 // Populate the search keywords to enable smart search filtering and label highlighting:
//                 keywords = new HashSet<string>(new[] { "Settings", "Space3x" })
//             };
//
//             return provider;
//         }
//     }
// }
