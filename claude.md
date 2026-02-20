# CLAUDE.md

本文件用于给 Claude/AI 编码代理提供本仓库的最小可执行约束。

## 项目概览
- 技术栈：`.NET 10` + `WPF`，单项目桌面应用。
- 解决方案：`MmLogView.sln`
- 核心入口：`App.xaml`、`MainWindow.xaml`、`MainWindow.xaml.cs`

## 目录约定
- `Core/`：日志读取、行索引、最近文件、Markdown 导出 PDF 等核心逻辑。
- `ViewModels/`：状态与交互行为（如 `MainViewModel`）。
- `Controls/`：自定义可视化控件（`LogViewport`、`JsonViewport`）。
- `Converters/`：数据转换器。
- `Themes/`：主题资源。
- `Properties/*.resx`：本地化资源。

## 常用命令
```bash
dotnet restore MmLogView.sln
dotnet build MmLogView.sln
dotnet run --project MmLogView.csproj
dotnet build MmLogView.csproj -c Release
```

## 编码规则
- 使用 4 空格缩进，遵循现有命名风格（类型/方法 `PascalCase`，私有字段 `_camelCase`）。
- 优先复用 `Core/` 现有逻辑，避免重复实现。
- 不改无关文件，保持最小改动面。
- 不写空 `catch`，不吞异常，不引入兜底 fallback 逻辑。

## 本地化规则（必须）
- 新增 UI 文案时，同时更新：
  - `Properties/Resources.resx`
  - `Properties/Resources.en-US.resx`
- 确保 `Resources.Designer.cs` 生成对应强类型属性。
- XAML 文案绑定使用现有 `ResourcesExtension` 模式。

## 验证要求
- 最低验证：`dotnet build` 成功。
- 若改动涉及功能：`dotnet run` 冒烟测试（log/txt、md、json 至少各验证一次相关路径）。

## 提交建议
- 提交信息简洁聚焦，建议 Conventional Commits：
  - `feat: ...`
  - `fix: ...`
  - `refactor: ...`
