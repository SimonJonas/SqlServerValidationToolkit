using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.Model.Entities
{
    [Table("Validation_Source")]
    public class Source
    {
        public Source()
        {
            this.Columns = new HashSet<Column>();
        }

        [Key]
        public int Source_id { get; set; }
        [MaxLength(100)]
        public string Name { get; set; }
        [Column("Id_Name")]
        [Required]
        public string IdColumnName { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Column> Columns { get; set; }
    }
}
