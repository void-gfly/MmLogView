using System.ComponentModel;

namespace MmLogView.Localization;

public sealed class LanguageManager : INotifyPropertyChanged
{
    public static LanguageManager Current { get; } = new();

    private bool _isEnglish;

    public bool IsEnglish
    {
        get => _isEnglish;
        set
        {
            if (_isEnglish == value) return;
            _isEnglish = value;
            // 通知所有属性变更（空字符串表示所有属性）
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        }
    }

    // ── Toolbar ──
    public string BtnOpen => _isEnglish ? "📂 Open" : "📂 打开";
    public string BtnSearch => _isEnglish ? "🔍 Search" : "🔍 搜索";
    public string BtnGoTo => _isEnglish ? "↕ Go To" : "↕ 跳行";
    public string ThemeTooltip => _isEnglish ? "Toggle Dark/Light Theme" : "切换深色/浅色主题";
    public string FeatureText => _isEnglish ? "Supports opening GB-sized log files" : "支持打开G级超大log文件,支持MarkDown渲染";

    // ── Status Bar ──
    public string ReadyStatus => _isEnglish
        ? "Ready — Drop file or Ctrl+O to open"
        : "就绪 — 拖拽文件或 Ctrl+O 打开";

    public string OpenFailed(string msg) => _isEnglish
        ? $"Open failed: {msg}"
        : $"打开失败: {msg}";

    public string LineScanning(long count) => _isEnglish
        ? $"Lines: {count:N0} (scanning...)"
        : $"行: {count:N0} (扫描中...)";

    public string LineDone(long count) => _isEnglish
        ? $"Lines: {count:N0}"
        : $"行: {count:N0}";

    public string SearchFoundAt(long line) => _isEnglish
        ? $"Line {line:N0}"
        : $"行 {line:N0}";

    public string SearchNotFound => _isEnglish ? "Not found" : "未找到";

    // ── Open File Dialog ──
    public string OpenDialogTitle => _isEnglish ? "Open File" : "打开文件";
    public string OpenDialogFilter => _isEnglish
        ? "Log files (*.log;*.txt)|*.log;*.txt|JSON (*.json)|*.json|Markdown (*.md)|*.md|All files (*.*)|*.*"
        : "日志文件 (*.log;*.txt)|*.log;*.txt|JSON (*.json)|*.json|Markdown (*.md)|*.md|所有文件 (*.*)|*.*";

    // ── GoToLine Dialog ──
    public string GoToLineTitle => _isEnglish ? "Go to Line" : "跳转到行";
    public string GoToLineLabel(long max) => _isEnglish
        ? $"Enter line number (1 - {max:N0}):"
        : $"输入行号 (1 - {max:N0}):";
    public string BtnOk => _isEnglish ? "OK" : "确定";
    public string BtnCancel => _isEnglish ? "Cancel" : "取消";
    public string InvalidLineInput(long max) => _isEnglish
        ? $"Please enter a line number between 1 and {max:N0}."
        : $"请输入 1 到 {max:N0} 之间的行号。";
    public string InvalidInputTitle => _isEnglish ? "Invalid Input" : "无效输入";

    // ── Context Menu ──
    public string MenuCopySelected => _isEnglish ? "Copy Selected" : "复制选中";
    public string MenuCopyPage => _isEnglish ? "Copy Page" : "复制整页";
    public string MenuOpenLineNotepad => _isEnglish
        ? "Copy Line & Open in Notepad"
        : "复制当前行用notepad打开";
    public string MenuOpenPageNotepad => _isEnglish
        ? "Copy Page & Open in Notepad"
        : "复制当前页用notepad打开";

    // ── Json Context Menu ──
    public string MenuCopyNode => _isEnglish ? "Copy Node" : "复制本节点";
    public string MenuCopyNodeAndChildren => _isEnglish ? "Copy Node & Children" : "复制本节点(连所有子节点)";

    public event PropertyChangedEventHandler? PropertyChanged;
}
