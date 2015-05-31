
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
                    int errorTypeIdMin = GetErrorTypeId("MinMax", "Too low");
                    int errorTypeIdMax = GetErrorTypeId("MinMax", "Too high");

                    return GetMinMaxQuery(errorTypeIdMin, errorTypeIdMax);
                }
                if (MinimumDateTime.HasValue || MaximumDateTime.HasValue)
                {
                    int errorTypeIdMin = GetErrorTypeId("MinMax", "Too early");
                    int errorTypeIdMax = GetErrorTypeId("MinMax", "Too late");
                    return GetMinMaxQueryForDates(errorTypeIdMin, errorTypeIdMax);
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// Returns the query for dateTime-values
        /// </summary>
        private string GetMinMaxQueryForDates(int errorTypeIdMin, int errorTypeIdMax)
        {
            string columnName = GetColumnName();
            
            int errorTypeNotEntered = GetErrorTypeId("Common", "not entered");

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

            return GetMinMaxQuery(errorTypeIdMin, errorTypeIdMax, minimumCheck, maximumCheck, minimumHasValue, maximumHasValue);
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
        private string GetMinMaxQuery(int errorTypeIdMin, int errorTypeIdMax)
        {
            string columnName = GetColumnName();

            int errorTypeNotEntered = GetErrorTypeId("Common", "not entered");

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

            return GetMinMaxQuery(errorTypeIdMin, errorTypeIdMax, minimumCheck, maximumCheck, minimumHasValue, maximumHasValue);
        }

        /// <summary>
        /// Returns the query for the minimum- maximum-check
        /// depending if the minimum or maximum-value is set or not
        /// </summary>
        private string GetMinMaxQuery(int errorTypeIdMin, int errorTypeIdMax, string minimumCheck, string maximumCheck, bool minimumHasValue, bool maximumHasValue)
        {

            string nullCheck = GetNullCheck();
            string nullCase = GetNullCase();


            if (minimumHasValue && maximumHasValue)
            {
                string queryFormat = @"SELECT {0},CASE WHEN ({1}) THEN {2} WHEN ({3}) THEN {4} {5} END AS ErrorType_fk
	FROM 
	{6}
	WHERE 
    ({1} OR {3})
	{7}";

                //0: id-column
                //1: min-check
                //2: min-errorId
                //3: max-check
                //4: max-errorId
                //5: null-case
                //6: source
                //7: null-check

                return string.Format(queryFormat,
                    Column.Source.IdColumnName,
                    minimumCheck,
                    errorTypeIdMin,
                    maximumCheck,
                    errorTypeIdMax,
                    nullCase,
                    Column.Source.Name,
                    nullCheck
                    );
            }
            else if (minimumHasValue)
            {

                //only minimum is set
                string queryFormat = @"SELECT {0},CASE WHEN ({1}) THEN {2} {3} END AS ErrorType_fk
	FROM 
	{4}
	WHERE 
    {1}
	{5}";

                //0: id-column
                //1: min-check
                //2: min-errorId
                //3: null-case
                //4: source
                //5: null-check

                return string.Format(queryFormat,
                    Column.Source.IdColumnName,
                    minimumCheck,
                    errorTypeIdMin,
                    nullCase,
                    Column.Source.Name,
                    nullCheck
                    );
            }
            else
            {

                //only maximum is set
                string queryFormat = @"SELECT {0},CASE WHEN ({1}) THEN {2} {3} END AS ErrorType_fk
	FROM 
	{4}
	WHERE 
    {1}
	{5}";

                //0: id-column
                //1: max-check
                //2: max-errorId
                //3: null-case
                //4: source
                //5: null-check

                return string.Format(queryFormat,
                    Column.Source.IdColumnName,
                    maximumCheck,
                    errorTypeIdMax,
                    nullCase,
                    Column.Source.Name,
                    nullCheck
                    );
            }
        }

        public override IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> Validate(System.ComponentModel.DataAnnotations.ValidationContext validationContext)
        {
            List<System.ComponentModel.DataAnnotations.ValidationResult> result = new List<ValidationResult>();

            if (!Minimum.HasValue && !Maximum.HasValue && !MinimumDateTime.HasValue && !MaximumDateTime.HasValue)
            {
                result.Add(new ValidationResult("no Minimum or Maximum value is set"));
            }

            if (Minimum.HasValue && Maximum.HasValue && Minimum > Maximum)
            {
                result.Add(new ValidationResult("The minimum must be smaller than the maximum", new List<string>()
                {
                    "Minimum","Maximum"
                }));
            }

            if (MinimumDateTime.HasValue && MaximumDateTime.HasValue && MinimumDateTime > MaximumDateTime)
            {
                result.Add(new ValidationResult("The minimum must be earlier than the maximum", new List<string>()
                {
                    "Minimum","Maximum"
                }));
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
