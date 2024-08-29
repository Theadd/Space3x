using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.Documentation.Settings.Providers
{
    // Register a SettingsProvider using UIElements for the drawing framework:
    public static class MdDocsSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateMdDocsSettingsProvider()
        {
            // First parameter is the path in the Settings window.
            // Second parameter is the scope of this setting: it only appears in the Settings window for the Project scope.
            var provider = new SettingsProvider("Project/Space3x/MdDocsSettings", SettingsScope.Project)
            {
                label = "Markdown Docs Generation",
                // activateHandler is called when the user clicks on the Settings item in the Settings window.
                activateHandler = (searchContext, rootElement) =>
                {
                    var settings = MdDocsSettings.GetSerializedSettings();
                    var container = new VisualElement();
                    container.AddToClassList("uitoolkit-settings-container");
                    var title = new Label()
                    {
                        text = "Markdown Docs Generation"
                    };
                    title.AddToClassList("uitoolkit-settings-header");
                    container.Add(title);
                    var someButton = new Button(() => Debug.Log("Hey!")) { text = "Some Button" };
                    container.Add(someButton);
                    var inspectorElement = new InspectorElement(settings);
                    container.Add(inspectorElement);

                    rootElement.Add(container);
                },

                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[] { "Markdown", "Docs" })
            };

            return provider;
        }
    }
}
