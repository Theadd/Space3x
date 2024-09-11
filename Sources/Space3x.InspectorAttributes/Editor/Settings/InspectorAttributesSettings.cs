using System.Collections.Generic;
using Space3x.Attributes.Types;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.Settings
{
    public class InspectorAttributesSettings : ScriptableObject
    {
        [AllowExtendedAttributes] 
        [NoScript]
        [NoLabel]
        [EditableRichText]
        public string Description = "";

        [SerializeReference]
        private List<StyleSheet> m_ActiveStylesheets;
        
        // [ListSource(nameof(GetAvailableAssemblyNames))]
        public List<StyleSheet> ActiveStylesheets => m_ActiveStylesheets ??= new List<StyleSheet>()
        {
            Resources.Load<StyleSheet>("Space3x.InspectorAttributes.Stylesheet")
        };
        
        #region SETUP
        public const string SettingsPath = "Assets/Settings/Space3x/InspectorAttributesSettings.asset";
        
        private static InspectorAttributesSettings s_Instance;

        public static InspectorAttributesSettings Instance => s_Instance ??= GetOrCreateSettings();
        
        public static SerializedObject GetSerializedSettings() => new(GetOrCreateSettings());
        
        private static InspectorAttributesSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<InspectorAttributesSettings>(SettingsPath);
            if (settings == null)
            {
                settings = CreateInstance<InspectorAttributesSettings>();
                AssetDatabase.CreateAsset(settings, SettingsPath);
                AssetDatabase.SaveAssets();
            }
            
            return settings;
        }
        #endregion
    }
}
