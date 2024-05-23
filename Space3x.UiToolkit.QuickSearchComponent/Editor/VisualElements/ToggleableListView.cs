using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.QuickSearchComponent.Editor.VisualElements
{
    
    public class ToggleableListItem
    {
        public string Name;
        public bool Active;
        
        public ToggleableListItem(string name, bool active)
        {
            Name = name;
            Active = active;
        }
        public override string ToString() => Name;
    }

    [UxmlElement]
    public partial class ToggleableListView : BindableElement
    {
        private ListView m_ListView;

        [UxmlAttribute] 
        public string FoldoutTitle = "Foldout";

        public List<ToggleableListItem> DataSource
        {
            get => m_DataSource;
            set
            {
                m_DataSource = value;
                if (m_ListView != null) m_ListView.itemsSource = value;
            }
        }

        public Action<int, bool> OnItemClicked;
        private List<ToggleableListItem> m_DataSource;

        private void OnValueChangedCallback(ChangeEvent<bool> ev)
        {
            if (ev.target is Toggle element)
            {
                var i = (int) element.userData;
                OnItemClicked?.Invoke(i, ev.newValue);
            }
        }

        private void BindToItem(VisualElement e, int i)
        {
            ((Toggle) e).userData = i;
            ((Toggle) e).text = (string) DataSource[i].Name;
            ((Toggle) e).SetValueWithoutNotify(DataSource[i].Active);
            e.style.paddingTop = new StyleLength(StyleKeyword.Null);
        }

        private VisualElement MakeItem()
        {
            var element = new Toggle() { };
            element.RegisterValueChangedCallback(OnValueChangedCallback);
            return element;
        }

        public ToggleableListView() : base()
        {
            DataSource = new List<ToggleableListItem>();
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        }

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
                virtualizationMethod = CollectionVirtualizationMethod.FixedHeight,
                fixedItemHeight = 20f,
                selectionType = SelectionType.None,
                allowAdd = false,
                allowRemove = false,
                showFoldoutHeader = true,
                headerTitle = FoldoutTitle,
                makeItem = MakeItem,
                bindItem = BindToItem
            };
            Add(m_ListView);
            m_ListView.itemsSource = DataSource;
        }
    }
}
