using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.Configurator.Controls
{
    class ValidationErrorMessage
    {
        public DbEntityValidationException Exception { get; set; }
    }
}
