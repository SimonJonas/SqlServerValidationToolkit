﻿using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SqlServerValidationToolkit.Configurator.Controls;
using SqlServerValidationToolkit.Configurator.Controls.Sources;
using SqlServerValidationToolkit.Configurator.Controls.UpdateEntities;
using SqlServerValidationToolkit.Configurator.Controls.WrongValues;
using SqlServerValidationToolkit.Configurator.Properties;
using SqlServerValidationToolkit.Model.Context;
using SqlServerValidationToolkit.Model.Entities;
using SqlServerValidationToolkit.Model.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SqlServerValidationToolkit.Configurator
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        public ICommand SaveCommand
        {
            get;
            set;
        }

        public ICommand RefreshCommand
        {
            get;
            set;
        }

        public ICommand ExecuteValidationCommand
        {
            get;
            set;
        }

        public ICommand AddNewComparisonColumnCommand { get; set; }
        public ICommand AddNewLikeColumnCommand { get; set; }
        public ICommand AddNewMinMaxColumnCommand { get; set; }
        public ICommand AddNewCustomQueryCommand { get; set; }

        public ICommand IgnoreSelectedWrongValueCommand { get; set; }
        public ICommand UnIgnoreSelectedWrongValueCommand { get; set; }

        public ICommand UpdateEntitiesCommand { get; set; }


        public ICommand ChangeDbConnectionStringCommand { get; set; }


        Validator _validator;
        SourcesViewViewModel _sourcesViewViewModel;

        public SourcesViewViewModel SourcesViewViewModel
        {
            get
            {
                return _sourcesViewViewModel;
            }
        }

        public MainWindowViewModel()
        {
            _sourcesViewViewModel = new SourcesViewViewModel();

            SaveCommand = new RelayCommand(() => { Save(); }, () => IsSavable);
            RefreshCommand = new RelayCommand(() => Init());

            Func<bool> canExecute = () => _sourcesViewViewModel.SelectedSourceEditViewViewModel != null;

            ExecuteValidationCommand = new RelayCommand(ExecuteValidation);

            IgnoreSelectedWrongValueCommand = new RelayCommand(() => SelectedWrongValue.Ignore = true);
            UnIgnoreSelectedWrongValueCommand = new RelayCommand(() => SelectedWrongValue.Ignore = false);
            UpdateEntitiesCommand = new RelayCommand(() => ShowUpdateEntitesView());

            ChangeDbConnectionStringCommand = new RelayCommand(() => ChangeDbConnectionString());
        }

        private void ChangeDbConnectionString()
        {
            ConnectionStringUpdater.UpdateDbConnectionString(
                () =>
                {
                    //don't show confirmation
                    OnPropertyChanged("Database");
                    OnPropertyChanged("DatabaseServer");
                },
                (ex) =>
                {
                    MessageBox.Show(string.Format("An error occurred during the installation of the validation toolkit: '{0}'" + Environment.NewLine + "The connection was not changed.", ex.Message), "Validation toolkit", MessageBoxButton.OK, MessageBoxImage.Error);

                }
                );

            if (_validator != null)
            {
                _validator.Dispose();
            }

            _validator = new Validator(Settings.Default.DbConnectionString);
            _sourcesViewViewModel.Validator = _validator;
            Init();
        }

        private void ShowUpdateEntitesView()
        {
            var vm = new UpdateEntitiesViewViewModel(this);
            vm.Init();
            Messenger.Default.Send(vm);
        }



        public void Save()
        {
            _sourcesViewViewModel.Save();
        }


        /// <summary>
        /// Requests the new Log-id and calls the stored procedure
        /// </summary>
        private void ExecuteValidation()
        {
            if (_validator.IsSavable)
            {
                var vm = new SaveChangesBeforeValidationMessage();
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(vm);

                switch (vm.HandleUnsavedChanges)
                {
                    case HandleUnsavedChanges.Save:
                        Save();
                        break;
                    case HandleUnsavedChanges.ExecuteWithoutSave:
                        //Nothing is saved
                        break;
                    case HandleUnsavedChanges.Cancel:
                        return;
                }
            }
            _validator.ExecuteValidation();
            ResetWrongValues();
            OnPropertyChanged("WrongValues");
            GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(new ValidationFinishedMessage());

        }

        private void ResetWrongValues()
        {
            WrongValues = _validator.WrongValues.Select(view => new WrongValueViewModel(view)).ToList();
        }

        private IEnumerable<ErrorType> _errorTypes;
        public IEnumerable<ErrorType> ErrorTypes
        {
            get { return _errorTypes; }
            set
            {
                _errorTypes = value;
                OnPropertyChanged("ErrorTypes");
            }
        }

        private IEnumerable<WrongValueViewModel> _wrongValues;
        public IEnumerable<WrongValueViewModel> WrongValues
        {
            get
            {
                return _wrongValues;
            }
            set
            {
                _wrongValues = value;
                OnPropertyChanged("WrongValues");
            }
        }

        public WrongValueViewModel SelectedWrongValue { get; set; }

        public void Init()
        {
            if (_validator == null)
            {
                var conStr = Settings.Default.DbConnectionString;

                if (conStr == string.Empty)
                {
                    ChangeDbConnectionString();
                }
                else
                {
                    _validator = new Validator(conStr);
                    _sourcesViewViewModel.Validator = _validator;
                }

            }
            else
            {
                _validator.Refresh();
            }

            ErrorTypes = _validator.ErrorTypes;
            ResetWrongValues();

            _sourcesViewViewModel.Init();
        }


        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        public string LogIdText { get; set; }

        public bool IsSavable
        {
            get
            {
                if (_validator == null)
                    return false;

                return _validator.IsSavable;
            }
        }

        /// <summary>
        /// Asks the user to save data and optionally saves it
        /// </summary>
        /// <returns>true if the close-event should be cancelled</returns>
        public bool OnClose(Func<MessageBoxResult> askUserToSaveData)
        {
            bool cancel = false;
            if (IsSavable)
            {
                var result = askUserToSaveData();
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        Save();
                        break;
                    case MessageBoxResult.Cancel:
                        cancel = true;
                        break;
                }
            }
            return cancel;
        }

        public string Database
        {
            get
            {
                using (var ctx = new SqlServerValidationToolkitContext(Settings.Default.DbConnectionString))
                {
                    return ctx.Database.Connection.Database;
                }
            }
        }
        public string DatabaseServer
        {
            get
            {
                using (var ctx = new SqlServerValidationToolkitContext(Settings.Default.DbConnectionString))
                {
                    return ctx.Database.Connection.DataSource;
                }
            }
        }
    }
}