using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.Model.Entities.Rule
{
    public class LikeRule : ValidationRule
    {
        [Required]
        public string LikeExpression { get; set; }

        public override string ToString()
        {
            return string.Format("Like '{0}'", this.LikeExpression);
        }

        public override string Query
        {
            get
            {
                string columnName = GetColumnName();

                string nullCheck = GetNullCheck();

                string queryFormat = @"SELECT {0}, CASE {6} ELSE {1} END as ErrorType_fk 
	FROM 
    {2}
	WHERE 
    {3} NOT LIKE '{4}'
	{5}";

                return string.Format(queryFormat,
                    Column.Source.IdColumnName,
                    GetErrorTypeId(),
                    Column.Source.Name,
                    columnName,
                    LikeExpression,
                    nullCheck,
                    GetNullCase());
            }
        }


        private int GetErrorTypeId()
        {
            return base.GetErrorTypeId("Like");
        }

    }
}
