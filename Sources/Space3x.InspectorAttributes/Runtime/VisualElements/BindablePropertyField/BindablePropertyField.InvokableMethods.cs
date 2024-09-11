using System;
using Space3x.Properties.Types;
using Space3x.UiToolkit.Types;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
{
    public partial class BindablePropertyField
    {
        private VisualElement BindToInvokablePropertyNode()
        {
            VisualElement field = null;
            field = ConfigureInvokableField<InvokableField, string>(Field as InvokableField, () => new InvokableField(label: Property.DisplayName())
            {
                Property = Property
            });
            
            return field;
        }

        private TField ConfigureInvokableField<TField, TValue>(
            TField field,
            Func<TField> factory)
            where TField : BaseField<TValue>
        {
            // TODO: This is a draft.
            if ((object) field == null)
            {
                field = (TField)factory().WithClasses(BaseField<TValue>.alignedFieldUssClassName);
                if (Property is IInvokablePropertyNode invokableProperty)
                {
                    if (field is BaseField<string> invokableField)
                    {
                        invokableProperty.ValueChanged += p => invokableField.value = p.Value?.ToString() ?? string.Empty;
                    }
                }
            }

            return field;
        }
    }
}