using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using UnityEditor;

namespace Space3x.UiToolkit.QuickSearchComponent.Editor.Extensions
{
    /// <summary>
    /// Utility class to provide documentation for various types where available with the assembly
    /// </summary>
    public static class DocumentationExtensions
    {
        private static string s_XmlAssemblyDocs = null;
        public static string XmlAssemblyDocs => s_XmlAssemblyDocs ??= XmlDocumentationGenerator.XmlAssemblyDocs;
        public static string EditorApplicationManaged => Path.Combine(EditorApplication.applicationContentsPath, "Managed");
        public static string NetFrameworkReferenceAssemblies { get; set; } = @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework";

        /// <summary>
        /// Provides the documentation comments for a specific method
        /// </summary>
        /// <param name="methodInfo">The MethodInfo (reflection data ) of the member to find documentation for</param>
        /// <returns>The XML fragment describing the method</returns>
        public static XmlElement GetDocumentation(this MethodInfo methodInfo)
        {
            // Calculate the parameter string as this is in the member name in the XML
            var parametersString = "";
            foreach (var parameterInfo in methodInfo.GetParameters())
            {
                if (parametersString.Length > 0) parametersString += ",";

                parametersString += parameterInfo.ParameterType.FullName;
            }

            //AL: 15.04.2008 ==> BUG-FIX remove “()” if parametersString is empty
            if (parametersString.Length > 0)
                return XmlFromName(methodInfo.DeclaringType, 'M', methodInfo.Name + "(" + parametersString + ")");
            else
                return XmlFromName(methodInfo.DeclaringType, 'M', methodInfo.Name);
        }

        /// <summary>
        /// Provides the documentation comments for a specific member
        /// </summary>
        /// <param name="memberInfo">The MemberInfo (reflection data) or the member to find documentation for</param>
        /// <returns>The XML fragment describing the member</returns>
        public static XmlElement GetDocumentation(this MemberInfo memberInfo)
        {
            // First character [0] of member type is prefix character in the name in the XML
            return XmlFromName(memberInfo.DeclaringType, memberInfo.MemberType.ToString()[0], memberInfo.Name);
        }
        /// <summary>
        /// Returns the Xml documenation summary comment for this member
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        public static string GetSummary(this MemberInfo memberInfo)
        {
            var element = memberInfo.GetDocumentation();
            var summaryElm = element?.SelectSingleNode("summary");
            if (summaryElm == null) return "";
            return summaryElm.InnerText.Trim();
        }

        /// <summary>
        /// Provides the documentation comments for a specific type
        /// </summary>
        /// <param name="type">Type to find the documentation for</param>
        /// <returns>The XML fragment that describes the type</returns>
        public static XmlElement GetDocumentation(this Type type)
        {
            // Prefix in type names is T
            return XmlFromName(type, 'T', "");
        }

        /// <summary>
        /// Gets the summary portion of a type's documenation or returns an empty string if not available
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetSummary(this Type type)
        {
            var element = type.GetDocumentation();
            var summaryElm = element?.SelectSingleNode("summary");
            if (summaryElm == null) return "";
            return summaryElm.InnerText.Trim();
        }
    
        public static bool TryGetSummary(this Type type, out string summary)
        {
            try
            {
                summary = GetSummary(type);
                return true;
            }
            catch (Exception)
            {
                summary = null;
                return false;
            }
        }

        /// <summary>
        /// Obtains the XML Element that describes a reflection element by searching the 
        /// members for a member that has a name that describes the element.
        /// </summary>
        /// <param name="type">The type or parent type, used to fetch the assembly</param>
        /// <param name="prefix">The prefix as seen in the name attribute in the documentation XML</param>
        /// <param name="name">Where relevant, the full name qualifier for the element</param>
        /// <returns>The member that has a name that describes the specified reflection element</returns>
        private static XmlElement XmlFromName(this Type type, char prefix, string name)
        {
            string fullName;
            if (string.IsNullOrEmpty(name))
                fullName = prefix + ":" + type.FullName;
            else
                fullName = prefix + ":" + type.FullName + "." + name;
            var xmlDocument = XmlFromAssembly(type.Assembly);
            var matchedElement = xmlDocument["doc"]["members"].SelectSingleNode("member[@name='" + fullName + "']") as XmlElement;

            return matchedElement;
        }

        /// <summary>
        /// A cache used to remember Xml documentation for assemblies
        /// </summary>
        private static readonly Dictionary<Assembly, XmlDocument> Cache = new Dictionary<Assembly, XmlDocument>();

