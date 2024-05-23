using System;
using System.Collections.Generic;
using System.Linq;
using Space3x.Attributes.Types.VisualElements;
using Space3x.UiToolkit.Types;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using ToolbarSearchField = Space3x.UiToolkit.SlicedText.InputFields.ToolbarSearchField;

namespace Space3x.UiToolkit.QuickSearchComponent.Editor.VisualElements
{
    [UxmlElement]
    [Serializable]
    public partial class QuickSearchElement : BindableElement
    {
        private SplitterContainer m_Splitter;
        private ToolbarSearchField m_SearchField;
        private ToolbarButton m_ToggleSidePanelButton;
        private QuickListView m_ListView;
        private ToggleableListView m_FiltersTabListView;
        private ToggleableListView m_FiltersTabListView2;
        private ToggleableListView m_AssembliesTabListView;
        private List<NamedSymbol> m_Datasource;
        private List<NamedSymbol> m_FilteredDatasource;
        private string m_SearchString;
        private bool m_IsFullyLoaded;
        
        public QuickSearchElement()
        {
            m_SearchString = string.Empty;
            InitializeAttributeFilters();
            
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            this.WithClasses("ui3x-modal").WithName("ui3x-modal");
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            UnregisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            (m_Splitter, m_SearchField, m_ToggleSidePanelButton, m_ListView, m_FiltersTabListView, m_FiltersTabListView2, m_AssembliesTabListView) = CreateElementGUI(
                FixedPaneInitialDimension, EnableFilterByType, EnableFilterByVisibility, EnableFilterByAssembly, IsSelectionMultiple);
            m_ListView.RegisterCallback<AttachToPanelEvent>(OnListViewAttachToPanel);
            m_SearchField.RegisterValueChangedCallback(OnSearchValueChanged);
            AddEventListeners();
            
            m_FiltersTabListView.OnItemClicked += OnFiltersTabListItemClicked;
            m_FiltersTabListView2.OnItemClicked += OnFiltersTabList2ItemClicked;
            m_AssembliesTabListView.OnItemClicked += OnAssemblyItemClicked;
            Add(m_Splitter);
        }

        private void OnSearchValueChanged(ChangeEvent<string> e)
        {
            m_SearchString = e.newValue;
            Refresh();
        }

        private void OnListViewAttachToPanel(AttachToPanelEvent evt)
        {
            m_ListView.UnregisterCallback<AttachToPanelEvent>(OnListViewAttachToPanel);
            if (m_Datasource == null) DataSource = Type.EmptyTypes; // typeof(IBaseType).GetAllTypes().ToArray();
            InitializeAssemblyFilters();
            RefreshFiltersTab();
            RefreshFiltersTab2();
            RefreshAssemblies();
            Refresh();
            m_IsFullyLoaded = true;
            m_SearchField.Focus();
        }

