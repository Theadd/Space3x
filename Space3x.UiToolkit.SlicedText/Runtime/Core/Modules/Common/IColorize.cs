using System.Text;

namespace Space3x.UiToolkit.SlicedText
{
    public interface IColorize
    {
        string Language { get; set; }
        string Format(string source);
        string Format(StringBuilder source);
        
        public static IColorize Default => new FakeColorizer();
        
        public static int MatchCharacterInFormattedText(string original, string formatted, int charPosWithinOriginal)
        {
            if (original.Length <= charPosWithinOriginal)
                return -1;

            var e = 0;
            var eLen = formatted.Length;
            
            for (var i = 0; i <= charPosWithinOriginal; i++)
            {
                var ci = original[i];
                var ce = formatted[e];
                
                if (ci == ce)
                {
                    if (ce == '<')
                    {
                        while (ce == '<')
                        {
                            if (formatted[e + 1] == '\u0003')
                            {
                                // if found a sanitized <tag> from original code,
                                // break inner loop and continue outer loop (SEE: 1)
                                e += 2;
                                break;
                            }

                            while (formatted[e] != '>' && e < eLen) e++;

                            if (e == eLen) return -1;

                            e++;
                            ce = formatted[e];
                        }

                        // (1)
                        if (ce == '<') continue;

                        return -1;
                    } 
                    else
                    {
                        if (ce == '\\' && formatted[e + 1] == '\u0003')
                            e++;
                        
                        e++;
                        continue;
                    }
                }
                
                if (ce == '<')
                {
                    while (formatted[e] != '>' && e < eLen) e++;

                    if (e == eLen) return -1;

                    e++;
                    i--;

                    continue;
                }
                
                return -1;
            }

            e--;

            return e;
        }
    }

    public class FakeColorizer : IColorize
    {
        public string Language { get; set; }
        
        public string Format(string source) => source.Replace("<", "<\u0003");

        public string Format(StringBuilder source) => source.Replace("<", "<\u0003").ToString();
    }
}
