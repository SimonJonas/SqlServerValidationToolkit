using SqlServerValidationToolkit.Model.Entities.Rule;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.Model.Entities
{
    [Table("Validation_WrongValue")]
    public class WrongValue
    {
        public long Id { get; set; }
        public string Value { get; set; }
        public string Value2 { get; set; }
        public string ValueForInformation1 { get; set; }
        public string ValueForInformation2 { get; set; }
        public int ErrorType_fk { get; set; }
        [Column] //(TypeName="System.Int64")
        public long Log_id { get; set; }
        public bool Is_Corrected { get; set; }
        public bool Ignore { get; set; }


        [Key]
        public int WrongValue_id { get; set; }
        public int ValidationRule_fk { get; set; }

        [ForeignKey("ErrorType_fk")]
        public virtual ErrorType Errortype { get; set; }
        [ForeignKey("ValidationRule_fk")]
        public virtual ValidationRule Validation_ValidationRule { get; set; }
    }
}
