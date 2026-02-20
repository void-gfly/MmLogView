using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using MdXaml;
using MmLogView.Controls;
using MmLogView.Core;
using MmLogView.Properties;

namespace MmLogView.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged, IDisposable
{
    private MappedLogFile? _logFile;
    private string _statusText = ResourcesExtension.Instance.ReadyStatus;
    private string _encodingText = "";
    private string _fileSizeText = "";
    private string _lineInfoText = "";
    private string _searchText = "";
    private string _searchResultInfo = "";
    private bool _isMarkdownMode;
    private string _markdownText = "";
    private string _currentFilePath = "";
    private bool _isJsonMode;
    private string _jsonText = "";
    private ObservableCollection<JsonNodeViewModel> _jsonRootNodes = [];
    private bool _isSearchVisible;
    private double _scanProgress;
    private bool _isScanningVisible;
    private bool _isDarkTheme = true;
    private int _selectedLanguageIndex;

    public LogViewport? ViewportControl { get; set; }
    public MarkdownScrollViewer? MarkdownViewer { get; set; }

    public ObservableCollection<string> RecentFiles => RecentFilesManager.Instance.Items;

    public ObservableCollection<string> LanguageItems { get; } = ["中文", "English"];

    public int SelectedLanguageIndex
    {
        get => _selectedLanguageIndex;
        set
        {
            if (SetField(ref _selectedLanguageIndex, value))
            {
                ResourcesExtension.Instance.CurrentCulture = value == 1 ? "en-US" : "zh-CN";
            }
        }
    }

    public string StatusText
    {
        get => _statusText;
        set => SetField(ref _statusText, value);
    }

    public string EncodingText
    {
        get => _encodingText;
        set => SetField(ref _encodingText, value);
    }

    public string FileSizeText
    {
        get => _fileSizeText;
        set => SetField(ref _fileSizeText, value);
    }

    public string LineInfoText
    {
        get => _lineInfoText;
        set => SetField(ref _lineInfoText, value);
    }

    public string SearchText
    {
        get => _searchText;
        set => SetField(ref _searchText, value);
    }

    public string SearchResultInfo
    {
        get => _searchResultInfo;
        set => SetField(ref _searchResultInfo, value);
    }

    public bool IsSearchVisible
    {
        get => _isSearchVisible;
        set => SetField(ref _isSearchVisible, value);
    }

    public bool IsMarkdownMode
    {
        get => _isMarkdownMode;
        set
        {
            if (SetField(ref _isMarkdownMode, value))
                OnPropertyChanged(nameof(IsLogMode));
        }
    }

    public bool IsJsonMode
    {
        get => _isJsonMode;
        set
        {
            if (SetField(ref _isJsonMode, value))
                OnPropertyChanged(nameof(IsLogMode));
        }
    }

    public bool IsLogMode => !IsMarkdownMode && !IsJsonMode;

    public string MarkdownText
    {
        get => _markdownText;
        set => SetField(ref _markdownText, value);
    }

    public string JsonText
    {
        get => _jsonText;
        set => SetField(ref _jsonText, value);
    }

    public ObservableCollection<JsonNodeViewModel> JsonRootNodes
    {
        get => _jsonRootNodes;
        set => SetField(ref _jsonRootNodes, value);
    }

    public double ScanProgress
    {
        get => _scanProgress;
        set => SetField(ref _scanProgress, value);
    }

    public bool IsScanningVisible
    {
        get => _isScanningVisible;
        set => SetField(ref _isScanningVisible, value);
    }

    public ICommand OpenFileCommand { get; }
    public ICommand OpenRecentFileCommand { get; }
    public ICommand GoToLineCommand { get; }
    public ICommand ToggleSearchCommand { get; }
    public ICommand SearchNextCommand { get; }
    public ICommand SearchPrevCommand { get; }
    public ICommand ToggleThemeCommand { get; }
    public ICommand ExportPdfCommand { get; }

