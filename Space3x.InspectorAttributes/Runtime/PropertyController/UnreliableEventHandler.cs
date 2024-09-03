using System;
using Space3x.Attributes.Types;
using Space3x.Properties.Types;

namespace Space3x.InspectorAttributes
{
    public class UnreliableEventHandler : IUnreliableEventHandler
    {
        public IBindablePropertyNode SourcePropertyNode => m_SourcePropertyNode;
        
        public bool IsTopLevelRuntimeEventHandler { get; private set; }
        
        private BindablePropertyNode m_SourcePropertyNode;

        private BindablePropertyNode m_ReliablePropertyNode;

        protected UnreliableEventHandler() { }

        public static UnreliableEventHandler Create(BindablePropertyNode sourcePropertyNode, bool isTopLevelRuntimeEventHandler = false)
        {
            var handler = new UnreliableEventHandler()
            {
                m_SourcePropertyNode = sourcePropertyNode.GetIndexerOrItself(),
                IsTopLevelRuntimeEventHandler = isTopLevelRuntimeEventHandler
            };

            // Register for changes on SourcePropertyNode's EventHandler (EventHandler in controller's parent property).
            // Except for top level EventHandlers, which don't need it.
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
                    // changedProperty is a children property of propertyNode
                    case 1:
                        DebugLog.Error($"[PAC!] #1 [UnreliableEventHandler] TRACKED VALUE CHANGE ValueChangedOnChildNode! {propertyNode.PropertyPath}");
                        propertyNode.NotifyValueChangedOnChildNode(changedProperty);
                        break;
                    case 0:
                        DebugLog.Error($"[PAC!] #0 [UnreliableEventHandler] TRACKED VALUE CHANGE ValueChangedOnNode! {propertyNode.PropertyPath}");
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
            IsTopLevelRuntimeEventHandler 
                ? this 
                : m_SourcePropertyNode?.Controller?.EventHandler?.GetTopLevelEventHandler() ?? this;

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
