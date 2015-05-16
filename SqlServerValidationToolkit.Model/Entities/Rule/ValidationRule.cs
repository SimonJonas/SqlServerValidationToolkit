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
    }
}
