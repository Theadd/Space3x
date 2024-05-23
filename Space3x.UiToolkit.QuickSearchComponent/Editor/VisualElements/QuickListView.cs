using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.QuickSearchComponent.Editor.VisualElements
{
    [UxmlElement]
    public partial class QuickListView : BindableElement, INotifyValueChanged<IEnumerable<int>>
    {
        private Columns m_Columns;
        private ListView m_ListView;
        private IList m_Datasource;
        private IEnumerable<int> m_Value = new List<int>();
        private bool m_IsSelectionMultiple = false;

        public event Action SelectionCompleted;

        public bool IsSelectionMultiple
        {
            get => m_IsSelectionMultiple;
            set
            {
                m_IsSelectionMultiple = value;
                if (m_ListView != null)
                    m_ListView.selectionType = m_IsSelectionMultiple ? SelectionType.Multiple : SelectionType.Single;
            }
        }

        private void BindToItem(VisualElement e, int i)
        {
            ((TextLine) e).text = (string) m_Datasource[i];
            e.style.paddingTop = new StyleLength(StyleKeyword.Null);
        }

        public QuickListView() : base()
        {
            m_Datasource = new List<string>();
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        }
        
        public void SetDatasource(IList datasource)
        {
            m_Datasource = datasource;
            if (m_ListView != null)
                m_ListView.itemsSource = m_Datasource;
        }
        
        public IEnumerable<object> GetSelectedItems() => m_ListView.selectedItems;

        public IEnumerable<int> GetSelectedIndices() => m_ListView.selectedIndices;

        public void ClearSelection() => m_ListView.ClearSelection();
        
        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            if (evt.destinationPanel == null) return;
            if (m_ListView == null)
                DelayedInitialize();
        }

        protected virtual void DelayedInitialize()
        {
            m_ListView = new ListView()
            {
                showBoundCollectionSize = false,
                fixedItemHeight = 19f,
                virtualizationMethod = CollectionVirtualizationMethod.FixedHeight,
                selectionType = m_IsSelectionMultiple ? SelectionType.Multiple : SelectionType.Single,
                allowAdd = false,
                allowRemove = false,
                makeItem = () => new TextLine() { text = "" }, 
                bindItem = BindToItem,
                showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
            };
            Add(m_ListView);
            m_ListView.itemsSource = m_Datasource;
            m_ListView.itemsChosen += chosen => SelectionCompleted?.Invoke(); 
            m_ListView.selectedIndicesChanged += OnSelectedIndicesChanged;
        }

        private void OnSelectedIndicesChanged(IEnumerable<int> selectedIndices)
        {
            var previousValue = m_Value.ToList();
            m_Value = (selectedIndices ?? new List<int>()).ToList();
            NotifyValueChanged(previousValue, m_Value);
        }

        public void SetValueWithoutNotify(IEnumerable<int> newValue)
        {
            m_Value = newValue;
            if (m_ListView != null)
                m_ListView.SetSelectionWithoutNotify(m_Value);
        }
        
        internal static readonly BindingId valueProperty = (BindingId) nameof (value);
        
        [CreateProperty]
        public IEnumerable<int> value
        {
            get => m_Value;
            set
            {
                var previousValue = m_Value.ToList();
                var newValue = (value ?? new List<int>()).ToList();

                if (panel != null)
                {
                    SetValueWithoutNotify(newValue);
                    NotifyValueChanged(previousValue, newValue);
                }
                else
                    SetValueWithoutNotify(newValue);
            }
        }

        private void NotifyValueChanged(IEnumerable<int> previousValue, IEnumerable<int> newValue)
        {
            using (ChangeEvent<IEnumerable<int>> pooled = ChangeEvent<IEnumerable<int>>.GetPooled(previousValue, newValue))
            {
                pooled.target = (VisualElement) this;
                SendEvent((EventBase) pooled);
            }
            NotifyPropertyChanged(in valueProperty);
        }
    }
}
