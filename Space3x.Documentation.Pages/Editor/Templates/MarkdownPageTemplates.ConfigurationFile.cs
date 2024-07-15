using System.IO;
using Space3x.Documentation.Settings;
using Space3x.UiToolkit.Types;
using UnityEngine;

namespace Space3x.Documentation.Templates
{
    public partial class MarkdownPageTemplates
    {
	    public void CreateConfigurationFile(string assemblyName, string configurationTarget = "default")
	    {
// 		    var cfgFile = MdDocumentationGenerator.GetConfigurationFileName(assemblyName);
// 		    var genPath = Paths.AbsolutePath(MdDocumentationGenerator.GeneratedPath);
// 		    var combined = Path.Combine(genPath, cfgFile);
// 		    var combAbs = Paths.AbsolutePath(combined);
// 		    var combRelProj = Paths.RelativePath(combAbs);
// 		    var combRelGenPath = Paths.RelativePath(combAbs, genPath);
// 		    
// 		    var cfgFileAbs = MdDocumentationGenerator.GetConfigurationFileFullPath(assemblyName);
// 		    var dllPath = Paths.AssemblyProjectDllPath(assemblyName);
// 		    var dllPathAbs = Paths.AbsolutePath(Paths.AssemblyProjectDllPath(assemblyName));
// 		    var dllPathRelToCfgFileAbs = Paths.RelativePath(dllPathAbs, cfgFileAbs);
// 		    
// 		    Debug.Log(@$"<color=#000000FF><b>cfgFile: {cfgFile}
// genPath: {genPath}
// combined: {combined}
// combAbs: {combAbs}
// combRelProj: {combRelProj}
// combRelGenPath: {combRelGenPath}
// ------------------------------------------ FUCK ME!??!? asas
// cfgFileAbs: {cfgFileAbs}
// dllPath: {dllPath}
// dllPathAbs: {dllPathAbs}
// dllPathRelToCfgFileAbs: {dllPathRelToCfgFileAbs}
// </b></color>");
		    
		    WriteToFile(
			    MdDocumentationGenerator.GetConfigurationFileName(assemblyName), 
			    ConfigurationFile(assemblyName, configurationTarget), 
			    Paths.AbsolutePath(MdDocumentationGenerator.GeneratedPath));
	    }
	    
