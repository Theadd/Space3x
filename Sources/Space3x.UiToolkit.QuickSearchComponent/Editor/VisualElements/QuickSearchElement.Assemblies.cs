using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.QuickSearchComponent.Editor.VisualElements
{
    public partial class QuickSearchElement
    {
        private StringFilter[] m_Assemblies;
        private string[] m_ActiveAssemblies;

        [UxmlAttribute]
        public string[] InitialAssemblies;
        
        [UxmlAttribute]
        public bool ShowAllAssemblies;

        [UxmlAttribute]
        public IEnumerable<Type> DataSource
        {
            get => m_Datasource?.Select(t => t.Value).ToArray() ?? Type.EmptyTypes;
            set
            {
                m_Datasource = value
                    .Select(t => new NamedSymbol(t))
                    .ToList();
                if (m_IsFullyLoaded)
                {
                    if (!ShowAllAssemblies)
                    {
                        var matchingAssemblies = m_Datasource.Select(t => t.AssemblyName).Distinct().ToArray();
                        m_Assemblies = matchingAssemblies.Select(a => new StringFilter(a, m_ActiveAssemblies.Contains(a))).ToArray();
                        RefreshAssemblies();
                    }
                    Refresh();
                }
            }
        }

        private void InitializeAssemblyFilters()
        {
            m_Assemblies = ShowAllAssemblies 
                ? AppDomain.CurrentDomain.GetAssemblies().Select(a => new StringFilter(a.GetName().Name)).ToArray() 
                : m_Datasource.Select(t => t.AssemblyName).Distinct().Select(a => new StringFilter(a)).ToArray();
            
            if (InitialAssemblies is not { Length: > 0 }) return;
            
            for (var i = 0; i < m_Assemblies.Length; i++)
            {
                m_Assemblies[i].Active = InitialAssemblies.Contains(m_Assemblies[i].Value);
            }
        }
        
        private void OnAssemblyItemClicked(int itemIndex, bool isToggle)
        {
            m_Assemblies[itemIndex].Active = isToggle;
            RefreshAssemblies();
            Refresh();
        }
        
        private void RefreshAssemblies()
        {
            m_ActiveAssemblies = m_Assemblies.Where(t => t.Active).Select(t => t.Value).ToArray();
            m_AssembliesTabListView.DataSource = m_Assemblies.Select(t => new ToggleableListItem(t.ToString(), t.Active)).ToList();
        }
    }
}
