using SqlServerValidationToolkit.Model.Context;
using SqlServerValidationToolkit.UnitTests.TestDatabase.Entities;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.UnitTests.TestDatabase
{
    public class TestDatabaseContext : DbContext
    {
        private TestDatabaseContext()
            : base("TestDatabaseContext")
        {
        }

        private TestDatabaseContext(DbConnection existingConnection, bool contextOwnsConnection)
            : base(existingConnection, contextOwnsConnection)
        { }


        public DbSet<Baby> Babies { get; set; }

        public static TestDatabaseContext Create(string connectionString)
        {

            SqlConnectionFactory f = new SqlConnectionFactory();
            var connection = f.CreateConnection(connectionString);
            return new TestDatabaseContext(connection, true);
        }
    }
}
