using System.Collections.Generic;
using Space3x.Documentation.Settings;

namespace Space3x.Documentation.Templates
{
    public partial class MarkdownPageTemplates
    {
        public string PageIndex(MdDocsSettings settings)
        {
            return $@"<!DOCTYPE html>
<html lang=""en"">

<head>
    <meta charset=""UTF-8"">
    <title>{settings.PageTitle}</title>
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge,chrome=1"" />
    <meta name=""description"" content=""{settings.PageDescription}"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0, shrink-to-fit=no"">
    {ExternalStyles(settings)}
    <link rel=""stylesheet"" href=""./resources/common.css"">

    {EmbeddedStyles(settings)}
</head>

<body>
    <div id=""app""></div>
    <script>
    {DocsifyConfiguration(settings)}
    </script>
    <script src=""./all-links.js""></script>
    <!-- Docsify v4 -->
    <script src=""https://cdn.jsdelivr.net/npm/docsify@4/lib/docsify.min.js""></script>
    {AdditionalScripts(settings)}
</body>

</html>";
        }
        
        private string EmbeddedStyles(MdDocsSettings settings)
        {
            switch (settings.PageTheme)
            {
                case MdPageThemes.Vue:
                    return @"<style>
    .markdown-section pre::after, .markdown-section output::after {
        content: """";
    }
    td > pre:first-child {
        margin: 0.5em 0;
    }
    td > pre:first-child > code {
        padding: 1em 5px;
    }
    span.token.class-name {
        color: dodgerblue;
    }
    .markdown-section table {
        display: table;
    }
    .markdown-section pre {
        border: 1px solid #dfdfdf;
    }
    .markdown-section kbd > code {
        background-color: transparent;
    }
    td > h3:first-child {
        height: 0;
        width: 0;
        position: absolute;
        opacity: 0;
    }
    td > h3:first-child > a {
        pointer-events: none;
    }
    table tr > td:first-child {
        max-width: 38vw;
    }
    .sidebar ul.app-sub-sidebar > li {
        margin: 0;
    }
    .sidebar-nav small {
        font-size: 0.7em;
        padding-left: 20px;
        font-weight: bolder;
        letter-spacing: 0.2em;
        opacity: 0.5;
        width: 100%;
        border-bottom: 1px solid #CCC;
        display: block;
        line-height: 1em;
        margin-top: 20px;
    }
    .sidebar-nav strong {
        display: block;
        background-color: #efefef;
        position: relative;
        margin-left: -15px;
        padding-left: 15px;
    }
    .search .input-wrap {
        border: 1px solid #e1e1e1;
        background-color: #f7f7f7;
    }
    .search .input-wrap > input {
        background-color: transparent;
    }
    td > details[open] {
        position: relative;
        margin-left: -13px;
        padding-left: 13px;
        background-color: rgba(0, 0, 0, 0.01);
        margin-right: -13px;
        border-top: 1px solid rgba(0, 0, 0, 0.08);
        border-bottom: 1px solid rgba(0, 0, 0, 0.07);
        padding-top: 10px;
        margin-top: -5px;
        margin-bottom: -1px;
    }
    td > details {
        margin-bottom: 10px;
        margin-top: -5px;
        padding-top: 15px;
    }
    .markdown-section td {
        padding-bottom: 0;
    }
    .markdown-section td p {
        margin: 1em 0 0.5em;
    }
    .markdown-section td > h3 + p {
        margin-top: 0;
    }
    .markdown-section td > h3 + p > img {
        margin-top: 13px;
        margin-bottom: -5px;
    }
    </style>";
                default:
                    return "";
            }
        }
        
        private string ExternalStyles(MdDocsSettings settings)
        {
            switch (settings.PageTheme)
            {
                case MdPageThemes.Vue:
                    return @"<link rel=""stylesheet"" href=""https://unpkg.com/docsify/themes/vue.css"">
    <!-- Available PrismJS themes: https://cdn.jsdelivr.net/npm/prismjs/themes/ -->
    <!-- coy, dark, funky, okaidia, solarizedlight, romorrow, twilight -->
    <link rel=""stylesheet"" href=""https://cdn.jsdelivr.net/npm/prismjs@1.25.0/themes/prism-coy.css"">";
                case MdPageThemes.Basic:
                    return "<link rel=\"stylesheet\" href=\"https://unpkg.com/docsify-themeable/dist/css/theme-defaults.css\">";
                case MdPageThemes.SimpleLight:
                    return "<link rel=\"stylesheet\" href=\"https://cdn.jsdelivr.net/npm/docsify-themeable@0/dist/css/theme-simple.css\">";
                case MdPageThemes.SimpleDark:
                    return "<link rel=\"stylesheet\" href=\"https://cdn.jsdelivr.net/npm/docsify-themeable@0/dist/css/theme-simple-dark.css\">";
                case MdPageThemes.SimpleAuto:
                default:
                    return @"<link rel=""stylesheet"" media=""(prefers-color-scheme: dark)"" href=""https://cdn.jsdelivr.net/npm/docsify-themeable@0/dist/css/theme-simple-dark.css"">
   <link rel=""stylesheet"" media=""(prefers-color-scheme: light)"" href=""https://cdn.jsdelivr.net/npm/docsify-themeable@0/dist/css/theme-simple.css"">";
            }
        }

        private string AdditionalScripts(MdDocsSettings settings)
        {
            var scriptTags = new List<string>();
            switch (settings.PageTheme)
            {
                case MdPageThemes.Vue:
                    break;
                case MdPageThemes.Basic:
                case MdPageThemes.SimpleLight:
                case MdPageThemes.SimpleDark:
                case MdPageThemes.SimpleAuto:
                default:
                    scriptTags.Add("<script src=\"https://cdn.jsdelivr.net/npm/docsify-themeable@0/dist/js/docsify-themeable.min.js\"></script>");
                    break;
            }
            
            // TODO: https://github.com/docsifyjs/docsify/blob/develop/docs/plugins.md#gitalk
            // scriptTags.Add("<script src=\"https://cdn.jsdelivr.net/npm/docsify/lib/plugins/search.min.js\"></script>");
            if (settings.AddSearchPlugin)
                scriptTags.Add("<script src=\"https://cdn.jsdelivr.net/npm/docsify@4/lib/plugins/search.min.js\"></script>");
            if (settings.AddCopyCodeToClipboardPlugin)
                scriptTags.Add("<script src=\"//cdn.jsdelivr.net/npm/docsify-copy-code/dist/docsify-copy-code.min.js\"></script>");
            scriptTags.Add("<script src=\"https://unpkg.com/prismjs/components/prism-bash.min.js\"></script>");
            scriptTags.Add("<script src=\"https://unpkg.com/prismjs/components/prism-csharp.min.js\"></script>");
            scriptTags.Add("<script src=\"https://unpkg.com/@rakutentech/docsify-code-inline/dist/index.min.js\"></script>");
            scriptTags.Add("<script src=\"js/common.js\"></script>");
            scriptTags.Add("<script src=\"js/docsify.plugins.space3x.js\"></script>");
                 
            return string.Join("\n    ", scriptTags);
        }

        private string DocsifyConfiguration(MdDocsSettings settings)
        {
            return $@"window.$docsify = {{
        name: '{settings.PageTitle}',
        repo: '{settings.RepoUrl}',
        coverpage: {(settings.AddCoverPage ? "\"coverpage.md\"" : "false")},
        // homepage: ""README.md"",
        homepage: 'https://raw.githubusercontent.com/Theadd/NavigateBack/main/README.md',
        loadSidebar: ""sidebar.md"",
        loadNavbar: ""navbar.md"",
        alias: {{
            '/api/(.*)': '/./g/$1'
        }},
        routes: {{
            '/bin/(.*)'(route, matched) {{
                return '# Binaries';
            }},
        }},
        // onlyCover: true,
        // loadSidebar: true,
        {(settings.AddSearchPlugin ? "search: 'auto'," : "")}
        auto2top: true,
        autoHeader: true,
        // maxLevel: 6,
        // subMaxLevel: 6,
        maxLevel: 3,
        subMaxLevel: 3,
        {DocsifyThemeConfiguration(settings)}
        // topMargin: 47,
        markdown: {{
            smartypants: false,
            renderer: {{
                // link: renderer.link
            }},
            sanitize: false,
            sanitizer: str => str,
            silent: false,
            gfm: true
        }},
        plugins: [
            {DocsifyPluginsConfiguration(settings)}
        ]
    }}";
        }

        private string DocsifyPluginsConfiguration(MdDocsSettings settings)
        {
            return "";
            return @"function(hook, vm) {
                hook.mounted(function() {
                // Called after initial completion. Only trigger once, no arguments.
                const linkRef = window.$docsify.markdown.renderer.link;

                const renderer = {
                    link: (href, title, text) => {
                        var targetMember = '';
                        if (href.length > 0 && href[0] == '#') {
                            targetMember = href.substring(1).split('#')[0];
                            switch ((targetMember.split('.')[0]).toLowerCase()) {
                                case 'system':
                                    href = 'https://learn.microsoft.com/en-us/dotnet/api/' + targetMember.toLowerCase() + '?view=net-8.0';
                                    break;
                                case 'unityeditor':
                                case 'unityengine':
                                    href = 'https://docs.unity3d.com/ScriptReference/' + targetMember.substr(targetMember.indexOf('.') + 1) + '.html';
                                    break;
                                case 'unity':
                                    href = 'https://docs.unity3d.com/ScriptReference/' + targetMember + '.html';
                                    break;
                                default:
                                    href = window.$allLinks.get(targetMember.split('(')[0]) ?? ('#' + targetMember);
                                    break;
                            }
                        } else {
                            if (href.length > 0 && href.indexOf('//') < 0 && href[0] != '.' && href[0] != '/')
                                href = location.hash.substr(2, Math.max(location.hash.lastIndexOf('/') - 1, 0)) + href.split('(')[0];
                        }
                        var r = linkRef(href, title, text);
                        console.log('link', { href, title, text, r, targetMember });
                        return r;
                    }
                };

                window.marked.use({ renderer });
                });
            }";
        }

        private string DocsifyThemeConfiguration(MdDocsSettings settings)
        {
            switch (settings.PageTheme)
            {
                case MdPageThemes.Vue:
                    return "";
                case MdPageThemes.Basic:
                case MdPageThemes.SimpleLight:
                case MdPageThemes.SimpleDark:
                case MdPageThemes.SimpleAuto:
                default:
                    return @"themeable: {
            readyTransition: true,
            responsiveTables: true
        },";
            }
        }
    }
}
