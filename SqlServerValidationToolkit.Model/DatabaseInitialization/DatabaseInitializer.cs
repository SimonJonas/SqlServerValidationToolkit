using SqlServerValidationToolkit.Model.Context;
using SqlServerValidationToolkit.Model.Entities;
using SqlServerValidationToolkit.Model.Properties;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.Model.DatabaseInitialization
{
    public class DropCreateSqlServerValidationToolkitDb : DropCreateDatabaseIfModelChanges<SqlServerValidationToolkitContext>
    {
        protected override void Seed(SqlServerValidationToolkitContext context)
        {
            context.Database.ExecuteSqlCommand(string.Format(
                        @"CREATE UNIQUE INDEX LX_{0} ON {0} ({1})",
                                 "Validation_Errortype", "Check_Type, Description"));

        }
    }

    /// <summary>
    /// Initializes the database
    /// </summary>
    public class DatabaseInitializer
    {
        public DatabaseInitializer(string connectionString)
        {
            _connectionString = connectionString;
        }

        private string _connectionString;

        /// <summary>
        /// Installs the required objects for the validation toolkit
        /// </summary>
        public void InstallValidationToolkit()
        {
            using (var ctx = SqlServerValidationToolkitContext.Create())
            {
                ctx.Errortypes.AddRange(new List<ErrorType>()
                {
                    new ErrorType("MinMax","Too low"),
                    new ErrorType("MinMax","Too high"),
                    new ErrorType("MinMax","Too early"),
                    new ErrorType("MinMax","Too late"),
                    new ErrorType("Common","not entered"),
                    new ErrorType("Comparison","greater than"),
                    new ErrorType("Comparison","smaller than"),
                    new ErrorType("Comparison","later than"),
                    new ErrorType("Comparison","earlier than"),
                    new ErrorType("Comparison","greater than or equals"),
                    new ErrorType("Comparison","smaller than or equals"),
                    new ErrorType("Comparison","later than or at the same time as"),
                    new ErrorType("Comparison","earlier than or at the same time as"),
                    new ErrorType("Comparison","not equals"),
                    new ErrorType("Comparison","equals"),
                    new ErrorType("Comparison","at the same time as"),
                    new ErrorType("Like","not like"),
                });
                ctx.SaveChanges();
            }
            string createToolkit =  Resources.ProceduresAndFunctions;
            ExecuteSplitByGo(createToolkit);
        }


        /// <summary>
        /// Opens a connection for each batch (seperated by a GO) and executes it
        /// </summary>
        /// <param name="cmd">SQL that contains GO-statements</param>
        public void ExecuteSplitByGo(string cmd)
        {

            string[] cmds = Regex.Split(cmd, "GO");

            foreach (string c in cmds)
            {
                if (!string.IsNullOrEmpty(c))
                {
                    Execute(c);
                }
            }
        }

        private void Execute(string cmd)
        {
            using (var secureString = _connectionString.DecryptString())
            {
                string decrypted = secureString.ToInsecureString();
                using (SqlConnection connection = new SqlConnection(decrypted))
                {
                    connection.Open();
                    // Create the Command and Parameter objects.
                    SqlCommand command = new SqlCommand(cmd, connection);

                    command.ExecuteNonQuery();

                    connection.Close();
                }
            }
            
        }

    }
}
