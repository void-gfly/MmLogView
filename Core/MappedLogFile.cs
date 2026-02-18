using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;

namespace MmLogView.Core;

/// <summary>
/// 内存映射日志文件。通过 mmap 打开大文件，按需读取行内容。
/// 支持读取被其他进程锁定的文件 (FileShare.ReadWrite)。
/// </summary>
public sealed class MappedLogFile : IDisposable
{
    private const long ScanChunkSize = 4 * 1024 * 1024; // 4MB per scan chunk
    private const long ReadChunkSize = 1 * 1024 * 1024; // 1MB max read chunk

    private readonly FileStream _fileStream;
    private readonly MemoryMappedFile _mmf;
    private readonly long _fileLength;
    private readonly Encoding _encoding;
    private readonly int _bomLength;
    private CancellationTokenSource? _scanCts;

    public LineIndex LineIndex { get; } = new();
    public long FileLength => _fileLength;
    public Encoding DetectedEncoding => _encoding;

    public MappedLogFile(string filePath)
    {
        _fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        _fileLength = _fileStream.Length;

        if (_fileLength == 0)
        {
            _encoding = Encoding.UTF8;
            _bomLength = 0;
            _mmf = MemoryMappedFile.CreateFromFile(_fileStream, null, 0, MemoryMappedFileAccess.Read, HandleInheritability.None, leaveOpen: true);
            LineIndex.MarkComplete();
            return;
        }

        _mmf = MemoryMappedFile.CreateFromFile(_fileStream, null, 0, MemoryMappedFileAccess.Read, HandleInheritability.None, leaveOpen: true);
        (_encoding, _bomLength) = DetectEncoding();
    }

    /// <summary>启动后台行索引扫描。</summary>
    public void StartIndexScan()
    {
        if (_fileLength == 0) return;
        _scanCts = new CancellationTokenSource();
        Task.Run(() => ScanLines(_scanCts.Token));
    }

    private void ScanLines(CancellationToken ct)
    {
        long startOffset = _bomLength;

        for (long offset = startOffset; offset < _fileLength && !ct.IsCancellationRequested;)
        {
            long chunkSize = Math.Min(ScanChunkSize, _fileLength - offset);
            using var accessor = _mmf.CreateViewAccessor(offset, chunkSize, MemoryMappedFileAccess.Read);

            byte[] buffer = new byte[chunkSize];
            accessor.ReadArray(0, buffer, 0, (int)chunkSize);

            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i] == (byte)'\n')
                {
                    long lineStart = offset + i + 1;
                    if (lineStart < _fileLength)
                    {
                        LineIndex.AddOffset(lineStart);
                    }
                }
            }

            offset += chunkSize;
            LineIndex.ReportProgress((double)offset / _fileLength);
        }

        LineIndex.MarkComplete();
    }

    /// <summary>读取指定范围的行 (0-indexed)。</summary>
    public string[] ReadLines(long startLine, int count)
    {
        long totalLines = LineIndex.ScannedLines;
        if (startLine < 0) startLine = 0;
        if (startLine >= totalLines) return [];

        int actualCount = (int)Math.Min(count, totalLines - startLine);
        var result = new string[actualCount];

        for (int i = 0; i < actualCount; i++)
        {
            result[i] = ReadSingleLine(startLine + i);
        }

        return result;
    }

    private string ReadSingleLine(long lineNumber)
    {
        long offset = LineIndex.GetOffset(lineNumber);
        if (offset < 0) return "";

        long length = LineIndex.GetLineLength(lineNumber, _fileLength);
        if (length <= 0) return "";

        // Cap read length to avoid excessive memory allocation
        length = Math.Min(length, ReadChunkSize);

        using var accessor = _mmf.CreateViewAccessor(offset, length, MemoryMappedFileAccess.Read);
        byte[] buffer = new byte[length];
        accessor.ReadArray(0, buffer, 0, (int)length);

        // Trim trailing \r\n or \n
        int end = (int)length;
        if (end > 0 && buffer[end - 1] == '\n') end--;
        if (end > 0 && buffer[end - 1] == '\r') end--;

        return _encoding.GetString(buffer, 0, end);
    }

    /// <summary>从指定行向前搜索文本。</summary>
    public long SearchForward(string text, long startLine)
    {
        long totalLines = LineIndex.ScannedLines;
        for (long i = startLine; i < totalLines; i++)
        {
            string line = ReadSingleLine(i);
            if (line.Contains(text, StringComparison.OrdinalIgnoreCase))
                return i;
        }
        // wrap around
        for (long i = 0; i < startLine && i < totalLines; i++)
        {
            string line = ReadSingleLine(i);
            if (line.Contains(text, StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return -1;
    }

    /// <summary>从指定行向后搜索文本。</summary>
    public long SearchBackward(string text, long startLine)
    {
        long totalLines = LineIndex.ScannedLines;
        if (startLine >= totalLines) startLine = totalLines - 1;

        for (long i = startLine; i >= 0; i--)
        {
            string line = ReadSingleLine(i);
            if (line.Contains(text, StringComparison.OrdinalIgnoreCase))
                return i;
        }
        // wrap around
        for (long i = totalLines - 1; i > startLine; i--)
        {
            string line = ReadSingleLine(i);
            if (line.Contains(text, StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return -1;
    }

    private (Encoding encoding, int bomLength) DetectEncoding()
    {
        using var accessor = _mmf.CreateViewAccessor(0, Math.Min(4, _fileLength), MemoryMappedFileAccess.Read);
        byte[] bom = new byte[Math.Min(4, _fileLength)];
        accessor.ReadArray(0, bom, 0, bom.Length);

        if (bom.Length >= 3 && bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF)
            return (Encoding.UTF8, 3);
        if (bom.Length >= 2 && bom[0] == 0xFF && bom[1] == 0xFE)
            return (Encoding.Unicode, 2); // UTF-16 LE
        if (bom.Length >= 2 && bom[0] == 0xFE && bom[1] == 0xFF)
            return (Encoding.BigEndianUnicode, 2); // UTF-16 BE

        return (Encoding.UTF8, 0); // default to UTF-8 without BOM
    }

    public void Dispose()
    {
        _scanCts?.Cancel();
        _scanCts?.Dispose();
        _mmf.Dispose();
        _fileStream.Dispose();
    }
}
