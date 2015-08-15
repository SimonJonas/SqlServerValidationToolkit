
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.Model.Entities.Rule
{
    public class MinMaxRule : ValidationRule, IValidatableObject
    {
        public const string TooLowErrorTypeCode = "TooLow";
        public const string TooHighErrorTypeCode = "TooHigh";
        public const string TooEarlyErrorTypeCode = "TooEarly";
        public const string TooLateErrorTypeCode = "TooLate";

        public Nullable<long> Minimum { get; set; }
        public Nullable<long> Maximum { get; set; }

        public Nullable<DateTime> MinimumDateTime { get; set; }
        public Nullable<DateTime> MaximumDateTime { get; set; }


        public override string Query
        {
            get
            {
                if (Minimum.HasValue || Maximum.HasValue)
                {
                    return GetMinMaxQuery();
                }
                if (MinimumDateTime.HasValue || MaximumDateTime.HasValue)
                {
                    return GetMinMaxQueryForDates();
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// Returns the query for dateTime-values
        /// </summary>
        private string GetMinMaxQueryForDates()
        {
            string columnName = GetColumnName();

            string minimumCheck = string.Format("{0} < '{1}'",
                    columnName,
                    ConvertToSqlFormat(MinimumDateTime)
                    );

            string maximumCheck = string.Format("'{0}' < {1}",
                    ConvertToSqlFormat(MaximumDateTime),
                    columnName
                );

            bool minimumHasValue = MinimumDateTime.HasValue;
            bool maximumHasValue = MaximumDateTime.HasValue;

            return GetMinMaxQuery(minimumCheck, maximumCheck, minimumHasValue, maximumHasValue);
        }

        /// <summary>
        /// Returns the datetime in sql-convertible format
        /// </summary>
        private string ConvertToSqlFormat(DateTime? d)
        {
            return d.HasValue ? d.Value.ToString("yyyy-MM-dd HH:mm:ss") : string.Empty;
        }

        /// <summary>
        /// Returns the query for the int-values
        /// </summary>
        private string GetMinMaxQuery()
        {
            string columnName = GetColumnName();

            string minimumCheck = string.Format("{0} < {1}",
                    columnName,
                    Minimum
                    );

            string maximumCheck = string.Format("{0} < {1}",
                    Maximum,
                    columnName
                );

            bool minimumHasValue = Minimum.HasValue;
            bool maximumHasValue = Maximum.HasValue;

            return GetMinMaxQuery(minimumCheck, maximumCheck, minimumHasValue, maximumHasValue);
        }

        /// <summary>
        /// Returns the query for the minimum- maximum-check
        /// depending if the minimum or maximum-value is set or not
        /// </summary>
        private string GetMinMaxQuery(string minimumCheck, string maximumCheck, bool minimumHasValue, bool maximumHasValue, bool isDateQuery=false)
        {

            string nullCheck = GetNullCheck();
            string nullCase = GetNullCase();

            string minErrorCode = isDateQuery ? TooEarlyErrorTypeCode : TooLowErrorTypeCode;
            string maxErrorCode = isDateQuery ? TooLateErrorTypeCode : TooHighErrorTypeCode;

            if (minimumHasValue && maximumHasValue)
            {
                string queryFormat = @"SELECT {0},CASE WHEN ({1}) THEN '{2}' WHEN ({3}) THEN '{4}' {5} END AS ErrorType_code
	FROM 
	{6}
	WHERE 
    ({1} OR {3})
	{7}";

                //0: id-column
                //1: min-check
                //2: min-error code
                //3: max-check
                //4: max-error code
                //5: null-case
                //6: source
                //7: null-check

                return string.Format(queryFormat,
                    Column.Source.IdColumnName,
                    minimumCheck,
                    minErrorCode,
                    maximumCheck,
                    maxErrorCode,
                    nullCase,
                    Column.Source.Name,
                    nullCheck
                    );
            }
            else if (minimumHasValue)
            {

                //only minimum is set
                string queryFormat = @"SELECT {0},CASE WHEN ({1}) THEN '{2}' {3} END AS ErrorType_code
	FROM 
	{4}
	WHERE 
    {1}
	{5}";

                //0: id-column
                //1: min-check
                //2: min-error code
                //3: null-case
                //4: source
                //5: null-check

                return string.Format(queryFormat,
                    Column.Source.IdColumnName,
                    minimumCheck,
                    minErrorCode,
                    nullCase,
                    Column.Source.Name,
                    nullCheck
                    );
            }
            else
            {

                //only maximum is set
                string queryFormat = @"SELECT {0},CASE WHEN ({1}) THEN '{2}' {3} END AS ErrorType_code
	FROM 
	{4}
	WHERE 
    {1}
	{5}";

                //0: id-column
                //1: max-check
                //2: max-error code
                //3: null-case
                //4: source
                //5: null-check

                return string.Format(queryFormat,
                    Column.Source.IdColumnName,
                    maximumCheck,
                    maxErrorCode,
                    nullCase,
                    Column.Source.Name,
                    nullCheck
                    );
            }
        }

        public override IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> Validate(System.ComponentModel.DataAnnotations.ValidationContext validationContext)
        {
            List<System.ComponentModel.DataAnnotations.ValidationResult> result = new List<ValidationResult>();

            var minMaxList = new List<string>()
                {
                    "Minimum","Maximum"
                };
            var minMaxDateTimeList = new List<string>()
                {
                    "MinimumDateTime","MaximumDateTime"
                };

            if (Column.IsNumeric && !Minimum.HasValue && !Maximum.HasValue)
            {
                result.Add(new ValidationResult("no Minimum or Maximum value is set", minMaxList));
            }
            if (Column.IsDateTime && !MinimumDateTime.HasValue && !MaximumDateTime.HasValue)
            {
                result.Add(new ValidationResult("no Minimum or Maximum value is set", minMaxDateTimeList));
            }


            if (Minimum.HasValue && Maximum.HasValue && Minimum > Maximum)
            {
                result.Add(new ValidationResult("The minimum must be smaller than the maximum", minMaxList));
            }

            if (MinimumDateTime.HasValue && MaximumDateTime.HasValue && MinimumDateTime > MaximumDateTime)
            {
                result.Add(new ValidationResult("The minimum must be earlier than the maximum", minMaxDateTimeList));
            }
            return result;
        }
        public override string ToString()
        {
            string minimum;
            if (Minimum.HasValue)
            {
                minimum = Minimum.ToString();
            } else if (MinimumDateTime.HasValue)
            {
                minimum = MinimumDateTime.ToString();
            }
            else
            {
                minimum = "-";
            }

            string maximum;
            if (Maximum.HasValue)
            {
                maximum = Maximum.ToString();
            } else if (MaximumDateTime.HasValue)
            {
                maximum = MaximumDateTime.ToString();
            }
            else
            {
                maximum = "-";
            }
            return string.Format("Min: {0} Max: {1}", minimum, maximum);
        }
    }
}
