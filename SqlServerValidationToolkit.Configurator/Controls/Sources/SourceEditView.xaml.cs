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

namespace SqlServerValidationToolkit.Configurator.Controls.Sources
{
    /// <summary>
    /// Interaction logic for SourceEditView.xaml
    /// </summary>
    public partial class SourceEditView : UserControl
    {
        public SourceEditView()
        {
            InitializeComponent();
            Loaded += (sender, e) =>
            {
                var s = DataContext as SourceEditViewViewModel;
                if (s.Source.Source_id == 0)
                {
                    txtName.Focus();
                }
            };
        }
    }
}
