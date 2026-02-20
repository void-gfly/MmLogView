using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Markdig;
using Westwind.WebView.HtmlToPdf;

namespace MmLogView.Core;

public static class Md2Pdf
{
    public static async Task ExportAsync(string markdownContent, string outputPdfPath)
    {
        // 1. Convert Markdown to HTML
        var pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();
        var htmlContent = Markdown.ToHtml(markdownContent, pipeline);

        // Wrap with GitHub-like styling
        var fullHtml = $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{
            font-family: ""Microsoft YaHei"", ""PingFang SC"", ""Segoe UI"", Arial, sans-serif;
            line-height: 1.6;
            color: #24292f;
            max-width: 860px;
            margin: 0 auto;
            padding: 30px;
        }}
        h1, h2, h3, h4, h5, h6 {{
            border-bottom: 1px solid #d0d7de;
            padding-bottom: 6px;
            margin-top: 24px;
        }}
        pre {{
            background-color: #f6f8fa;
            padding: 16px;
            border-radius: 6px;
            overflow: auto;
        }}
        code {{
            font-family: ui-monospace, ""SFMono-Regular"", Consolas, ""Liberation Mono"", monospace;
            background-color: rgba(175,184,193,0.2);
            padding: 0.2em 0.4em;
            border-radius: 4px;
            font-size: 90%;
        }}
        pre code {{ background: none; padding: 0; }}
        blockquote {{
            border-left: 4px solid #dfe2e5;
            color: #6a737d;
            padding-left: 1em;
            margin-left: 0;
        }}
        table {{ border-collapse: collapse; width: 100%; }}
        th, td {{ border: 1px solid #dfe2e5; padding: 6px 13px; }}
        th {{ background-color: #f6f8fa; }}
        img {{ max-width: 100%; }}
    </style>
</head>
<body>
{htmlContent}
</body>
</html>";

        // 2. Write HTML to a temp file (HtmlToPdfHost requires a file path, not a string)
        var tempHtmlPath = Path.Combine(Path.GetTempPath(), $"mmlogview_{Guid.NewGuid():N}.html");
        // WebView2 PrintToPdfAsync may hang with non-ASCII (Chinese) output paths.
        // Write to an ASCII temp path first, then move to the real destination.
        var tempPdfPath = Path.Combine(Path.GetTempPath(), $"mmlogview_{Guid.NewGuid():N}.pdf");
        try
        {
            await File.WriteAllTextAsync(tempHtmlPath, fullHtml, System.Text.Encoding.UTF8);

            // 3. Use Edge WebView2 (pre-installed on Win10/11) to render and export PDF.
            //    UseServerPdfGeneration = true bypasses the DevTools protocol (which requires
            //    an active desktop window) and uses the built-in WebView PDF API instead.
            await Task.Run(async () =>
            {
                HtmlToPdfDefaults.UseServerPdfGeneration = true;

                var host = new HtmlToPdfHost();
                var result = await host.PrintToPdfAsync(tempHtmlPath, tempPdfPath, new WebViewPrintSettings
                {
                    MarginTop = 0.5,
                    MarginBottom = 0.5,
                    MarginLeft = 0.4,
                    MarginRight = 0.4,
                    ScaleFactor = 1.0f
                });

                if (!result.IsSuccess)
                    throw new Exception(result.Message ?? "PDF generation failed.");
            });

            // Move from ASCII temp path to the user's desired (possibly non-ASCII) path
            if (File.Exists(outputPdfPath)) File.Delete(outputPdfPath);
            File.Move(tempPdfPath, outputPdfPath);
        }
        finally
        {
            if (File.Exists(tempHtmlPath)) File.Delete(tempHtmlPath);
            if (File.Exists(tempPdfPath)) File.Delete(tempPdfPath);
        }
    }
}
