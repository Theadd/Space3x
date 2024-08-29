using Space3x.Documentation.Settings;

namespace Space3x.Documentation.Templates
{
    public partial class MarkdownPageTemplates
    {

        public string AllLinks(MdDocsSettings settings)
        {
            return "window.$allLinks = " + MdLinkParser.Stringify() + ";\n";
        }
    }
}
