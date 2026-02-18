using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using MmLogView.Controls;
using MmLogView.Core;
using MmLogView.Localization;

namespace MmLogView.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged, IDisposable
{
    private MappedLogFile? _logFile;
    private string _statusText = LanguageManager.Current.ReadyStatus;
    private string _encodingText = "";
    private string _fileSizeText = "";
    private string _lineInfoText = "";
    private string _searchText = "";
    private string _searchResultInfo = "";
    private bool _isSearchVisible;
    private double _scanProgress;
    private bool _isScanningVisible;
    private bool _isDarkTheme = true;
    private int _selectedLanguageIndex;

    private readonly LanguageManager _lang = LanguageManager.Current;

    public LogViewport? ViewportControl { get; set; }

    public LanguageManager Lang => _lang;

    public ObservableCollection<string> RecentFiles => RecentFilesManager.Instance.Items;

    public ObservableCollection<string> LanguageItems { get; } = ["中文", "English"];

    public int SelectedLanguageIndex
    {
        get => _selectedLanguageIndex;
        set
        {
            if (SetField(ref _selectedLanguageIndex, value))
            {
                _lang.IsEnglish = value == 1;
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

    public MainViewModel()
    {
        OpenFileCommand = new RelayCommand(OnOpenFile);
        OpenRecentFileCommand = new RelayCommand<string>(OnOpenRecentFile);
        GoToLineCommand = new RelayCommand(OnGoToLine, () => _logFile is not null);
        ToggleSearchCommand = new RelayCommand(OnToggleSearch);
        SearchNextCommand = new RelayCommand(OnSearchNext, () => _logFile is not null && !string.IsNullOrEmpty(SearchText));
        SearchPrevCommand = new RelayCommand(OnSearchPrev, () => _logFile is not null && !string.IsNullOrEmpty(SearchText));
        ToggleThemeCommand = new RelayCommand(OnToggleTheme);

        _lang.PropertyChanged += OnLanguageChanged;
    }

    private void OnLanguageChanged(object? sender, PropertyChangedEventArgs e)
    {
        // 通知 XAML 刷新所有通过 Lang.xxx 绑定的静态文本
        OnPropertyChanged(nameof(Lang));

        // 刷新动态文本
        if (_logFile is null)
        {
            StatusText = _lang.ReadyStatus;
        }

        if (_logFile is not null)
        {
            if (IsScanningVisible)
                LineInfoText = _lang.LineScanning(_logFile.LineIndex.ScannedLines);
            else
                LineInfoText = _lang.LineDone(_logFile.LineIndex.ScannedLines);
        }

        ViewportControl?.RefreshContextMenuLanguage();
    }

    public void OpenFile(string filePath)
    {
        _logFile?.Dispose();

        try
        {
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

            // 添加到最近文件历史
            RecentFilesManager.Instance.Add(filePath);
        }
        catch (Exception ex)
        {
            StatusText = _lang.OpenFailed(ex.Message);
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
            LineInfoText = _lang.LineScanning(_logFile!.LineIndex.ScannedLines);
        });
    }

    private void OnScanCompleted()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            IsScanningVisible = false;
            LineInfoText = _lang.LineDone(_logFile!.LineIndex.ScannedLines);
            ViewportControl?.OnLineCountChanged();
        });
    }

    private void OnOpenFile()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = _lang.OpenDialogTitle,
            Filter = _lang.OpenDialogFilter,
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
            SearchResultInfo = _lang.SearchFoundAt(result + 1);
        }
        else
        {
            SearchResultInfo = _lang.SearchNotFound;
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
            SearchResultInfo = _lang.SearchFoundAt(result + 1);
        }
        else
        {
            SearchResultInfo = _lang.SearchNotFound;
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
