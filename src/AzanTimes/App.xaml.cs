using System.Windows;
using System.Windows.Threading;

namespace AzanTimes;

public partial class App : Application
{
    private AppHost? _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        DispatcherUnhandledException += OnDispatcherUnhandledException;

        _host = new AppHost(this);
        if (!_host.Start())
            Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _host?.Dispose();
        base.OnExit(e);
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(e.Exception.Message, "AzanTimes", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }
}
