using System.Collections.Generic;
using System.Linq;
using Space3x.Attributes.Types;
using Space3x.Documentation.Templates;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Space3x.Documentation.Settings
{
    public enum MdPageThemes
    {
        Vue,
        Basic,
        SimpleLight,
        SimpleDark,
        SimpleAuto,
    }
    
    public class MdDocsSettings : ScriptableObject
    {
        // [AllowExtendedAttributes]
        [NoScript]
        [SerializeReference]
        [ListSource(nameof(GetAvailableAssemblyNames))]
        public List<string> TargetAssemblies = new List<string>();
        
        // [ShowInInspector]
        // [ListSource(nameof(GetAvailableAssemblyNames))]
        // private List<string> OtherAssemblies = new List<string>();
        
        [FileDialog(Title = "Select DefaultDocumentation.Console.exe", Text = "Path To DefaultDocumentation Binary")]
        public string DefaultDocumentationBinary = "Library/Documentation/gen-docs/resources/DefaultDocumentation/DefaultDocumentation.Console.exe";

        [Visible(false)]
        public bool IsGeneratingMdDocs = false;
        
        [Visible(false)]
        public bool IsServerRunning = false;
        
        public MdPageThemes PageTheme = MdPageThemes.Vue;

        public bool AddCoverPage = true;

        [Multiline]
        public string CoverPage = @"# docsify-themeable

> A delightfully simple theme system for [docsify.js](https://docsify.js.org)

- Based on CSS custom properties
- No packages to install or files to build
- Improved desktop and mobile experience
- Multiple themes available
- Legacy browser support (IE10+)

[Get Started](#main)
[GitHub](https://github.com/jhildenbiddle/docsify-themeable)
[Unity Asset Store](https://www.npmjs.com/package/docsify-themeable)
";
        
        [Multiline]
        public string ReadmePage = @"# Demo

This sandbox is a demonstration of the [docsify-themeable](https://jhildenbiddle.github.io/docsify-themeable/) theme system for [docsify.js](https://docsify.js.org/). 

?> You can ignore the `package.json` file in this sandbox. This file is required by codesandbox.io, not [docsify.js](https://docsify.js.org/).
";
        
        public string PageTitle = "Documentation Page";
        
        public string PageDescription = "";
        
        public string RepoUrl = "https://github.com/User/Repository/";
        
        public bool AddCopyCodeToClipboardPlugin = true;
        
        public bool AddSearchPlugin = true;
        
        [EnableOn(nameof(IsGeneratingMdDocs), Enabled = false)]
        [Button(nameof(Generate), Text = "Generate Markdown Pages")]
        [Button(nameof(RebuildStaticPages))]
        [Header("Preview (HTTP Server)")]
        [EnableOn(nameof(IsServerRunning), Enabled = false)]
        [Button(nameof(StartServer))]
        [EnableOn(nameof(IsServerRunning), Enabled = true)]
        [Button(nameof(StopServer))]
        [EnableOn(nameof(IsServerRunning), Enabled = true)]
        [Button(nameof(PreviewInBrowser))]
        public bool ShowAll = true;
        
        // private static IEnumerable<string> ProjectFiles => 
        //     Directory.EnumerateFiles(Project, "*.csproj", SearchOption.TopDirectoryOnly);

        public List<string> GetAvailableAssemblyNames() => MdDocumentationGenerator.AvailableAssemblyNames.ToList();

        private async void Generate()
        {
            IsGeneratingMdDocs = true;
            MdDocumentationGenerator.ClearAnyPreviousGeneratedFiles();
            MdLinkParser.Reset();
            foreach (var targetAssembly in TargetAssemblies)
            {
                var output = await MdDocumentationGenerator.Generate(DefaultDocumentationBinary, targetAssembly);
                Debug.Log(output);
            }
            IsGeneratingMdDocs = false;
        }

        private void RebuildStaticPages()
        {
            MarkdownPageTemplates.Instance.SaveAll(this);
        }

        public void HandleServerRunningChanged(bool isRunning)
        {
            IsServerRunning = isRunning;
        }

        private void StartServer()
        {
            MdDocumentationPreviewer.OnServerRunningChanged -= HandleServerRunningChanged;
            MdDocumentationPreviewer.OnServerRunningChanged += HandleServerRunningChanged;
            MdDocumentationPreviewer.Start();
        }

        private void StopServer() => MdDocumentationPreviewer.Stop();

        private void PreviewInBrowser() => MdDocumentationPreviewer.Open();

        #region SETUP
        public const string SettingsPath = "Assets/Settings/Space3x/MdDocsSettings.asset";

        public static SerializedObject GetSerializedSettings() => new(GetOrCreateSettings());
        
        private static MdDocsSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<MdDocsSettings>(SettingsPath);
            if (settings == null)
            {
                settings = CreateInstance<MdDocsSettings>();
                AssetDatabase.CreateAsset(settings, SettingsPath);
                AssetDatabase.SaveAssets();
            }
            
            return settings;
        }
        #endregion
    }
}