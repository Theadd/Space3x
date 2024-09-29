using System;
using Space3x.UiToolkit.Types;
using UnityEditor;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes.Editor
{
    [InitializeOnLoad]
    public class InputValueDialogImplementationProvider : IInputValueDialog
    {
        static InputValueDialogImplementationProvider() =>
            InvokableField.RegisterImplementationProvider(new InputValueDialogImplementationProvider());
        
        public void Prompt<TField, TValue>(Func<TField> fieldFactory, Action<TValue> callback, string message = "", string title = "Input") 
            where TField : BaseField<TValue>
        {
            InputValueDialog.Prompt<TField, TValue>(fieldFactory, callback, message, title);
        }
    }
}
