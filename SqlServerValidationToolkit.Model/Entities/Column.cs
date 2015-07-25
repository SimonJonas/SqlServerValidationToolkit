using SqlServerValidationToolkit.Model.Context;
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
    [Table("Validation_Column")]
    public class Column
    {
        public Column()
        {
            this.ValidationRules = new HashSet<ValidationRule>();
        }

        [Key]
        public int Column_id { get; set; }

        public int Source_fk { get; set; }
        
        [MaxLength(100)]
        public string Name { get; set; }

        public string Description { get; set; }
        public string Type { get; set; }

        [ForeignKey("Source_fk")]
        public virtual Source Source { get; set; }
        public virtual ICollection<ValidationRule> ValidationRules { get; set; }

        [NotMapped]
        public bool IsNumeric
        {
            get
            {
                //"string",
                //"int",
                //"numeric",
                //"int_from_string",
                //"numeric_from_string",
                //"datetime"
                string[] numericTypes = new string[]{
                    "int",
                    "numeric",
                    "int_from_string",
                    "numeric_from_string",
                };
                return (numericTypes.Contains(Type));


            }
        }

        [NotMapped]
        public bool IsDateTime
        {
            get
            {
                return this.Type == "datetime";
            }
        }

        /// <summary>
        /// Fills the wrongValues-table with all wrong values from the column's rules
        /// </summary>
        public void Validate(System.Data.Common.DbConnection connection, SqlServerValidationToolkitContext ctx)
        {
            foreach (var rule in this.ValidationRules)
            {
                rule.Validate(connection, ctx);       
            }
        }

        
    }
}
