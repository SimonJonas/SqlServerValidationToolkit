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
                return string.Format("'{0}' {1} '{2}'", ComparedColumn, "{0}", Column.Name);
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

                string selectErrorTypeIdFormatString = @"SELECT {0}, CASE {7} ELSE {1} END AS ErrorType_fk
FROM {2}
WHERE NOT({3} {4} {5})
{6}";

                int errorTypeId;


                errorTypeId = GetErrorTypeId();

                return string.Format(selectErrorTypeIdFormatString,
                    Column.Source.IdColumnName,
                    errorTypeId,
                    Column.Source.Name,
                    GetColumnName(),
                    ComparisonSymbol,
                    ComparedColumn,
                    GetNullCheck(),
                    GetNullCase()
                    );
            }
        }


        /// <summary>
        /// Returns the error type id depending on the comparison symbol
        /// </summary>
        private int GetErrorTypeId()
        {
            int errorTypeId;
            string comparisonSymbol = ComparisonSymbol.Trim();

            if (comparisonSymbol == "<" || comparisonSymbol == "<=")
            {
                if (Column.Type == "datetime")
                {
                    errorTypeId = GetLaterThanErrorTypeId();
                }
                else
                {
                    errorTypeId = GetGreaterThanErrorTypeId();
                }
            }
            else if (comparisonSymbol == ">" || comparisonSymbol == ">=")
            {
                if (Column.Type == "datetime")
                {
                    errorTypeId = GetEarlierThanErrorTypeId();
                }
                else
                {
                    errorTypeId = GetSmallerThanErrorTypeId();
                }
            }
            else if (comparisonSymbol == "=")
            {
                errorTypeId = GetNotEqualsErrorTypeId();
            }
            else if (comparisonSymbol == "!=")
            {
                errorTypeId = GetEqualsErrorTypeId();
            }
            else
            {
                throw new ArgumentException(string.Format("Unknown comparison symbol: {0}", comparisonSymbol));
            }
            return errorTypeId;
        }

        private new int GetErrorTypeId(string description)
        {
            return base.GetErrorTypeId("Comparison", description);
        }

        private int GetGreaterThanErrorTypeId()
        {
            return GetErrorTypeId("greater than");
        }
        private int GetSmallerThanErrorTypeId()
        {
            return GetErrorTypeId("smaller than");
        }
        private int GetLaterThanErrorTypeId()
        {
            return GetErrorTypeId("later than");
        }
        private int GetEarlierThanErrorTypeId()
        {
            return GetErrorTypeId("earlier than");
        }
        private int GetNotEqualsErrorTypeId()
        {
            return GetErrorTypeId("not equals");
        }
        private int GetEqualsErrorTypeId()
        {
            return GetErrorTypeId("equals");
        }
        private int GetAtTheSameTimeAsErrorTypeId()
        {
            return GetErrorTypeId("at the same time as");
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
