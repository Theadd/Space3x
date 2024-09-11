using System.Collections.Generic;
using Space3x.Attributes.Types.DeveloperNotes;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.VisualElements
{
    [Experimental]
    [UxmlElement]
    [HideInInspector]
    public partial class VariableListView : BindableElement, INotifyValueChanged<List<string>>
    {
        public static readonly BindingId ValueProperty = nameof(value);
        public static readonly BindingId AvailableItemsProperty = nameof(AvailableItems);
        
        private ListView m_ListView;
        [SerializeField]
        [DontCreateProperty]
        private List<string> m_SelectedItems;
        [DontCreateProperty]
        private List<string> m_AvailableItems;
        [DontCreateProperty]
        private List<string> m_AllItems;
        [DontCreateProperty]
        private string m_Text = "";

        [UxmlAttribute, CreateProperty]
        public List<string> value
        {
            get => m_SelectedItems ?? new List<string>();
            set
            {
                m_SelectedItems = value;
                UpdateDataSource();
            }
        }
        
        [UxmlAttribute, CreateProperty]
        public List<string> AvailableItems
        {
            get => m_AvailableItems ?? new List<string>();
            set
            {
                m_AvailableItems = value;
                UpdateDataSource();
            }
        }
        
        [UxmlAttribute, CreateProperty]
        public string Text
        {
            get => m_Text;
            set
            {
                m_Text = value;
                if (m_ListView != null)
                {
                    m_ListView.showFoldoutHeader = !string.IsNullOrEmpty(m_Text);
                    m_ListView.headerTitle = m_Text;
                }
            }
        }

        private void UpdateDataSource()
        {
            if (m_SelectedItems == null || m_AvailableItems == null) return;
            if (m_ListView != null)
            {
                m_AllItems = new List<string>(m_AvailableItems);
                foreach (var item in m_SelectedItems)
                    if (!m_AllItems.Contains(item)) m_AllItems.Add(item);
                m_ListView.itemsSource = m_AllItems;
            }
        }

        private void OnValueChangedCallback(ChangeEvent<bool> ev)
        {
            if (ev.target is Toggle element)
            {
                var i = (int) element.userData;
                var item = m_AllItems[i];
                var prevValue = new List<string>(m_SelectedItems);
                if (ev.newValue)
                {
                    if (!m_SelectedItems.Contains(item)) m_SelectedItems.Add(item);
                }
                else 
                { 
                    for (var j = 0; j < m_SelectedItems.Count; j++)
                    {
                        if (m_SelectedItems[j] == item)
                        {
                            m_SelectedItems.RemoveAt(j);
                            break;
                        }
                    }
                }

                using (ChangeEvent<List<string>> pooled = ChangeEvent<List<string>>.GetPooled(prevValue, m_SelectedItems))
                {
                    pooled.target = (VisualElement) this;
                    this.SetValueWithoutNotify(m_SelectedItems);
                    this.SendEvent((EventBase) pooled);
                    this.NotifyPropertyChanged(in ValueProperty);
                }
            }
        }
        
        private void BindToItem(VisualElement e, int i)
        {
            if (m_AllItems != null && e is Toggle element)
            {
                var item = m_AllItems[i];
                element.userData = i;
                var isSelected = m_SelectedItems.Contains(item);
                var isInvalid = !m_AvailableItems.Contains(item);
                element.text = isInvalid ? $"<color=#FF0000FF>{item}</color>" : item;
                element.SetValueWithoutNotify(isSelected);
                element.style.paddingTop = new StyleLength(StyleKeyword.Null);
            }
        }

        private VisualElement MakeItem()
        {
            var element = new Toggle() { };
            element.RegisterValueChangedCallback(OnValueChangedCallback);
            return element;
        }

        public VariableListView() => RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            if (evt.destinationPanel == null) return;
            if (m_ListView == null)
                DelayedInitialize();
        }

        protected void DelayedInitialize()
        {
            m_ListView = new ListView()
            {
                showBoundCollectionSize = false,
                virtualizationMethod = CollectionVirtualizationMethod.FixedHeight,
                fixedItemHeight = 20f,
                selectionType = SelectionType.Multiple,
                allowAdd = false,
                allowRemove = false,
                showFoldoutHeader = !string.IsNullOrEmpty(m_Text),
                headerTitle = m_Text,
                showBorder = true,
                makeItem = MakeItem,
                bindItem = BindToItem
            };
            Add(m_ListView);
            UpdateDataSource();
        }

        public void SetValueWithoutNotify(List<string> newValue) => m_SelectedItems = new List<string>(newValue);
    }
}
