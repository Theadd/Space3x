using System;
using System.Collections.Generic;
using System.Linq;
using Space3x.UiToolkit.Types;
using Space3x.Unstable;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.UiToolkit.QuickSearchComponent.Editor.VisualElements
{
    [UxmlElement(uxmlName: "ui3x.TypeField")]
    [HideInInspector]
    public partial class TypeField : BaseField<string>, IQuickSearchable
    {
        public TextField TextField;
        public Button Button;
        public SerializedProperty Property { get; protected set; }
        public SerializedProperty CollectionProperty { get; protected set; }
        public int PropertyIndex { get; protected set; }
        public VisualElement VisualInput { get; }
        
        private const string UndoGroupName = "Selected Type Change";

        public Action<TypeField, VisualElement, ShowWindowMode> OnShowPopup;

        private Type m_Type;
        private bool m_ShowLabel;

        protected Type CurrentType => m_Type;

        public TypeField() : this(true) {}
        
        public TypeField(bool showLabel, string initialLabel = "Type Field") : this(visualInput: new TextField()
        {
            multiline = false,
            isReadOnly = true,
            selectAllOnFocus = false,
            selectAllOnMouseUp = false,
            textEdition = { hidePlaceholderOnFocus = true, placeholder = "None" },
            textSelection = { doubleClickSelectsWord = true, tripleClickSelectsLine = true },
            style =
            {
                marginRight = 0f,
            }
        }, initialLabel: showLabel ? initialLabel : null)
        {
            m_ShowLabel = showLabel;
            this.WithClasses(m_ShowLabel, alignedFieldUssClassName, "unity-base-text-field");
        }
        
        protected TypeField(VisualElement visualInput, string initialLabel = null)
            : base(label: initialLabel, visualInput)
        {
            VisualInput = visualInput;
            RegisterCallbackOnce<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<SerializedPropertyChangeEvent>(OnPropertyChanged);
        }

        protected virtual void OnAttachToPanel(AttachToPanelEvent evt)
        {
            Add(CreateSearchableGUI());
            label = m_ShowLabel ? Property?.displayName ?? (string.IsNullOrEmpty(label) ? "Type Field" : label) : null;
            ApplyFieldStyles();
            MarkDirtyRepaint();
        }

        protected virtual VisualElement CreateSearchableGUI()
        {
            if (VisualInput is TextField)
            {
                TextField = (TextField)VisualInput;
            }
            else
            {
                TextField = new TextField()
                {
                    multiline = false,
                    isReadOnly = true,
                    selectAllOnFocus = false,
                    selectAllOnMouseUp = false,
                    textEdition = { hidePlaceholderOnFocus = true, placeholder = "None" },
                    textSelection = { doubleClickSelectsWord = true, tripleClickSelectsLine = true },
                    style =
                    {
                        marginRight = 0f,
                    }
                };
            }
            Button = new Button(() => OnShowPopup?.Invoke(this, TextField, ShowWindowMode.NormalWindow)) { text = " " };
            TextField.Add(Button);
            if (m_Type != null) TextField.value = m_Type.Name;
            return TextField;
        }

        protected void ApplyFieldStyles()
        {
            if (TextField == null) return;
            TextField.WithClasses("unity-base-text-field__input", "unity-base-text-field__input--single-line", "ui3x-text-link")
                // .WithClasses(false, "unity-base-field__inspector-field")
                .style.flexShrink = 0f;
            TextField.style.paddingRight = 0f;
            labelElement?.WithClasses("unity-base-field__label", "unity-base-text-field__label");
            var (first, second, _) = TextField.AsChildren();
            VisualElement textInput = (first is Label) ? second : first; 
            var (textElement, _) = textInput.AsChildren();
            Button.WithClasses(false, "unity-text-element", "unity-button")
                .WithClasses("unity-object-field__selector");
            textInput.WithClasses(false, "unity-base-text-field__input", "unity-base-text-field__input--single-line");
            textInput.style.cursor = new StyleCursor(StyleKeyword.Initial);
            textElement.style.cursor = new StyleCursor(StyleKeyword.Initial);
            textElement.style.unityTextAlign = TextAnchor.MiddleLeft;
            textElement.style.paddingLeft = 2f;
            textElement.AddManipulator(new Clickable(() => OnShowPopup?.Invoke(this, TextField, ShowWindowMode.Popup)));
            
        }

        public virtual void Unbind()
        {
            ((VisualElement) this).Unbind();
            CollectionProperty = null;
            PropertyIndex = -1;
            Property = null;
        }
        
        public virtual void BindProperty(SerializedProperty property, int propertyIndex = -1)
        {
            CollectionProperty = propertyIndex >= 0 ? property : null;
            PropertyIndex = propertyIndex;
            Property = propertyIndex == -1 ? property : property.GetArrayElementAtIndex(propertyIndex);
            label = m_ShowLabel ? Property?.displayName ?? (string.IsNullOrEmpty(label) ? "Type Field" : label) : null;
            SetValue(GetTypeFromSerializedPropertyValue(Property));
            ((VisualElement) this).Unbind();
            ((IBindable) this).BindProperty(Property);
            TypeUndoRedoController.Bind(Property, this);
//            UpdateLabel();
            MarkDirtyRepaint();
        }

        private void OnPropertyChanged(SerializedPropertyChangeEvent ev)
        {
            Debug.Log($"[TypeField] OnPropertyChanged");
            var type = GetTypeFromSerializedPropertyValue(ev.changedProperty);
            SetValue(type);
        }
        
        protected void SetValue(Type type)
        {
            m_Type = type;
            if (TextField != null) TextField.value = TypeRewriter.AsDisplayName(type, TypeRewriter.NoStyle);     // type?.Name ?? string.Empty;
        }
        
        protected virtual Type GetTypeFromSerializedPropertyValue(SerializedProperty property)
        {
            if (property.GetUnderlyingValue() != null)
            {
                dynamic sType = Convert.ChangeType(property.GetUnderlyingValue(), property.GetUnderlyingType());
                return ((Type) sType);
            }

            return (Type) null;
        }

        public void OnShow(QuickSearchElement element)
        {
            element.RegisterValueChangedCallback(OnValueChangeCallback);

            if (m_Type == null)
                element.SetValueWithoutNotify(new List<Type>() {});
            else
                element.SetValueWithoutNotify(new List<Type>() { m_Type });
        }

        public void OnHide(QuickSearchElement element)
        {
            element.UnregisterValueChangedCallback(OnValueChangeCallback);
        }
        
        private void OnValueChangeCallback(ChangeEvent<IEnumerable<Type>> e) => OnValueChange(e.newValue);

        protected virtual void OnValueChange(IEnumerable<Type> newValues)
        {
            var enumerable = newValues.ToList();
            Type newValue = enumerable.Any() ? enumerable.First() : null;

            Property.serializedObject.Update();
            Type oldValue = GetTypeFromSerializedPropertyValue(Property);
            TypeUndoRedoController.RecordObject(Property.serializedObject.targetObject, UndoGroupName);

            SetPropertyValue(newValue);
            SetValue(newValue);
            
            Property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            TypeUndoRedoController.AddValue(Undo.GetCurrentGroup(), Property);
            TypeUndoRedoController.RegisterValueChange(Undo.GetCurrentGroup(), oldValue, newValue);
            Undo.FlushUndoRecordObjects();
            Undo.IncrementCurrentGroup();
        }

//        protected virtual void OnValueChange(IEnumerable<Type> newValues)
//        {
//            var enumerable = newValues.ToList();
//            Type newValue = enumerable.Any() ? enumerable.First() : null;
//
//            var so = new SerializedObject(Property.serializedObject.targetObject);
//            SetPropertyValue(newValue, null, so);
//            SetValue(newValue);
//            so.ApplyModifiedProperties();
//            Property.serializedObject.Update();
//        }

        protected virtual void SetPropertyValue(Type newValue, object newValueInstance = null)
        {
            if (PropertyIndex == -1)
                Property.SetUnderlyingValue(newValue != null
                    ? Unsafe3x.CreateWrapper<Type>(Property.GetUnderlyingType(), newValue)
                    : null);
            else
                CollectionProperty.GetArrayElementAtIndex(PropertyIndex).boxedValue = newValue != null
                    ? Unsafe3x.CreateWrapper<Type>(Property.GetUnderlyingType(), newValue)
                    : null;
        }

//        protected virtual void SetPropertyValue(Type newValue, object newValueInstance = null, SerializedObject serializedObj = null)
//        {
//            var propertyIndex = PropertyIndex;
//            SerializedProperty property;
//            SerializedProperty collectionProperty;
//            if (serializedObj != null)
//            {
//                property = serializedObj.FindProperty(Property.propertyPath);
//                collectionProperty = propertyIndex == -1 ? null : serializedObj.FindProperty(CollectionProperty.propertyPath);
//            }
//            else
//            {
//                property = Property;
//                collectionProperty = CollectionProperty;
//            }
//                
//            if (propertyIndex == -1)
//                property.SetUnderlyingValue(newValue != null
//                    ? Unsafe3x.CreateWrapper<Type>(property.GetUnderlyingType(), newValue)
//                    : null);
//            else
//                collectionProperty.GetArrayElementAtIndex(propertyIndex).boxedValue = newValue != null
//                    ? Unsafe3x.CreateWrapper<Type>(property.GetUnderlyingType(), newValue)
//                    : null;
//        }

        public virtual void OnUndoRedoCallback(in UndoRedoInfo undo)
        {
//            Debug.Log("[TypeField] OnUndoRedoCallback: " + undo.undoGroup.ToString() + " @ " + this);
            
            if (!undo.undoName.Equals(UndoGroupName))
                throw new Exception("Invalid call to TypeField.OnUndoRedoCallback({`" + undo.undoName + "`})");
            
            try
            {
                Property.serializedObject.Update();
                // var t = GetTypeFromSerializedPropertyValue(Property);
                var t = undo.isRedo 
                    ? TypeUndoRedoController.GetRedoValue(undo.undoGroup) 
                    : TypeUndoRedoController.GetUndoValue(undo.undoGroup);
                Debug.Log($"[TypeField] OnUndoRedoCallback: {PropertyIndex} {undo.undoGroup}; t: {t}; CurrentType: {CurrentType}");
                if (t != CurrentType)
                {
                    SetPropertyValue(t);
                    Property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    SetValue(t);
                    ((IBindable) this).BindProperty(Property);
                }
            }
            catch (Exception)
            {
                Debug.LogError("Ignored exception " + undo.undoGroup.ToString() + " @ " + this);
            }
        }
    }
}
