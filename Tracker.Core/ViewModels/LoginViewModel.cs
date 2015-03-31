using System.Windows.Input;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;

namespace Tracker.Core.ViewModels
{
    public class LoginViewModel : CloseableViewModel
    {
        private string _userName;

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

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand<string>(Login);
            RegisterCommand = new RelayCommand(Register);
        }

        private void Login(string password)
        {
            var viewModel = GetViewModel<MainViewModel>();
            viewModel.ShowAsync(NavigationConstants.IsDialog.ToValue(false));
            CloseAsync(null);
        }

        private void Register()
        {
            var viewModel = GetViewModel<RegisterViewModel>();
            viewModel.ShowAsync(NavigationConstants.IsDialog.ToValue(false));
            CloseAsync(null);
        }

    }
}
