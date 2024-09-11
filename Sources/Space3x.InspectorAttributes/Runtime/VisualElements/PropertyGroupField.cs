using Space3x.Attributes.Types;
using Space3x.UiToolkit.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace Space3x.InspectorAttributes
{
    [UxmlElement]
    [HideInInspector]
    public partial class PropertyGroupField : BaseField<bool>, ILayoutElement
    {
        public static readonly string UssClassName = "ui3x-property-group-field";
        public static readonly string UssWithLabelClassName = UssClassName + "--with-label";
        public static readonly string UssWithNoLabelClassName = UssClassName + "--with-no-label";
        
        [UxmlAttribute]
        public string GroupName { get; set; } = string.Empty;
        
        public GroupType Type { get; set; }

        public bool IsUsed { get; set; } = false;
        
        [UxmlAttribute]
        public string Text
        {
            get => this.label;
            set
            {
                this.label = value;
                Update();
            }
        }

        private VisualElement m_Container;
        private Label m_Label;
        private bool m_IsAttached;
        
        private PropertyGroupField(string label, VisualElement visualInput) : base(label, visualInput)
        {
            m_Label = labelElement;
            m_Label.WithClasses("unity-text-element", "unity-label", "unity-base-field__label", "unity-property-field__label");
            m_Container = visualInput;
            this.WithClasses("ui3x-property-group-field")
                .WithClasses(!string.IsNullOrEmpty(Text), BaseField<bool>.alignedFieldUssClassName);
            RegisterCallbackOnce<AttachToPanelEvent>(OnAttachToPanel);
        }
        
        public PropertyGroupField(string label) : this(label, new PropertyGroup()) { }

        public PropertyGroupField() : this((string) null) { }

        private void OnAttachToPanel(AttachToPanelEvent ev) => Update(true);

        private void Update(bool setAsAttached = false)
        {
            if (setAsAttached)
                m_IsAttached = true;
            // m_Label.style.display = string.IsNullOrEmpty(Text) ? DisplayStyle.None : DisplayStyle.Flex;
            this.WithClasses(!string.IsNullOrEmpty(Text), BaseField<bool>.alignedFieldUssClassName, UssWithLabelClassName)
                .WithClasses(string.IsNullOrEmpty(Text), UssWithNoLabelClassName)
                .WithClasses(false, $"ui3x-group-type--{GroupType.None.ToString().ToLower()}");
            if (m_IsAttached)
                this.WithClasses($"ui3x-group-type--{Type.ToString().ToLower()}");
        }
        
        public override VisualElement contentContainer => m_Container ?? this;
        
        public bool GroupContains(VisualElement element) => element.hierarchy.parent == contentContainer;
    }
}
