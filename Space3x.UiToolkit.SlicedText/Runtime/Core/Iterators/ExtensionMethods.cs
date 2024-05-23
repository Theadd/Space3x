using System.Text;

namespace Space3x.UiToolkit.SlicedText.Iterators
{
    /// <summary>
    /// Extensions for StringBuilder
    /// </summary>
    public static class StringBuilderExtensions
    {
        /// <summary>
        /// Appends the specified slice to this <see cref="StringBuilder"/> instance.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="slice">The slice.</param>
        public static StringBuilder Append(this StringBuilder builder, StringSlice slice) => builder.Append(slice.Text, slice.Start, slice.Length);

        internal static string GetStringAndReset(this StringBuilder builder)
        {
            string text = builder.ToString();
            builder.Length = 0;
            return text;
        }
    }
    
    public static class CharExtensions
    {
        public static bool IsWhitespace(this char c)
        {
            // 2.1 Characters and lines 
            // A whitespace character is a space(U + 0020), tab(U + 0009), newline(U + 000A), line tabulation (U + 000B), form feed (U + 000C), or carriage return (U + 000D).
            return c <= ' ' && (c == ' ' || c == '\t' || c == '\n' || c == '\v' || c == '\f' || c == '\r');
        }
        
        public static char EscapeInsecure(this char c)
        {
            // 2.3 Insecure characters
            // For security reasons, the Unicode character U+0000 must be replaced with the REPLACEMENT CHARACTER (U+FFFD).
            return c == '\0' ? '\ufffd' : c;
        }
        
        public static bool IsControl(this char c)
        {
            return c < ' ' || char.IsControl(c);
        }
    }
}
