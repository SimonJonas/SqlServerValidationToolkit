using SqlServerValidationToolkit.Model.Context;
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

namespace SqlServerValidationToolkit.Configurator.Controls.ValidationRules
{
    /// <summary>
    /// Interaction logic for ValidationRuleEditView.xaml
    /// </summary>
    public partial class ValidationRuleEditView : UserControl
    {
        public ValidationRuleEditView(UserControl subView)
        {
            InitializeComponent();
            detailPresenter.Content = subView;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as ValidationRuleEditViewViewModel;
            if (vm != null)
            {
                ShowQuery(vm);
            }
        }

        private void ShowQuery(ValidationRuleEditViewViewModel vm)
        {
            using (var ctx = SqlServerValidationToolkitContext.Create())
            {
                string query = vm.Rule.Query;
                MessageBox.Show(query);
            }
        }

        private void DataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            var vm = DataContext as ValidationRuleEditViewViewModel;
            vm.NotifyErrorTypeChanged();
        }
    }
}
