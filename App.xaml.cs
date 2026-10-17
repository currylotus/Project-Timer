using System.Windows;
using System.Windows.Threading;

namespace ProjectTimer;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        base.OnStartup(e);
        new MainWindow().Show();
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(e.Exception.Message, "Project Timer", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }
}