using System;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.Types
{
    public abstract partial class TextInputFieldBase<TValueType>
    {
        [Obsolete("SetVerticalScrollerVisibility is deprecated. Use TextField.verticalScrollerVisibility instead.")]
        public bool SetVerticalScrollerVisibility(ScrollerVisibility sv) => textInputBase.SetVerticalScrollerVisibility(sv);
        
        protected void OnPlaceholderChanged()
        {
            if (!string.IsNullOrEmpty(this.textEdition.placeholder))
                this.RegisterCallback<ChangeEvent<TValueType>>(
                    new EventCallback<ChangeEvent<TValueType>>(this.UpdatePlaceholderClassList));
            else
                this.UnregisterCallback<ChangeEvent<TValueType>>(
                    new EventCallback<ChangeEvent<TValueType>>(this.UpdatePlaceholderClassList));
            this.UpdatePlaceholderClassList();
        }

        protected void UpdatePlaceholderClassList(ChangeEvent<TValueType> evt = null)
        {
            if (this.textInputBase.textElement.ShowPlaceholderText())
                this.visualInput.AddToClassList(TextInputFieldBase<TValueType>.placeholderUssClassName);
            else
                this.visualInput.RemoveFromClassList(TextInputFieldBase<TValueType>.placeholderUssClassName);
        }
    }
}
