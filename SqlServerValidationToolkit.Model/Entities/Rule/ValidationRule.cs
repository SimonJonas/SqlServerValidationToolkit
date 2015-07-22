using SqlServerValidationToolkit.Model.Context;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.Model.Entities.Rule
{
    [Table("Validation_ValidationRule")]
    public abstract class ValidationRule
    {
        public ValidationRule()
        {
            this.Validation_WrongValue = new HashSet<WrongValue>();
            this.Errortypes = new HashSet<ErrorType>();
            this.IsActive = true;
        }

        [Key]
        public int ValidationRule_id { get; set; }
        public int Column_fk { get; set; }
        public bool IsActive { get; set; }
        
        public Nullable<NullValueTreatment> NullValueTreatment { get; set; }
        public string CompiledQuery { get; set; }

        [ForeignKey("Column_fk")]
        public virtual Column Column { get; set; }
        public virtual ICollection<WrongValue> Validation_WrongValue { get; set; }
        public virtual ICollection<ErrorType> Errortypes { get; set; }
        public virtual string ErrorDescriptionFormat
        {
            get
            {
                return "{0}";
            }
        }

        /// <summary>
        /// The Query the returns the id and the errorId of the invalid entries
        /// </summary>
        public abstract string Query
        {
            get;
        }

        protected int GetErrorTypeId(string checkType)
        {
            return Errortypes.Single(et => et.Check_Type.Equals(checkType)).ErrorType_id;
        }

        protected int GetErrorTypeId(string checkType, string description)
        {
            return Errortypes.Single(et => et.Check_Type.Equals(checkType) && et.Description == description).ErrorType_id;
        }

        /// <summary>
        /// Returns the check if the column is null depending on the NullValueTreatment
        /// </summary>
        protected string GetNullCheck()
        {
            string columnName = GetColumnName();
            string nullCheck = "";
            switch (NullValueTreatment)
            {
                case Entities.NullValueTreatment.Ignore:
                    nullCheck = string.Format("AND {0} IS NOT NULL", columnName);
                    break;
                case Entities.NullValueTreatment.InterpretAsError:
                    nullCheck = string.Format("OR {0} IS NULL", columnName);
                    break;
                case Entities.NullValueTreatment.ConvertToDefaultValue:
                    nullCheck = "";
                    break;
                default:
                    throw new NotImplementedException();
            }
            return nullCheck;
        }

        /// <summary>
        /// Returns the case inside the switch-statement if the value is null and returns the correct errorTypeId
        /// </summary>
        protected string GetNullCase()
        {
            int errorTypeNotEntered = GetErrorTypeId("Common", "not entered");
            return NullValueTreatment.HasValue && NullValueTreatment.Value == Entities.NullValueTreatment.InterpretAsError ?
                string.Format("WHEN ({0} IS NULL) THEN {1}",
                    Column.Name, errorTypeNotEntered
                    ) : "WHEN (1=0) THEN -1";
        }

        /// <summary>
        /// Returns the column name or the default value for null-values which should be converted
        /// </summary>
        protected string GetColumnName()
        {
            string columnName = Column.Name;
            if (NullValueTreatment == Entities.NullValueTreatment.ConvertToDefaultValue)
            {
                //TODO: Find out how to get the default-value in SQL
                columnName = string.Format("ISNULL({0},DEFAULT()", columnName);
            }
            return columnName;
        }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return new List<ValidationResult>();
        }

        /// <summary>
        /// Fills the wrongValues-table with all wrong values from the rule
        /// </summary>
        public void Validate(SqlServerValidationToolkitContext ctx)
        {
            var connection = ctx.Database.Connection;

            if (connection.State==System.Data.ConnectionState.Closed)
            {
                connection.Open();
            }

            

            try
            {
                if (!IsActive)
                {
                    SetAllWrongValuesToCorrected(connection);
                }
                else
                {
                    UpdateWrongValues(connection);
                }
                
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Exception occurred while executing '{0}': {1}", CompiledQuery, e.GetBaseException().Message));
            }
            finally
            {
                connection.Close();
            }
        }

        private void UpdateWrongValues(System.Data.Common.DbConnection connection)
        {
            string q = CompiledQuery;
            var c = connection.CreateCommand();
            c.CommandText = q;
            var reader = c.ExecuteReader();


            while (reader.Read())
            {
                //the query returns the id of the invalid value and the errortype-id
                int invalidValueId = reader.GetInt32(0);

                int errorTypeId = reader.GetInt32(1);

                WrongValue existingWrongValue = Validation_WrongValue.SingleOrDefault(wv=>
                    wv.ErrorType_fk==errorTypeId 
                    &&
                    wv.Id == invalidValueId
                    );

                string value = GetValue(invalidValueId, connection);

                if (existingWrongValue==null)
                {
                    WrongValue wrongValue = new WrongValue()
                    {
                        ErrorType_fk = errorTypeId,
                        Id = invalidValueId,
                        Value = value
                    };
                    Validation_WrongValue.Add(wrongValue);
                }
                else
                {
                    existingWrongValue.Value = value;
                }
            }
        }

        private void SetAllWrongValuesToCorrected(System.Data.Common.DbConnection connection)
        {
            foreach (var wrongValue in Validation_WrongValue)
            {
                wrongValue.Is_Corrected = true;
            }
        }


        /// <summary>
        /// Returns the value of the column for the id
        /// </summary>
        private string GetValue(int invalidValueId, System.Data.Common.DbConnection connection)
        {
            string selectValueSqlFormat = "SELECT [{0}] FROM [{1}] WHERE [{2}]={3}";
            string selectValueSql = string.Format(selectValueSqlFormat,
                Column.Name,
                Column.Source.Name,
                Column.Source.IdColumnName,
                invalidValueId);
            var c = connection.CreateCommand();
            c.CommandText = selectValueSql;
            var result = c.ExecuteScalar();
            return result.ToString();
        }
    }
}
