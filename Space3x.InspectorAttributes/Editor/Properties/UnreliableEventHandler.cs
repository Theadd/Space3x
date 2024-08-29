using System;
using Space3x.InspectorAttributes.Editor.Extensions;
using Space3x.Properties.Types.Editor;
using UnityEngine;

namespace Space3x.InspectorAttributes.Editor
{
    public class UnreliableEventHandler : IUnreliableEventHandler
    {
        public IBindablePropertyNode SourcePropertyNode => m_SourcePropertyNode;
        
        private BindablePropertyNode m_SourcePropertyNode; // { get; set; }

        private BindablePropertyNode m_ReliablePropertyNode;

        protected UnreliableEventHandler() { }

        public static UnreliableEventHandler Create(BindablePropertyNode sourcePropertyNode)
        {
            var handler = new UnreliableEventHandler()
            {
                m_SourcePropertyNode = sourcePropertyNode.GetIndexerOrItself()
            };

            if (handler.m_SourcePropertyNode?.Controller?.EventHandler is UnreliableEventHandler parentHandler)
            {
                if (parentHandler.m_SourcePropertyNode is INonSerializedPropertyNode nonSerializedPropertyNode)
                    parentHandler.TrackPropertyChanges(handler.m_SourcePropertyNode, nonSerializedPropertyNode.NotifyValueChanged);
                else    
                    parentHandler.TrackPropertyChanges(handler.m_SourcePropertyNode);
            }

            return handler;
        }

        public void TrackPropertyChanges(BindablePropertyNode propertyNode, Action<IBindablePropertyNode> callback = null)
        {
            // Debug.LogWarning($"[PAC!] [UnreliableEventHandler] TRACKING! {propertyNode.PropertyPath}");
            m_SourcePropertyNode.ValueChangedOnChildNode += changedProperty =>
            {
                // Debug.LogWarning($"[PAC!] <b><u>[ValueChangedOnChildNode] STOP HERE!</u> COMPARING DEST: {changedProperty.PropertyPath} == SRC: {propertyNode.PropertyPath}</b>");
                switch (ComparePropertyPaths(changedProperty.PropertyPath, propertyNode.PropertyPath))
                {
                    case 1:
                        propertyNode.NotifyValueChangedOnChildNode(changedProperty);
                        break;
                    case 0:
                        // propertyNode instead of changedProperty to avoid being ignored by an equality check.
                        callback?.Invoke(propertyNode);
                        break;
                    case -1:
                        // Ignore
                        break;
                }
            };
        }

        /// <summary>
        /// Returns 1 if the source path is a child (nested, longer) path of the destination path, if are equal,
        /// returns 0, otherwise returns -1.
        /// </summary>
        private static int ComparePropertyPaths(string srcPath, string dstPath) =>
            srcPath != dstPath
                ? srcPath.Length > dstPath.Length && srcPath[..dstPath.Length] == dstPath
                    ? 1 
                    : -1 
                : 0;
        
        public IUnreliableEventHandler GetTopLevelEventHandler() => 
            m_SourcePropertyNode?.Controller?.EventHandler?.GetTopLevelEventHandler() ?? this;

        public IBindablePropertyNode GetReliablePropertyNode() => 
            ((BindablePropertyNode)GetTopLevelEventHandler().SourcePropertyNode).GetIndexerOrItself();

        public void NotifyValueChanged(IBindablePropertyNode propertyNode)
        {
            // Debug.LogWarning($"[PAC!] [NotifyValueChangedOnChildNode] {propertyNode.PropertyPath}");
            m_ReliablePropertyNode ??= (BindablePropertyNode)GetReliablePropertyNode();
            // Debug.LogWarning($"[PAC!] [NotifyValueChangedOnChildNode] ReliablePropertyNode: {m_ReliablePropertyNode.PropertyPath}");
            m_ReliablePropertyNode.NotifyValueChangedOnChildNode(propertyNode);
        }
        
        
    }
}
