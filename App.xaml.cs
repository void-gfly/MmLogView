using System.Windows;

namespace MmLogView;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        string? filePath = e.Args.Length > 0 ? e.Args[0] : null;
        var window = new MainWindow(filePath);
        MainWindow = window;
        window.Show();
    }
}
