using System.Collections.Generic;
using System.Linq;

namespace Space3x.Documentation
{
    public partial class MdLinkParser
    {
        private static LinkStore s_Linker = null;
        protected internal static LinkStore Linker => s_Linker ??= CreateLinker();

        public static void Reset() => s_Linker = null;

        private static LinkStore CreateLinker() => new();

        public static string Stringify() => "new Map([\n" + Linker.ToString() + "\n])";

        protected internal class LinkStore
        {
            private List<string> m_AssemblyNames = new List<string>();
            private Dictionary<string, string> m_Values = new Dictionary<string, string>();

            public int GetAssemblyIndex(string assemblyName)
            {
                var index = m_AssemblyNames.IndexOf(assemblyName);
                if (index == -1)
                {
                    m_AssemblyNames.Add(assemblyName);
                    index = m_AssemblyNames.Count - 1;
                }
                return index;
            }

            public void Add(string source, string target)
            {
                m_Values.TryAdd(source, target);
            }

            public override string ToString()
            {
                // const second = new Map([
                //     [1, "uno"],
                //     [2, "dos"],
                // ]);
                return string.Join(",\n", m_Values.Select((pair) => "[\"" + pair.Key + "\", \"" + pair.Value + "\"]"));
            }
        }
        
    }
}
