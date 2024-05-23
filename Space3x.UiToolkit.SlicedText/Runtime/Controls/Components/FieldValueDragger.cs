using System;
using Space3x.UiToolkit.SlicedText.InputFields;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.SlicedText.Components
{
    public class FieldValueDragger<TValue, TInputField, TBaseField> : FieldMouseDragger<TValue>, IValueField<TValue>
        where TInputField : TextValueInputField<TValue>, new()
        where TBaseField : TextBaseField<TValue, TInputField>
        where TValue : struct, IConvertible
    {
        private static bool s_UseYSign;
        private TBaseField m_TargetField;

        public TValue value
        {
            get => m_TargetField.value;
            set => m_TargetField.value = value;
        }

        private FieldValueDragger(IValueField<TValue> drivenField, TBaseField target) : base(drivenField)
        {
            m_TargetField = target;
            m_TargetField.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt) => m_TargetField = null;

        public static FieldValueDragger<TValue, TInputField, TBaseField> Create(TBaseField target, VisualElement draggable)
        {
            ValueFieldLinker drivenField = new ValueFieldLinker();
            var result = new FieldValueDragger<TValue, TInputField, TBaseField>(drivenField, target);
            drivenField.Bind(result);
            result.SetDragZone(draggable, new Rect(0.0f, 0.0f, -1f, -1f));
            draggable.EnableInClassList(BaseField<TValue>.labelDraggerVariantUssClassName, true);
            return result;
        }
        
        public void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, TValue startValue)
        {
            double intDragSensitivity = (double) CalculateIntDragSensitivity(Convert.ToDouble(startValue));
            float acceleration = Acceleration(speed == DeltaSpeed.Fast, speed == DeltaSpeed.Slow);
            long num = (long)Convert.ToInt64(m_TargetField.VisualInput.StringToValue(m_TargetField.VisualInput.Text))
                       + (long) Math.Round((double) NiceDelta((Vector2) delta, acceleration) * intDragSensitivity);
            m_TargetField.value = (TValue)(object)Convert.ToInt32(ClampToInt(num));
        }

        public void StartDragging()
        {
            m_TargetField.VisualInput.EditorEventHandler.PerformOperation(
                EditorAction.SelectNone,
                m_TargetField.VisualInput.ReadOnly);
            m_TargetField.VisualInput.MarkAsDirty();
        }

        public void StopDragging() => m_TargetField.VisualInput.MarkAsDirty();

        private static double CalculateIntDragSensitivity(double value) => Math.Max(1.0, Math.Pow(Math.Abs(value), 0.5) * 0.029999999329447746);
        
        private static float Acceleration(bool shiftPressed, bool altPressed) => (float) ((shiftPressed ? 4.0 : 1.0) * (altPressed ? 0.25 : 1.0));

        private static float NiceDelta(Vector2 deviceDelta, float acceleration)
        {
            deviceDelta.y = -deviceDelta.y;
            if ((double) Mathf.Abs(Mathf.Abs(deviceDelta.x) - Mathf.Abs(deviceDelta.y)) / (double) Mathf.Max(Mathf.Abs(deviceDelta.x), Mathf.Abs(deviceDelta.y)) > 0.10000000149011612)
                s_UseYSign = (double) Mathf.Abs(deviceDelta.x) <= (double) Mathf.Abs(deviceDelta.y);
            return s_UseYSign ? Mathf.Sign(deviceDelta.y) * deviceDelta.magnitude * acceleration : Mathf.Sign(deviceDelta.x) * deviceDelta.magnitude * acceleration;
        }
        
        private static int ClampToInt(long value)
        {
            if (value < (long) int.MinValue)
                return int.MinValue;
            return value > (long) int.MaxValue ? int.MaxValue : (int) value;
        }
        
        private class ValueFieldLinker : IValueField<TValue>
        {
            private IValueField<TValue> m_Linker;
            
            public void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, TValue startValue) => m_Linker.ApplyInputDeviceDelta(delta, speed, startValue);

            public void StartDragging() => m_Linker.StartDragging();

            public void StopDragging() => m_Linker.StopDragging();
        
            public void Bind(IValueField<TValue> field) => m_Linker = field;

            public TValue value
            {
                get => m_Linker.value;
                set => m_Linker.value = value;
            }
        }
    }
}
