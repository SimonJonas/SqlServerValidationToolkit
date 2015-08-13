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
                RaisePropertyChanged(() => NoMinimum);
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
                RaisePropertyChanged(() => NoMaximum);
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
                RaisePropertyChanged(() => MinimumDateTime);
                RaisePropertyChanged(() => MaximumDateTime);
                RaisePropertyChanged(() => NoMinimum);
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
                RaisePropertyChanged(() => MinimumDateTime);
                RaisePropertyChanged(() => MaximumDateTime);
                RaisePropertyChanged(() => NoMaximum);
                RaisePropertyChanged(() => Header);
                RecompileQuery();
            }
        }

        public bool NoMaximum
        {
            get
            {
                if (IsNumeric)
                {
                    return _rule.Maximum == null;
                } else
                {
                    return _rule.MaximumDateTime == null;
                }
            }
            set
            {
                if (IsNumeric)
                {

                    long? newValue = value ? (long?)null : 0;
                    Maximum = newValue;
                }
                else
                {
                    DateTime? newValueDateTime = value ? (DateTime?)null : DateTime.Today;
                    MaximumDateTime = newValueDateTime;
                }

            }
        }
        public bool NoMinimum
        {
            get
            {
                if (IsNumeric)
                {
                    return _rule.Minimum == null;
                }
                else
                {
                    return _rule.MinimumDateTime == null;
                }
            }
            set
            {
                if (IsNumeric)
                {

                    long? newValue = value ? (long?)null : 0;
                    Minimum = newValue;
                }
                else
                {
                    DateTime? newValueDateTime = value ? (DateTime?)null : DateTime.Today;
                    MinimumDateTime = newValueDateTime;
                }

            }
        }
    }
}