using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Space3x.Properties.Types;
using Space3x.Properties.Types.Editor;
using Space3x.UiToolkit.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
{
    [UxmlElement]
    [HideInInspector]
    public partial class InvokableField : TextField
    {
        private static IInputValueDialog s_Implementation;
        private Button m_Button;
        private List<object> m_ParameterValues;
        private List<ParameterInfo> m_ParameterInfos;

        public IPropertyNode Property;

        public static void RegisterImplementationProvider(IInputValueDialog provider) => s_Implementation = provider;
        
        public InvokableField() : this(string.Empty) { }

        public InvokableField(string label) : base(label)
        {
            this.WithClasses(UssConstants.UssInvokableField);
            this.isReadOnly = true;
            m_Button = new Button(GetMethodParameters)
            {
                text = "\u25b8",
            };
            m_Button.WithClasses(UssConstants.UssButtonGroupButton, UssConstants.UssButtonGroupButtonRight);
            Add(m_Button);
        }

        private void GetMethodParameters()
        {
            var targetObject = (object)Property.GetDeclaringObject();
            m_ParameterValues = new List<object>();
            m_ParameterInfos = new List<ParameterInfo>();
            var methodInfo = targetObject?.GetType().GetMethod(Property.Name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (methodInfo != null)
            {
                m_ParameterInfos = methodInfo.GetParameters().ToList();
                RequestParameterValues();
            }
        }

        private void RequestParameterValues()
        {
            if (m_ParameterInfos.Count == 0)
            {
                InvokeWithParameters();
                return;
            }
            var parameterInfo = m_ParameterInfos.FirstOrDefault();
            m_ParameterInfos = m_ParameterInfos.Skip(1).ToList();
            #if UNITY_EDITOR
            var parameterName = UnityEditor.ObjectNames.NicifyVariableName(parameterInfo!.Name);
            #else
            var parameterName = parameterInfo.Name;
            #endif
            if (!ShowInputPrompt(parameterInfo.ParameterType, parameterName))
                Debug.LogError($"Could not show input prompt for parameter '{parameterInfo.Name}' of type '{parameterInfo.ParameterType}'");
        }

        private void OnParameterValueReceived<TValue>(TValue obj)
        {
            m_ParameterValues.Add((object)obj);
            RequestParameterValues();
        }

        private bool ShowInputPrompt(Type parameterType, string parameterName)
        {
            return parameterType switch
            {
                not null when parameterType == typeof(long) => TryShowPrompt<LongField, long>(parameterName),
                not null when parameterType == typeof(ulong) => TryShowPrompt<UnsignedLongField, ulong>(parameterName),
                not null when parameterType == typeof(uint) => TryShowPrompt<UnsignedIntegerField, uint>(parameterName),
                not null when parameterType == typeof(int) => TryShowPrompt<IntegerField, int>(parameterName),
                not null when parameterType == typeof(double) => TryShowPrompt<DoubleField, double>(parameterName),
                not null when parameterType == typeof(float) => TryShowPrompt<FloatField, float>(parameterName),
                not null when parameterType == typeof(bool) => TryShowPrompt<Toggle, bool>(parameterName),
                not null when parameterType == typeof(string) => TryShowPrompt<TextField, string>(parameterName),
                not null when parameterType == typeof(Vector2) => TryShowPrompt<Vector2Field, Vector2>(parameterName),
                not null when parameterType == typeof(Vector3) => TryShowPrompt<Vector3Field, Vector3>(parameterName),
                not null when parameterType == typeof(Vector4) => TryShowPrompt<Vector4Field, Vector4>(parameterName),
                not null when parameterType == typeof(Rect) => TryShowPrompt<RectField, Rect>(parameterName),
                not null when parameterType == typeof(Bounds) => TryShowPrompt<BoundsField, Bounds>(parameterName),
                not null when parameterType == typeof(Vector2Int) => TryShowPrompt<Vector2IntField, Vector2Int>(parameterName),
                not null when parameterType == typeof(Vector3Int) => TryShowPrompt<Vector3IntField, Vector3Int>(parameterName),
                not null when parameterType == typeof(RectInt) => TryShowPrompt<RectIntField, RectInt>(parameterName),
                not null when parameterType == typeof(BoundsInt) => TryShowPrompt<BoundsIntField, BoundsInt>(parameterName),
                not null when parameterType == typeof(Hash128) => TryShowPrompt<Hash128Field, Hash128>(parameterName),
                #if UNITY_EDITOR
                not null when parameterType == typeof(Color) => TryShowPrompt<UnityEditor.UIElements.ColorField, Color>(parameterName),
                not null when parameterType == typeof(AnimationCurve) => TryShowPrompt<UnityEditor.UIElements.CurveField, AnimationCurve>(parameterName),
                not null when parameterType == typeof(Gradient) => TryShowPrompt<UnityEditor.UIElements.GradientField, Gradient>(parameterName),
                not null when typeof(UnityEngine.Object).IsAssignableFrom(parameterType) => TryShowObjectPrompt(parameterName, parameterType),
                #endif
                _ => false
            };
        }

        private bool TryShowPrompt<TField, TValue>(string parameterName)
            where TField : BaseField<TValue>, new()
        {
            if (s_Implementation == null)
                throw new NotImplementedException(
                    $"You must register a provider implementing {nameof(IInputValueDialog)} for " +
                    $"{nameof(InvokableField)} to be able to work with non-parameterless methods in player builds.\n" +
                    $"See {nameof(Space3x)}.InspectorAttributes.{nameof(InvokableField)}.{nameof(RegisterImplementationProvider)}.");
            s_Implementation.Prompt<TField, TValue>(() => new TField() { label = parameterName }, OnParameterValueReceived);
            return true;
        }
        
        #if UNITY_EDITOR
        private bool TryShowObjectPrompt(string parameterName, Type objectType)
        {
            s_Implementation.Prompt<UnityEditor.UIElements.ObjectField, UnityEngine.Object>(() => new UnityEditor.UIElements.ObjectField()
            {
                label = parameterName,
                objectType = objectType,
                allowSceneObjects = true
            }, OnParameterValueReceived);
            return true;
        }
        #endif
        
        private void InvokeWithParameters()
        {
            if (Property.TryCreateInvokable<object, object>(Property.Name, out var invokable))
            {
                var result = invokable.InvokeWith(m_ParameterValues.ToArray());
                if (Property is IInvokablePropertyNode invokablePropertyNode)
                    invokablePropertyNode.SetValue(result);
            }
        }
    }
}
