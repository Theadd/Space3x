using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Space3x.Attributes.Types;
using UnityEditor.Build;
using UnityEngine;

namespace Space3x.UiToolkit.Types
{
    public static class XmlDocumentationGenerator
    {
        public static string Project => Directory.GetParent(Application.dataPath)?.FullName;
        public static string XmlAssemblyDocs => CreatePath(Path.Combine(Project, "Library", "XmlAssemblyDocs"));
        public static string IntermediateOutputPath => CreatePath(Path.Combine(Project, "Temp", "Bin", "Debug"));
        public static string MsBuildPath { get; set; } = Paths.projectBuilder;
        
        private static string CreatePath(string path) => 
            !Directory.Exists(path) ? Directory.CreateDirectory(path).FullName : path;

        private static IEnumerable<string> SolutionFiles => 
            Directory.EnumerateFiles(Project, "*.sln", SearchOption.TopDirectoryOnly);

        private static void Validate()
        {
            if (MsBuildPath == null || !File.Exists(MsBuildPath))
                throw new Exception("MsBuild.exe not found. You can download the latest MSBuild from: " + Paths.MsBuildDownloadLink);
        }
        
        /// <summary>
        /// Generates the *.xml documentation of all csproj files in the project after building
        /// the *.sln (VS solution file) and copying all Unity's generated Xml documentation files to
        /// Library/XmlAssemblyDocs path.
        /// </summary>
        public static async Awaitable GenerateAll()
        {
            Paths.SyncUnitySolution();
            Validate();
            // TODO: @see: EditorApplication.LockReloadAssemblies and EditorApplication.UnlockReloadAssemblies
            await Awaitable.BackgroundThreadAsync();
            var cleanAfterBuild = !Directory.Exists(IntermediateOutputPath);
            foreach (var solutionFile in SolutionFiles)
            {
                // Execute msbuild.exe to build the solution.
                await ExecuteMsBuild($"\"{solutionFile}\"");
                var filesToCopy = Directory.EnumerateFiles(
                    Path.Combine(IntermediateOutputPath, "Assembly-CSharp"), "*.xml");
                var xmlTargetPath = XmlAssemblyDocs;
                // Copy all generated *.xml files into the XmlAssemblyDocs path.
                foreach (var xmlOrigin in filesToCopy)
                    File.Copy(
                        xmlOrigin, 
                        Path.Combine(xmlTargetPath, Path.GetFileName(xmlOrigin)), 
                        overwrite: true);
            }
            // Only delete the OutputPath if we're the ones who created it.
            if (cleanAfterBuild)
                Directory.Delete(IntermediateOutputPath, recursive: true);
            DocumentationExtensions.ClearAssemblyCache();
            
            // Generate documentation for all csproj files in the project.
            foreach (var csproj in Directory.EnumerateFiles(Project, "*.csproj", SearchOption.TopDirectoryOnly)) 
                await Generate(csproj);
            await Awaitable.MainThreadAsync();
        }
        
        private static async Awaitable<string> ExecuteMsBuild(string arguments, string workingDirectory = null)
        {
            await Awaitable.BackgroundThreadAsync();
            const int timeout = 300000;
            using (var process = new Process 
                   { StartInfo = new ProcessStartInfo
                       {
                           FileName = MsBuildPath,
                           Arguments = arguments,
                           UseShellExecute = false,
                           RedirectStandardOutput = true,
                           RedirectStandardError = true,
                           CreateNoWindow = true,
                           WorkingDirectory = workingDirectory ?? Project
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
                            throw new BuildFailedException($"Failed to build: {process.StartInfo.Arguments}\n{error}\n{output}");
                        DebugLog.Info($"[DONE] {process.StartInfo.Arguments}\n{output}\n{error}");
                        return output.ToString();
                    }
                    else
                        throw new TimeoutException("Build process timed out.\n" + arguments);
                }
            }   
                
        }

        public static async Awaitable<string> Generate(string csprojPath)
        {
            var projectXml = XDocument.Load(csprojPath);
            var projectRootNamespace = projectXml.Root?.GetDefaultNamespace();
            var assemblyName = projectXml.Descendants(projectRootNamespace + "AssemblyName").Single().Value;
            var documentationPath = Path.Combine(XmlAssemblyDocs, assemblyName + ".xml");

            var output = await ExecuteMsBuild("/t:ResolveAssemblyReferences " +
                                              "/nr:false " +
                                              "/p:Configuration=Debug " +
                                              "/p:GenerateDocumentation=true " +
                                              "/p:GenerateDocumentationFile=true " +
                                              "/p:WarningLevel=0 " +
                                              $"/p:DocumentationFile=\"{documentationPath}\" " +
                                              $"\"{csprojPath}\"");

            return output;
        }
    }
}
