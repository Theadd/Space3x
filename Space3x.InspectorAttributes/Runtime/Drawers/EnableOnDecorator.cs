﻿using Space3x.Attributes.Types;
using Space3x.Properties.Types;

namespace Space3x.InspectorAttributes.Editor.Drawers
{
#if UNITY_EDITOR
    [UnityEditor.CustomPropertyDrawer(typeof(EnableOnAttribute), true)]
#endif
    [CustomRuntimeDrawer(typeof(EnableOnAttribute), true)]
    public class EnableOnDecorator : Decorator<AutoDecorator, EnableOnAttribute>, IAttributeExtensionContext<EnableOnAttribute>
    {
        public override EnableOnAttribute Target => (EnableOnAttribute) attribute;
        public IAttributeExtensionContext<EnableOnAttribute> Context => this;

        protected override bool UpdateOnAnyValueChange => true;
        
        public override void OnUpdate()
        {
            Context.WithExtension<EnableOnEx, IEnableOnEx, bool>(
                out var isTrue, 
                defaultValue: true);
        }
    }
}
