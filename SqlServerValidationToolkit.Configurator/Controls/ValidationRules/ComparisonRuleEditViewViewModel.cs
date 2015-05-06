using SqlServerValidationToolkit.Model.Entities.Rule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.Configurator.Controls.ValidationRules
{
    public class ComparisonRuleEditViewViewModel : ValidationRuleEditViewViewModel
    {
        public ComparisonRuleEditViewViewModel() : base(null) { }

        private ComparisonRule _rule;

        public ComparisonRuleEditViewViewModel(ComparisonRule comparisonRule)
            : base(comparisonRule)
        {
            _rule = comparisonRule;
        }

        public string ComparedColumn
        {
            get { return _rule.ComparedColumn; }
            set
            {
                _rule.ComparedColumn = value;
                RaisePropertyChanged(() => ComparedColumn);
                RaisePropertyChanged(() => Header);
                RecompileQuery();
            }
        }

        public string ComparisonSymbol
        {
            get { return _rule.ComparisonSymbol; }
            set
            {
                _rule.ComparisonSymbol = value;
                RaisePropertyChanged(() => ComparisonSymbol);
                RaisePropertyChanged(() => Header);
                RecompileQuery();
            }
        }


    }
}
