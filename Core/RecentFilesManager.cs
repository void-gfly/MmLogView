using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace MmLogView.Core;

/// <summary>
/// 管理最近打开的文件历史，最多保存 10 条，持久化到本地 JSON 文件。
/// </summary>
public sealed class RecentFilesManager
{
    private const int MaxCount = 10;
    private static readonly string ConfigDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MmLogView");
    private static readonly string FilePath = Path.Combine(ConfigDir, "recent_files.json");

    public static RecentFilesManager Instance { get; } = new();

    public ObservableCollection<string> Items { get; } = [];

    private RecentFilesManager()
    {
        Load();
    }

    /// <summary>
    /// 将文件路径添加（或提升）到最近列表顶部，并持久化。
    /// </summary>
    public void Add(string filePath)
    {
        // 移除已有的重复项
        for (int i = Items.Count - 1; i >= 0; i--)
        {
            if (string.Equals(Items[i], filePath, StringComparison.OrdinalIgnoreCase))
                Items.RemoveAt(i);
        }

        // 插入到顶部
        Items.Insert(0, filePath);

        // 超出上限则移除末尾
        while (Items.Count > MaxCount)
            Items.RemoveAt(Items.Count - 1);

        Save();
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return;
            var json = File.ReadAllText(FilePath);
            var list = JsonSerializer.Deserialize<List<string>>(json);
            if (list is null) return;
            foreach (var item in list.Take(MaxCount))
                Items.Add(item);
        }
        catch
        {
            // 文件损坏时静默忽略
        }
    }

    private void Save()
    {
        try
        {
            Directory.CreateDirectory(ConfigDir);
            var json = JsonSerializer.Serialize(Items.ToList(),
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }
        catch
        {
            // 写失败时静默忽略
        }
    }
}
