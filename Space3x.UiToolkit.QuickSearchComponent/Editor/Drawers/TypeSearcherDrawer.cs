using System;
using System.Collections.Generic;
using Space3x.Attributes.Types;
using Space3x.UiToolkit.QuickSearchComponent.Editor.Extensions;
using UnityEditor;

namespace Space3x.UiToolkit.QuickSearchComponent.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(TypeSearcherAttribute), true)]
    public class TypeSearcherDrawer : BaseSearchableTypeDrawer<TypeSearcherAttribute>
    {
        public override TypeSearcherAttribute Target => (TypeSearcherAttribute) attribute;
        
        protected override List<Type> GetAllTypes() => Target.GetAllTypes();
        
        protected override void OnReload() => Target.ReloadCache();

        protected override void Dispose(bool disposing)
        {
            // Any cleanup code here.
            base.Dispose(disposing);
        }
    }
}
