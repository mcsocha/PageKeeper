using System;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace PageKeeper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs args)
        {
            Current.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler((object exSender, DispatcherUnhandledExceptionEventArgs exArgs) =>
            {
                MessageBox.Show("Unhandled Exception: " + exArgs.Exception.Message + "\n\n");
                exArgs.Handled = true;
            });

            Uri startUri = null;
            if (args.Args.Length > 0)
                startUri = new Uri(args.Args[0]);

            var win = new MainWindow(startUri);
            win.Show();
        }
    }
}
