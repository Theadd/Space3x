namespace Space3x.UiToolkit.Types
{
    public readonly struct StyleTag
    {
        public readonly string Start;
        public readonly string End;

        public StyleTag(string start, string end) { Start = start; End = end; }
        
        public string Wrap(string value) => string.IsNullOrEmpty(value) ? string.Empty : Start + value + End;
        
        public string Wrap(StyleTag other, string value) => string.IsNullOrEmpty(value) ? string.Empty : Start + other.Wrap(value) + End;
        
        public static readonly StyleTag Primary = new StyleTag(start: "<color=#5594CC>", end: "</color>");      // Blue
        public static readonly StyleTag Secondary = new StyleTag(start: "<color=#ad8af9>", end: "</color>");    // Purple
        public static readonly StyleTag Highlight = new StyleTag(start: "<color=#e08e2a>", end: "</color>");    // Orange
        public static readonly StyleTag Alternative = new StyleTag(start: "<color=#37c596>", end: "</color>");  // Green
        public static readonly StyleTag Grey = new StyleTag(start: "<color=#777674>", end: "</color>");         // Grey
        public static readonly StyleTag Light = new StyleTag(start: "<color=#DCb862>", end: "</color>");        // Yellow
        public static readonly StyleTag NoStyle = new StyleTag(start: "", end: "");                             // White
        public static readonly StyleTag Alpha100 = new StyleTag(start: "<alpha=#FF>", end: "");
        public static readonly StyleTag Alpha80 = new StyleTag(start: "<alpha=#CC>", end: "<alpha=#FF>");
        public static readonly StyleTag Alpha50 = new StyleTag(start: "<alpha=#80>", end: "<alpha=#FF>");
        public static readonly StyleTag Alpha25 = new StyleTag(start: "<alpha=#40>", end: "<alpha=#FF>");
        public static readonly StyleTag Bold = new StyleTag(start: "<b>", end: "</b>");
        public static readonly StyleTag Uppercase = new StyleTag(start: "<allcaps>", end: "</allcaps>");
        public static readonly StyleTag Lowercase = new StyleTag(start: "<lowercase>", end: "</lowercase>");
        public static readonly StyleTag NoWrap = new StyleTag(start: "<nobr>", end: "</nobr>");
        public static readonly StyleTag Underline = new StyleTag(start: "<u>", end: "</u>");
        public static readonly StyleTag Strikethrough = new StyleTag(start: "<s>", end: "</s>");
    }
}