    public MainViewModel()
    {
        OpenFileCommand = new RelayCommand(OnOpenFile);
        OpenRecentFileCommand = new RelayCommand<string>(OnOpenRecentFile);
        GoToLineCommand = new RelayCommand(OnGoToLine, () => _logFile is not null);
        ToggleSearchCommand = new RelayCommand(OnToggleSearch);
        SearchNextCommand = new RelayCommand(OnSearchNext, () => _logFile is not null && !string.IsNullOrEmpty(SearchText));
        SearchPrevCommand = new RelayCommand(OnSearchPrev, () => _logFile is not null && !string.IsNullOrEmpty(SearchText));
        ToggleThemeCommand = new RelayCommand(OnToggleTheme);
        ExportPdfCommand = new RelayCommand(OnExportPdf, () => IsMarkdownMode);

        ResourcesExtension.Instance.PropertyChanged += OnLanguageChanged;
    }

    private void OnLanguageChanged(object? sender, PropertyChangedEventArgs e)
    {
        // 刷新动态文本
        if (_logFile is null)
        {
            StatusText = ResourcesExtension.Instance.ReadyStatus;
        }

        if (_logFile is not null)
        {
            if (IsScanningVisible)
                LineInfoText = string.Format(ResourcesExtension.Instance.LineScanning, _logFile.LineIndex.ScannedLines);
            else
                LineInfoText = string.Format(ResourcesExtension.Instance.LineDone, _logFile.LineIndex.ScannedLines);
        }

        ViewportControl?.RefreshContextMenuLanguage();
    }

    public void OpenFile(string filePath)
    {
        _logFile?.Dispose();

        try
        {
            var ext = Path.GetExtension(filePath);
            if (string.Equals(ext, ".md", StringComparison.OrdinalIgnoreCase))
            {
                // Markdown 模式
                _currentFilePath = filePath;
                MarkdownText = File.ReadAllText(filePath);
                IsMarkdownMode = true;
                IsJsonMode = false;
                IsScanningVisible = false;

                var fileName = Path.GetFileName(filePath);
                StatusText = fileName;
                var fi = new FileInfo(filePath);
                FileSizeText = FormatFileSize(fi.Length);
                EncodingText = "UTF-8";
                LineInfoText = "";
            }
            else if (string.Equals(ext, ".json", StringComparison.OrdinalIgnoreCase))
            {
                // Json 模式
                var jsonString = File.ReadAllText(filePath);
                try
                {
                    var (formattedText, rootNode) = JsonTreeBuilder.Build(jsonString);
                    JsonText = formattedText;
                    JsonRootNodes = [rootNode];
                }
                catch
                {
                    JsonText = jsonString;
                    JsonRootNodes = [];
                }

                IsJsonMode = true;
                IsMarkdownMode = false;
                IsScanningVisible = false;

                var fileName = Path.GetFileName(filePath);
                StatusText = fileName;
                var fi = new FileInfo(filePath);
                FileSizeText = FormatFileSize(fi.Length);
                EncodingText = "UTF-8";
                LineInfoText = "";
            }
            else
            {
                // 日志模式
                IsMarkdownMode = false;
                IsJsonMode = false;
                MarkdownText = "";
                JsonText = "";

                _logFile = new MappedLogFile(filePath);
                _logFile.LineIndex.ProgressChanged += OnScanProgressChanged;
                _logFile.LineIndex.ScanCompleted += OnScanCompleted;

                var fileName = Path.GetFileName(filePath);
                StatusText = fileName;
                EncodingText = _logFile.DetectedEncoding.WebName.ToUpperInvariant();
                FileSizeText = FormatFileSize(_logFile.FileLength);
                IsScanningVisible = true;
                ScanProgress = 0;

                ViewportControl?.SetLogFile(_logFile);

                _logFile.StartIndexScan();
            }

            // 添加到最近文件历史
            RecentFilesManager.Instance.Add(filePath);
        }
        catch (Exception ex)
        {
            StatusText = string.Format(ResourcesExtension.Instance.OpenFailed, ex.Message);
        }
    }

    private void OnOpenRecentFile(string? filePath)
    {
        if (filePath is not null && File.Exists(filePath))
            OpenFile(filePath);
    }

