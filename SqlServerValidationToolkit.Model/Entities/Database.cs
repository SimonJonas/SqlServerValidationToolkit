using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.Model.Entities
{
    [Table("Validation_Database")]
    public class Database
    {
        public Database()
        {
            this.Sources = new HashSet<Source>();
        }

        [Key]
        public int Database_id { get; set; }

        public string EncryptedConnectionString { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Source> Sources { get; set; }
    }
}
