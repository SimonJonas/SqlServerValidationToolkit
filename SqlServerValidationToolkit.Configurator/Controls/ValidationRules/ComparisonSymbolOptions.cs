using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.Configurator.Controls.ValidationRules
{
    class ComparisonSymbolOptions
    {
        public static List<string> ComparisonSymbols { get; set; }

        static ComparisonSymbolOptions()
        {
            ComparisonSymbols = new List<string>()
            {
                "< ",
                "> ",
                "<=",
                ">=",
                "= ",
                "!="
            };
        }
    }
}
