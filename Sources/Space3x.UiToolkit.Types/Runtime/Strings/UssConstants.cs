namespace Space3x.UiToolkit.Types
{
    /// <summary>
    /// Common/known/reusable USS class names gathered in a single place.
    /// </summary>
    public static class UssConstants
    {
        public const string UnityPrefix = "unity-";
        public const string CustomPrefix = "ui3x-";
        public const string UssDisabled = UnityPrefix + "disabled";
        public const string UssLabel = UnityPrefix + "label";
        public const string UssHelpBox = UnityPrefix + "help-box";
        public const string UssButtonGroupButton = UnityPrefix + "button-group__button";
        public const string UssButtonGroupButtonRight = UssButtonGroupButton + "--right";
        public const string UssBaseField = UnityPrefix + "base-field";
        public const string UssDecoratorDrawersContainer = UnityPrefix + "decorator-drawers-container";
        public const string UssUnityAligned = UssBaseField + "__aligned";
        public const string UssAligned = CustomPrefix + "aligned";
        public const string UssAlignedAuto = UssAligned + "--auto";
        public const string UssInspector = UnityPrefix + "inspector-element";
        public const string UssInspectorContainer = UnityPrefix + "inspector-main-container";
        public const string UssHidden = CustomPrefix + "hidden";
        public const string UssDecorator = CustomPrefix + "decorator";
        public const string UssBlockDecorator = CustomPrefix + "block-decorator";
        public const string UssGhostDecorator = CustomPrefix + "ghost-decorator";
        public const string UssGroupMarker = CustomPrefix + "group-marker";
        public const string UssUsedGroupMarker = UssGroupMarker + "__used";
        public const string UssNonProportional = CustomPrefix + "non-proportional";
        public const string UssTextArea = CustomPrefix + "text-area";
        public const string UssAttributesExtended = CustomPrefix + "attributes-extended";
        public const string UssEditableLabel = CustomPrefix + "editable-label";
        public const string UssEditableRichText = CustomPrefix + "editable-rich-text";
        public const string UssEditMode = CustomPrefix + "edit-mode";
        public const string UssInvokableField = CustomPrefix + "invokable-field";
        public const string UssShowInInspector = CustomPrefix + "show-in-inspector";
        public const string UssTypePicker = CustomPrefix + "type-picker";
        public const string UssTypePickerInstanceContainer = UssTypePicker + "-instance-container";
        public const string UssFactoryPopulated = CustomPrefix + "factory-populated";
        public const string UssEditorUI = CustomPrefix + "editor-ui";
    }
}