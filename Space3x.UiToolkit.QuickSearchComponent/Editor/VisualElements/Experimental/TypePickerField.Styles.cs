using System;
using Space3x.UiToolkit.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.QuickSearchComponent.Editor.VisualElements
{
    public partial class TypePickerField
    {
        protected void ApplyFieldStyles()
        {
            if (m_TextField == null) return;
            m_TextField.WithClasses("unity-base-text-field__input", "unity-base-text-field__input--single-line", "ui3x-text-link")
                .WithClasses(false, "unity-base-field")
                .style.flexShrink = 0f;
            m_TextField.style.paddingRight = 0f;
            labelElement?.WithClasses("unity-base-field__label", "unity-base-text-field__label");
            var (first, second, _) = m_TextField.AsChildren();
            VisualElement textInput = (first is Label) ? second : first; 
            var (textElement, _) = textInput.AsChildren();
            m_Button.WithClasses(false, "unity-text-element", "unity-button")
                .WithClasses("unity-object-field__selector");
            textInput.WithClasses(false, "unity-base-text-field__input", "unity-base-text-field__input--single-line");
            textInput.style.cursor = new StyleCursor(StyleKeyword.Initial);
            textElement.style.cursor = new StyleCursor(StyleKeyword.Initial);
            textElement.style.unityTextAlign = TextAnchor.MiddleLeft;
            textElement.style.paddingLeft = 2f;
            textElement.AddManipulator(new Clickable(() => OnShowPopup?.Invoke(this, m_TextField, ShowWindowMode.Popup)));
        }
    }
}
