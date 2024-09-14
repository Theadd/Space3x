using System;
using System.Reflection;
using Space3x.Attributes.Types;
using Space3x.InspectorAttributes.Types;
using Space3x.UiToolkit.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
{
    public partial class BindablePropertyField
    {
        private bool TryGetAssignedPropertyValueAndType(out object propertyValue, out Type propertyType)
        {
            var propertyInfo = m_Controller.DeclaringObject?.GetType().GetField(Property.Name, 
                BindingFlags.Instance 
                | BindingFlags.Static
                | BindingFlags.NonPublic
                | BindingFlags.Public);
            if (propertyInfo == null)
            {
                propertyValue = null;
                propertyType = null;
                return false;
            }
            propertyValue = propertyInfo.GetValue(m_Controller.DeclaringObject);
            propertyType = propertyValue?.GetType() ?? propertyInfo.FieldType;
            return true;
        }
        
        private VisualElement BindToBuiltInField()
        {
            VisualElement field = null;
            var isCollectionElement = Property.IsArrayOrListElement();
            var isCollection = Property.IsArrayOrList();
            // TODO: remove block
            if (Property.PropertyPath == "UIView.primary")
            {
                Debug.Log("STOP");
            }
            // The declared underlying element type for collections and collections elements, or the declared property type for non-collections.
            Type declaredPropertyType = isCollectionElement
                ? Property.GetParentProperty().GetUnderlyingElementType()
                : isCollection
                    ? Property.GetUnderlyingElementType()
                    : Property.GetUnderlyingType();
            if (!TryGetAssignedPropertyValueAndType(out object propertyValue, out Type propertyType))
            {
                propertyValue = Property.GetValue();
                propertyType = propertyValue?.GetType();
            }

            if (isCollection)
            {
                Func<VisualElement> itemFactory = () => new BindablePropertyField().Resolve(showInInspector: true);
                field = declaredPropertyType switch
                {
                    _ when declaredPropertyType == typeof(int) => ConfigureListView<int>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(uint) => ConfigureListView<uint>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(long) => ConfigureListView<long>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(ulong) => ConfigureListView<ulong>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(double) => ConfigureListView<double>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(float) => ConfigureListView<float>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(bool) => ConfigureListView<bool>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(string) => ConfigureListView<string>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(Color) => ConfigureListView<Color>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(Vector2) => ConfigureListView<Vector2>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(Vector3) => ConfigureListView<Vector3>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(Vector4) => ConfigureListView<Vector4>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(Rect) => ConfigureListView<Rect>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(Bounds) => ConfigureListView<Bounds>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(Vector2Int) => ConfigureListView<Vector2Int>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(Vector3Int) => ConfigureListView<Vector3Int>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(RectInt) => ConfigureListView<RectInt>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(BoundsInt) => ConfigureListView<BoundsInt>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(AnimationCurve) => ConfigureListView<AnimationCurve>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(Gradient) => ConfigureListView<Gradient>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(Hash128) => ConfigureListView<Hash128>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(NamedType) => ConfigureListView<NamedType>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(SerializableType) => ConfigureListView<SerializableType>(itemFactory, Field as ListView),
                    _ when declaredPropertyType == typeof(UnityEngine.Object) => ConfigureListView<UnityEngine.Object>(itemFactory, Field as ListView),
                    _ when Property.HasChildren() => ConfigureDynamicListView(itemFactory, Field as ListView),
                    _ when declaredPropertyType.IsEnum => ConfigureDynamicListView(itemFactory, Field as ListView),
                    _ => FieldNotImplemented()
                };
            }
            else
            {
                DebugLog.Info($"[USK3] [BindablePropertyField] BindToBuiltInField ON ({declaredPropertyType?.Name}): {Property.PropertyPath}");
                if (declaredPropertyType == null)
                    return FieldNotImplemented();
                
                if (declaredPropertyType.IsPrimitive)
                {
                    field = declaredPropertyType switch
                    {
                        _ when declaredPropertyType == typeof(long) => ConfigureField<LongField, long>(Field as LongField, () => new LongField(label: Property.DisplayName())),
                        _ when declaredPropertyType == typeof(ulong) => ConfigureField<UnsignedLongField, ulong>(Field as UnsignedLongField, () => new UnsignedLongField(label: Property.DisplayName())),
                        _ when declaredPropertyType == typeof(uint) => ConfigureField<UnsignedIntegerField, uint>(Field as UnsignedIntegerField, () => new UnsignedIntegerField(label: Property.DisplayName())),
                        _ when declaredPropertyType == typeof(int) => ConfigureField<IntegerField, int>(Field as IntegerField, () => new IntegerField(label: Property.DisplayName())),
                        _ when declaredPropertyType == typeof(double) => ConfigureField<DoubleField, double>(Field as DoubleField, () => new DoubleField(label: Property.DisplayName())),
                        _ when declaredPropertyType == typeof(float) => ConfigureField<FloatField, float>(Field as FloatField, () => new FloatField(label: Property.DisplayName())),
                        _ when declaredPropertyType == typeof(bool) => ConfigureField<Toggle, bool>(Field as Toggle, () => new Toggle(label: Property.DisplayName())),
                        _ => null
                    };
                } 
                else if (declaredPropertyType.IsValueType)
                {
#if UNITY_EDITOR
                    if (!m_Controller.IsRuntimeUI)
                        field = (declaredPropertyType != typeof(Color)) ? null : ConfigureField<UnityEditor.UIElements.ColorField, Color>(Field as UnityEditor.UIElements.ColorField, () => new UnityEditor.UIElements.ColorField(label: Property.DisplayName()));
#endif
                    field ??= declaredPropertyType switch
                    {
                        _ when declaredPropertyType == typeof(Vector2) => ConfigureField<Vector2Field, Vector2>(Field as Vector2Field, () => new Vector2Field(label: Property.DisplayName())),
                        _ when declaredPropertyType == typeof(Vector3) => ConfigureField<Vector3Field, Vector3>(Field as Vector3Field, () => new Vector3Field(label: Property.DisplayName())),
                        _ when declaredPropertyType == typeof(Vector4) => ConfigureField<Vector4Field, Vector4>(Field as Vector4Field, () => new Vector4Field(label: Property.DisplayName())),
                        _ when declaredPropertyType == typeof(Rect) => ConfigureField<RectField, Rect>(Field as RectField, () => new RectField(label: Property.DisplayName())),
                        _ when declaredPropertyType == typeof(Bounds) => ConfigureField<BoundsField, Bounds>(Field as BoundsField, () => new BoundsField(label: Property.DisplayName())),
                        _ when declaredPropertyType == typeof(Vector2Int) => ConfigureField<Vector2IntField, Vector2Int>(Field as Vector2IntField, () => new Vector2IntField(label: Property.DisplayName())),
                        _ when declaredPropertyType == typeof(Vector3Int) => ConfigureField<Vector3IntField, Vector3Int>(Field as Vector3IntField, () => new Vector3IntField(label: Property.DisplayName())),
                        _ when declaredPropertyType == typeof(RectInt) => ConfigureField<RectIntField, RectInt>(Field as RectIntField, () => new RectIntField(label: Property.DisplayName())),
                        _ when declaredPropertyType == typeof(BoundsInt) => ConfigureField<BoundsIntField, BoundsInt>(Field as BoundsIntField, () => new BoundsIntField(label: Property.DisplayName())),
                        _ when declaredPropertyType == typeof(Hash128) => ConfigureField<Hash128Field, Hash128>(Field as Hash128Field, () => new Hash128Field(label: Property.DisplayName())),
                        _ => null
                    };
                }
#if UNITY_EDITOR
                if (field == null && !m_Controller.IsRuntimeUI)
                    field = declaredPropertyType switch
                    {
                        _ when declaredPropertyType == typeof(AnimationCurve) => ConfigureField<UnityEditor.UIElements.CurveField, AnimationCurve>(Field as UnityEditor.UIElements.CurveField, () => new UnityEditor.UIElements.CurveField(label: Property.DisplayName())),
                        _ when declaredPropertyType == typeof(Gradient) => ConfigureField<UnityEditor.UIElements.GradientField, Gradient>(Field as UnityEditor.UIElements.GradientField, () => new UnityEditor.UIElements.GradientField(label: Property.DisplayName())),
                        _ when declaredPropertyType.IsEnum && declaredPropertyType.IsDefined(typeof(FlagsAttribute), false) =>
                            ConfigureField<UnityEditor.UIElements.EnumFlagsField, Enum>(Field as UnityEditor.UIElements.EnumFlagsField, () => new UnityEditor.UIElements.EnumFlagsField(label: Property.DisplayName(), (Enum)propertyValue ?? default)),
                        _ when typeof(UnityEngine.Object).IsAssignableFrom(declaredPropertyType) => ConfigureObjectField<UnityEditor.UIElements.ObjectField, UnityEngine.Object>(Field as UnityEditor.UIElements.ObjectField, () => new UnityEditor.UIElements.ObjectField(label: Property.DisplayName()) 
                        { 
                            objectType = declaredPropertyType, 
                            allowSceneObjects = true 
                        }),
                        _ => null
                    };
#endif
                if (field != null) return field;
                field = declaredPropertyType switch
                {
                    _ when declaredPropertyType == typeof(string) => ConfigureField<TextField, string>(Field as TextField, () => new TextField(label: Property.DisplayName())),
                    _ when typeof(UnityEngine.Object).IsAssignableFrom(declaredPropertyType) => ConfigureChildrenFields(propertyType ?? declaredPropertyType, isNullValue: propertyValue == null),
                    _ when Property.HasChildren() => ConfigureChildrenFields(propertyType ?? declaredPropertyType, isNullValue: propertyValue == null),
                    _ when declaredPropertyType.IsEnum =>
                        ConfigureField<EnumField, Enum>(Field as EnumField, () => new EnumField(label: Property.DisplayName(), (Enum)propertyValue ?? default)),
                    _ => FieldNotImplemented()
                };
            }
            return field;
        }
        
        private VisualElement FieldNotImplemented() => new Label(Property.DisplayName() + " - [NOT IMPLEMENTED]");
    }
}
