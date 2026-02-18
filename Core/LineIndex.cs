namespace MmLogView.Core;

/// <summary>
/// 行索引：渐进式构建的行号 ↔ 偏移量映射。
/// 支持边扫描边使用。
/// </summary>
public sealed class LineIndex
{
    private readonly List<long> _offsets = [0]; // 第一行始终从偏移 0 开始
    private volatile bool _isComplete;

    /// <summary>已扫描的行数。</summary>
    public long ScannedLines => _offsets.Count;

    /// <summary>扫描是否已完成。</summary>
    public bool IsComplete => _isComplete;

    /// <summary>扫描进度变化回调 (0.0 ~ 1.0)。</summary>
    public event Action<double>? ProgressChanged;

    /// <summary>扫描完成回调。</summary>
    public event Action? ScanCompleted;

    /// <summary>添加一个行起始偏移量。</summary>
    public void AddOffset(long offset)
    {
        lock (_offsets)
        {
            _offsets.Add(offset);
        }
    }

    /// <summary>标记扫描已完成。</summary>
    public void MarkComplete()
    {
        _isComplete = true;
        ScanCompleted?.Invoke();
    }

    /// <summary>报告扫描进度。</summary>
    public void ReportProgress(double progress) => ProgressChanged?.Invoke(progress);

    /// <summary>获取指定行的起始偏移量 (0-indexed)。</summary>
    public long GetOffset(long lineNumber)
    {
        lock (_offsets)
        {
            if (lineNumber < 0 || lineNumber >= _offsets.Count)
                return -1;
            return _offsets[(int)lineNumber];
        }
    }

    /// <summary>获取指定行的长度（字节数）。如果是最后一行则返回到文件末尾的长度。</summary>
    public long GetLineLength(long lineNumber, long fileLength)
    {
        lock (_offsets)
        {
            if (lineNumber < 0 || lineNumber >= _offsets.Count)
                return 0;

            long start = _offsets[(int)lineNumber];
            long end = (lineNumber + 1 < _offsets.Count)
                ? _offsets[(int)(lineNumber + 1)]
                : fileLength;

            return end - start;
        }
    }
}
