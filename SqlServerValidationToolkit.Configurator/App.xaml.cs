using log4net;
using SqlServerValidationToolkit.Configurator.Properties;
using SqlServerValidationToolkit.Model.Context;
using SqlServerValidationToolkit.Model.DatabaseInitialization;
using SqlServerValidationToolkit.Model.Entities;
using SqlServerValidationToolkit.Model.Validation;
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
        ILog _log = LogManager.GetLogger(typeof(App));

        protected override void OnStartup(StartupEventArgs e)
        {
            using (var ctx = SqlServerValidationToolkitContext.Create())
            {
                if (!ctx.Databases.Any())
                {
                    MessageBox.Show("Welcome to the Validation Toolkit. Please select the database to validate", "Welcome", MessageBoxButton.OK);

                    SetDbConnectionString();
                    
                }
                else
                {
                    var decryptedConnectionString = Validator.GetDecryptedConnectionString();
                    if (decryptedConnectionString == null || decryptedConnectionString == string.Empty)
                    {

                        _log.Error("Error occurred while decrypting the connection string, connection string will be reset by the current user");
                        MessageBox.Show("Welcome to the Validation Toolkit. The current local database file was created by another user. Please select the connection string again for the validated database.", "Welcome", MessageBoxButton.OK);
                        SetDbConnectionString();
                    }
                }
            }

            base.OnStartup(e);

        }

        private void SetDbConnectionString()
        {
            ConnectionStringUpdater.UpdateDbConnectionString(
            () =>
            {
            },
            (ex) =>
            {
                _log.Error("Exception occurred while updating the connection string", ex);
                MessageBox.Show(string.Format("An error occurred: '{0}'" + Environment.NewLine + "The application will shut down. Please start it again and select another database.", ex.Message), "Validation toolkit", MessageBoxButton.OK, MessageBoxImage.Error);
                App.Current.Shutdown();
            }
            );
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var baseException = e.Exception.GetBaseException();
            _log.Error("Exception occurred", e.Exception);
            string message = string.Format("{0}" + Environment.NewLine + "at {1}", baseException.Message, baseException.StackTrace);
            MessageBox.Show(message, "Unhandled Exception occured");
            e.Handled = true;
        }
    }
}
