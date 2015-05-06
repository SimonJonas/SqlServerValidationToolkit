using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.Configurator.Controls
{

    /// <summary>
    /// An item that can be displayed in a list. It can be selected and be marked.
    /// </summary>
    public class SelectableViewModel : ViewModelBase
    {
        private bool _isSelected;
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                RaisePropertyChanged(() => IsSelected);
            }
        }

        private bool _isMarked;
        public bool IsMarked
        {
            get
            {
                return _isMarked;
            }
            set
            {
                _isMarked = value;
                RaisePropertyChanged(() => IsMarked);
            }
        }
    }
}
