using Markdig;
var md = ""## 全局选项\n## Vault 命令\n## 第一步：配置文件准备"";
var pipeline = new Markdig.MarkdownPipelineBuilder().UseAdvancedExtensions().UseAutoIdentifiers().Build();
Console.WriteLine(Markdig.Markdown.ToHtml(md, pipeline));
