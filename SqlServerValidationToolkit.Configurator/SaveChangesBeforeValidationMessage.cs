using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.Configurator
{
    public enum HandleUnsavedChanges
    {
        Save,
        ExecuteWithoutSave,
        Cancel
    }
    class SaveChangesBeforeValidationMessage
    {
        public HandleUnsavedChanges HandleUnsavedChanges { get; set; }
    }
}
