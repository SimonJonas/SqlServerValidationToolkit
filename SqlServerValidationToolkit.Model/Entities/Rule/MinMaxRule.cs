
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.Model.Entities.Rule
{
    public class MinMaxRule : ValidationRule
    {
        [Required]
        public Nullable<long> Minimum { get; set; }
        [Required]
        public Nullable<long> Maximum { get; set; }


        public override string Query
        {
            get
            {
                int errorTypeIdMin = GetErrorTypeId("MinMax", "Too low");
                int errorTypeIdMax = GetErrorTypeId("MinMax", "Too high");

                return GetMinMaxQuery(errorTypeIdMin, errorTypeIdMax);
            }
        }

        private string GetMinMaxQuery(int errorTypeIdMin, int errorTypeIdMax)
        {
            string columnName = GetColumnName();


            string nullCheck = GetNullCheck();
            string nullCase = GetNullCase();


            if (Column.Type == "int_from_string")
            {
                columnName = string.Format("CONVERT(int,{0})", columnName);
            }
            if (Column.Type == "numeric_from_string")
            {
                columnName = string.Format("CONVERT(numeric(12,2),{0})", columnName);
            }
            string queryFormat = @"SELECT {0},CASE WHEN ({2} < {3}) THEN {7} WHEN ({4} < {5}) THEN {8} {9} END AS ErrorType_fk
	FROM 
	{1}
	WHERE 
    ({2} < {3} OR {4} < {5})
	{6}";

            int errorTypeNotEntered = GetErrorTypeId("Common", "not entered");
            return string.Format(queryFormat, Column.Source.IdColumnName,
                Column.Source.Name,
                columnName,
                Minimum,
                Maximum,
                columnName,
                nullCheck,
                errorTypeIdMin,
                errorTypeIdMax,
                nullCase
                );
        }

        public override IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> Validate(System.ComponentModel.DataAnnotations.ValidationContext validationContext)
        {
            List<System.ComponentModel.DataAnnotations.ValidationResult> result = new List<ValidationResult>();
            
            if (Minimum > Maximum)
            {
                result.Add(new ValidationResult("The minimum must be smaller than the maximum", new List<string>()
                {
                    "Minimum","Maximum"
                }));
            }
            return result;
        }
        public override string ToString()
        {
            return string.Format("Min: {0} Max: {1}", Minimum, Maximum);
        }
    }
}
