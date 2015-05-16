using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.Model.Entities.Rule
{
    public class CustomQueryRule : ValidationRule
    {
        [Required]
        public string CustomQuery { get; set; }


        public override string ToString()
        {
            const int maxSubstringLength = 10;
            string query = CustomQuery == null ? "no query" : CustomQuery;
            return string.Format("Custom query: {0}", query.Length > maxSubstringLength ? query.Substring(0, maxSubstringLength) + "..." : query);
        }
        public override string Query
        {
            get
            {
                return this.CustomQuery;
            }
        }
    }
}
