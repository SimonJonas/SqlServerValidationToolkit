using SqlServerValidationToolkit.Model.Context;
using SqlServerValidationToolkit.Model.DatabaseInitialization;
using SqlServerValidationToolkit.Model.Entities;
using SqlServerValidationToolkit.Model.Factory;
using SqlServerValidationToolkit.UnitTests.Properties;
using SqlServerValidationToolkit.UnitTests.TestDatabase;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.UnitTests
{
    class MigrationConfiguration : DbMigrationsConfiguration<TestDatabaseContext>
    {
        public MigrationConfiguration()
        {
            AutomaticMigrationsEnabled = true;
        }
    }


    class Initializer
    {
        //private static Initializer _initializer = new Initializer();

        public Initializer()
        {
            string databaseName;
            using (var ctx = new SqlServerValidationToolkitContext(Settings.Default.ConnectionString))
            {
                databaseName = ctx.Database.Connection.Database;
            }

            string deleteQueryFormat = @"
USE [master]
ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE [{0}] ;";
            string deleteQuery = string.Format(deleteQueryFormat, databaseName);
            string encryptedConnectionString;

            using (var secureConnectionString = Settings.Default.ConnectionString.ToSecureString())
            {
                encryptedConnectionString = secureConnectionString.EncryptString();
            }

            DatabaseInitializer initializer = new DatabaseInitializer(encryptedConnectionString);
            initializer.ExecuteSplitByGo(deleteQuery);

            System.Data.Entity.Database.SetInitializer<SqlServerValidationToolkitContext>(null);
            using (var ctx = new SqlServerValidationToolkitContext(Settings.Default.ConnectionString))
            {
                ctx.Database.Create();
            }

            ValidationRuleFactory = new ValidationRuleFactory();


            initializer.InstallValidationToolkit();

            System.Data.Entity.Database.SetInitializer<TestDatabaseContext>(null);

            initializer.ExecuteSplitByGo(Resources.CREATE_and_fill_Validation_Test_Database);
        }

        private ValidationRuleFactory ValidationRuleFactory { get; set; }

        //public static void InitializeDatabase()
        //{
        //    DatabaseInitializer initializer = new DatabaseInitializer(Settings.Default.ConnectionString);

        //    //initializer.ExecuteSplitByGo(Resources.DropAll);

        //    initializer.InstallValidationToolkit();

        //    //initializer.ExecuteSplitByGo(Resources.CREATE_and_fill_Validation_Test_Database);
        //}

        //public static void AddValidation(SqlServerValidationToolkitContext ctx)
        //{
        //    _initializer.AddValidationInt(ctx);
        //}

        public void AddValidation(SqlServerValidationToolkitContext ctx)
        {
            SqlServerValidationToolkit.Model.Entities.Database database = new Model.Entities.Database()
            {
                Name = ctx.Database.Connection.Database,
                
            };
            ctx.Databases.Add(database);

            ValidationRuleFactory.LoadErrorTypes(ctx);
            var source = new Source()
            {
                DatabaseName = ctx.Database.Connection.Database,
                Name = "Babies",
                Schema = "dbo",
                IdColumnName = "BabyID",
                Description = "",
                Database = database
            };
            source.UpdateNameForSqlQuery();
            AddLenghtColumn(source);
            AddWeightColumn(source);
            AddEmailColumn(source);
            AddHospitalEntryColumn(source);
            AddBirthDateColumn(source, ctx);

            ctx.Sources.Add(source);

            //set the compiled query for each validation rule
            ctx.ValidationRules.Local.ToList().ForEach(vr => vr.CompiledQuery = vr.Query);

            Save(ctx);
        }

        private static void AddBirthDateColumn(Source source, SqlServerValidationToolkitContext ctx)
        {
            var cBirthDate = new Column()
            {
                Name = "Birth_date",
                Description = "The birthdate of the child",
                Type = "datetime",

            };


            var errorType = new ErrorType()
            {
                Check_Type = "CustomQuery",
                Description = "The birthdate must be less than two days after the Hospital Entry",
            };
            ctx.Errortypes.Add(errorType);
            ctx.SaveChanges();


            var birthDateValidationRule = ValidationRuleFactory.CreateCustomQueryRule(string.Format(@"SELECT [BabyID], {0} AS ErrorType_fk
  FROM [dbo].[Babies]
  WHERE DATEDIFF(day,Hospital_entry,Birth_date)>1", errorType.ErrorType_id));

            birthDateValidationRule.Errortypes.Add(errorType);

            cBirthDate.ValidationRules.Add(birthDateValidationRule);
            source.Columns.Add(cBirthDate);
        }

        private void AddHospitalEntryColumn(Source source)
        {

            var cHospitalEntry = new Column()
            {
                Name = "Hospital_entry",
                Description = "Entry-date of the mother",
                Type = "datetime",

            };
            ;
            cHospitalEntry.ValidationRules.Add(
                ValidationRuleFactory.CreateComparisonRule("<", "Birth_date", "birth date of the child"));
            source.Columns.Add(cHospitalEntry);

        }

        private void AddEmailColumn(Source source)
        {
            var cEmail = new Column()
            {
                Name = "Email",
                Description = "Email-Address",
                Type = "string",

            };

            cEmail.ValidationRules.Add(
                ValidationRuleFactory.CreateLikeRule("%@%")
                );
            source.Columns.Add(cEmail);
        }

        private void AddWeightColumn(Source source)
        {

            var cWeight = new Column()
            {
                Name = "Weight",
                Description = "Weight in Grams",
                Type = "int",

            };
            cWeight.ValidationRules.Add(
                ValidationRuleFactory.CreateMinMaxRule(100, 5000, NullValueTreatment.InterpretAsError));
            source.Columns.Add(cWeight);

        }

        private void AddLenghtColumn(Source source)
        {
            var cLength = new Column()
            {
                Name = "Length",
                Description = "Length in cm",
                Type = "numeric",

            };
            cLength.ValidationRules.Add(
                ValidationRuleFactory.CreateMinMaxRule(10, 60)
                );
            source.Columns.Add(cLength);
        }

        private static void Save(SqlServerValidationToolkitContext ctx)
        {
            try
            {
                ctx.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                string errorMessage = "";
                foreach (var errorEntity in e.EntityValidationErrors)
                {
                    foreach (var error in errorEntity.ValidationErrors)
                    {
                        errorMessage += error.PropertyName + ": " + error.ErrorMessage;
                    }
                }
                throw new Exception("Validation-Exception:" + Environment.NewLine + errorMessage);
            }
        }

    }
}
