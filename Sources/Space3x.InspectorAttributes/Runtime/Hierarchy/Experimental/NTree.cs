using System;
using System.Collections.Generic;
using System.Linq;
using Space3x.Attributes.Types.DeveloperNotes;

namespace Space3x.InspectorAttributes
{
    [Experimental(Text = "Implementation making use of this class is still in progress.")]
    public class NTree<T>
    {
        private readonly T m_Value;
        private readonly LinkedList<NTree<T>> m_Nodes = new();
        public NTree<T> parent;

        public NTree(T data, NTree<T> parentTree)
        {
            m_Value = data;
            parent = parentTree;
        }

        public T Value => m_Value;

        public void AddChild(T data) => m_Nodes.AddFirst(new NTree<T>(data, this));

        public NTree<T> GetChild(int i) => m_Nodes.FirstOrDefault(n => --i == 0);

        public void Traverse(NTree<T> node, Action<T> visitor)
        {
            visitor(node.m_Value);
            foreach (var kid in node.m_Nodes)
                Traverse(kid, visitor);
        }
    }
}
