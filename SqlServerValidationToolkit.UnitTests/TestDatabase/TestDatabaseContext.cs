using SqlServerValidationToolkit.Model.Context;
using SqlServerValidationToolkit.UnitTests.TestDatabase.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.UnitTests.TestDatabase
{
    public class TestDatabaseContext : DbContext
    {
        public TestDatabaseContext()
            : base("TestDatabaseContext")
        {
        }

        public TestDatabaseContext(string connectionString)
            : base(connectionString)
        {

        }

        public DbSet<Baby> Babies { get; set; }
    }
}
