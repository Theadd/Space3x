using System;
using System.Collections.Generic;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor;
using Space3x.UiToolkit.QuickSearchComponent.Editor.VisualElements;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.QuickSearchComponent.Editor.Drawers
{
    public interface ITypeSearchHandler : ITypePickerAttributeHandler
    {
        IEnumerable<Type> CachedTypes { get; set; }
        void OnShowPopup(IPropertyNode property, IQuickTypeSearch target, VisualElement selectorField, ShowWindowMode mode);
    }
}
