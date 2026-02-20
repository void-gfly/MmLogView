# MmLogView

[![.NET 10](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE.txt)

**MmLogView** 是一款专为开发者设计的高性能、轻量级日志查看工具。基于 .NET 10 WPF 开发，利用内存映射文件 (Memory-Mapped Files) 技术，实现对超大日志文件（GB 级别）的秒级加载与极速检索，同时支持 Markdown 文档的渲染预览。

## 🚀 核心特性

- **极致性能**：通过 `MemoryMappedFile` 技术直接映射文件到内存，无需将整个文件读入内存，即使是数 GB 的日志也能瞬时打开。
- **零开销渲染**：自定义 `LogViewport` 控件，使用 `DrawingVisual` 直接在视口绘制可见行，彻底解决传统 WPF 列表控件在处理数百万行数据时的卡顿问题。
- **智能索引**：后台异步扫描文件行偏移量，在不阻塞 UI 的前提下建立索引，支持快速跳转与定位。
- **强大的搜索**：支持全文前向与后向搜索，并能处理其他进程正在写入的日志文件 (FileShare.ReadWrite)。
- **Markdown 渲染**：内置 Markdown 文档渲染引擎（基于 MdXaml），打开 `.md` 文件时自动切换为渲染预览模式，完美适配深色/浅色主题。
- **PDF 导出**：支持将渲染预览的 Markdown 文本一键导出为高质量的 PDF 文件。
- **JSON 可视化**：载入 `.json` 文件时自动生成节点树状图，提供折叠/展开管理，并与原始文本实现节点级的双向同步高亮。
- **多语言与主题**：
  - 引入成熟的资源文件体系(resx)和强类型的多语言框架，支持中英文动态热切换。
  - 支持 **深色 (Dark)** 与 **浅色 (Light)** 主题切换。
- **便捷操作**：
  - 支持最近文件列表。
  - 右键菜单一键复制当前行/页，或在记事本中打开选中内容。
  - 快速跳转至指定行。

## 🛠️ 技术栈

- **框架**：.NET 10.0 (WPF)
- **核心组件**：
  - `MappedLogFile`: 处理大文件映射与编码检测。
  - `LogViewport`: 高性能绘图引擎。
  - `RecentFilesManager`: 状态持久化管理。
  - `MdXaml`: Markdown → FlowDocument 原生渲染。

## 📥 安装与运行

### 环境要求
- Windows 10/11
- [.NET 10 Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)

### 编译运行
1. 克隆仓库：
   ```bash
   git clone https://github.com/your-username/MmLogView.git
   ```
2. 使用 Visual Studio 2022 (latest) 或 .NET SDK 编译：
   ```bash
   dotnet build -c Release
   ```
3. 运行输出目录中的 `MmLogView.exe`。

## ⌨️ 快捷键

- `Ctrl + O`: 打开文件
- `Ctrl + F`: 搜索
- `Ctrl + G`: 跳转至行
- `F3`: 查找下一个
- `Shift + F3`: 查找上一个
- `Ctrl + T`: 切换主题

## 📄 开源协议

本项目采用 [MIT License](LICENSE.txt)。
