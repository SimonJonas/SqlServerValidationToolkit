using GalaSoft.MvvmLight.Messaging;
using SqlServerValidationToolkit.Configurator.Controls;
using SqlServerValidationToolkit.Configurator.Controls.UpdateEntities;
using SqlServerValidationToolkit.Configurator.Messages;
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

namespace SqlServerValidationToolkit.Configurator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MainWindowViewModel viewModel = (MainWindowViewModel)DataContext;

            Loaded += (object sender, RoutedEventArgs e) =>
            {
                viewModel.Init();
                if (!viewModel.SourcesViewViewModel.Sources.Any())
                {
                    //use a task to let the GUI build up while the message box is shown
                    Task t = new Task(() =>
                    {
                       
                        MessageBox.Show("Press Ctrl+U to import the tables that you want to validate. You can also create sources manually");
                    });
                    t.Start();
                }

            };

            Messenger.Default.Register<ValidationErrorMessage>(this, ShowValidationError);
            Messenger.Default.Register<UpdateEntitiesViewViewModel>(this, ShowUpdateEntitesView);
            Messenger.Default.Register<ValidationFinishedMessage>(this, m => MessageBox.Show("Validation finished"));
            Messenger.Default.Register<SaveChangesBeforeValidationMessage>(this, m => AskUserHowToHandleUnsavedChanges(m));
            Messenger.Default.Register<UninstallSuccessfulMessage>(this, m => ShowUninstallSuccessful(m.Exception));
        }

        private void ShowUninstallSuccessful(Exception exception)
        {
            if (exception!=null)
            {
                MessageBox.Show(string.Format("Uninstallation was not successful, the message was '{0}'. The application will stop. Please remove the database objects manually.",exception.Message));
            }
            else
            {
                MessageBox.Show(string.Format("Uninstallation was successful. The application will stop. Restart it to install it again."));
            }
        }



        private void AskUserHowToHandleUnsavedChanges(SaveChangesBeforeValidationMessage m)
        {
            var res = MessageBox.Show("There are unsaved changes. Would you want to save them before executing the validation?", "Unsaved changes", MessageBoxButton.YesNoCancel);
            HandleUnsavedChanges answer;
            switch (res)
            {
                case MessageBoxResult.Yes:
                    answer = HandleUnsavedChanges.Save;
                    break;
                case MessageBoxResult.No:
                    answer = HandleUnsavedChanges.ExecuteWithoutSave;
                    break;
                default:
                    answer = HandleUnsavedChanges.Cancel;
                    break;
            }
            m.HandleUnsavedChanges = answer;
        }

        private void ShowUpdateEntitesView(UpdateEntitiesViewViewModel vm)
        {
            UpdateEntitiesView v = new UpdateEntitiesView()
            {
                DataContext = vm
            };
            Window w = new Window()
            {
                Content = v,
                Owner = this,
                SizeToContent= System.Windows.SizeToContent.WidthAndHeight,
                Title = "Select tables to import"
            };
            vm.OnSourceUpdated = () => w.Close();
            var res = w.ShowDialog();
            if (res.HasValue && res.Value)
            {

            }
        }
        private void ShowWindow(string title, UserControl uc)
        {
            Window w = new Window();
            w.Title = title;
            w.SizeToContent = SizeToContent.WidthAndHeight;
            w.Content = uc;
            w.Show();
        }

        private void ShowValidationError(ValidationErrorMessage m)
        {
            foreach (var eve in m.Exception.EntityValidationErrors)
            {
                string message = string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                    eve.Entry.Entity.GetType().Name, eve.Entry.State);


                Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                    eve.Entry.Entity.GetType().Name, eve.Entry.State);
                string propertyErrors = "";
                foreach (var ve in eve.ValidationErrors)
                {
                    propertyErrors += string.Format("- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage) + Environment.NewLine;
                }

                string messageToShow = message + Environment.NewLine + propertyErrors;
                MessageBox.Show(messageToShow, "Validation Error");
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            var vm = this.DataContext as MainWindowViewModel;

            if (CtrlKeyPressed(e, Key.S))
            {
                if (vm.IsSavable)
                {
                    vm.Save();
                }
            }
            else if (CtrlKeyPressed(e, Key.R))
            {
                vm.Init();
            }
            else if (CtrlKeyPressed(e, Key.U))
            {
                vm.ShowUpdateEntitesView();
            }
            else if (CtrlKeyPressed(e, Key.E))
            {
                vm.ExecuteValidation();
            }
            else if (CtrlKeyPressed(e, Key.D))
            {
                vm.ChangeDbConnectionString();
            }
        }

        private static bool CtrlKeyPressed(KeyEventArgs e, Key key)
        {
            bool ctrlPressed = Keyboard.Modifiers == ModifierKeys.Control;

            bool keyPressed = ctrlPressed && e.Key == key;

            return keyPressed;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var vm = DataContext as MainWindowViewModel;
            bool cancel = vm.OnClose(() => MessageBox.Show(this, "There is unsaved data. Do you want to save the data before closing the application?", "Close?", MessageBoxButton.YesNoCancel));
            e.Cancel = cancel;
        }

    }
}
