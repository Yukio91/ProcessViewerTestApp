using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ProcessViewerTestApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public new static App Current => Application.Current as App;
        public new MainWindow MainWindow => base.MainWindow as MainWindow;

        public App()
        {
            FrameworkCompatibilityPreferences.KeepTextBoxDisplaySynchronizedWithTextProperty = false;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            //var manager = new ProcessManager();
            //var list = manager.GetProcessInfos(CancellationToken.None);
            
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            //todo App_OnDispatcherUnhandledException
        }
    }
}
