using System;
using System.Collections.Generic;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor;
using Space3x.UiToolkit.QuickSearchComponent.Editor.Extensions;
using UnityEditor;
using UnityEngine;

namespace Space3x.UiToolkit.QuickSearchComponent.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(DerivedTypeSearcherAttribute), useForChildren: false)]
    public class DerivedTypeSearcherDrawer : BaseSearchableTypeDrawer<DerivedTypeSearcherAttribute>
    {
        public override DerivedTypeSearcherAttribute Target => (DerivedTypeSearcherAttribute) attribute;
        
        protected override List<Type> GetAllTypes()
        {
            Debug.LogWarning("// TODO: Implement GetAllTypes() for non SerializedProperty-based drawers.");
            return Target.GetAllTypes(Property.GetSerializedProperty());
        }

        protected override void OnReload() => Target.ReloadCache();
    }
}
