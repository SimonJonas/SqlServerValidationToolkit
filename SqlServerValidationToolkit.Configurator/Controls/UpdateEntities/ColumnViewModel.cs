using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.Configurator.Controls.UpdateEntities
{
    class ColumnViewModel
    {
        public string Name { get; set; }
        public string SqlType { get; set; }
        public bool Nullable { get; set; }
    }
}