        /// <summary>
        /// A cache used to store failure exceptions for assembly lookups
        /// </summary>
        private static readonly Dictionary<Assembly, Exception> FailCache = new Dictionary<Assembly, Exception>();

        /// <summary>
        /// Obtains the documentation file for the specified assembly
        /// </summary>
        /// <param name="assembly">The assembly to find the XML document for</param>
        /// <returns>The XML document</returns>
        /// <remarks>This version uses a cache to preserve the assemblies, so that 
        /// the XML file is not loaded and parsed on every single lookup</remarks>
        public static XmlDocument XmlFromAssembly(this Assembly assembly)
        {
            if (FailCache.TryGetValue(assembly, out var value))
                throw value;
            try
            {
                if (!Cache.ContainsKey(assembly))
                    Cache[assembly] = XmlFromAssemblyNonCached(assembly);
                return Cache[assembly];
            }
            catch (Exception exception)
            {
                FailCache[assembly] = exception;
                throw;
            }
        }
    
        /// <summary>
        /// Clears the assembly documentation cache.
        /// </summary>
        public static void ClearAssemblyCache()
        {
            Cache.Clear();
            FailCache.Clear();
        }
    
        /// <summary>
        /// Removes an assembly from the cached assembly documentation.
        /// </summary>
        /// <param name="assembly"></param>
        public static void RemoveAssemblyFromCache(Assembly assembly)
        {
            if (assembly == null) return;
            Cache.Remove(assembly);
            FailCache.Remove(assembly);
        }
    
        /// <summary>
        /// Removes an assembly by name from the XmlDocument's cache.
        /// </summary>
        /// <param name="assemblyName"></param>
        public static void RemoveAssemblyFromCache(string assemblyName) => 
            RemoveAssemblyFromCache(AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(asm => asm.GetName().Name == assemblyName));

        /// <summary>
        /// Loads and parses the documentation file for the specified assembly
        /// </summary>
        /// <param name="assembly">The assembly to find the XML document for</param>
        /// <returns>The XML document</returns>
        private static XmlDocument XmlFromAssemblyNonCached(Assembly assembly)
        {
            var assemblyFullPath = assembly.Location;
   
            if (!string.IsNullOrEmpty(assemblyFullPath))
            {
                var assemblyFileName = Path.GetFileName(assemblyFullPath);
                var xmlPath = Path.ChangeExtension(assemblyFullPath, ".xml");
                var xmlPathAlt = GetXmlAlternativeFilePath(assemblyFileName);
                var lastWriteTime = File.Exists(xmlPath) ? File.GetLastWriteTime(xmlPath) : DateTime.MinValue;
                if (xmlPath != xmlPathAlt)
                {
                    var lastWriteTimeAlt = File.Exists(xmlPathAlt) ? File.GetLastWriteTime(xmlPathAlt) : DateTime.MinValue;
                    if (lastWriteTimeAlt >= lastWriteTime)
                    {
                        lastWriteTime = lastWriteTimeAlt;
                        xmlPath = xmlPathAlt;
                    }
                }

                if (lastWriteTime == DateTime.MinValue) 
                    throw new Exception("XML documentation file for " + assemblyFileName + " not found.\n" + assemblyFullPath);
            
                StreamReader streamReader = new StreamReader(xmlPath);

                var xmlDocument = new XmlDocument();
                xmlDocument.Load(streamReader);
                return xmlDocument;
            }
            else
            {
                throw new Exception("Could not ascertain assembly filename", null);
            }
        }
    
        /// <summary>
        /// Returns an XML documentation file path from alternative paths.
        /// </summary>
        /// <param name="assemblyFileName"></param>
        /// <returns></returns>
        private static string GetXmlAlternativeFilePath(string assemblyFileName)
        {
            foreach (var path in AlternativeAssemblyDocsPaths)
            {
                var filePath = Path.ChangeExtension(Path.Combine(path, assemblyFileName), ".xml");
                if (File.Exists(filePath)) return filePath;
            }
            return Path.ChangeExtension(Path.Combine(XmlAssemblyDocs, assemblyFileName), ".xml");
        }
    
        private static string[] AlternativeAssemblyDocsPaths => new[]
        {
            EditorApplicationManaged,
            Path.Combine(EditorApplicationManaged, "UnityEngine"),
            XmlAssemblyDocs,
            Path.Combine(NetFrameworkReferenceAssemblies, "v4.7.2"),
            Path.Combine(NetFrameworkReferenceAssemblies, "v4.8"),
        };
    }
}
