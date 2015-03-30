using System.Windows.Input;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;

namespace Tracker.Portable.ViewModels
{
    public class LoginViewModel : CloseableViewModel
    {
        private string _userName;
        private string _password;

        public ICommand LoginCommand { get; private set; }
        public ICommand RegisterCommand { get; private set; }

        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(Login);
            LoginCommand = new RelayCommand(Register);
        }

        private void Login()
        {
            var viewModel = GetViewModel<MainViewModel>();
            viewModel.ShowAsync();
        }

        private void Register()
        {
            var viewModel = GetViewModel<RegisterViewModel>();
            viewModel.ShowAsync();
        }

    }
}
