using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Space3x.Attributes.Types;
using Space3x.Documentation.Templates;
using Space3x.UiToolkit.Types;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Space3x.Documentation
{
    public static class MdDocumentationGenerator
    {
        public static string DocumentationPath = Paths.Resolve(Path.Combine(Paths.project, "Library", "Documentation"));
        public static bool HasExistingDocumentation => File.Exists(Path.Combine(DocumentationPath, "index.html"));
        
        private static string s_GenDocsPath = Paths.Resolve(Path.Combine(DocumentationPath, "g"));
        private static string s_GeneratedPath = Paths.Resolve(Path.Combine(s_GenDocsPath, ""));
        private static string s_GeneratedLinksPath = Paths.Resolve(Path.Combine(s_GenDocsPath, ""));

        public static string GeneratedPath => s_GeneratedPath;
        public static string GeneratedLinksPath => s_GeneratedLinksPath;
        
        public static string GetLinksFileName(string assemblyName) => $"{assemblyName}-links.txt";
        
        public static string GetConfigurationFileName(string assemblyName) => $"{assemblyName}-config.json";
        
        public static string GetConfigurationFileFullPath(string assemblyName) => Paths.AbsolutePath(Path.Combine(GeneratedPath, GetConfigurationFileName(assemblyName)));

        private static string CreatePath(string path) => 
            !Directory.Exists(path) ? Directory.CreateDirectory(path).FullName : path;

        public static IEnumerable<string> AvailableAssemblyNames => 
            Directory.EnumerateFiles(XmlDocumentationGenerator.XmlAssemblyDocs, "*.dll", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileNameWithoutExtension);
        
        public static void ClearAnyPreviousGeneratedFiles()
        {
            if (Directory.Exists(GeneratedPath))
                Directory.Delete(GeneratedPath, recursive: true);
            CreatePath(GeneratedPath);
            if (GeneratedLinksPath == GeneratedPath) return;
            if (Directory.Exists(GeneratedLinksPath))
                Directory.Delete(GeneratedLinksPath, recursive: true);
            CreatePath(GeneratedLinksPath);
        }
        
        private static string Execute(string binaryPath, string arguments)
        {
            const int timeout = 1800000;    // 300000;
            using (var process = new Process 
                   { StartInfo = new ProcessStartInfo
                       {
                           FileName = binaryPath,
                           Arguments = arguments,
                           UseShellExecute = false,
                           RedirectStandardOutput = true,
                           RedirectStandardError = true,
                           CreateNoWindow = true,
                           WorkingDirectory = Paths.project
                       }
                   }) 
            {
                var output = new StringBuilder();
                var error = new StringBuilder();

                using (var outputWaitHandle = new AutoResetEvent(false))
                using (var errorWaitHandle = new AutoResetEvent(false))
                {
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                            outputWaitHandle.Set();
                        else
                            output.AppendLine(e.Data);
                    };

                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                            errorWaitHandle.Set();
                        else
                            error.AppendLine(e.Data);
                    };
                    
                    // Debug.Log($"Process: {projectBuilderPath} {process.StartInfo.Arguments}\n");
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    
                    if (process.WaitForExit(timeout) &&
                        outputWaitHandle.WaitOne(timeout) &&
                        errorWaitHandle.WaitOne(timeout))
                    {
                        if (process.ExitCode != 0) 
                            DebugLog.Error(new BuildFailedException($"Failed to build: {process.StartInfo.Arguments}\n{error}\n{output}").ToString());
                        DebugLog.Info($"[DONE] {process.StartInfo.Arguments}\n{output}\n{error}");
                        return output.ToString();
                    }
                    else
                    {
                        DebugLog.Error(new TimeoutException($"Build process timed out ({(timeout / 1000)} seconds).\n" +
                                                   arguments).ToString());
                        return string.Empty;
                    }
                }
            }
        }

        private static string GenerateInternal(string binaryPath, string assemblyName)
        {
            // ./resources/DefaultDocumentation/DefaultDocumentation.Console.exe -a ./resources/Space3x.InspectorAttributes.Editor.dll
            // --FileNameFactory Name -g Types -n Space3x.InspectorAttributes.Editor -o ./generated/Space3x.InspectorAttributes.Editor/ -p ../../../

            var linksPath = Paths.Resolve(Path.Combine(GeneratedLinksPath, GetLinksFileName(assemblyName)));
            // var args = $"-j {GetConfigurationFileFullPath(assemblyName)} --FileNameFactory Name --UrlFactories DocItem|MdDocs3xUrl";
            var args = $"-j {GetConfigurationFileFullPath(assemblyName)} --FileNameFactory Name --UrlFactories MdDocs3xUrl";
            // var args = $"-a {assemblyPath} --FileNameFactory Name -g Types -n {assemblyName} -o {outputPath} -l {linksPath} -p .\\";
            // var args = $"-a {assemblyPath} --FileNameFactory Name -g Types -n {assemblyName} -o {outputPath} -p .\\";

            var res = Execute(binaryPath, args);
            MdLinkParser.Parse(linksPath, assemblyName);

            return res;
        }

        private static string GenerateSidebarTOC(string binaryPath, string assemblyName)
        {
            MarkdownPageTemplates.Instance.CreateConfigurationFile(assemblyName, "sidebar");
            // var args = $"-j {GetConfigurationFileFullPath(assemblyName)} --FileNameFactory Name --UrlFactories DocItem|MdDocs3xUrl";
            var args = $"-j {GetConfigurationFileFullPath(assemblyName)} --FileNameFactory Name --UrlFactories MdDocs3xUrl";
            var res = Execute(binaryPath, args);
            return res;
        }
        
        public static async Awaitable<string> Generate(string binaryPath, string assemblyName)
        {
            EditorApplication.LockReloadAssemblies();
            await Awaitable.BackgroundThreadAsync();
            var csprojPath = Paths.Resolve(Path.Combine(Paths.project, assemblyName + ".csproj"));
            if (File.Exists(csprojPath))
                await XmlDocumentationGenerator.GenerateInternal(csprojPath);
            MarkdownPageTemplates.Instance.CreateConfigurationFile(assemblyName);
            var output = GenerateInternal(binaryPath, assemblyName);
            var sidebarOutput = GenerateSidebarTOC(binaryPath, assemblyName);
            await Awaitable.MainThreadAsync();
            EditorApplication.UnlockReloadAssemblies();

            return output + "\n\n" + sidebarOutput;
        }
    }
}
