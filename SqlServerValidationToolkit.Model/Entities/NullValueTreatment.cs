using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.Model.Entities
{
    public enum NullValueTreatment : short
    {
        Ignore = 0,
        InterpretAsError = 1,
        ConvertToDefaultValue = 2
    }
}
