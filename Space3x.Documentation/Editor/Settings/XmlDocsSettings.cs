using System.Linq;
using Space3x.Attributes.Types;
using Space3x.UiToolkit.Types;
using UnityEditor;
using UnityEngine;

namespace Space3x.Documentation.Settings
{
    public class XmlDocsSettings : ScriptableObject
    {
        public const string XmlDocsSettingsPath = "Assets/Settings/Space3x/XmlDocsSettings.asset";

        [SerializeField]
        private int m_Number;

        [SerializeField]
        [EnableOn(nameof(AllVisible))]
        private string m_SomeString;

        [Green]
        [Button(nameof(SaySomething))]
        public float SomeFloatValue = 0.65f;

        [Yellow]
        [EnableOn(nameof(IsBusyGeneratingXmlDocs), Enabled = false)]
        [Button(nameof(GenerateXmlDocs))]
        [Button(nameof(ClearAssemblyCache))]
        [Button(nameof(PrintAllGenerationSources))]
        public bool ShowAll = true;
        
        [Visible(false)]
        public bool m_IsGeneratingXmlDocs = false;
        public void SaySomething() => Debug.Log("Something!");

        public bool AllVisible()
        {
            return ShowAll;
        }

        public bool IsBusyGeneratingXmlDocs()
        {
            return m_IsGeneratingXmlDocs;
        }
        
        private async void GenerateXmlDocs()
        {
            m_IsGeneratingXmlDocs = true;
            Debug.Log("BEGIN");
            await XmlDocumentationGenerator.GenerateAll();
            m_IsGeneratingXmlDocs = false;
            Debug.Log("END");
        }

        private void ClearAssemblyCache() => DocumentationExtensions.ClearAssemblyCache();

        private void PrintAllGenerationSources()
        {
            XmlDocumentationGenerator.GetAllGenerationSources().ToList().ForEach(Debug.Log);
        }

        public static XmlDocsSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<XmlDocsSettings>(XmlDocsSettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<XmlDocsSettings>();
                settings.m_Number = 42;
                settings.m_SomeString = "The answer to the universe";
                AssetDatabase.CreateAsset(settings, XmlDocsSettingsPath);
                AssetDatabase.SaveAssets();
            }
            return settings;
        }

        public static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
    }
}