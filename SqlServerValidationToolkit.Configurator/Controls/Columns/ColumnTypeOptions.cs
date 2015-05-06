using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.Configurator.Controls.Columns
{
    public class ColumnTypeOptions
    {
        public static List<string> ColumnTypes { get; set; }


        static ColumnTypeOptions()
        {
            ColumnTypes = new List<string>()
            {
                "string",
                "int",
                "numeric",
                "int_from_string",
                "numeric_from_string",
                "datetime"
            };
        }

    }
}
