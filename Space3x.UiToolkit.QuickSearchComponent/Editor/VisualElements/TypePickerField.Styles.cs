using System;
using Space3x.UiToolkit.Types;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.QuickSearchComponent.Editor.VisualElements
{
    public partial class TypePickerField
    {
        private static readonly TypeRewriter.IStyles DisplayStyle = new TypeRewriter.Styles(StyleTag.NoStyle, StyleTag.Light, StyleTag.Grey);
        
        protected void ApplyFieldStyles()
        {
            if (m_TextField == null) return;
            m_TextField //.WithClasses("unity-base-text-field__input", "unity-base-text-field__input--single-line", "ui3x-text-link")
                .WithClasses(ObjectField.ussClassName)
                .WithClasses(false, "unity-base-field");
                // .style.flexShrink = 0f;
            // m_TextField.style.paddingRight = 0f;
            labelElement?.WithClasses("unity-base-field__label", "unity-base-text-field__label");
            var (first, second, _) = m_TextField.AsChildren();
            VisualElement textInput = (first is Label) ? second : first;
            textInput.WithClasses(ObjectField.inputUssClassName, "unity-base-text-field__input", "unity-base-text-field__input--single-line", "ui3x-text-link")
                .style.flexShrink = 0f;
            textInput.style.paddingRight = 0f;
            var (textElement, _) = textInput.AsChildren();
            ((TextElement)textElement).enableRichText = true;
            m_Button = new Button(() => OnShowPopup?.Invoke(this, m_TextField, ShowWindowMode.NormalWindow)) { text = " " };
            textInput.Add(m_Button);
            m_Button.WithClasses(false, "unity-text-element", "unity-button")
                .WithClasses("unity-object-field__selector");
            textInput.WithClasses(false, "unity-base-text-field__input", "unity-base-text-field__input--single-line");
            textInput.style.cursor = new StyleCursor(StyleKeyword.Initial);
            textInput.style.flexDirection = FlexDirection.Row;
            textElement.style.cursor = new StyleCursor(StyleKeyword.Initial);
            textElement.style.unityTextAlign = TextAnchor.MiddleLeft;
            textElement.style.paddingLeft = 2f;
            textElement.AddManipulator(new Clickable(() => OnShowPopup?.Invoke(this, m_TextField, ShowWindowMode.Popup)));
        }
    }
}
