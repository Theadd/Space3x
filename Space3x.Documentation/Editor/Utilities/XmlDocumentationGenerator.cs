﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Space3x.Attributes.Types;
using Space3x.UiToolkit.Types;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Space3x.Documentation
{
    public static class XmlDocumentationGenerator
    {
        public static string Project => Directory.GetParent(Application.dataPath)?.FullName;
        public static string XmlAssemblyDocs => CreatePath(Paths.XmlAssemblyDocs);
        public static string IntermediateOutputPath => CreatePath(s_IntermediateOutputPath);
        public static string MsBuildPath { get; set; } = Paths.projectBuilder;

        private static string s_IntermediateOutputPath = Path.Combine(Project, "Temp", "Bin", "Debug");
        private static string s_IntermediateObjDebugPath = Path.Combine(Project, "obj", "Debug");
        
        private static string CreatePath(string path) => 
            !Directory.Exists(path) ? Directory.CreateDirectory(path).FullName : path;

        private static IEnumerable<string> SolutionFiles => 
            Directory.EnumerateFiles(Project, "*.sln", SearchOption.TopDirectoryOnly);
        
        private static IEnumerable<string> ProjectFiles => 
            Directory.EnumerateFiles(Project, "*.csproj", SearchOption.TopDirectoryOnly);
        
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
            EditorApplication.LockReloadAssemblies();
            await Awaitable.BackgroundThreadAsync();
            var cleanAfterBuild = !Directory.Exists(s_IntermediateOutputPath);
            Debug.Log($"CleanAfterBuild: {cleanAfterBuild}");
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
            foreach (var csproj in ProjectFiles) 
                await GenerateInternal(csproj);
            
            await Awaitable.MainThreadAsync();
            EditorApplication.UnlockReloadAssemblies();
        }

        public static IEnumerable<string> GetAllGenerationSources() =>
            new List<string>()
                .Concat(SolutionFiles)
                .Concat(ProjectFiles);

        private static async Awaitable<string> ExecuteMsBuild(string arguments, string workingDirectory = null)
        {
            await Awaitable.BackgroundThreadAsync();
            const int timeout = 1800000;    // 300000;
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

        public static async Awaitable<string> GenerateInternal(string csprojPath)
        {
            var projectXml = XDocument.Load(csprojPath);
            var projectRootNamespace = projectXml.Root?.GetDefaultNamespace();
            var assemblyName = projectXml.Descendants(projectRootNamespace + "AssemblyName").Single().Value;
            var documentationPath = Path.Combine(XmlAssemblyDocs, assemblyName + ".xml");
            var cleanAfterBuild = !Directory.Exists(s_IntermediateOutputPath);

            var output = await ExecuteMsBuild("/t:Compile " +
                                              "/m " +
                                              "/nr:false " +
                                              "/p:Configuration=Debug " +
                                              "/p:GenerateDocumentation=true " +
                                              "/p:GenerateDocumentationFile=true " +
                                              "/p:WarningLevel=0 " +
                                              $"/p:DocumentationFile=\"{documentationPath}\" " +
                                              $"\"{csprojPath}\"");

            CopyGeneratedAssembly(assemblyName);
            if (cleanAfterBuild)
                Directory.Delete(IntermediateOutputPath, recursive: true);
            
            return output;
        }

        private static void CopyGeneratedAssembly(string assemblyName)
        {
            var generatedAssemblyPath = Path.Combine(s_IntermediateOutputPath, assemblyName, assemblyName + ".dll");
            if (!File.Exists(generatedAssemblyPath))
                generatedAssemblyPath = Path.Combine(s_IntermediateObjDebugPath, assemblyName + ".dll");
            if (File.Exists(generatedAssemblyPath))
                File.Copy(
                    generatedAssemblyPath, 
                    Path.Combine(XmlAssemblyDocs, assemblyName + ".dll"), 
                    overwrite: true);
        }
        
        public static async Awaitable<string> Generate(string csprojPath)
        {
            Paths.SyncUnitySolution();
            Validate();
            EditorApplication.LockReloadAssemblies();
            await Awaitable.BackgroundThreadAsync();
            var output = await GenerateInternal(csprojPath);
            await Awaitable.MainThreadAsync();
            EditorApplication.UnlockReloadAssemblies();

            return output;
        }
    }
}
