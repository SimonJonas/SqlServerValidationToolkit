using SqlServerValidationToolkit.Model.Context;
using SqlServerValidationToolkit.Model.Entities;
using SqlServerValidationToolkit.Model.Entities.Rule;
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
    public class CreateSqlServerValidationToolkitDb : CreateDatabaseIfNotExists<SqlServerValidationToolkitContext>
    {
        protected override void Seed(SqlServerValidationToolkitContext context)
        {
            //context.Database.ExecuteSqlCommand(string.Format(
            //            @"CREATE UNIQUE INDEX LX_{0} ON {0} ({1})",
            //                     "Validation_Errortype", "Check_Type, Description"));
            context.Database.ExecuteSqlCommand(Resources.Views);

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
                AddErrorTypes(ctx);
            }
            string createToolkit =  Resources.ProceduresAndFunctions;
            ExecuteSplitByGo(createToolkit);
        }

        public static void AddErrorTypes(SqlServerValidationToolkitContext ctx)
        {
            if (ctx.Errortypes.Any())
            {
                return;
            }
            ctx.Errortypes.AddRange(new List<ErrorType>()
                {
                    new ErrorType(MinMaxRule.TooLowErrorTypeCode,"MinMax","Too low"),
                    new ErrorType(MinMaxRule.TooHighErrorTypeCode,"MinMax","Too high"),
                    new ErrorType(MinMaxRule.TooEarlyErrorTypeCode,"MinMax","Too early"),
                    new ErrorType(MinMaxRule.TooLateErrorTypeCode,"MinMax","Too late"),
                    new ErrorType(ValidationRule.NotEnteredErrorTypeCode,"Common","not entered"),
                    new ErrorType(ComparisonRule.GreaterThanErrorType,"Comparison","greater than"),
                    new ErrorType(ComparisonRule.SmallerThanErrorTypeCode,"Comparison","smaller than"),
                    new ErrorType(ComparisonRule.LaterThanErrorTypeCode,"Comparison","later than"),
                    new ErrorType(ComparisonRule.EarlierThanErrorTypeCode,"Comparison","earlier than"),
                    new ErrorType(ComparisonRule.GreaterThanOrEqualsErrorTypeCode,"Comparison","greater than or equals"),
                    new ErrorType(ComparisonRule.SmallerThanOrEqualsErrorTypeCode,"Comparison","smaller than or equals"),
                    new ErrorType(ComparisonRule.LaterThanOrEqualsErrorTypeCode,"Comparison","later than or at the same time as"),
                    new ErrorType(ComparisonRule.EarlierThanOrEqualsErrorTypeCode,"Comparison","earlier than or at the same time as"),
                    new ErrorType(ComparisonRule.NotEqualsErrorTypeCode,"Comparison","not equals"),
                    new ErrorType(ComparisonRule.EqualsErrorTypeCode,"Comparison","equals"),
                    new ErrorType(LikeRule.NotLikeErrorTypeCode,"Like","not like"),
                });
            ctx.SaveChanges();
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
