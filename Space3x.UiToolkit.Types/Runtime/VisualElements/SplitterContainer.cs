using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.Types
{
    [UxmlElement]
    [HideInInspector]
    public partial class SplitterContainer : TwoPaneSplitView
    {
        private IVisualElementScheduledItem m_DelayedTask;
        private bool m_IsFixedPanelCollapsed;
        private bool m_FixedPanelCollapsedState;
        private bool m_Attached;
        public VisualElement ActivePanel { get; private set; }

        public override VisualElement contentContainer => ActivePanel ?? base.contentContainer;

        [UxmlAttribute]
        public bool IsFixedPanelActive
        {
            get => ActivePanel != null && ActivePanel == fixedPane;
            set => ActivePanel = value ? fixedPane : flexedPane;
        }
        
        [UxmlAttribute]
        public bool IsFixedPanelCollapsed
        {
            get => m_IsFixedPanelCollapsed;
            set
            {
                if (m_IsFixedPanelCollapsed != value)
                {
                    m_IsFixedPanelCollapsed = value;
                    UpdateFixedPanelCollapsedState();
                }
            }
        }

        public SplitterContainer()
        {
            IsFixedPanelActive = false;
            AddToClassList("ui3x-splitter-setup");
            RegisterCallback<AttachToPanelEvent>(AddEventListeners);
        }

        private void AddEventListeners(AttachToPanelEvent ev)
        {
            UnregisterCallback<AttachToPanelEvent>(AddEventListeners);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChange);
        }
        
        private void OnGeometryChange(GeometryChangedEvent ev)
        {
            if (m_DelayedTask == null)
                m_DelayedTask = schedule.Execute(DelayedSync);
            m_DelayedTask.ExecuteLater(250);
        }

        private void DelayedSync()
        {
            if (!m_Attached)
            {
                m_Attached = true;
                UpdateFixedPanelCollapsedState();
                RemoveFromClassList("ui3x-splitter-setup");
            }
        }

        private void UpdateFixedPanelCollapsedState()
        {
            if (!m_Attached) return;
            if (m_FixedPanelCollapsedState == m_IsFixedPanelCollapsed)
                return;
            if (m_IsFixedPanelCollapsed)
                CollapseChild(fixedPaneIndex);
            else
                UnCollapse();
            m_FixedPanelCollapsedState = m_IsFixedPanelCollapsed;
        }
    }
}
