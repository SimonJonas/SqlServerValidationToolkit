using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SqlServerValidationToolkit.Configurator.Controls.Columns
{
    /// <summary>
    /// Interaction logic for ColumnEditView.xaml
    /// </summary>
    public partial class ColumnEditView : UserControl
    {
        public ColumnEditView()
        {
            InitializeComponent();
            Loaded += (sender, e) =>
            {
                var s = DataContext as ColumnEditViewViewModel;
                if (s.Column.Column_id == 0)
                {
                    txtName.Focus();
                }
            };
        }
    }
}
