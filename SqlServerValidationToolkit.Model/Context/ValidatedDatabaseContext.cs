using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.Model.Context
{
    /// <summary>
    /// Context for the database that is validated
    /// </summary>
    class ValidatedDatabaseContext : DbContext
    {
        
        public ValidatedDatabaseContext()
            : base()
        {

        }

        public ValidatedDatabaseContext(DbConnection existingConnection, bool contextOwnsConnection)
            : base(existingConnection, contextOwnsConnection)
        { }

        public ValidatedDatabaseContext(string connectionString)
            : base(connectionString)
        {

        }

    }
}