        public void Refresh()
        {
            m_Stopwatch = new System.Diagnostics.Stopwatch();
            m_Stopwatch.Start();
            var filterAssemblies = m_ActiveAssemblies.Length != m_Assemblies.Length;
            var searchFilter = m_SearchString != string.Empty;
            var typeFilter = m_ActiveAttributeTypeFilters.Count != m_AttributeTypeFilters.Count;
            var visibilityFilter = m_ActiveAttributeVisibilityFilters.Count != m_AttributeVisibilityFilters.Count;

            m_FilteredDatasource = (filterAssemblies, searchFilter, typeFilter, visibilityFilter) switch
            {
                (false, false, false, false) => m_Datasource,
                (true, false, false, false) => m_Datasource.Where(t => m_ActiveAssemblies.Contains(t.AssemblyName)).ToList(),
                (false, true, false, false) => m_Datasource.Where(t => t.Name.ToLowerInvariant().Contains(m_SearchString.ToLowerInvariant())).ToList(),
                (false, false, true, false) => m_Datasource.Where(t => m_ActiveAttributeTypeFilters.Any(f => (t.Attributes & m_AttributeTypeFiltersMask) == f)).ToList(),
                (false, false, false, true) => m_Datasource.Where(t => m_ActiveAttributeVisibilityFilters.Contains(t.Attributes & m_AttributeVisibilityFiltersMask)).ToList(),
                (true, true, false, false) => m_Datasource
                    .Where(t => m_ActiveAssemblies.Contains(t.AssemblyName) && t.Name.ToLowerInvariant().Contains(m_SearchString.ToLowerInvariant()))
                    .ToList(),
                (true, false, true, false) => m_Datasource.Where(t =>
                        m_ActiveAssemblies.Contains(t.AssemblyName) && m_ActiveAttributeTypeFilters.Any(f => (t.Attributes & m_AttributeTypeFiltersMask) == f))
                    .ToList(),
                (true, false, false, true) => m_Datasource.Where(t =>
                        m_ActiveAssemblies.Contains(t.AssemblyName) && m_ActiveAttributeVisibilityFilters.Contains(t.Attributes & m_AttributeVisibilityFiltersMask))
                    .ToList(),
                (false, true, true, false) => m_Datasource.Where(t =>
                        m_ActiveAttributeTypeFilters.Any(f => (t.Attributes & m_AttributeTypeFiltersMask) == f) &&
                        t.Name.ToLowerInvariant().Contains(m_SearchString.ToLowerInvariant()))
                    .ToList(),
                (false, true, false, true) => m_Datasource.Where(t =>
                        m_ActiveAttributeVisibilityFilters.Contains(t.Attributes & m_AttributeVisibilityFiltersMask) &&
                        t.Name.ToLowerInvariant().Contains(m_SearchString.ToLowerInvariant()))
                    .ToList(),
                (true, true, true, false) => m_Datasource.Where(t =>
                        m_ActiveAssemblies.Contains(t.AssemblyName) && m_ActiveAttributeTypeFilters.Any(f => (t.Attributes & m_AttributeTypeFiltersMask) == f) &&
                        t.Name.ToLowerInvariant().Contains(m_SearchString.ToLowerInvariant()))
                    .ToList(),
                (true, true, false, true) => m_Datasource.Where(t =>
                        m_ActiveAssemblies.Contains(t.AssemblyName) && m_ActiveAttributeVisibilityFilters.Contains(t.Attributes & m_AttributeVisibilityFiltersMask) &&
                        t.Name.ToLowerInvariant().Contains(m_SearchString.ToLowerInvariant()))
                    .ToList(),
                (true, false, true, true) => m_Datasource.Where(t =>
                        m_ActiveAssemblies.Contains(t.AssemblyName) && m_ActiveAttributeTypeFilters.Any(f => (t.Attributes & m_AttributeTypeFiltersMask) == f) &&
                        m_ActiveAttributeVisibilityFilters.Contains(t.Attributes & m_AttributeVisibilityFiltersMask))
                    .ToList(),
                (false, true, true, true) => m_Datasource.Where(t =>
                        m_ActiveAttributeTypeFilters.Any(f => (t.Attributes & m_AttributeTypeFiltersMask) == f) &&
                        m_ActiveAttributeVisibilityFilters.Contains(t.Attributes & m_AttributeVisibilityFiltersMask) &&
                        t.Name.ToLowerInvariant().Contains(m_SearchString.ToLowerInvariant()))
                    .ToList(),
                (true, true, true, true) => m_Datasource.Where(t =>
                        m_ActiveAssemblies.Contains(t.AssemblyName) && m_ActiveAttributeTypeFilters.Any(f => (t.Attributes & m_AttributeTypeFiltersMask) == f) &&
                        m_ActiveAttributeVisibilityFilters.Contains(t.Attributes & m_AttributeVisibilityFiltersMask) &&
                        t.Name.ToLowerInvariant().Contains(m_SearchString.ToLowerInvariant()))
                    .ToList(),
                _ => throw new ArgumentOutOfRangeException()
            };
            m_Stopwatch.Stop();
            Debug.Log($"[QuickSearchElement.Refresh()] Time elapsed: {m_Stopwatch.ElapsedMilliseconds} ms");
            m_Stopwatch.Restart();
            m_ListView.SetDatasource(m_FilteredDatasource.Select(t => t.DisplayName).ToList());
            m_Stopwatch.Stop();
            Debug.Log($"[QuickSearchElement.RefreshSetDatasource()] Time elapsed: {m_Stopwatch.ElapsedMilliseconds} ms");
            m_Stopwatch.Restart();
            UpdateSelectedIndicesInFilteredDatasource();
            m_Stopwatch.Stop();
            Debug.Log($"[QuickSearchElement.RefreshUpdateSelectedIndicesInFilteredDatasource()] Time elapsed: {m_Stopwatch.ElapsedMilliseconds} ms");
        }
    }
}

