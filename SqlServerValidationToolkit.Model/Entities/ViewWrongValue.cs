using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.Model.Entities
{
    public class ViewWrongValue
    {
        public string SourceName { get; set; }
        public string SourceDescription { get; set; }
        public long Id { get; set; }
        public string ColumnName { get; set; }
        public string ColumnDescription { get; set; }
        public string Value { get; set; }
        public string WhoIsResponsible { get; set; }
        public long Log_id { get; set; }
        public bool IsCorrected { get; set; }
        public string Description { get; set; }
        public int ErrorType_id { get; set; }
        public bool Ignore { get; set; }
        public int Column_id { get; set; }
    }
}
