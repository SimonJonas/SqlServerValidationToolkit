using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SqlServerValidationToolkit.Model.Entities;
using SqlServerValidationToolkit.Model.Entities.Rule;
using SqlServerValidationToolkit.Model.Validation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SqlServerValidationToolkit.Configurator.Controls.Columns;
using System.Data.Entity.Validation;
using SqlServerValidationToolkit.Configurator.Controls.ValidationRules;

namespace SqlServerValidationToolkit.Configurator.Controls.Sources
{
    class SourcesViewViewModel : ViewModelBase
    {
        public SourcesViewViewModel()
        {
            AddNewSourceCommand = new RelayCommand(() =>
            {
                var vm = new SourceEditViewViewModel()
                {
                    Source = new Source()
                };
                Sources.Add(vm);
                SelectedSourceEditViewViewModel = vm;

            });
            AddNewColumnCommand = new RelayCommand(() => AddColumn());
            DeleteSelectedSourcesCommand = new RelayCommand(() => Delete(Sources.Where(s => s.IsSelected)));
            DeletedSelectedColumnsCommand = new RelayCommand(() => Delete(Columns.Where(c => c.IsSelected)));

            DeleteSelectedValidationRulesCommand = new RelayCommand(() => SelectedColumn.DeleteSelectedValidationRules());
            AddMinMaxRuleCommand = new RelayCommand(() => SelectedColumn.AddRule(new MinMaxRule()));
            AddComparisonRuleCommand = new RelayCommand(() => SelectedColumn.AddRule(new ComparisonRule()));
            AddLikeRuleCommand = new RelayCommand(() => SelectedColumn.AddRule(new LikeRule()));
            AddCustomQueryRuleCommand = new RelayCommand(() => SelectedColumn.AddRule(new CustomQueryRule()));
        }




        public Validator Validator
        {
            get
            {
                return _validator;
            }
            set
            {
                _validator = value;
            }
        }

        private void Delete(IEnumerable<SourceEditViewViewModel> sources)
        {
            foreach (var source in sources.ToList())
            {
                Delete(source);
            }
        }

        private void Delete(SourceEditViewViewModel source)
        {
            Sources.Remove(source);
        }

        public void Init()
        {

            Columns = new ObservableCollection<ColumnEditViewViewModel>();
            SelectedSourceEditViewViewModel = null;
            ResetSources();


        }


        /// <summary>
        /// Saves all changed entities
        /// </summary>
        public void Save()
        {
            try
            {
                _validator.Save(Sources.Select(s => s.Source).ToList());

            }
            catch (DbEntityValidationException e)
            {
                Messenger.Default.Send<ValidationErrorMessage>(new ValidationErrorMessage()
                {
                    Exception = e
                });
            }
        }

        private Validator _validator;

        public ICommand AddNewSourceCommand { get; set; }
        public ICommand AddNewColumnCommand { get; set; }
        public ICommand DeletedSelectedColumnsCommand { get; set; }
        public ICommand DeleteSelectedSourcesCommand { get; set; }


        public ICommand DeleteColumnCommand { get; set; }
        public ICommand DeleteSelectedValidationRulesCommand { get; set; }
        public ICommand AddMinMaxRuleCommand { get; set; }
        public ICommand AddComparisonRuleCommand { get; set; }
        public ICommand AddLikeRuleCommand { get; set; }
        public ICommand AddCustomQueryRuleCommand { get; set; }

        private ObservableCollection<SourceEditViewViewModel> _sources;
        public ObservableCollection<SourceEditViewViewModel> Sources
        {
            get
            {
                return _sources;
            }
            set
            {
                _sources = value;
                RaisePropertyChanged(() => Sources);
            }
        }

        /// <summary>
        /// copies the source-collection from the validator
        /// </summary>
        private void ResetSources()
        {
            if (Sources != null)
            {
                Sources.CollectionChanged -= OnSourceCollectionChanged;
                Sources.Clear();
            }
            else
            {
                Sources = new ObservableCollection<SourceEditViewViewModel>();
            }

            foreach (var s in _validator.Sources)
            {
                Sources.Add(new SourceEditViewViewModel()
                {
                    Source = s
                });
            }
            Sources.CollectionChanged += OnSourceCollectionChanged;
            Columns.Clear();
            RaisePropertyChanged(() => this.SelectedColumn);
        }


