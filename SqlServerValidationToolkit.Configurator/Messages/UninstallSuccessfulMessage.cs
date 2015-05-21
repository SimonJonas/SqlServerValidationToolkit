using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.Configurator.Messages
{
    public class UninstallSuccessfulMessage
    {
        public UninstallSuccessfulMessage(Exception e)
        {
            Exception = e;
        }
        public Exception Exception { get; set; }
    }
}
