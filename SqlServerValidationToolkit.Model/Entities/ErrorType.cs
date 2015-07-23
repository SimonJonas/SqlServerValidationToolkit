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
    [Table("Validation_Errortype")]
    public class ErrorType
    {
        public ErrorType()
        {
            this.WrongValues = new HashSet<WrongValue>();
            this.ValidationRules = new HashSet<ValidationRule>();
        }

        public ErrorType(string checkType, string description)
        {
            Check_Type = checkType;
            Description = description;
        }

        [Key]
        public int ErrorType_id { get; set; }

        public int ErrorTypeIdForValidationQueries { get; set; }

        [MaxLength(100)]
        public string Check_Type { get; set; }

        [MaxLength(100)]
        public string Description { get; set; }

        public virtual ICollection<WrongValue> WrongValues { get; set; }
        public virtual ICollection<ValidationRule> ValidationRules { get; set; }
    }
}
