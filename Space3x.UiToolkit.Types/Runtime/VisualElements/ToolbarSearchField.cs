using System;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.Types
{
    public class ToolbarSearchField : SearchValueFieldBase<TextValueField, string>
    {
        /// <summary>
        ///        <para>
        /// USS class name of text elements in elements of this type.
        /// </para>
        ///      </summary>
        public new static readonly string textUssClassName = SearchValueFieldBase<TextValueField, string>.textUssClassName;

        /// <summary>
        ///        <para>
        /// USS class name of search buttons in elements of this type.
        /// </para>
        ///      </summary>
        public new static readonly string searchButtonUssClassName =
            SearchValueFieldBase<TextValueField, string>.searchButtonUssClassName;

        /// <summary>
        ///        <para>
        /// USS class name of cancel buttons in elements of this type.
        /// </para>
        ///      </summary>
        public new static readonly string cancelButtonUssClassName =
            SearchValueFieldBase<TextValueField, string>.cancelButtonUssClassName;

        /// <summary>
        ///        <para>
        /// USS class name of cancel buttons in elements of this type, when they are off.
        /// </para>
        ///      </summary>
        public new static readonly string cancelButtonOffVariantUssClassName =
            SearchValueFieldBase<TextValueField, string>.cancelButtonOffVariantUssClassName;

        /// <summary>
        ///        <para>
        /// USS class name of elements of this type, when they are using a popup menu.
        /// </para>
        ///      </summary>
        public new static readonly string popupVariantUssClassName =
            SearchValueFieldBase<TextValueField, string>.popupVariantUssClassName;

        /// <summary>
        ///        <para>
        /// USS class name of elements of this type.
        /// </para>
        ///      </summary>
        public new static readonly string ussClassName = "unity-toolbar-search-field";

        /// <summary>
        ///        <para>
        /// The search button.
        /// </para>
        ///      </summary>
        protected new Button searchButton => base.searchButton;

        public TextValueField TextField => base.textInputField;

        /// <summary>
        ///        <para>
        /// Constructor.
        /// </para>
        ///      </summary>
        public ToolbarSearchField() => this.AddToClassList(ToolbarSearchField.ussClassName);

        /// <summary>
        ///        <para>
        /// Removes the text when clearing the field.
        /// </para>
        ///      </summary>
        protected override void ClearTextField()
        {
            // TextField.text = String.Empty;
            this.value = string.Empty;
        }

        /// <summary>
        ///        <para>
        /// Tells if the string is null or empty.
        /// </para>
        ///      </summary>
        /// <param name="fieldValue"></param>
        protected override bool FieldIsEmpty(string fieldValue) => string.IsNullOrEmpty(fieldValue);
    }
}