        private string ConfigurationFile(string assemblyName, string configurationTarget)
        {
	        var groupMembersByKind = true;
	        var generatedPages = "Assembly,Types";
	        var assemblyFileNameFactory = "Name";
	        var assemblyDocItemSections = @"""CustomTOCv2""";
	        var outputDirectoryPath = assemblyName;
	        var linksOutputFilePath = Paths
		        .RelativePath(
			        Path.Combine(MdDocumentationGenerator.GeneratedLinksPath,
				        MdDocumentationGenerator.GetLinksFileName(assemblyName)),
			        Paths.DirectoryPath(MdDocumentationGenerator.GetConfigurationFileFullPath(assemblyName)))
		        .Replace(@"\", @"\\");

	        if (configurationTarget == "sidebar")
	        {
		        generatedPages = "Assembly";
		        linksOutputFilePath = "";
		        assemblyFileNameFactory = "SidebarTOCName";
		        assemblyDocItemSections = @"""SidebarTOC""";
		        outputDirectoryPath = assemblyName + "-sidebar";
	        }

	        return $@"{{
	""AssemblyFilePath"": ""{Paths.RelativePath(Paths.AbsolutePath(Paths.AssemblyProjectDllPath(assemblyName)), Paths.DirectoryPath(MdDocumentationGenerator.GetConfigurationFileFullPath(assemblyName))).Replace(@"\", @"\\")}"",
	""OutputDirectoryPath"": ""{outputDirectoryPath}"",
	""Plugins"": [
		""..\\bin\\MdDocs3x\\DefaultDocumentation.MdDocs3x.dll""
	],
	""GeneratedAccessModifiers"": ""Public,Protected"",
	""GeneratedPages"": ""{generatedPages}"",
	""AssemblyPageName"": ""{assemblyName}"",
	""LinksBaseUrl"": ""#"",
	""ExternLinksFilePaths"": [
		"".\\{(
			Paths.RelativePath(
				Path.Combine(
					MdDocumentationGenerator.GeneratedLinksPath, 
					MdDocumentationGenerator.GetLinksFileName("*")), 
				Paths.DirectoryPath(
					MdDocumentationGenerator.GetConfigurationFileFullPath(assemblyName)))
				.Replace(@"\", @"\\"))}""
	],
	""LinksOutputFilePath"": ""{linksOutputFilePath}"",
	""AssemblyDocItem"": {{
		""FileNameFactory"": ""{assemblyFileNameFactory}"",
		""Sections"": [
			{assemblyDocItemSections}
		],
		""Markdown.TableOfContentsModes"": ""{(groupMembersByKind ? "Grouped" : "IncludeKind")},IncludeSummaryWithNewLine"",
		""Markdown.NestedTypeVisibilities"": ""DeclaringType""
	}},
	""MethodDocItem"": {{
		""Sections"": [
			{InlineDocItemSections}
		]
	}},
	""PropertyDocItem"": {{
		""Sections"": [
			{InlineDocItemSections}
		]
	}},
	""DelegateDocItem"": {{
		""Sections"": [
			{InlineDocItemSections}
		]
	}},
	""ConstructorDocItem"": {{
		""Sections"": [
			{InlineDocItemSections}
		]
	}},
	""EnumFieldDocItem"": {{
		""Sections"": [
			{InlineDocItemSections}
		]
	}},
	""EventDocItem"": {{
		""Sections"": [
			{InlineDocItemSections}
		]
	}},
	""ExplicitInterfaceImplementationDocItem"": {{
		""Sections"": [
			{InlineDocItemSections}
		]
	}},
	""FieldDocItem"": {{
		""Sections"": [
			{InlineDocItemSections}
		]
	}},
	""OperatorDocItem"": {{
		""Sections"": [
			{InlineDocItemSections}
		]
	}},
	""ClassDocItem"": {{
		""FileNameFactory"": ""Name"",
		""Sections"": [
			{StandaloneDocItemSections}
		],
		""Markdown.NestedTypeVisibilities"": ""DeclaringType""
	}},
	""StructDocItem"": {{
		""FileNameFactory"": ""Name"",
		""Sections"": [
			{StandaloneDocItemSections}
		],
		""Markdown.NestedTypeVisibilities"": ""DeclaringType""
	}},
	""InterfaceDocItem"": {{
		""FileNameFactory"": ""Name"",
		""Sections"": [
			{StandaloneDocItemSections}
		],
		""Markdown.NestedTypeVisibilities"": ""DeclaringType""
	}},
	""ProjectDirectoryPath"": ""..\\..\\..\\"",
	""Sections"": [
	  ""Header"",
	  ""Default""
	]
}}";
        }

        private static string InlineDocItemSections => @"""Foldout"", ""QuickDefinition"", ""summary"", ""FoldoutDetails"", ""Definition"", ""Inheritance"", 
			""Implement"", ""Derived"", ""seealso"", ""remarks"", ""example"", ""exception"", 
			""Fields"", ""Properties"", ""Constructors"", ""Methods"", ""Operators"", 
			""Events"", ""ExplicitInterfaceImplementations"", ""EndFoldout""";

        private static string StandaloneDocItemSections => @"""QuickDefinition"", ""summary"", ""remarks"", ""example"", ""seealso"", 
			""Definition"", ""Inheritance"", ""Implement"", ""Derived"", ""exception"", 
			""Fields"", ""Properties"", ""Constructors"", ""Methods"", ""Operators"", 
			""Events"", ""ExplicitInterfaceImplementations""";
    }
}
// ""FileNameFactory"": ""Name"", asdasdasd
// ""UrlFactories"": [
// ""DocItem""
// 	],
// ""NamespaceDocItem"": {{
// 	""Sections"": [
// 	""Title"",
// 	""TableOfContents""
// 		],
// 	""Markdown.TableOfContentsModes"": ""{(groupMembersByKind ? "Grouped" : "IncludeKind")},IncludeSummary"",
// 	""Markdown.NestedTypeVisibilities"": ""DeclaringType""
// }},
// ""NamespaceDocItem"": {{
// 	""FileNameFactory"": ""SidebarName"",
// 	""Sections"": [
// 	""Sidebar""
// 		],
// 	""Markdown.NestedTypeVisibilities"": ""Namespace""
// }},
// ""AssemblyDocItem"": {{
// 	""Sections"": [
// 	""Title"",
// 	""summary"",
// 	""TableOfContents""
// 		],
// 	""Markdown.TableOfContentsModes"": ""{(groupMembersByKind ? "Grouped" : "IncludeKind")},IncludeSummary"",
// 	""Markdown.NestedTypeVisibilities"": ""DeclaringType""
// }},
