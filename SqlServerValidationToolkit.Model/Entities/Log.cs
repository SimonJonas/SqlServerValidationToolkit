using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.Model.Entities
{
    [Table("Validation_Log")]
    public class Log
    {

        [Key]
        public int ValidationLog_id { get; set; }
        public DateTime ExecutedAt { get; set; }
    }
}
