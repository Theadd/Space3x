using System.IO;
using Space3x.Documentation.Settings;
using UnityEditor;

namespace Space3x.Documentation.Templates
{
    public partial class MarkdownPageTemplates
    {
        private static MarkdownPageTemplates s_Instance = null;

        public static MarkdownPageTemplates Instance => s_Instance ??= new MarkdownPageTemplates();

        public void SaveAll(MdDocsSettings settings)
        {
            if (settings.AddCoverPage) 
                WriteToFile(CoverPageFileName, settings.CoverPage);
            
            WriteToFile(AllLinksFileName, AllLinks(settings));
            WriteToFile(PageIndexFileName, PageIndex(settings));
            WriteToFile(SidebarFileName, Sidebar(settings));
        }

        private void WriteToFile(string fileName, string content, string directoryPath = null)
        {
            using (var stream = File.CreateText(Path.Combine(directoryPath ?? MdDocumentationGenerator.DocumentationPath, fileName)))
            {
                stream.WriteLine(content);
            }	
        }
    }
}
