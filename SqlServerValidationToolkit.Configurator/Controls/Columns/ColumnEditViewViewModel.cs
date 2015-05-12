using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight.Messaging;
using SqlServerValidationToolkit.Model.Entities;
using SqlServerValidationToolkit.Model.Entities.Rule;
using SqlServerValidationToolkit.Model.Validation;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlServerValidationToolkit.Configurator.Controls.Sources;
using SqlServerValidationToolkit.Configurator.Controls.ValidationRules;

namespace SqlServerValidationToolkit.Configurator.Controls.Columns
{
    class ColumnEditViewViewModel : SelectableViewModel
    {
        Validator _validator;

        public ColumnEditViewViewModel()
        { }

        public ColumnEditViewViewModel(SourcesViewViewModel sourcesViewViewModel, Column column, Validator validator)
        {
            Column = column;
            _validator = validator;


            ValidationRules = new ObservableCollection<ValidationRuleEditViewViewModel>();
            foreach (var r in column.ValidationRules)
            {
                ValidationRuleEditViewViewModel vm = GetViewModel(r);

                ValidationRules.Add(vm);
            }
        }

        private static ValidationRuleEditViewViewModel GetViewModel(ValidationRule r)
        {
            ValidationRuleEditViewViewModel vm;
            if (r is MinMaxRule)
            {
                vm = new MinMaxRuleEditViewViewModel((MinMaxRule)r);
            }
            else if (r is ComparisonRule)
            {
                vm = new ComparisonRuleEditViewViewModel((ComparisonRule)r);
            }
            else if (r is CustomQueryRule)
            {
                vm = new CustomQueryRuleEditViewViewModel((CustomQueryRule)r);
            }
            else if (r is LikeRule)
            {
                vm = new LikeRuleEditViewViewModel((LikeRule)r);
            }
            else
            {
                throw new ArgumentException("unknown rule");
            }
            return vm;
        }

        /// <summary>
        /// Adds the rule to the model and sets the default values
        /// </summary>
        /// <param name="r"></param>
        public void AddRule(ValidationRule r)
        {
            r.Column = this.Column;
            Column.ValidationRules.Add(r);
            _validator.AssignErrorTypes(r);
            r.NullValueTreatment = NullValueTreatment.Ignore;
            AddEditViewModel(r);

        }

        /// <summary>
        /// Adds the EditViewModel to the list
        /// </summary>
        private void AddEditViewModel(ValidationRule r)
        {
            ValidationRuleEditViewViewModel vm;
            if (r is MinMaxRule)
            {
                vm = new MinMaxRuleEditViewViewModel((MinMaxRule)r);
            }
            else if (r is ComparisonRule)
            {
                vm = new ComparisonRuleEditViewViewModel((ComparisonRule)r);
            }
            else if (r is LikeRule)
            {
                vm = new LikeRuleEditViewViewModel((LikeRule)r);
            }
            else if (r is CustomQueryRule)
            {
                vm = new CustomQueryRuleEditViewViewModel((CustomQueryRule)r);
            }
            else
            {
                throw new ArgumentException("Unknown type");
            }
            ValidationRules.Add(vm);
            SelectedValidationRule = vm;
        }


        public void DeleteSelectedValidationRules()
        {
            ValidationRules.Where(r => r.IsSelected).ToList().ForEach(ruleVm =>
            {
                _validator.Remove(ruleVm.Rule);
                ValidationRules.Remove(ruleVm);
            });
        }


        public Column Column { get; set; }


        public string ColumnTypeTrimmed
        {
            get
            {
                if (Column == null)
                    return null;
                if (Column.Type == null)
                    return null;

                return Column.Type.Trim();
            }
            set
            {
                if (Column == null)
                {
                    throw new ArgumentException("Column is not set");
                }
                Column.Type = value;
                RaisePropertyChanged(() => IsNumeric);
                RaisePropertyChanged(() => IsString);
            }
        }

        public bool IsNumeric
        {
            get
            {
                if (Column == null)
                    return false;
                //"string",
                //"int",
                //"numeric",
                //"int_from_string",
                //"numeric_from_string",
                //"datetime"
                string[] numericTypes = new string[]{
                    "int",
                    "numeric",
                    "int_from_string",
                    "numeric_from_string",
                };
                return (numericTypes.Contains(Column.Type));


            }
        }

        public bool IsString
        {
            get
            {
                return !IsNumeric;
            }
        }

        public ObservableCollection<ValidationRuleEditViewViewModel> ValidationRules { get; set; }

        private ValidationRuleEditViewViewModel _selectedValidationRule;
        public ValidationRuleEditViewViewModel SelectedValidationRule
        {
            get
            {
                return _selectedValidationRule;
            }
            set
            {
                _selectedValidationRule = value;
                RaisePropertyChanged(() => SelectedValidationRule);
                RaisePropertyChanged(() => ValidationRuleIsSelected);

                _selectedValidationRule.PropertyChanged += _selectedValidationRule_PropertyChanged;
                Messenger.Default.Send(new EntitySelectedMessage(value));
            }
        }

        public bool ValidationRuleIsSelected
        {
            get
            {
                return SelectedValidationRule != null;
            }
        }

        void _selectedValidationRule_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //ValidationRuleIsSelected
            //e.PropertyName == "ValidationRuleIsSelected"
        }

    }
}
