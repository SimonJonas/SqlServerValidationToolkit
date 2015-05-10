using Microsoft.Data.ConnectionUI;
using SqlServerValidationToolkit.Configurator.Messages;
using SqlServerValidationToolkit.Configurator.Properties;
using SqlServerValidationToolkit.Model.Context;
using SqlServerValidationToolkit.Model.DatabaseInitialization;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SqlServerValidationToolkit.Configurator
{
    /// <summary>
    /// Updates the connection string
    /// </summary>
    class ConnectionStringUpdater
    {
        /// <summary>
        /// Asks the user for the new connection string and updates it in the user settings
        /// </summary>
        public static void UpdateDbConnectionString(Action succeededHandler, Action<Exception> failedHandler)
        {
            var m = new GetDbConnectionStringMessage()
            {
                ConnectionString = Settings.Default.DbConnectionString
            };
            UpdateDbConnectionString(m);
            if (m.ConnectionString == string.Empty)
            {
                //No connection string was provided, shut down and ask next time again
                Application.Current.Shutdown();
                return;
            }

            try
            {
                //Check if new db-objects must be installed/updated
                if (!IsValidationToolKitInstalled(m.ConnectionString))
                {
                    MessageBox.Show("The validation toolkit is not installed, the procedures, views and tables will be installed (they all start with [Validation...]", "Validation toolkit", MessageBoxButton.OK);
                    DatabaseInitializer initializer = new DatabaseInitializer(m.ConnectionString);
                    initializer.InstallValidationToolkit();
                }

                Settings.Default.DbConnectionString = m.ConnectionString;
                Settings.Default.Save();
                succeededHandler();
            }
            catch (Exception e)
            {
                failedHandler(e);
                //MessageBox.Show(string.Format("An error occurred during the installation of the validation toolkit: '{0}'" + Environment.NewLine + "The application will shut down.", e.Message), "Validation toolkit", MessageBoxButton.OK, MessageBoxImage.Error);
                ////App.Current.Shutdown();
                //return;
            }


        }

        private static bool IsValidationToolKitInstalled(string connectionString)
        {
            using (var ctx = new SqlServerValidationToolkitContext(connectionString))
            {
                var adapter = (IObjectContextAdapter)ctx;
                var connection = ((EntityConnection)adapter.ObjectContext.Connection).StoreConnection;
                connection.Open();
                var c = connection.CreateCommand();
                c.CommandText = "SELECT COUNT(*) FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.[Validation_USP_ExecuteValidation]') AND type in (N'P')";
                var reader = c.ExecuteReader();
                bool readIsSuccessfull = reader.Read();
                //bool resultExists= reader.NextResult();
                int count = reader.GetInt32(0);
                reader.Close();
                if (count == 0)
                {
                    return false;
                }
                connection.Close();
            }
            return true;

        }

        private static void CheckExistenceOfRequiredDbObjects()
        {

        }

        public static void UpdateDbConnectionString(GetDbConnectionStringMessage m)
        {
            DataConnectionDialog dcd = new DataConnectionDialog();

            DataProvider dataProvider = DataProvider.SqlDataProvider;
            DataSource dataSource = new DataSource(dataProvider.Name, dataProvider.DisplayName);

            dataSource.Providers.Add(dataProvider);

            dcd.DataSources.Add(dataSource);
            dcd.SelectedDataSource = dataSource;
            dcd.SelectedDataProvider = dataProvider;

            if (m.ConnectionString != string.Empty)
            {
                dcd.ConnectionString = m.ConnectionString;
            }

            if (DataConnectionDialog.Show(dcd) == System.Windows.Forms.DialogResult.OK)
            {
                m.ConnectionString = dcd.ConnectionString;
            }
        }
    }
}
