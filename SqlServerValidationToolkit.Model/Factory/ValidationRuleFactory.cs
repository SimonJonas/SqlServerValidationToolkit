using SqlServerValidationToolkit.Model.Context;
using SqlServerValidationToolkit.Model.Entities;
using SqlServerValidationToolkit.Model.Entities.Rule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.Model.Factory
{
    public class ValidationRuleFactory
    {
        public ValidationRuleFactory()
        {
            Assigner = new ErrorTypeAssigner();
        }

        private ErrorTypeAssigner Assigner { get; set; }

        public void LoadErrorTypes(SqlServerValidationToolkitContext ctx)
        {
            Assigner.LoadErrorTypes(ctx);

        }


        public MinMaxRule CreateMinMaxRule(long min, long max, NullValueTreatment nullValueTreatment = NullValueTreatment.Ignore)
        {
            var minMaxRule = new MinMaxRule()
            {
                Minimum = min,
                Maximum = max,
                NullValueTreatment = nullValueTreatment,
                IsActive = true,
            };

            Assigner.AddErrorTypes(minMaxRule);
            return minMaxRule;
        }


        public ComparisonRule CreateComparisonRule(string comparisonSymbol, string comparisonColumn, string comparedColumnDescription)
        {
            var r = new ComparisonRule()
            {
                ComparedColumn = comparisonColumn,
                ComparedColumnDescription = comparedColumnDescription,
                ComparisonSymbol = comparisonSymbol,
                IsActive = true,
            };
            SetNullValueTreatment(r);
            Assigner.AddErrorTypes(r);
            return r;
        }

        public LikeRule CreateLikeRule(string likeExpression)
        {
            var r = new LikeRule()
            {
                LikeExpression = likeExpression,
                IsActive = true,
            };
            SetNullValueTreatment(r);
            Assigner.AddErrorTypes(r);
            return r;
        }



        public static CustomQueryRule CreateCustomQueryRule(string customQuery)
        {
            var r = new CustomQueryRule()
            {
                //Discriminator = "CustomQuery",
                CustomQuery = customQuery,
                IsActive = true,
            };
            return r;
        }
        private static void SetNullValueTreatment(ValidationRule rule)
        {
            rule.NullValueTreatment = NullValueTreatment.Ignore;
        }
    }
}
