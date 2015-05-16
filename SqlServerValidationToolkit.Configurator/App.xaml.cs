using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SqlServerValidationToolkit.Configurator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var baseException = e.Exception.GetBaseException();
            string message = string.Format("{0}" + Environment.NewLine + "at {1}", baseException.Message, baseException.StackTrace);
            MessageBox.Show(message, "Unhandled Exception occured");
            e.Handled = true;
        }
    }
}