        /// <summary>
        /// adds and removes the added and removed Sources from the Validator.
        /// </summary>
        private void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var i in e.NewItems)
                {
                    var s = (SourceEditViewViewModel)i;
                    _validator.Add(s.Source);
                }
            }
            if (e.OldItems != null)
            {
                foreach (var i in e.OldItems)
                {
                    var s = (SourceEditViewViewModel)i;

                    foreach (var c in s.Source.Columns.ToList())
                    {
                        _validator.Remove(c);
                    }
                    Sources.Remove(s);
                    _validator.Remove(s.Source);
                }
            }
        }


        private SourceEditViewViewModel _sourceEditViewViewModel;
        public SourceEditViewViewModel SelectedSourceEditViewViewModel
        {
            get
            {
                return _sourceEditViewViewModel;
            }
            set
            {
                _sourceEditViewViewModel = value;
                RaisePropertyChanged(() => SelectedSourceEditViewViewModel);
                RaisePropertyChanged(() => SourceIsSelected);
                
                RefreshColumns(value);

                Messenger.Default.Send(new EntitySelectedMessage(value));
            }
        }


        /// <summary>
        /// Removes the column-list and adds viewModels for the source's columns
        /// </summary>
        /// <param name="source"></param>
        private void RefreshColumns(SourceEditViewViewModel source)
        {
            if (Columns != null)
            {
                Columns.Clear();

                if (source != null)
                {
                    foreach (var column in source.Source.Columns)
                    {
                        Columns.Add(new ColumnEditViewViewModel(this, column, _validator));
                    }
                }
            }
        }


        private void Delete(IEnumerable<ColumnEditViewViewModel> columns)
        {
            foreach (var column in columns.ToList())
            {
                Delete(column);
            }
        }

        public void Delete(ColumnEditViewViewModel vm)
        {
            SelectedSourceEditViewViewModel.Source.Columns.Remove(vm.Column);
            Columns.Remove(vm);
            _validator.Remove(vm.Column);
        }

        private ObservableCollection<ColumnEditViewViewModel> _columns;
        public ObservableCollection<ColumnEditViewViewModel> Columns
        {
            get { return _columns; }
            set
            {
                _columns = value;
                RaisePropertyChanged("Columns");
            }
        }

        private ColumnEditViewViewModel _selectedColumn;
        public ColumnEditViewViewModel SelectedColumn
        {
            get
            {
                return _selectedColumn;
            }
            set
            {
                if (_selectedColumn!=null)
                {
                    _selectedColumn.PropertyChanged -= _selectedColumn_PropertyChanged;
                }
                _selectedColumn = value;
                _selectedColumn.PropertyChanged +=_selectedColumn_PropertyChanged;
                RaisePropertyChanged(() => SelectedColumn);
                RaisePropertyChanged(() => SelectedColumnIsNumeric);
                RaisePropertyChanged(() => SelectedColumnIsNotNumeric);
                RaisePropertyChanged(() => ColumnIsSelected);
                Messenger.Default.Send(new EntitySelectedMessage(value));
            }
        }

        void _selectedColumn_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedValidationRule")
            {
                RaisePropertyChanged(() => ValidationRuleIsSelected);
            }
        }

        /// <summary>
        /// Add and select new column
        /// </summary>
        private void AddColumn()
        {
            var selectedSource = SelectedSourceEditViewViewModel.Source;
            var column = new Column()
            {
                Source = selectedSource
            };
            var vm = new ColumnEditViewViewModel(this, column, _validator);
            Columns.Add(vm);
            selectedSource.Columns.Add(column);
            SelectedColumn = vm;
        }

        public bool SelectedColumnIsNumeric
        {
            get
            {
                return SelectedColumn != null
                    &&
                    SelectedColumn.IsNumeric;
            }
        }
        public bool SelectedColumnIsNotNumeric
        {
            get
            {
                return SelectedColumn != null
                    &&
                    !SelectedColumn.IsNumeric;
            }
        }
        public bool ColumnIsSelected
        {
            get
            {
                return SelectedColumn != null;
            }
        }
        public bool SourceIsSelected
        {
            get
            {
                return SelectedSourceEditViewViewModel != null;
            }
        }
        public bool ValidationRuleIsSelected
        {
            get
            {
                if (ColumnIsSelected)
                {
                    return SelectedColumn.SelectedValidationRule != null;
                }
                return false;
            }
        }
    }
}
