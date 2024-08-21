using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.QuickSearchComponent.Editor.VisualElements
{
    public partial class QuickSearchElement : INotifyValueChanged<IEnumerable<Type>>
    {
        [UxmlAttribute] 
        public bool IsSidePanelDisabled;

        [SerializeField]
        private bool IsSidePanelOpen;
        
        private List<NamedSymbol> m_ActiveSelection = new List<NamedSymbol>();
        private List<NamedSymbol> m_InitialSelection = null;
        private IEnumerable<Type> m_Value;

        private void AddEventListeners()
        {
            // EDIT: m_SearchField.TextField.VisualInput.ForceRenderCursorCaret = true;
            m_SearchField.TextField.textInput.textElement.ForceRenderCursorCaret = true;
            m_SearchField.TextField.selectAllOnFocus = false;
            m_SearchField.TextField.selectAllOnMouseUp = false;
            m_SearchField.TextField.selectWordByDoubleClick = true;
            m_SearchField.TextField.RegisterCallback<FocusEvent>(OnTextFieldFocus);
            m_SearchField.TextField.RegisterCallback<AttachToPanelEvent>(ev => SetFocusToListViewContainer());
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            m_ListView.RegisterValueChangedCallback(OnSelectionChanged);
            m_ListView.SelectionCompleted += OnSelectionCompleted;
            m_ToggleSidePanelButton.clickable = new Clickable(OnToggleSidePanelClick);
            if (!IsSidePanelOpen || IsSidePanelDisabled) m_Splitter.EnableInClassList("ui3x-full-width", true);
            UpdateSidePanelCollapsedState();
            SetFocusToListViewContainer();
            RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt) => m_InitialSelection = null;

        public void SetValueWithoutNotify(IEnumerable<Type> newValue)
        {
            m_Value = (newValue ?? new List<Type>()).ToList();
            m_ActiveSelection = m_Value.Select(t => new NamedSymbol(t)).ToList();
            if (m_InitialSelection == null)
                m_InitialSelection = m_ActiveSelection.ToList();
        }

        internal static readonly BindingId valueProperty = (BindingId) nameof (value);
        
        [CreateProperty]
        public IEnumerable<Type> value
        {
            get => m_Value;
            set
            {
                var previousValue = m_Value.ToList();
                var newValue = (value ?? new List<Type>()).ToList();
                if (panel != null)
                {
                    SetValueWithoutNotify(newValue);
                    NotifyValueChanged(previousValue, newValue);
                }
                else
                    SetValueWithoutNotify(newValue);
            }
        }

        private void NotifyValueChanged(IEnumerable<Type> previousValue, IEnumerable<Type> newValue)
        {
            using (ChangeEvent<IEnumerable<Type>> pooled = ChangeEvent<IEnumerable<Type>>.GetPooled(previousValue, newValue))
            {
                pooled.target = (VisualElement) this;
                SendEvent((EventBase) pooled);
            }
            NotifyPropertyChanged(in valueProperty);
        }

        private void OnSelectionCompleted()
        {
            m_InitialSelection = null;
            CloseWindow();
        }

        private void OnSelectionChanged(ChangeEvent<IEnumerable<int>> ev)
        {
            m_ActiveSelection = ev.newValue.Select(i => m_FilteredDatasource[i]).ToList();
            value = m_ActiveSelection.Select(t => t.Value).ToList();
        }
        
        private void UpdateSelectedIndicesInFilteredDatasource()
        {
            var selectedIndices = m_ActiveSelection
                .Select(t => m_FilteredDatasource.IndexOf(t))
                .Where(i => i >= 0)
                .ToList();
            m_ListView.SetValueWithoutNotify(selectedIndices);
        }

        private void OnTextFieldFocus(FocusEvent evt)
        {
            SetFocusToListViewContainer();
        }

        private void OnToggleSidePanelClick(EventBase e)
        {
            if (!IsSidePanelDisabled)
                m_Splitter.ToggleInClassList("ui3x-full-width");
            else
                m_Splitter.RemoveFromClassList("ui3x-full-width");
            UpdateSidePanelCollapsedState();
        }

        private void UpdateSidePanelCollapsedState()
        {
            IsSidePanelOpen = !m_Splitter.ClassListContains("ui3x-full-width");
            m_Splitter.IsFixedPanelCollapsed = !IsSidePanelOpen;
        }
        
        /* Keyboard Events */
 
        private void OnKeyDown(KeyDownEvent ev)
        {
            switch (ev.keyCode)
            {
                case KeyCode.DownArrow:
                case KeyCode.UpArrow:
                    // Ok
                    break;
                case KeyCode.Escape:
                    if (m_ListView.GetSelectedItems().Any())
                    {
                        m_ListView.ClearSelection();
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(m_SearchString))
                        {
                            m_SearchField.value = string.Empty;
                        }
                        else
                        {
                            CloseWindow();
                        }
                    }
                    break;
                case KeyCode.KeypadEnter:
                case KeyCode.Return:
                    break;
                default:
                    m_SearchField.TextField.textInput.HandleKeyDownEvent(ev);
                    break;
            }
        }

        private void SetFocusToListViewContainer() => m_ListView.Q<VisualElement>("unity-content-container")?.Focus();

        private IVisualElementScheduledItem m_CloseWindowTask;
        
        private void CloseWindow()
        {
            m_CloseWindowTask = schedule.Execute(DoCloseWindow);
            m_CloseWindowTask.ExecuteLater(1);
        }

        private void DoCloseWindow()
        {
            m_CloseWindowTask.Pause();
            try
            {
                if (EditorWindow.focusedWindow.rootVisualElement.worldBound.Equals(panel.visualTree.worldBound) || EditorWindow.focusedWindow is QuickSearchWindow) 
                    EditorWindow.focusedWindow.Close();
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
