using System;
using System.Runtime.CompilerServices;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Editor;
using Space3x.UiToolkit.Types;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
{
    public partial class BindablePropertyField
    {
        private TField ConfigureField<TField, TValue>(
            TField field,
            Func<TField> factory)
            where TField : BaseField<TValue>
        {
            if ((object) field == null)
            {
                field = (TField) factory().WithClasses(BaseField<TValue>.alignedFieldUssClassName);
                
                // TODO: Uncomment
                // field.RegisterValueChangedCallback<TValue>((ev =>
                // {
                //     DebugLog.Info($"<color=#FFFF00FF><b>CALLBACK</b> ConfigureField.RegisterValueChangedCallback&lt;<`{typeof(TValue).Name}`>({field.GetType().Name}): {Property.PropertyPath}</color>");
                //     DebugLog.Info($"<color=#FFFF00FF>// TODO: Uncomment - ITEM! [lV!] .RegisterValueChangedCallback => " +
                //                   $"NotifyValueChanged(); {Property.PropertyPath} - {ev.newValue}</color> (PREV: {ev.previousValue}, CUR: {Property.GetValue()})");
                //     // if (this.dataSource is DataSourceBinding bindableSource)
                //     //     if (RuntimeHelpers.Equals((TValue)bindableSource.Value, ev.newValue) && !RuntimeHelpers.Equals(ev.previousValue, ev.newValue))
                //     //         bindableSource.NotifyValueChanged();
                // }));
                this.dataSource = new DataSourceBinding(Property);
            }
            field.SetBinding(nameof(BaseField<TValue>.value), new DataBinding
            {
                
                dataSourcePath = new PropertyPath(nameof(DataSourceBinding.Value)),
                bindingMode = BindingMode.TwoWay
            });
            
            return field;
        }
        
        private TField ConfigureObjectField<TField, TValue>(
            TField field,
            Func<TField> factory)
            where TValue : UnityEngine.Object
            where TField : BaseField<TValue>
        {
            if ((object) field == null)
            {
                field = (TField) factory().WithClasses(BaseField<TValue>.alignedFieldUssClassName);
                field.RegisterValueChangedCallback<TValue>((ev =>
                {
                    if (this.dataSource is DataSourceObjectBinding bindableSource)
                        if (RuntimeHelpers.Equals((TValue)bindableSource.Value, ev.newValue) && !RuntimeHelpers.Equals(ev.previousValue, ev.newValue))
                            bindableSource.NotifyValueChanged();
                }));
                this.dataSource = new DataSourceObjectBinding(Property);
            }
            field.SetBinding(nameof(BaseField<TValue>.value), new DataBinding
            {
                dataSourcePath = new PropertyPath(nameof(DataSourceObjectBinding.Value)),
                bindingMode = BindingMode.TwoWay
            });
            return field;
        }
    }
}