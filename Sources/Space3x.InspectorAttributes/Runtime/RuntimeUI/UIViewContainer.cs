using Space3x.Attributes.Types;
using UnityEngine;

namespace Space3x.InspectorAttributes
{
    public class UIViewContainer : ScriptableObject
    {
        [AllowExtendedAttributes]
        public int instanceId;
        
        [ShowInInspector]
        private ScriptableUIView uiView;

        private static int s_LastId = 0;
        
        private void Awake()
        {
            instanceId = s_LastId++;
            Debug.Log($"@ UIViewContainer.Awake(): instanceId = {instanceId};");
        }

        public void SetView(ScriptableUIView view) => uiView = view;
    }
}
