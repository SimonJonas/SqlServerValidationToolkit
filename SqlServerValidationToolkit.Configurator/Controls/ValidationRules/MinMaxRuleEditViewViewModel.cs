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

        public bool IsNumeric
        {
            get
            {
                return _rule.Column.IsNumeric;
            }
        }
        public bool IsDateTime
        {
            get
            {
                //TODO: Fix
                return !_rule.Column.IsNumeric;
            }
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

        public DateTime? MinimumDateTime
        {
            get
            {
                return _rule.MinimumDateTime;
            }
            set
            {
                _rule.MinimumDateTime = value;
                RaisePropertyChanged(() => Minimum);
                RaisePropertyChanged(() => Maximum);
                RaisePropertyChanged(() => Header);
                RecompileQuery();
            }
        }

        public DateTime? MaximumDateTime
        {
            get
            {
                return _rule.MaximumDateTime;
            }
            set
            {
                _rule.MaximumDateTime = value;
                RaisePropertyChanged(() => Minimum);
                RaisePropertyChanged(() => Maximum);
                RaisePropertyChanged(() => Header);
                RecompileQuery();
            }
        }
    }
}
