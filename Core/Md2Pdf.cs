using System.Text.RegularExpressions;
using Markdig;

namespace MmLogView.Core;

public static partial class Md2Pdf
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    // Match <h1>...</h1> through <h6>...</h6> tags without an existing id attribute
    [GeneratedRegex(@"<(h[1-6])>(.*?)</\1>", RegexOptions.IgnoreCase)]
    private static partial Regex HeadingRegex();

    /// <summary>
    /// Generate GitHub-style slug from heading text:
    /// lowercase, replace spaces with hyphens, strip punctuation except CJK and hyphens.
    /// </summary>
    private static string GitHubSlug(string text)
    {
        // Strip HTML tags from heading text
        var plain = Regex.Replace(text, "<.*?>", "").Trim();
        // Lowercase
        plain = plain.ToLowerInvariant();
        // Replace spaces with hyphens
        plain = Regex.Replace(plain, @"\s+", "-");
        // Remove characters that are not: word chars (\w includes CJK), hyphens, Chinese/Japanese/Korean
        plain = Regex.Replace(plain, @"[^\w\u4e00-\u9fff\u3400-\u4dbf\u3000-\u303f\uff00-\uffef-]", "");
        return plain;
    }

    /// <summary>
    /// Post-process HTML to add GitHub-style id attributes to headings.
    /// </summary>
    private static string AddHeadingIds(string html)
    {
        return HeadingRegex().Replace(html, match =>
        {
            var tag = match.Groups[1].Value;
            var content = match.Groups[2].Value;
            var slug = GitHubSlug(content);
            return $"<{tag} id=\"{slug}\">{content}</{tag}>";
        });
    }

    public static string ConvertToHtml(string markdownContent, bool isDarkTheme = false)
    {
        var htmlContent = Markdown.ToHtml(markdownContent, Pipeline);
        // Add GitHub-style heading IDs for TOC anchor navigation
        htmlContent = AddHeadingIds(htmlContent);

        var bgColor = isDarkTheme ? "#1e1e1e" : "#ffffff";
        var textColor = isDarkTheme ? "#d4d4d4" : "#24292f";
        var borderColor = isDarkTheme ? "#444" : "#d0d7de";
        var codeBg = isDarkTheme ? "#2d2d2d" : "#f6f8fa";
        var quoteBorder = isDarkTheme ? "#555" : "#dfe2e5";
        var quoteColor = isDarkTheme ? "#aaa" : "#6a737d";
        var tableBorder = isDarkTheme ? "#555" : "#dfe2e5";
        var linkColor = isDarkTheme ? "#58a6ff" : "#0969da";

        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{
            font-family: ""Microsoft YaHei"", ""PingFang SC"", ""Segoe UI"", Arial, sans-serif;
            line-height: 1.6;
            color: {textColor};
            background-color: {bgColor};
            max-width: 860px;
            margin: 0 auto;
            padding: 30px;
        }}
        h1, h2, h3, h4, h5, h6 {{
            border-bottom: 1px solid {borderColor};
            padding-bottom: 6px;
            margin-top: 24px;
        }}
        pre {{
            background-color: {codeBg};
            padding: 16px;
            border-radius: 6px;
            overflow: auto;
        }}
        code {{
            font-family: ui-monospace, ""SFMono-Regular"", Consolas, ""Liberation Mono"", monospace;
            background-color: {codeBg};
            padding: 0.2em 0.4em;
            border-radius: 4px;
            font-size: 90%;
        }}
        pre code {{ background: none; padding: 0; }}
        blockquote {{
            border-left: 4px solid {quoteBorder};
            color: {quoteColor};
            padding-left: 1em;
            margin-left: 0;
        }}
        table {{ border-collapse: collapse; width: 100%; }}
        th, td {{ border: 1px solid {tableBorder}; padding: 6px 13px; }}
        th {{ background-color: {codeBg}; }}
        img {{ max-width: 100%; }}
        a {{ color: {linkColor}; cursor: pointer; }}
    </style>
</head>
<body>
{htmlContent}
<script>
// Intercept anchor clicks and scroll to target element
document.addEventListener('click', function(e) {{
    var a = e.target.closest('a');
    if (!a) return;
    var href = a.getAttribute('href');
    if (!href) return;
    var hashIdx = href.indexOf('#');
    if (hashIdx === -1) return;
    var id = decodeURIComponent(href.substring(hashIdx + 1));
    if (!id) return;
    var target = document.getElementById(id);
    if (target) {{
        e.preventDefault();
        e.stopPropagation();
        target.scrollIntoView({{ behavior: 'smooth', block: 'start' }});
    }}
}}, true);
</script>
</body>
</html>";
    }
}
