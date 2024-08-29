using System.Collections.Generic;
using System.IO;
using System.Linq;
using Space3x.Documentation.Settings;
using Space3x.UiToolkit.Types;
using UnityEngine;

namespace Space3x.Documentation.Templates
{
    public partial class MarkdownPageTemplates
    {
        public string Sidebar(MdDocsSettings settings)
        {
            List<AssemblyDocumentationToC> assembliesToC = null;
            List<string> res = new List<string>();
            var baseUrl = "./" + Paths.RelativePath(Paths.AbsolutePath(MdDocumentationGenerator.GeneratedPath),
                Paths.AbsolutePath(MdDocumentationGenerator.DocumentationPath)).Replace(@"\", "/");
            if (!baseUrl.EndsWith("/")) baseUrl += "/";
            Debug.Log($"[Sidebar] baseUrl: |{baseUrl}|");
            
            // base-sidebar.md
            foreach (string baseSidebarLine in File.ReadLines(Path.Combine(
                         Paths.TrailingSlash(Paths.AbsolutePath(MdDocumentationGenerator.DocumentationPath)),
                         "base-sidebar.md")))
            {
                res.Add(baseSidebarLine);
            }
            
            // - [Space3x.Attributes.Types](./g/Space3x.Attributes.Types.md "Space3x.Attributes.Types")
            // - [Space3x.InspectorAttributes.Editor](./g/Space3x.InspectorAttributes.Editor.md "Space3x.InspectorAttributes.Editor")
            // - [Space3x.UiToolkit.Types](./g/Space3x.UiToolkit.Types.md "Space3x.UiToolkit.Types")
            // - [Space3x.UiToolkit.Types.Editor](./g/Space3x.UiToolkit.Types.Editor.md "Space3x.UiToolkit.Types.Editor")
            
            List<string> allSidebarDirectories = Directory.EnumerateDirectories(
                    Paths.AbsolutePath(MdDocumentationGenerator.GeneratedPath), 
                    "*-sidebar", 
                    SearchOption.TopDirectoryOnly)
                .ToList();
            allSidebarDirectories.Sort();

            assembliesToC = allSidebarDirectories.Select(d => AssemblyDocumentationToC.Create(d)).ToList();
            
            for (var i = 0; i < assembliesToC.Count; i++)
            {
                List<string> assemblySidebar = new List<string>(res);
                for (var j = 0; j < assembliesToC.Count; j++)
                {
                    assemblySidebar.Add(assembliesToC[j].GetSidebarItem(baseUrl));
                    if (j == i)
                    {
                        assemblySidebar = assemblySidebar.Concat(assembliesToC[j].GetSidebarItemContent()).ToList();
                    }
                }
                assemblySidebar.Add("");
                WriteToFile("sidebar.md", string.Join("\n", assemblySidebar),
                    Path.Combine(MdDocumentationGenerator.GeneratedPath, assembliesToC[i].AssemblyName));
            }

            for (var e = 0; e < assembliesToC.Count; e++)
            {
                res.Add(assembliesToC[e].GetSidebarItem(baseUrl));
                assembliesToC[e].MoveAssemblyToCFile();
            }
            res.Add("");
            WriteToFile("sidebar.md", string.Join("\n", res), MdDocumentationGenerator.GeneratedPath);
            
            return string.Join("\n", res);
        }
    }

    public class AssemblyDocumentationToC
    {
        public string SidebarDirectory;
        public string AssemblyName;
        
        public static AssemblyDocumentationToC Create(string fromSidebarDirectory)
        {
            var res = new AssemblyDocumentationToC();
            res.SidebarDirectory = Paths.TrailingSlash(fromSidebarDirectory);
            var dirName = Path.GetFileName(Path.GetDirectoryName(res.SidebarDirectory));
            res.AssemblyName = dirName.Substring(0, dirName.Length - 8);
            return res;
        }
        
        public string GetSidebarItem(string basePath) => 
            $"- [{AssemblyName}]({basePath}{AssemblyName}.md \"{AssemblyName}\")";

        public List<string> GetSidebarItemContent() => 
            File.ReadLines(Path.Combine(SidebarDirectory, AssemblyName + ".md")).ToList();
        
        public void MoveAssemblyToCFile() => 
            File.Move(GetAssemblyToCOriginalPath(), GetAssemblyToCFinalPath());
        
        private string GetAssemblyToCOriginalPath() => 
            Path.Combine(
                Path.Combine(
                    Paths.TrailingSlash(Path.GetDirectoryName(Path.GetDirectoryName(SidebarDirectory))), 
                    AssemblyName), 
                AssemblyName + ".md");
        
        private string GetAssemblyToCFinalPath() => 
            Path.Combine(
                Paths.TrailingSlash(Path.GetDirectoryName(Path.GetDirectoryName(SidebarDirectory))), 
                AssemblyName + ".md");
    }
}
