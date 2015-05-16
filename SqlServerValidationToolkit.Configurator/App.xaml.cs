using SqlServerValidationToolkit.Configurator.Properties;
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
        protected override void OnStartup(StartupEventArgs e)
        {
            if (Settings.Default.DbConnectionString == string.Empty)
            {
                MessageBox.Show("Welcome to the Validation Toolkit. At first, please enter the connection to the database in the next screen to which you want to install the validation toolkit.", "Welcome", MessageBoxButton.OK);
                ConnectionStringUpdater.UpdateDbConnectionString(
                    () =>
                    {
                        MessageBox.Show("The toolkit was succesfully installed, go to ... to import the tables you want to validate");
                    },
                    (ex) =>
                    {
                        MessageBox.Show(string.Format("An error occurred during the installation of the validation toolkit: '{0}'" + Environment.NewLine + "The application will shut down. Please start it again and select another database.", ex.Message), "Validation toolkit", MessageBoxButton.OK, MessageBoxImage.Error);
                        App.Current.Shutdown();
                    }
                    );
            }

            base.OnStartup(e);

        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var baseException = e.Exception.GetBaseException();
            string message = string.Format("{0}" + Environment.NewLine + "at {1}", baseException.Message, baseException.StackTrace);
            MessageBox.Show(message, "Unhandled Exception occured");
            e.Handled = true;
        }
    }
}
