using System;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.Types
{
    public interface IInputValueDialog
    {
        void Prompt<TField, TValue>(Func<TField> fieldFactory, Action<TValue> callback, string message = "",
            string title = "Input")
            where TField : BaseField<TValue>;
    }
}
