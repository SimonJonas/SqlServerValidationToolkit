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

        private string _databaseName;
        public string DatabaseName {
            get
            {
                return _databaseName;
            }
            set
            {
                _databaseName = value;
                UpdateNameForSqlQuery(); 
            }
        }

        public string Schema { get; set; }

        public string NameForSqlQuery
        {
            get;
            set;
        }

        private string _name;
        [MaxLength(100)]
        public string Name {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                UpdateNameForSqlQuery();
            }
        }

        public void SetName(string databaseName, string schema, string name)
        {
            _databaseName = databaseName;
            _name = name;
            Schema = schema;
            UpdateNameForSqlQuery();
        }


        /// <summary>
        /// Defines the name that can be used in the sql query
        /// </summary>
        public void UpdateNameForSqlQuery()
        {
            NameForSqlQuery = string.Format("[{0}].{1}.[{2}]", DatabaseName, Schema, Name);
        }
        [Column("Id_Name")]
        [Required]
        public string IdColumnName { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Column> Columns { get; set; }
    }
}
