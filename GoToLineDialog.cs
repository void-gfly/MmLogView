using System.Windows;
using System.Windows.Controls;
using MmLogView.Properties;

namespace MmLogView;

/// <summary>跳转到指定行号的对话框。</summary>
public class GoToLineDialog : Window
{
    private readonly TextBox _textBox;

    public long LineNumber { get; private set; }

    public GoToLineDialog(long maxLines)
    {
        var res = ResourcesExtension.Instance;
        Title = res.GoToLineTitle;
        Width = 360;
        Height = 180;
        SizeToContent = SizeToContent.Height;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize;
        WindowStyle = WindowStyle.ToolWindow;
        Background = (System.Windows.Media.Brush?)Application.Current.TryFindResource("WindowBackground")
            ?? System.Windows.Media.Brushes.White;

        var panel = new StackPanel { Margin = new Thickness(16) };

        var label = new TextBlock
        {
            Text = string.Format(res.GoToLineLabel, maxLines),
            Margin = new Thickness(0, 0, 0, 8),
            Foreground = (System.Windows.Media.Brush?)Application.Current.TryFindResource("WindowForeground")
                ?? System.Windows.Media.Brushes.Black
        };
        panel.Children.Add(label);

        _textBox = new TextBox
        {
            Height = 28,
            Margin = new Thickness(0, 0, 0, 12)
        };
        _textBox.KeyDown += (_, e) =>
        {
            if (e.Key == System.Windows.Input.Key.Enter) TryAccept(maxLines);
        };
        panel.Children.Add(_textBox);

        var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        var okButton = new Button { Content = res.BtnOk, Width = 80, Margin = new Thickness(0, 0, 8, 0) };
        okButton.Click += (_, _) => TryAccept(maxLines);
        var cancelButton = new Button { Content = res.BtnCancel, Width = 80, IsCancel = true };
        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);
        panel.Children.Add(buttonPanel);

        Content = panel;
        Loaded += (_, _) => _textBox.Focus();
    }

    private void TryAccept(long maxLines)
    {
        if (long.TryParse(_textBox.Text, out long line) && line >= 1 && line <= maxLines)
        {
            LineNumber = line;
            DialogResult = true;
        }
        else
        {
            var res = ResourcesExtension.Instance;
            MessageBox.Show(string.Format(res.InvalidLineInput, maxLines), res.InvalidInputTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
