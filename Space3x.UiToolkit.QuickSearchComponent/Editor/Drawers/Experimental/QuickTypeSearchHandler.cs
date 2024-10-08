﻿using System;
using System.Collections.Generic;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor;
using Space3x.Properties.Types;
using Space3x.UiToolkit.QuickSearchComponent.Editor.Extensions;
using Space3x.UiToolkit.QuickSearchComponent.Editor.VisualElements;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.QuickSearchComponent.Editor.Drawers
{
    public class QuickTypeSearchHandler : ITypeSearchHandler
    {
        public QuickSearchPopup Popup { get; set; }
        
        public QuickSearchElement PopupContent { get; set; }
        
        public IEnumerable<Type> CachedTypes { get; set; }
        
        protected ITypePickerAttribute Target { get; set; }
        
        /// <summary>
        /// Whether to include interfaces and abstract types in the search or not.
        /// </summary>
        public bool IncludeAbstractTypes { get; set; } = true;

        protected virtual IEnumerable<Type> GetAllTypes(IPropertyNode property) => Target.GetAllTypes(property, IncludeAbstractTypes);
        
        protected virtual void OnReload() => Target.ReloadCache();
        
        public QuickTypeSearchHandler(ITypePickerAttribute attribute) => Target = attribute;

        public void OnShowPopup(IPropertyNode property, IQuickTypeSearch target, VisualElement selectorField, ShowWindowMode mode)
        {
            PopupContent ??= CreateQuickSearchElement(property);
            Popup ??= new QuickSearchPopup() { };
            Popup.WithContent(PopupContent).WithSearchable(target).Show(selectorField, mode);
        }
        
        private QuickSearchElement CreateQuickSearchElement(IPropertyNode property) => 
            new() { DataSource = GetAllTypes(property) };
    }
}
