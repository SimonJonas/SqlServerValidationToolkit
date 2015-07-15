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
        public void Validate(SqlServerValidationToolkitContext ctx)
        {
            var connection = ctx.Database.Connection;
            
            foreach (var rule in this.ValidationRules)
            {
                connection.Open();
                string q = rule.CompiledQuery;
                var c = connection.CreateCommand();
                c.CommandText = q;
                try
                {
                    var reader = c.ExecuteReader();


                    while (reader.Read())
                    {
                        //the query returns the id of the invalid value and the errortype-id
                        int invalidValueId = reader.GetInt32(0);

                        int errorTypeId = reader.GetInt32(1);

                        WrongValue wrongValue = new WrongValue()
                        {
                            ErrorType_fk = errorTypeId,
                            Id = invalidValueId,
                            Value = GetValue(invalidValueId, ctx)
                        };
                        rule.Validation_WrongValue.Add(wrongValue);
                    }
                } catch (Exception e)
                {
                    throw new Exception(string.Format("Exception occurred while executing '{0}': {1}", q, e.GetBaseException().Message));
                } finally
                {
                    connection.Close();
                }
                
            }
        }

        /// <summary>
        /// Returns the value of the column for the id
        /// </summary>
        private string GetValue(int invalidValueId, SqlServerValidationToolkitContext ctx)
        {
            string selectValueSqlFormat = "SELECT [{0}] FROM [{1}] WHERE [{2}]={3}";
            string selectValueSql = string.Format(selectValueSqlFormat,
                this.Name,
                this.Source.Name,
                this.Source.IdColumnName,
                invalidValueId);
            var c = ctx.Database.Connection.CreateCommand();
            c.CommandText = selectValueSql;
            var result =  c.ExecuteScalar() ;
            return result.ToString();
        }
    }
}
