using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using MmLogView.Core;
using MmLogView.Properties;

namespace MmLogView.Controls;

/// <summary>
/// 高性能日志视口控件。
/// 使用 DrawingVisual 直接绘制可见行，无 UI 元素虚拟化开销。
/// </summary>
public sealed class LogViewport : FrameworkElement
{
    private MappedLogFile? _logFile;
    private readonly ScrollBar _scrollBar;
    private readonly DrawingVisual _visual;
    private readonly MenuItem _copySelectedMenuItem;
    private readonly MenuItem _copyPageMenuItem;
    private readonly MenuItem _openLineInNotepadMenuItem;
    private readonly MenuItem _openPageInNotepadMenuItem;

    private double _lineHeight = 20;
    private long _firstVisibleLine;
    private string[] _visibleLines = [];
    private Typeface _typeface = new("Cascadia Mono, Consolas, Courier New");
    private double _fontSize = 14;
    private int _lineNumberWidth = 50;
    private long _selectedLine = -1;

    public long FirstVisibleLine => _firstVisibleLine;

    public LogViewport()
    {
        _visual = new DrawingVisual();
        AddVisualChild(_visual);

        _scrollBar = new ScrollBar
        {
            Orientation = Orientation.Vertical,
            SmallChange = 1,
            LargeChange = 10,
            Minimum = 0,
            Maximum = 0,
            Width = 14
        };
        _scrollBar.ValueChanged += OnScrollChanged;
        AddVisualChild(_scrollBar);
        AddLogicalChild(_scrollBar);

        var res = ResourcesExtension.Instance;
        _copySelectedMenuItem = new MenuItem { Header = res.MenuCopySelected };
        _copyPageMenuItem = new MenuItem { Header = res.MenuCopyPage };
        _openLineInNotepadMenuItem = new MenuItem { Header = res.MenuOpenLineNotepad };
        _openPageInNotepadMenuItem = new MenuItem { Header = res.MenuOpenPageNotepad };
        _copySelectedMenuItem.Click += (_, _) => CopySelectedLine();
        _copyPageMenuItem.Click += (_, _) => CopyVisiblePage();
        _openLineInNotepadMenuItem.Click += (_, _) => OpenSelectedLineInNotepad();
        _openPageInNotepadMenuItem.Click += (_, _) => OpenVisiblePageInNotepad();
        ContextMenu = new ContextMenu
        {
            Items =
            {
                _copySelectedMenuItem,
                _copyPageMenuItem,
                new Separator(),
                _openLineInNotepadMenuItem,
                _openPageInNotepadMenuItem
            }
        };
        ContextMenuOpening += OnContextMenuOpening;

        ClipToBounds = true;
        Focusable = true;
    }

    public void SetLogFile(MappedLogFile logFile)
    {
        _logFile = logFile;
        _firstVisibleLine = 0;
        _selectedLine = -1;
        UpdateScrollBar();
        Render();
    }

    public void RefreshContextMenuLanguage()
    {
        var res = ResourcesExtension.Instance;
        _copySelectedMenuItem.Header = res.MenuCopySelected;
        _copyPageMenuItem.Header = res.MenuCopyPage;
        _openLineInNotepadMenuItem.Header = res.MenuOpenLineNotepad;
        _openPageInNotepadMenuItem.Header = res.MenuOpenPageNotepad;
    }

    public void OnLineCountChanged()
    {
        UpdateScrollBar();
        Render();
    }

    public void ScrollToLine(long lineNumber)
    {
        if (_logFile is null) return;
        long max = Math.Max(0, _logFile.LineIndex.ScannedLines - VisibleLineCount);
        _firstVisibleLine = Math.Clamp(lineNumber, 0, max);
        _scrollBar.Value = _firstVisibleLine;
        Render();
    }

    private int VisibleLineCount => Math.Max(1, (int)(ActualHeight / _lineHeight) + 1);

    private void UpdateScrollBar()
    {
        if (_logFile is null) return;
        long totalLines = _logFile.LineIndex.ScannedLines;
        _scrollBar.Maximum = Math.Max(0, totalLines - VisibleLineCount + 1);
        _scrollBar.LargeChange = VisibleLineCount;
        _scrollBar.ViewportSize = VisibleLineCount;
    }

