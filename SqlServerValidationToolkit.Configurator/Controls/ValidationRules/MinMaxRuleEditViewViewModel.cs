using SqlServerValidationToolkit.Model.Entities.Rule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.Configurator.Controls.ValidationRules
{
    public class MinMaxRuleEditViewViewModel : ValidationRuleEditViewViewModel
    {

        public MinMaxRuleEditViewViewModel() : base(null) { }

        private MinMaxRule _rule;
        public MinMaxRuleEditViewViewModel(MinMaxRule r)
            : base(r)
        {
            _rule = r;
        }

        public long? Minimum
        {
            get
            {
                return _rule.Minimum;
            }
            set
            {
                _rule.Minimum = value;
                RaisePropertyChanged(() => Minimum);
                RaisePropertyChanged(() => Maximum);
                RaisePropertyChanged(() => Header);
                RecompileQuery();
            }
        }

        public long? Maximum
        {
            get
            {
                return _rule.Maximum;
            }
            set
            {
                _rule.Maximum = value;
                RaisePropertyChanged(() => Minimum);
                RaisePropertyChanged(() => Maximum);
                RaisePropertyChanged(() => Header);
                RecompileQuery();
            }
        }
    }
}
