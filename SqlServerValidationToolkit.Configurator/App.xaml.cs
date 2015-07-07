using SqlServerValidationToolkit.Configurator.Properties;
using SqlServerValidationToolkit.Model.Context;
using SqlServerValidationToolkit.Model.DatabaseInitialization;
using SqlServerValidationToolkit.Model.Entities;
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
            using (var ctx = SqlServerValidationToolkitContext.Create())
            {
                if (!ctx.Databases.Any())
                {
                    MessageBox.Show("Welcome to the Validation Toolkit. Please select the database to validate", "Welcome", MessageBoxButton.OK);
                    
                    ConnectionStringUpdater.UpdateDbConnectionString(
                    () =>
                    {
                        //MessageBox.Show("The toolkit was succesfully installed, press Ctrl+U to import the tables from the database that you want to validate. Then create some rules and press Ctrl+V to execute the validation. Under 'Wrong values' the validation results are listed.");
                    },
                    (ex) =>
                    {
                        MessageBox.Show(string.Format("An error occurred: '{0}'" + Environment.NewLine + "The application will shut down. Please start it again and select another database.", ex.Message), "Validation toolkit", MessageBoxButton.OK, MessageBoxImage.Error);
                        App.Current.Shutdown();
                    }
                    );
                    
                }
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
