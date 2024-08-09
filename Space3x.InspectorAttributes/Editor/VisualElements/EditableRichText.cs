using Space3x.InspectorAttributes.Editor.Extensions;
using Space3x.UiToolkit.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor.VisualElements
{
    [UxmlElement]
    [HideInInspector]
    public partial class EditableLabel : TextElement
    {
        public EditableLabel() => this.WithClasses(UssConstants.UssEditableLabel, UssConstants.UssHelpBox);
    }
    
    [UxmlElement]
    public partial class EditableRichText : TextField
    {
        private TextField m_TextField;
        private EditableLabel m_EditableLabel;
        
        public EditableRichText()
        {
            multiline = true;
            isDelayed = true;
            doubleClickSelectsWord = false;
            tripleClickSelectsLine = false;
            selectAllOnFocus = false;
            selectAllOnMouseUp = false;
            style.whiteSpace = WhiteSpace.PreWrap;
            CreateEditableLabel();
            AddToClassList(UssConstants.UssEditableRichText);
            Add(m_EditableLabel);
            this.RegisterValueChangedCallback(OnValueChange);
            this.RegisterCallbackOnce<AttachToPanelEvent>(OnAttachToPanel);
        }

        private void OnAttachToPanel(AttachToPanelEvent ev)
        {
            this.GetChildren<TextElement>().ForEach(e =>
            {
                var dblClick = new Clickable(OnDoubleClick);
                dblClick.activators.Clear();
                dblClick.activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse, clickCount = 2 });
                e.AddManipulator(dblClick);
            });
        }

        private void OnDoubleClick(EventBase ev) => ToggleInClassList(UssConstants.UssEditMode);

        private void OnValueChange(ChangeEvent<string> ev) => m_EditableLabel.text = ev.newValue;

        private void CreateEditableLabel()
        {
            m_EditableLabel = new EditableLabel()
            {
                style = { whiteSpace = WhiteSpace.PreWrap },
                parseEscapeSequences = true,
                selection =
                {
                    doubleClickSelectsWord = false, 
                    tripleClickSelectsLine = false,
                    isSelectable = true,
                    selectAllOnFocus = false,
                    selectAllOnMouseUp = false
                },
                displayTooltipWhenElided = false,
                enableRichText = true
            };
        }
    }
}
