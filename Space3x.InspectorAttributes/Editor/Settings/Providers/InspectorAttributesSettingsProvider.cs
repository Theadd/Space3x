using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Settings
{
    // Register a SettingsProvider using UIElements for the drawing framework:
    public static class InspectorAttributesSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateInspectorAttributesSettingsProvider()
        {
            // First parameter is the path in the Settings window.
            // Second parameter is the scope of this setting: it only appears in the Settings window for the Project scope.
            var provider = new SettingsProvider("Project/Space3x/InspectorAttributesSettings", SettingsScope.Project)
            {
                label = "Inspector Attributes",
                // activateHandler is called when the user clicks on the Settings item in the Settings window.
                activateHandler = (searchContext, rootElement) =>
                {
                    var settings = InspectorAttributesSettings.GetSerializedSettings();
                    var container = new VisualElement();
                    container.AddToClassList("uitoolkit-settings-container");
                    var title = new Label()
                    {
                        text = "Inspector Attributes"
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
                keywords = new HashSet<string>(new[] { "Inspector", "Attribute", "Attributes" })
                // keywords = new HashSet<string>(
                //     L10n.Tr(new[] { "Inspector", "Attribute", "Attributes" })
                // )
            };

            return provider;
        }
    }
}
