using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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
            // Global exception handling  
            Application.Current.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler((object exSender, DispatcherUnhandledExceptionEventArgs exArgs) =>
            {
                MessageBox.Show("Unhandled Exception: " + exArgs.Exception.Message + "\n\n");
                exArgs.Handled = true;
            });
        }
    }
}
