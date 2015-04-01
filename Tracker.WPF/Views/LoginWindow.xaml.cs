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
using System.Windows.Shapes;
using MugenMvvmToolkit.Models;
using Tracker.Core.ViewModels;

namespace Tracker.WPF.Views
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            PasswordBox.InputBindings.Add(new KeyBinding(new RelayCommand(Login), Key.Enter, ModifierKeys.None));
        }

        private void Login()
        {
            var vm = DataContext as LoginViewModel;
            vm.LoginCommand.Execute(PasswordBox.Password);
        }

        private void LoginClick(object sender, RoutedEventArgs e)
        {
            Login();
        }
    }
}
