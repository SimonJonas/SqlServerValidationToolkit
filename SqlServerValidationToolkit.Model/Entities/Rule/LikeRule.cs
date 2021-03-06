﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.Model.Entities.Rule
{
    public class LikeRule : ValidationRule
    {
        public const string NotLikeErrorTypeCode = "NotLike";
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

                string queryFormat = @"SELECT {0}, CASE {6} ELSE '{1}' END as ErrorType_code 
	FROM 
    {2}
	WHERE 
    {3} NOT LIKE '{4}'
	{5}";

                //0: id column
                //1: error type code
                //2: source-name
                //3: column-name
                //4: like-expression
                //5: null-check
                //6: null-case

                return string.Format(queryFormat,
                    Column.Source.IdColumnName,
                    NotLikeErrorTypeCode,
                    Column.Source.Name,
                    columnName,
                    LikeExpression,
                    nullCheck,
                    GetNullCase());
            }
        }

        public override string ErrorDescriptionFormat
        {
            get
            {
                return string.Format("{0} {1} '{2}'", Column.Name, "{0}", LikeExpression);
            }
        }

    }
}
