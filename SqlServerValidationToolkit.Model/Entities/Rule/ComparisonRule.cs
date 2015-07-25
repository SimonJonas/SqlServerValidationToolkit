using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.Model.Entities.Rule
{
    /// <summary>
    /// Compares two columns
    /// </summary>
    public class ComparisonRule : ValidationRule
    {
        [Required]
        public string ComparedColumn { get; set; }
        public string ComparedColumnDescription { get; set; }

        public override string ErrorDescriptionFormat
        {
            get
            {
                return string.Format("'{0}' {1} '{2}'", Column.Name, "{0}", ComparedColumn);
            }
        }

        [Required]
        public string ComparisonSymbol { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1}", ComparisonSymbol, ComparedColumn);
        }


        public override string Query
        {
            get
            {

                string selectErrorTypeIdFormatString = @"SELECT {0}, CASE {7} ELSE '{1}' END AS ErrorType_fk
FROM {2}
WHERE NOT({3} {4} {5})
{6}";

                string errorTypeCode = GetErrorTypeCode();
                return string.Format(selectErrorTypeIdFormatString,
                    Column.Source.IdColumnName,
                    errorTypeCode,
                    Column.Source.Name,
                    GetColumnName(),
                    ComparisonSymbol,
                    ComparedColumn,
                    GetNullCheck(),
                    GetNullCase()
                    );
            }
        }

        public const string GreaterThanErrorType = "GreaterThan";
        public const string SmallerThanErrorTypeCode = "SmallerThan";
        public const string GreaterThanOrEqualsErrorTypeCode = "GreaterThanOrEquals";
        public const string SmallerThanOrEqualsErrorTypeCode = "SmallerThanOrEquals";

        public const string LaterThanErrorTypeCode = "LaterThan";
        public const string EarlierThanErrorTypeCode = "EarlierThan";
        public const string LaterThanOrEqualsErrorTypeCode = "LaterThanOrEquals";
        public const string EarlierThanOrEqualsErrorTypeCode = "EarlierThanOrEquals";

        public const string NotEqualsErrorTypeCode = "NotEquals";
        public const string EqualsErrorTypeCode = "Equals";
        public const string AtTheSameTimeAsErrorTypeCode = "AtTheSameTimeAs";
        

        /// <summary>
        /// Returns the error type code depending on the comparison symbol
        /// </summary>
        private string GetErrorTypeCode()
        {
            string errorTypeCode;
            string comparisonSymbol = ComparisonSymbol.Trim();

            if (comparisonSymbol == "<")
            {
                if (Column.Type == "datetime")
                {
                    errorTypeCode = LaterThanOrEqualsErrorTypeCode;
                }
                else
                {
                    errorTypeCode = GreaterThanOrEqualsErrorTypeCode;
                }
            }
            else if (comparisonSymbol == "<=")
            {
                if (Column.Type == "datetime")
                {
                    errorTypeCode = LaterThanErrorTypeCode;
                }
                else
                {
                    errorTypeCode = GreaterThanErrorType;
                }
            }
            else if (comparisonSymbol == ">")
            {
                if (Column.Type == "datetime")
                {
                    errorTypeCode = EarlierThanOrEqualsErrorTypeCode;
                }
                else
                {
                    errorTypeCode = SmallerThanOrEqualsErrorTypeCode;
                }
            }
            else if (comparisonSymbol == ">=")
            {
                if (Column.Type == "datetime")
                {
                    errorTypeCode = EarlierThanErrorTypeCode;
                }
                else
                {
                    errorTypeCode = SmallerThanErrorTypeCode;
                }
            }
            else if (comparisonSymbol == "=")
            {
                errorTypeCode = NotEqualsErrorTypeCode;
            }
            else if (comparisonSymbol == "!=")
            {
                errorTypeCode = EqualsErrorTypeCode;
            }
            else
            {
                throw new ArgumentException(string.Format("Unknown comparison symbol: {0}", comparisonSymbol));
            }
            return errorTypeCode;
        }



        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> result = new List<ValidationResult>();
            if (ComparedColumn == string.Empty)
            {
                result.Add(new ValidationResult("Compared column must be set", new List<string>() { "ComparedColumn" }));
            }
            return result;
        }
    }
}
