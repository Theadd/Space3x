using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.Documentation.Settings.Providers
{
    // Register a SettingsProvider using UIElements for the drawing framework:
    public static class XmlDocsSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateXmlDocsSettingsProvider()
        {
            // First parameter is the path in the Settings window.
            // Second parameter is the scope of this setting: it only appears in the Settings window for the Project scope.
            var provider = new SettingsProvider("Project/Space3x/XmlDocsSettings", SettingsScope.Project)
            {
                label = "Xml Comments Doc Generation",
                // activateHandler is called when the user clicks on the Settings item in the Settings window.
                activateHandler = (searchContext, rootElement) =>
                {
                    var settings = XmlDocsSettings.GetSerializedSettings();
                    var container = new VisualElement()
                    {
                        // style =
                        // {
                        //     backgroundColor = new StyleColor(Color.yellow)
                        // }
                    };
                    container.AddToClassList("uitoolkit-settings-container");
                    var title = new Label()
                    {
                        text = "Xml Comments Documentation Generation"
                    };
                    title.AddToClassList("uitoolkit-settings-header");
                    container.Add(title);
                    var someButton = new Button(() => Debug.Log("Hey!")) { text = "Some Button" };
                    container.Add(someButton);
                    var inspectorElement = new InspectorElement(settings);
                    container.Add(inspectorElement);
                    // var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/settings_ui.uss");
                    // rootElement.styleSheets.Add(styleSheet);
                    
                    
                    rootElement.Add(container);

                    // var properties = new VisualElement()
                    // {
                    //     style =
                    //     {
                    //         flexDirection = FlexDirection.Column
                    //     }
                    // };
                    // properties.AddToClassList("property-list");
                    // rootElement.Add(properties);
                    //
                    // properties.Add(new PropertyField(settings.FindProperty("m_SomeString")));
                    // properties.Add(new PropertyField(settings.FindProperty("m_Number")));

                    // rootElement.Bind(settings);
                },

                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[] { "Number", "Some String" })
            };

            return provider;
        }
    }
}
