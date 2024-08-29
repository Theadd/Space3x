using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Space3x.UiToolkit.Types
{
    public static class Paths
    {
        static Paths()
        {
            assets = Application.dataPath;
            #if UNITY_EDITOR
            editor = EditorApplication.applicationPath;
            editorContents = EditorApplication.applicationContentsPath;

            try
            {
                SyncVS = typeof(Editor).Assembly.GetType("UnityEditor.SyncVS", true);
                SyncVS_SyncSolution = SyncVS.GetMethod("SyncSolution", BindingFlags.Static | BindingFlags.Public);

                if (SyncVS_SyncSolution == null)
                {
                    throw new MissingMemberException(SyncVS.ToString(), "SyncSolution");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load UnityEditor.SyncVS", ex);
            }
            #endif
        }

        public static string assets { get; }

        public static string editor { get; }

        public static string editorContents { get; }

        public static string project => Directory.GetParent(assets).FullName;

        public static string projectName => Path.GetFileName(project.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

        public static string projectSettings => Path.Combine(project, "ProjectSettings");

        public static string editorDefaultResources => Path.Combine(assets, "Editor Default Resources");

        public static string backups => Path.Combine(project, "Backups");


        #region Assembly Projects

        private static Type SyncVS; // internal class UnityEditor.SyncVS : AssetPostprocessor

        private static MethodInfo SyncVS_SyncSolution; // public static void SyncSolution()

        [Conditional("UNITY_EDITOR")]
        public static void SyncUnitySolution()
        {
            try
            {
                SyncVS_SyncSolution.Invoke(null, null);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to invoke UnityEditor.SyncVS.SyncSolution using reflection.", ex);
            }
        }

        public static string runtimeAssemblyFirstPassProject =>
            PreferredProjectPath
            (
                Path.Combine(project, projectName + ".Plugins.csproj"),
                Path.Combine(project, "Assembly-CSharp-firstpass.csproj")
            );

        public static string runtimeAssemblySecondPassProject =>
            PreferredProjectPath
            (
                Path.Combine(project, projectName + ".csproj"),
                Path.Combine(project, "Assembly-CSharp.csproj")
            );

        public static string editorAssemblyFirstPassProject =>
            PreferredProjectPath
            (
                Path.Combine(project, projectName + ".Editor.csproj"),
                Path.Combine(project, "Assembly-CSharp-Editor-firstpass.csproj")
            );

        public static string editorAssemblySecondPassProject =>
            PreferredProjectPath
            (
                Path.Combine(project, projectName + ".Editor.Plugins.csproj"),
                Path.Combine(project, "Assembly-CSharp-Editor.csproj")
            );

        public static IEnumerable<string> assemblyProjects
        {
            get
            {
                var firstPass = runtimeAssemblyFirstPassProject;
                var secondPass = runtimeAssemblySecondPassProject;
                var editorFirstPass = editorAssemblyFirstPassProject;
                var editorSecondPass = editorAssemblySecondPassProject;

                if (firstPass != null)
                {
                    yield return firstPass;
                }

                if (secondPass != null)
                {
                    yield return secondPass;
                }

                if (editorFirstPass != null)
                {
                    yield return editorFirstPass;
                }

                if (editorSecondPass != null)
                {
                    yield return editorSecondPass;
                }
            }
        }

        private static string PreferredProjectPath(string path1, string path2)
        {
            if (!File.Exists(path1) && !File.Exists(path2))
            {
                return null;
            }

            if (!File.Exists(path1))
            {
                return path2;
            }

            if (!File.Exists(path2))
            {
                return path1;
            }

            var timestamp1 = File.GetLastWriteTime(path1);
            var timestamp2 = File.GetLastWriteTime(path2);

            if (timestamp1 >= timestamp2)
            {
                return path1;
            }

            return path2;
        }

        #endregion


        #region .NET

        public const string MsBuildDownloadLink = "https://aka.ms/vs/15/release/vs_buildtools.exe";

        private static IEnumerable<string> environmentPaths
        {
            get
            {
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    return Environment.GetEnvironmentVariable("PATH").Split(';');
                }

                // http://stackoverflow.com/a/41318134/154502
                var start = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = "-l -c \"echo $PATH\"", // -l = 'login shell' to execute /etc/profile
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                var process = Process.Start(start);
                process.WaitForExit();
                var path = process.StandardOutput.ReadToEnd().Trim();
                return path.Split(':');
            }
        }

        // ProgramFilesx86 is not available until .NET 4
        // https://stackoverflow.com/questions/194157/
        private static string ProgramFilesx86
        {
            get
            {
                if (IntPtr.Size == 8 || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432")))
                {
                    return Environment.GetEnvironmentVariable("ProgramFiles(x86)");
                }

                return Environment.GetEnvironmentVariable("ProgramFiles");
            }
        }

        public static string msBuild
        {
            get
            {
                if (Application.platform != RuntimePlatform.WindowsEditor)
                {
                    return null;
                }

                try
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = Path.Combine(ProgramFilesx86, @"Microsoft Visual Studio\Installer\vswhere.exe"),
                        Arguments = @"-latest -prerelease -products * -requires Microsoft.Component.MSBuild -find **\Bin\MSBuild.exe",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    };

                    using (var vsWhere = Process.Start(startInfo))
                    {
                        var firstPath = vsWhere.StandardOutput.ReadLine();
                        vsWhere.WaitForExit();
                        return firstPath;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to find MSBuild path via VSWhere utility.\n{ex}");
                    return null;
                }
            }
        }

        public static string xBuild
        {
            get
            {
                if (Application.platform == RuntimePlatform.WindowsEditor)
                    return null;

                var path = TryPathsForFile("xbuild", environmentPaths);
                return path;
            }
        }

        public static string roslynCompiler => Path.Combine(Path.GetDirectoryName(editor), "Data/tools/Roslyn/csc.exe");

        public static string projectBuilder => Application.platform == RuntimePlatform.WindowsEditor ? msBuild : xBuild;

        #endregion
        
        public static string XmlAssemblyDocs => Resolve(Path.Combine(project, "Library", "XmlAssemblyDocs"));
        
        /// <summary>
        /// Gets the path relative to project where the Xml Documentation Generator generates the *.dll file for a
        /// given assembly name, which also corresponds to the name of the *.csproj file, without the extension.
        /// <seealso cref="Space3x.Documentation.XmlDocumentationGenerator"/>  
        /// </summary>
        /// <param name="assemblyName">The assembly name, e.g. "Unity.Timeline".</param>
        /// <returns></returns>
        public static string AssemblyProjectDllPath(string assemblyName) => 
            RelativePath(Path.Combine(XmlAssemblyDocs, assemblyName + ".dll"));
        
        public static bool IsWindowsPlatform =>
            Application.platform switch
            {
                RuntimePlatform.WindowsPlayer or RuntimePlatform.WindowsEditor or RuntimePlatform.WindowsServer
                    or RuntimePlatform.WSAPlayerX64 or RuntimePlatform.WSAPlayerX86
                    or RuntimePlatform.WSAPlayerARM => true,
                _ => false
            };

        public static string TryPathsForFile(string fileName, IEnumerable<string> directories)
        {
            return directories.Select(directory => Path.Combine(directory, fileName)).FirstOrDefault(File.Exists);
        }
        
        public static string Npx => TryPathsForFile(IsWindowsPlatform ? "npx.cmd" : "npx", environmentPaths) ?? "";
        
        public static string Python => TryPathsForFile(IsWindowsPlatform ? "python.exe" : "python", environmentPaths) ?? "";

        /// <summary>
        /// Returns the absolute path of the given path.
        /// If the path is already absolute, it returns the normalized full path.
        /// Otherwise, it returns the normalized full path with the given base path.
        /// If no base path is provided, it uses the project path.
        /// </summary>
        /// <param name="path">The path to convert to an absolute path.</param>
        /// <param name="basePath">The base path to use if the path is not absolute. Default is null.</param>
        /// <returns>The absolute path of the given path.</returns>
        public static string AbsolutePath(string path, string basePath = null) =>
            Path.IsPathRooted(path) 
                ? Path.GetFullPath(path) 
                : Path.GetFullPath(path, basePath ?? project);

        /// <summary>
        /// Returns the normalized relative path from 'path' to 'relativeTo' if 'path' is rooted; otherwise, returns
        /// the normalized relative path from 'path' to the full path of 'path' using 'relativeTo' as the base path.
        /// </summary>
        public static string RelativePath(string path, string relativeTo = null) =>
            Path.IsPathFullyQualified(path)     // Path.IsPathRooted(path) 
                ? Path.GetRelativePath(relativeTo ?? project, path) 
                : Path.GetRelativePath(relativeTo ?? project, Path.GetFullPath(path, relativeTo ?? project));
                // : Path.GetRelativePath(relativeTo ?? project, Path.GetFullPath(relativeTo ?? project, path));

        public static string DirectoryPath(string path)
        {
            var dir = Path.GetDirectoryName(path);
            return string.IsNullOrEmpty(dir) ? path : TrailingSlash(dir);
        }

        public static string TrailingSlash(string path) => 
            path.EndsWith(Path.DirectorySeparatorChar) ? path : path + Path.DirectorySeparatorChar;

        // var cfgFileAbs = MdDocumentationGenerator.GetConfigurationFileFullPath(assemblyName);
        // var dllPath = Paths.AssemblyProjectDllPath(assemblyName);
        // var dllPathAbs = Paths.AbsolutePath(Paths.AssemblyProjectDllPath(assemblyName));
        // var dllPathRelToCfgFileAbs = Paths.RelativePath(dllPathAbs, cfgFileAbs);
        
        
        /// <summary>
        /// Returns the same path but normalized.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string Resolve(string path) =>
            Path.IsPathRooted(path)
                ? Path.GetFullPath(path)
                : Path.Combine(Path.GetDirectoryName(path), Path.GetFileName(path));
    }
}
