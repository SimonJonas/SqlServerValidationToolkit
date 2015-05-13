using SqlServerValidationToolkit.Model.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.Configurator.Controls.WrongValues
{
    class WrongValueViewModel : INotifyPropertyChanged
    {
        public WrongValueViewModel(WrongValue wrongValue)
        {
            _wrongValue = wrongValue;
        }

        public WrongValue WrongValue
        {
            get
            {
                return _wrongValue;
            }
            set
            {
                _wrongValue = value;
            }
        }
        private WrongValue _wrongValue;

        public bool Ignore
        {
            get
            {
                return _wrongValue.Ignore;
            }
            set
            {
                //change the real-table not the view
                _wrongValue.Ignore = value;
                OnPropertyChanged("Ignore");
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
