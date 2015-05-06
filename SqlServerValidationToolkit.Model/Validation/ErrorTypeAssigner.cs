using SqlServerValidationToolkit.Model.Context;
using SqlServerValidationToolkit.Model.Entities;
using SqlServerValidationToolkit.Model.Entities.Rule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.Model.Validation
{
    /// <summary>
    /// Assigns error types to the validation rules
    /// </summary>
    public class ErrorTypeAssigner
    {
        private List<ErrorType> _errorTypes = new List<ErrorType>();

        /// <summary>
        /// Loads the errorTypes-list
        /// </summary>
        public void LoadErrorTypes(SqlServerValidationToolkitContext ctx)
        {
            _errorTypes.Clear();
            _errorTypes.AddRange(ctx.Errortypes);
        }

        /// <summary>
        /// Adds the errortypes to the rule with the specified checkType and  the notEntered-errorType
        /// </summary>
        public void AddErrorTypes(ValidationRule rule, string checkType)
        {
            foreach (var et in _errorTypes.Where(et => et.Check_Type == checkType))
            {
                rule.Errortypes.Add(et);
            }
            var notEntered = _errorTypes.Single(et => et.Check_Type == "Common" && et.Description == "not entered");
            rule.Errortypes.Add(notEntered);
        }
        /// <summary>
        /// Adds the errortypes to the rule with it's checkType and  the notEntered-errorType
        /// </summary>
        public void AddErrorTypes(ValidationRule rule)
        {
            string checkType = GetCheckType(rule);
            AddErrorTypes(rule, checkType);
        }

        /// <summary>
        /// Returns the checkType of the rule
        /// </summary>
        private string GetCheckType(ValidationRule rule)
        {
            if (rule is MinMaxRule)
                return "MinMax";
            else if (rule is ComparisonRule)
                return "Comparison";
            else if (rule is LikeRule)
                return "Like";
            else if (rule is CustomQueryRule)
                return "CustomQuery";
            else
                throw new ArgumentException("Unknown type");
        }
    }
}
