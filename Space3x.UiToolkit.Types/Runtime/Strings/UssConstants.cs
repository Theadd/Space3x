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
        public const string UssHidden = CustomPrefix + "hidden";
        public const string UssDecorator = CustomPrefix + "decorator";
        public const string UssBlockDecorator = CustomPrefix + "block-decorator";
        public const string UssGhostDecorator = CustomPrefix + "ghost-decorator";
        public const string UssGroupMarker = CustomPrefix + "group-marker";
        public const string UssUsedGroupMarker = UssGroupMarker + "__used";
        public const string UssNonProportional = CustomPrefix + "non-proportional";
        
    }
}