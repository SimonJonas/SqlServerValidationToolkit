using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.Configurator.Controls.UpdateEntities
{
    class TableViewModel
    {
        public TableViewModel(string schema, string name)
        {
            Schema = schema;
            Name = name;
            Columns = new List<ColumnViewModel>();
        }

        public string PrimaryKeyColumnName { get; set; }
        public string Schema { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
        public List<ColumnViewModel> Columns { get; set; }
    }
}
