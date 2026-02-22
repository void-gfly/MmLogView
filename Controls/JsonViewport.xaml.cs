using System.Windows;
using System.Windows.Controls;
using MmLogView.ViewModels;

namespace MmLogView.Controls;

public partial class JsonViewport : UserControl
{
    private bool _isSyncing;

    public JsonViewport()
    {
        InitializeComponent();
    }

    public bool SearchNext(string text)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(JsonTextBox.Text)) return false;

        int startIndex = JsonTextBox.SelectionStart + JsonTextBox.SelectionLength;
        int index = JsonTextBox.Text.IndexOf(text, startIndex, StringComparison.OrdinalIgnoreCase);

        if (index < 0)
        {
            // wrap around
            index = JsonTextBox.Text.IndexOf(text, 0, StringComparison.OrdinalIgnoreCase);
        }

        if (index >= 0)
        {
            JsonTextBox.Focus();
            JsonTextBox.Select(index, text.Length);

            var rect = JsonTextBox.GetRectFromCharacterIndex(index);
            if (!rect.IsEmpty)
            {
                JsonTextBox.ScrollToVerticalOffset(JsonTextBox.VerticalOffset + rect.Top - JsonTextBox.ViewportHeight / 2);
            }
            return true;
        }
        return false;
    }

    public bool SearchPrev(string text)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(JsonTextBox.Text)) return false;

        int startIndex = JsonTextBox.SelectionStart - 1;
        if (startIndex < 0) startIndex = JsonTextBox.Text.Length - 1;

        int index = JsonTextBox.Text.LastIndexOf(text, startIndex, StringComparison.OrdinalIgnoreCase);

        if (index < 0)
        {
            // wrap around
            index = JsonTextBox.Text.LastIndexOf(text, JsonTextBox.Text.Length - 1, StringComparison.OrdinalIgnoreCase);
        }

        if (index >= 0)
        {
            JsonTextBox.Focus();
            JsonTextBox.Select(index, text.Length);

            var rect = JsonTextBox.GetRectFromCharacterIndex(index);
            if (!rect.IsEmpty)
            {
                JsonTextBox.ScrollToVerticalOffset(JsonTextBox.VerticalOffset + rect.Top - JsonTextBox.ViewportHeight / 2);
            }
            return true;
        }
        return false;
    }

    private void JsonTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (_isSyncing || e.NewValue is not JsonNodeViewModel node) return;

        _isSyncing = true;
        try
        {
            if (node.TextStart >= 0 && node.TextLength > 0 && node.TextStart + node.TextLength <= JsonTextBox.Text.Length)
            {
                JsonTextBox.Select(node.TextStart, node.TextLength);
                
                // 滚动到所选位置的开头
                var rect = JsonTextBox.GetRectFromCharacterIndex(node.TextStart);
                if (!rect.IsEmpty)
                {
                    JsonTextBox.ScrollToVerticalOffset(JsonTextBox.VerticalOffset + rect.Top - JsonTextBox.ViewportHeight / 2);
                }
            }
        }
        finally
        {
            _isSyncing = false;
        }
    }

    private void JsonTextBox_SelectionChanged(object sender, RoutedEventArgs e)
    {
        if (_isSyncing) return;

        var dc = DataContext as MainViewModel;
        if (dc == null || dc.JsonRootNodes.Count == 0) return;

        int caretPos = JsonTextBox.CaretIndex;
        // 寻找包含光标的最深叶子节点
        var targetNode = FindDeepestNodeContainingPosition(dc.JsonRootNodes[0], caretPos);
        if (targetNode != null)
        {
            _isSyncing = true;
            try
            {
                // 展开沿途的所有父节点
                var parent = targetNode.Parent;
                while (parent != null)
                {
                    parent.IsExpanded = true;
                    parent = parent.Parent;
                }
                
                // 树会自动对 IsSelected 的改变做出响应
                targetNode.IsSelected = true;
                
                // 选中文本（只有真正有包含长度的文本才选）
                if (targetNode.TextStart >= 0 && targetNode.TextLength > 0 && targetNode.TextStart + targetNode.TextLength <= JsonTextBox.Text.Length)
                {
                     JsonTextBox.Select(targetNode.TextStart, targetNode.TextLength);
                }
            }
            finally
            {
                _isSyncing = false;
            }
        }
    }

    private JsonNodeViewModel? FindDeepestNodeContainingPosition(JsonNodeViewModel node, int position)
    {
        if (position >= node.TextStart && position <= node.TextStart + node.TextLength)
        {
            foreach (var child in node.Children)
            {
                var found = FindDeepestNodeContainingPosition(child, position);
                if (found != null) return found;
            }
            return node;
        }
        return null; // Not in this node's range
    }

    private void MenuCopyNode_Click(object sender, RoutedEventArgs e)
    {
        if (JsonTreeView.SelectedItem is JsonNodeViewModel node)
        {
            // 复制如 "key": "value" 这样的简单节点信息
            if (!string.IsNullOrEmpty(node.Value) && node.Children.Count == 0)
            {
                Clipboard.SetText($@"""{node.Name}"": {node.Value}");
            }
            else
            {
                // 如果是 Object/Array 的头，仅复制节点名称
                Clipboard.SetText($@"""{node.Name}""");
            }
        }
    }

    private void MenuCopyNodeAndChildren_Click(object sender, RoutedEventArgs e)
    {
        if (JsonTreeView.SelectedItem is JsonNodeViewModel node)
        {
            if (node.TextStart >= 0 && node.TextLength > 0 && node.TextStart + node.TextLength <= JsonTextBox.Text.Length)
            {
                string textToCopy = JsonTextBox.Text.Substring(node.TextStart, node.TextLength);
                Clipboard.SetText(textToCopy);
            }
        }
    }
    private void TreeViewItem_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is StackPanel panel && panel.DataContext is JsonNodeViewModel node)
        {
            node.IsSelected = true;
        }
    }

    private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
    {
        if (sender is TreeViewItem tvi)
        {
            tvi.BringIntoView();
            e.Handled = true;
        }
    }
}
