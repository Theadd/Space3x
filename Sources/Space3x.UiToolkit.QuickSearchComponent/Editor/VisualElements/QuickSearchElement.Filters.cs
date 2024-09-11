using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.QuickSearchComponent.Editor.VisualElements
{
    public partial class QuickSearchElement
    {
//        private int m_AttributesBitmask;
//        private List<EnumFlagsFilter<TypeAttributes>> m_AttributeFilters;
        private List<EnumFilter<TypeAttributes>> m_AttributeTypeFilters;
        private TypeAttributes m_AttributeTypeFiltersMask;
        private List<TypeAttributes> m_ActiveAttributeTypeFilters;
        private List<EnumFilter<TypeAttributes>> m_AttributeVisibilityFilters;
        private TypeAttributes m_AttributeVisibilityFiltersMask;
        private List<TypeAttributes> m_ActiveAttributeVisibilityFilters;

        [UxmlAttribute]
        public TypeAttributes[] InitialVisibilityFilters;
        
        private void InitializeAttributeFilters()
        {
            m_AttributeTypeFilters = new List<EnumFilter<TypeAttributes>>()
            {
                new(TypeAttributes.Class | TypeAttributes.SequentialLayout, "Struct*"),
                new(TypeAttributes.Class, "Class*"),
                new(TypeAttributes.Interface, "Interface"),
            };
            m_AttributeTypeFiltersMask = TypeAttributes.Class | TypeAttributes.SequentialLayout | TypeAttributes.Interface;
            m_AttributeVisibilityFilters = new List<EnumFilter<TypeAttributes>>()
            {
                new(TypeAttributes.NotPublic, "Internal"),
                new(TypeAttributes.Public, "Public"),
                new(TypeAttributes.NestedPublic, "Public (Nested)"),
                new(TypeAttributes.NestedPrivate, "Private (Nested)"),
                new(TypeAttributes.NestedFamily, "Protected (Nested)"),
                new(TypeAttributes.NestedAssembly, "Internal (Nested)"),
                new(TypeAttributes.NestedFamORAssem, "Protected Internal (Nested)"),
            };
            m_AttributeVisibilityFiltersMask = TypeAttributes.VisibilityMask;
            if (InitialVisibilityFilters != null && InitialVisibilityFilters.Length > 0)
            {
                for (var i = 0; i < m_AttributeVisibilityFilters.Count; i++)
                {
                    var item = m_AttributeVisibilityFilters[i];
                    item.Active = InitialVisibilityFilters.Contains(item.Value);
                    m_AttributeVisibilityFilters[i] = item;
                }
            }
        }
        
        private void OnFiltersTabListItemClicked(int itemIndex, bool isToggle)
        {
            var item = m_AttributeTypeFilters[itemIndex];
            item.Active = isToggle;
            m_AttributeTypeFilters[itemIndex] = item;
            RefreshFiltersTab();
            Refresh();
        }
        
        private void OnFiltersTabList2ItemClicked(int itemIndex, bool isToggle)
        {
            var item = m_AttributeVisibilityFilters[itemIndex];
            item.Active = isToggle;
            m_AttributeVisibilityFilters[itemIndex] = item;
            RefreshFiltersTab2();
            Refresh();
        }
        
        private void RefreshFiltersTab()
        {
            m_ActiveAttributeTypeFilters = m_AttributeTypeFilters.Where(t => t.Active).Select(t => t.Value).ToList();
            m_FiltersTabListView.DataSource = m_AttributeTypeFilters.Select(t => new ToggleableListItem(t.ToString(), t.Active)).ToList();
        }
        
        private void RefreshFiltersTab2()
        {
            m_ActiveAttributeVisibilityFilters = m_AttributeVisibilityFilters.Where(t => t.Active).Select(t => t.Value).ToList();
            m_FiltersTabListView2.DataSource = m_AttributeVisibilityFilters.Select(t => new ToggleableListItem(t.ToString(), t.Active)).ToList();
        }
    }
}
