using SqlServerValidationToolkit.Model.Entities.Rule;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.Model.Entities.Temp
{
    [Table("Validation_TempId")]
    public class TempId
    {
        [Key]
        public int TempIdKey { get; set; }

        public int Id { get; set; }
        public int ValidationRule_fk { get; set; }
        public int ErrorType_fk { get; set; }

        [ForeignKey("ValidationRule_fk")]
        public virtual ValidationRule ValidationRule { get; set; }
        [ForeignKey("ErrorType_fk")]
        public virtual ErrorType ErrorType { get; set; }
    }
}
