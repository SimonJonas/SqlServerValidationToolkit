using GalaSoft.MvvmLight.Messaging;
using SqlServerValidationToolkit.Configurator.Controls.Columns;
using SqlServerValidationToolkit.Configurator.Controls.ValidationRules;
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
    /// Interaction logic for SourcesView.xaml
    /// </summary>
    public partial class SourcesView : UserControl
    {
        public SourcesView()
        {
            InitializeComponent();



            Messenger.Default.Register<EntitySelectedMessage>(this, ShowEntityEditView);
        }
        /// <summary>
        /// When the ListBoxItem is focussed, the view sends a selected-message
        /// </summary>
        public void OnGotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is ListBoxItem)
            {
                var lbi = sender as ListBoxItem;

                Messenger.Default.Send(new EntitySelectedMessage(lbi.DataContext));
            }
        }


        private void ShowEntityEditView(EntitySelectedMessage m)
        {
            var entity = m.SelectedEntity;

            //the entity is a viewModel, depending on the viewModel-type the corresponding view is created

            //this.selectedEntityEditViewContainer.DataContext = entity;
            UserControl uc;
            if (entity == null)
            {
                uc = null;
            }
            else if (entity is SourceEditViewViewModel)
            {
                uc = new SourceEditView();
            }
            else if (entity is ColumnEditViewViewModel)
            {
                uc = new ColumnEditView();
            }
            else if (entity is MinMaxRuleEditViewViewModel)
            {
                uc = new ValidationRuleEditView(
                        new MinMaxRuleEditView()
                    );
            }
            else if (entity is CustomQueryRuleEditViewViewModel)
            {
                uc = new ValidationRuleEditView(
                        new CustomQueryRuleEditView()
                    );
            }
            else if (entity is ComparisonRuleEditViewViewModel)
            {
                uc = new ValidationRuleEditView(
                        new ComparisonRuleEditView()
                    );
            }
            else if (entity is LikeRuleEditViewViewModel)
            {
                uc = new ValidationRuleEditView(
                        new LikeRuleEditView()
                    );
            }
            else
            {
                throw new ArgumentException("unknown class " + entity.GetType());
            }
            if (uc != null)
            {
                uc.DataContext = entity;
            }
            selectedEntityEditViewContainer.Content = uc;
        }
    }
}
