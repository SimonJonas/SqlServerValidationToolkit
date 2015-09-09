using SqlServerValidationToolkit.Model.DatabaseInitialization;
using SqlServerValidationToolkit.Model.Entities;
using SqlServerValidationToolkit.Model.Entities.Rule;
using SqlServerValidationToolkit.Model.Entities.Temp;
using SqlServerValidationToolkit.Model.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.Model.Context
{
    public class SqlServerValidationToolkitContext : DbContext
    {
        public SqlServerValidationToolkitContext()
            : base()
        {

        }

        public SqlServerValidationToolkitContext(DbConnection existingConnection, bool contextOwnsConnection)
            : base(existingConnection, contextOwnsConnection)
        { }

        public SqlServerValidationToolkitContext(string connectionString)
            : base(connectionString)
        {

        }

        public static SqlServerValidationToolkitContext Create()
        {
            SqlServerValidationToolkitContext ctx;

            if (Settings.Default.StoreMetadataInSqlServer)
            {
                ctx = SqlServerValidationToolkitContext.Create(Settings.Default.SqlServerConnectionString);
            } else
            {
                string databaseFileName = "SqlServerValidationToolkit.Model.Context.SqlServerValidationToolkitContext.sdf";
                string sqlServerCompactConnectionString = string.Format("Data Source={0}", databaseFileName);

                ctx = new SqlServerValidationToolkitContext(sqlServerCompactConnectionString);
                if (!File.Exists(databaseFileName))
                {
                    ctx.Database.CreateIfNotExists();
                }
            }

            DatabaseInitializer.AddErrorTypes(ctx);

            return ctx;

        }

        public static DbConnection CreateConnection(string connectionString)
        {
            SqlConnectionFactory f = new SqlConnectionFactory();
            var connection = f.CreateConnection(connectionString);
            return connection;
        }

        public static SqlServerValidationToolkitContext Create(string connectionString)
        {

            SqlConnectionFactory f = new SqlConnectionFactory();
            var connection = f.CreateConnection(connectionString);
            return new SqlServerValidationToolkitContext(connection, true);
        }

        public DbSet<SqlServerValidationToolkit.Model.Entities.Database> Databases { get; set; }
        public DbSet<Source> Sources { get; set; }
        public DbSet<ErrorType> Errortypes { get; set; }
        public DbSet<WrongValue> WrongValues { get; set; }
        public DbSet<ValidationRule> ValidationRules { get; set; }
        public DbSet<Column> Columns { get; set; }

        public DbSet<Log> Logs { get; set; }
        public DbSet<TempId> TempIds { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Entity<ValidationRule>()
                .HasMany(x => x.Errortypes)
                .WithMany(x => x.ValidationRules)
                .Map(x =>
                {
                    x.ToTable("Validation_ValidationRule_ErrorType");
                    x.MapLeftKey("ValidationRule_fk");
                    x.MapRightKey("ErrorType_fk");
                });

            modelBuilder 
                .Entity<Source>()
                .Property(s => s.Name)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute()));

            modelBuilder
                .Entity<Column>()
                .Property(c => c.Name)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute()));
            
        }

        public void Validate(SqlServerValidationToolkitContext ctxSqlServer)
        {

            foreach (var source in Sources
                //include the errorType to get to the code of the wrong-values
                .Include(
                s=>s.Columns
                    .Select(c=>c.ValidationRules
                        .Select(r=>r.Validation_WrongValue
                            .Select(wv=>wv.Errortype)
                            )
                            )
                            )
                            )
            {
                source.Validate(ctxSqlServer.Database.Connection, this);
            }
            SaveChanges();
            this.Database.ExecuteSqlCommand("DELETE FROM Validation_WrongValue WHERE Is_Corrected=1");
        }

        //public void Validate()
        //{
        //    this.Database.ExecuteSqlCommand("EXECUTE [dbo].[Validation_USP_ExecuteValidation]");

        //}
    }
}
