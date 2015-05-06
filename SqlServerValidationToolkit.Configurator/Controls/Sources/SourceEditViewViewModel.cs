using SqlServerValidationToolkit.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.Configurator.Controls.Sources
{
    class SourceEditViewViewModel : SelectableViewModel
    {
        public Source Source { get; set; }
    }
}