    private void OnScanProgressChanged(double progress)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            ScanProgress = progress * 100;
            LineInfoText = string.Format(ResourcesExtension.Instance.LineScanning, _logFile!.LineIndex.ScannedLines);
        });
    }

    private void OnScanCompleted()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            IsScanningVisible = false;
            LineInfoText = string.Format(ResourcesExtension.Instance.LineDone, _logFile!.LineIndex.ScannedLines);
            ViewportControl?.OnLineCountChanged();
        });
    }

    private void OnOpenFile()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = ResourcesExtension.Instance.OpenDialogTitle,
            Filter = ResourcesExtension.Instance.OpenDialogFilter,
            FilterIndex = 1
        };

        if (dialog.ShowDialog() == true)
        {
            OpenFile(dialog.FileName);
        }
    }

    private void OnGoToLine()
    {
        var dialog = new GoToLineDialog(_logFile!.LineIndex.ScannedLines);
        dialog.Owner = Application.Current.MainWindow;
        if (dialog.ShowDialog() == true)
        {
            ViewportControl?.ScrollToLine(dialog.LineNumber - 1); // 0-indexed
        }
    }

    private void OnToggleSearch()
    {
        IsSearchVisible = !IsSearchVisible;
    }

    private void OnSearchNext()
    {
        if (_logFile is null || string.IsNullOrEmpty(SearchText)) return;

        long startLine = ViewportControl?.FirstVisibleLine + 1 ?? 0;
        var result = _logFile.SearchForward(SearchText, startLine);
        if (result >= 0)
        {
            ViewportControl?.ScrollToLine(result);
            SearchResultInfo = string.Format(ResourcesExtension.Instance.SearchFoundAt, result + 1);
        }
        else
        {
            SearchResultInfo = ResourcesExtension.Instance.SearchNotFound;
        }
    }

    private void OnSearchPrev()
    {
        if (_logFile is null || string.IsNullOrEmpty(SearchText)) return;

        long startLine = ViewportControl?.FirstVisibleLine - 1 ?? 0;
        var result = _logFile.SearchBackward(SearchText, startLine);
        if (result >= 0)
        {
            ViewportControl?.ScrollToLine(result);
            SearchResultInfo = string.Format(ResourcesExtension.Instance.SearchFoundAt, result + 1);
        }
        else
        {
            SearchResultInfo = ResourcesExtension.Instance.SearchNotFound;
        }
    }

    private async void OnExportPdf()
    {
        if (!IsMarkdownMode || string.IsNullOrEmpty(MarkdownText)) return;

        var defaultName = string.IsNullOrEmpty(_currentFilePath)
            ? "output"
            : Path.GetFileNameWithoutExtension(_currentFilePath);

        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = ResourcesExtension.Instance.CurrentCulture == "en-US" ? "Export PDF" : "导出PDF",
            Filter = "PDF (*.pdf)|*.pdf",
            FileName = defaultName
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                StatusText = ResourcesExtension.Instance.CurrentCulture == "en-US" ? "Exporting PDF..." : "正在导出PDF...";
                await Md2Pdf.ExportAsync(MarkdownText, dialog.FileName);
                StatusText = ResourcesExtension.Instance.CurrentCulture == "en-US" ? "Export PDF success" : "导出PDF成功";

                var prompt = ResourcesExtension.Instance.CurrentCulture == "en-US"
                    ? "Open the exported PDF?"
                    : "是否打开导出的PDF文件?";
                var title = ResourcesExtension.Instance.CurrentCulture == "en-US" ? "Export PDF" : "导出PDF";
                if (MessageBox.Show(prompt, title, MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(dialog.FileName) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                StatusText = ResourcesExtension.Instance.CurrentCulture == "en-US" ? $"Export PDF failed: {ex.Message}" : $"导出PDF失败: {ex.Message}";
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    private void OnToggleTheme()
    {
        _isDarkTheme = !_isDarkTheme;
        var themeUri = _isDarkTheme
            ? new Uri("Themes/DarkTheme.xaml", UriKind.Relative)
            : new Uri("Themes/LightTheme.xaml", UriKind.Relative);

        Application.Current.Resources.MergedDictionaries.Clear();
        Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = themeUri });

        if (MarkdownViewer is not null)
            MarkdownViewer.MarkdownStyle = _isDarkTheme ? MarkdownStyle.Sasabune : MarkdownStyle.GithubLike;

        ViewportControl?.InvalidateVisual();
    }

    private static string FormatFileSize(long bytes)
    {
        string[] suffixes = ["B", "KB", "MB", "GB", "TB"];
        int i = 0;
        double size = bytes;
        while (size >= 1024 && i < suffixes.Length - 1)
        {
            size /= 1024;
            i++;
        }
        return $"{size:F1} {suffixes[i]}";
    }

    public void Dispose()
    {
        _logFile?.Dispose();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(name);
        return true;
    }
}
