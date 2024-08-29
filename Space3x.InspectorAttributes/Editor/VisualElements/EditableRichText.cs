using System.Linq;
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
            RegisterCallbackOnce<AttachToPanelEvent>(OnAttachToPanel);
        }

        private void OnAttachToPanel(AttachToPanelEvent ev)
        {
            var dblClick = new Clickable(OnDoubleClick);
            dblClick.activators.Clear();
            dblClick.activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse, clickCount = 2 });
            m_EditableLabel.AddManipulator(dblClick);
            Children()
                .FirstOrDefault(c => c is not Label && c != m_EditableLabel)
                .GetChildren<TextElement>()
                .ForEach(e => e.RegisterCallback<BlurEvent>(OnBlur));
        }

        private void OnBlur(BlurEvent evt) => this.WithClasses(false, UssConstants.UssEditMode);
        
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
        
        public override void SetValueWithoutNotify(string newValue)
        {
            base.SetValueWithoutNotify(newValue);
            if (m_EditableLabel != null)
                m_EditableLabel.text = newValue;
        }
    }
}