    private void OnScrollChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        _firstVisibleLine = (long)e.NewValue;
        Render();
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);
        int delta = e.Delta > 0 ? -3 : 3;
        long newLine = Math.Clamp(_firstVisibleLine + delta, 0, (long)_scrollBar.Maximum);
        _firstVisibleLine = newLine;
        _scrollBar.Value = newLine;
        Render();
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        long delta = e.Key switch
        {
            Key.Up => -1,
            Key.Down => 1,
            Key.PageUp => -VisibleLineCount,
            Key.PageDown => VisibleLineCount,
            Key.Home => -_firstVisibleLine,
            Key.End => (long)_scrollBar.Maximum - _firstVisibleLine,
            _ => 0
        };

        if (delta != 0)
        {
            long newLine = Math.Clamp(_firstVisibleLine + delta, 0, (long)_scrollBar.Maximum);
            _firstVisibleLine = newLine;
            _scrollBar.Value = newLine;
            Render();
            e.Handled = true;
        }
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        Focus();
        SelectLineAtPoint(e.GetPosition(this));
        e.Handled = true;
    }

    protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseRightButtonDown(e);
        Focus();
        SelectLineAtPoint(e.GetPosition(this));
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        UpdateScrollBar();

        // Position scrollbar
        _scrollBar.Arrange(new Rect(
            ActualWidth - _scrollBar.Width, 0,
            _scrollBar.Width, ActualHeight));

        Render();
    }

    private void Render()
    {
        if (_logFile is null || ActualWidth <= 0 || ActualHeight <= 0)
        {
            ClearVisual();
            return;
        }

        int visibleCount = VisibleLineCount;
        _visibleLines = _logFile.ReadLines(_firstVisibleLine, visibleCount);

        // Calculate line number gutter width
        long maxLineNum = _firstVisibleLine + _visibleLines.Length;
        _lineNumberWidth = (int)(MeasureText(maxLineNum.ToString()).Width + 24);

        using var dc = _visual.RenderOpen();

        var editorBg = FindBrush("EditorBackground", Brushes.White);
        var editorFg = FindBrush("EditorForeground", Brushes.Black);
        var gutterBg = FindBrush("GutterBackground", Brushes.LightGray);
        var gutterFg = FindBrush("GutterForeground", Brushes.Gray);
        var gutterLine = FindBrush("GutterLineBrush", Brushes.DarkGray);
        var selectionBg = FindBrush("SelectionBackground", Brushes.LightSkyBlue);
        var selectionFg = FindBrush("SelectionForeground", Brushes.White);

        double contentWidth = ActualWidth - _scrollBar.Width;

        // Background
        dc.DrawRectangle(editorBg, null, new Rect(0, 0, contentWidth, ActualHeight));

        // Gutter background
        dc.DrawRectangle(gutterBg, null, new Rect(0, 0, _lineNumberWidth, ActualHeight));

        // Gutter separator line
        dc.DrawLine(new Pen(gutterLine, 1), new Point(_lineNumberWidth, 0), new Point(_lineNumberWidth, ActualHeight));

        for (int i = 0; i < _visibleLines.Length; i++)
        {
            double y = i * _lineHeight;
            long lineNum = _firstVisibleLine + i + 1;

            bool isSelected = _selectedLine == lineNum - 1;
            if (isSelected)
            {
                dc.DrawRectangle(selectionBg, null, new Rect(_lineNumberWidth + 1, y, Math.Max(0, contentWidth - _lineNumberWidth - 1), _lineHeight));
            }

            // Line number
            var lineNumText = CreateFormattedText(lineNum.ToString(), gutterFg);
            double lineNumX = _lineNumberWidth - lineNumText.Width - 8;
            dc.DrawText(lineNumText, new Point(lineNumX, y + (_lineHeight - lineNumText.Height) / 2));

            // Line content
            string lineContent = _visibleLines[i];
            if (!string.IsNullOrEmpty(lineContent))
            {
                var contentText = CreateFormattedText(lineContent, isSelected ? selectionFg : editorFg);
                contentText.MaxTextWidth = Math.Max(1, contentWidth - _lineNumberWidth - 8);
                contentText.MaxTextHeight = _lineHeight;
                dc.DrawText(contentText, new Point(_lineNumberWidth + 8, y + (_lineHeight - contentText.Height) / 2));
            }
        }
    }

    private void ClearVisual()
    {
        using var dc = _visual.RenderOpen();
        // empty
    }

    private FormattedText CreateFormattedText(string text, Brush foreground)
    {
        return new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            _typeface,
            _fontSize,
            foreground,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);
    }

    private Size MeasureText(string text)
    {
        var ft = CreateFormattedText(text, Brushes.Black);
        return new Size(ft.Width, ft.Height);
    }

    private Brush FindBrush(string resourceKey, Brush fallback)
    {
        return TryFindResource(resourceKey) is SolidColorBrush brush ? brush : fallback;
    }

    private void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        bool hasSelected = TryGetSelectedLineContent(out _);
        bool hasPage = _visibleLines.Length > 0;
        _copySelectedMenuItem.IsEnabled = hasSelected;
        _copyPageMenuItem.IsEnabled = hasPage;
        _openLineInNotepadMenuItem.IsEnabled = hasSelected;
        _openPageInNotepadMenuItem.IsEnabled = hasPage;
    }

    private void SelectLineAtPoint(Point point)
    {
        if (TryGetLineAtPoint(point, out long line))
        {
            _selectedLine = line;
            Render();
        }
    }

    private bool TryGetLineAtPoint(Point point, out long line)
    {
        line = -1;

        if (_logFile is null)
        {
            return false;
        }

        if (point.X < 0 || point.X >= ActualWidth - _scrollBar.Width || point.Y < 0)
        {
            return false;
        }

        int index = (int)(point.Y / _lineHeight);
        if (index < 0 || index >= _visibleLines.Length)
        {
            return false;
        }

        line = _firstVisibleLine + index;
        return true;
    }

    private bool TryGetSelectedLineContent(out string content)
    {
        content = string.Empty;
        if (_selectedLine < _firstVisibleLine)
        {
            return false;
        }

        long index = _selectedLine - _firstVisibleLine;
        if (index < 0 || index >= _visibleLines.Length)
        {
            return false;
        }

        content = _visibleLines[index];
        return true;
    }

    private void CopySelectedLine()
    {
        if (TryGetSelectedLineContent(out string content))
        {
            Clipboard.SetText(content ?? string.Empty);
        }
    }

    private void CopyVisiblePage()
    {
        if (_visibleLines.Length == 0)
        {
            return;
        }

        Clipboard.SetText(string.Join(Environment.NewLine, _visibleLines));
    }

    private void OpenSelectedLineInNotepad()
    {
        if (!TryGetSelectedLineContent(out string content)) return;
        OpenInNotepad(content);
    }

    private void OpenVisiblePageInNotepad()
    {
        if (_visibleLines.Length == 0) return;
        OpenInNotepad(string.Join(Environment.NewLine, _visibleLines));
    }

    private static void OpenInNotepad(string text)
    {
        var tmpFile = Path.Combine(Path.GetTempPath(), $"MmLogView_{DateTime.Now:yyyyMMdd_HHmmss_fff}.txt");
        File.WriteAllText(tmpFile, text, System.Text.Encoding.UTF8);
        Process.Start(new ProcessStartInfo("notepad.exe", tmpFile) { UseShellExecute = true });
    }

    // VisualChildrenCount and GetVisualChild for the DrawingVisual + ScrollBar
    protected override int VisualChildrenCount => 2;

    protected override Visual GetVisualChild(int index) => index switch
    {
        0 => _visual,
        1 => _scrollBar,
        _ => throw new ArgumentOutOfRangeException(nameof(index))
    };

    protected override Size MeasureOverride(Size availableSize)
    {
        _scrollBar.Measure(new Size(_scrollBar.Width, availableSize.Height));
        return availableSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _scrollBar.Arrange(new Rect(
            finalSize.Width - _scrollBar.Width, 0,
            _scrollBar.Width, finalSize.Height));
        return finalSize;
    }
}
