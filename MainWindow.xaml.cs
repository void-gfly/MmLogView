using System.IO;
using System.Windows;
using System.Windows.Input;
using MmLogView.ViewModels;

namespace MmLogView;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm;

    public MainWindow(string? filePath)
    {
        InitializeComponent();

        var ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        Title = $"MmLogView v{ver!.Major}.{ver.Minor}.{ver.Build}";

        _vm = new MainViewModel();
        DataContext = _vm;

        _vm.ViewportControl = LogView;
        _vm.WebView = MdWebView;
        _ = InitWebView2Async();

        if (filePath is not null && File.Exists(filePath))
        {
            _vm.OpenFile(filePath);
        }
    }

    private async System.Threading.Tasks.Task InitWebView2Async()
    {
        var userDataFolder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "MmLogView_WebView2");
        var env = await Microsoft.Web.WebView2.Core.CoreWebView2Environment.CreateAsync(null, userDataFolder);
        await MdWebView.EnsureCoreWebView2Async(env);
        _vm.OnWebViewReady();
    }

    private void Window_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
            if (files.Length > 0)
            {
                _vm.OpenFile(files[0]);
            }
        }
    }

    private void Window_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) 
            ? DragDropEffects.Copy 
            : DragDropEffects.None;
        e.Handled = true;
    }

    private void RecentDropdownBtn_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button btn && btn.ContextMenu is not null)
        {
            btn.ContextMenu.DataContext = DataContext;
            btn.ContextMenu.PlacementTarget = btn;
            btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            btn.ContextMenu.IsOpen = true;
        }
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && _vm.IsSearchVisible)
        {
            _vm.IsSearchVisible = false;
        }
    }

    private void SearchBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            _vm.SearchNextCommand.Execute(null);
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _vm.Dispose();
        base.OnClosed(e);
    }
}
