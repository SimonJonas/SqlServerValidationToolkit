using SqlServerValidationToolkit.Model.Entities;
using SqlServerValidationToolkit.Model.Entities.Rule;
using SqlServerValidationToolkit.Model.Entities.Temp;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.Model.Context
{
    public class SqlServerValidationToolkitContext : DbContext
    {
        //public SqlServerValidationToolkitContext()
        //    : base("SqlServerValidationToolkitContext")
        //{
        //}
        public SqlServerValidationToolkitContext()
            : base()
        {

        }

        public SqlServerValidationToolkitContext(string connectionString)
            : base(connectionString)
        {

        }

        /// <summary>
        /// Creates a context with the unencrypted connection string
        /// </summary>
        public static SqlServerValidationToolkitContext Create(string encryptedConnectionString)
        {
            using (var secureConnectionString = encryptedConnectionString.DecryptString())
            {
                string unencryptedConnectionString = secureConnectionString.ToInsecureString();
                return new SqlServerValidationToolkitContext(unencryptedConnectionString);
            }
            
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

        public void Validate()
        {
            this.Database.ExecuteSqlCommand("EXECUTE [dbo].[Validation_USP_ExecuteValidation]");
        }

        //public void SaveChanges()
        //{
        //    this.SaveChanges();
        //}
    }
}
