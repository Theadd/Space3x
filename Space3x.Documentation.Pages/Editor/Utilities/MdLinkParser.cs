using System.Collections.Generic;
using System.IO;
using Space3x.UiToolkit.Types;
using UnityEngine;

namespace Space3x.Documentation
{
    public partial class MdLinkParser
    {
        public static void Parse(string linksPath, string assemblyName)
        {
            var index = Linker.GetAssemblyIndex(assemblyName);
            var fullPath = Paths.AbsolutePath(linksPath);
            if (!File.Exists(fullPath))
                return;
            
            // var baseUrl = "./gen-docs/generated/" + assemblyName + "/";
            var baseUrl = "./" + Paths.RelativePath(Paths.AbsolutePath(Path.Combine(MdDocumentationGenerator.GeneratedPath, assemblyName + @"\")),
                Paths.AbsolutePath(MdDocumentationGenerator.DocumentationPath)).Replace(@"\", "/");
            Debug.Log($"baseUrl: |{baseUrl}| == |./g/{assemblyName}/| ? (old: \"{("./gen-docs/generated/" + assemblyName + "/")}\")");
            var allLines = File.ReadLines(fullPath);
            foreach (var line in allLines)
            {
                if (line.Length < 8) continue;
                var lineValues = line.Substring(2).TrimEnd(' ', '\t', '\n', '\r').Split('|');
                var entryType = line[..1];

                switch (entryType)
                {
                    case "N":
                        break;
                    case "M":
                    default:
                        // M:Space3x.Attributes.Types.IgnoreMe.ShowInInspectorAttribute.AnyoneElse(System.String,System.Boolean)
                        // |ShowInInspectorAttribute.md#AnyoneElse(string_bool)|AnyoneElse(string, bool)
                        var source = lineValues[0].Split('(')[0].Replace('`', '-');
                        // linkRef() adds "#/" as prefix and replaces: ".md#" => "?id="
                        var target = baseUrl + lineValues[1].Split('(')[0];
                        Linker.Add(source, target);
                        break;
                }
            }
            // Process line
        }
    }
}
