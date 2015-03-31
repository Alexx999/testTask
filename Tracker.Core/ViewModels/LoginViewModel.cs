using System.Windows.Input;
using MugenMvvmToolkit;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;
using Tracker.Core.Services;

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

        private async void Login(string password)
        {
            if(IsBusy) return;
            var id = BeginBusy();
            var server = IocContainer.Get<IServerService>();
            if (await server.Login(UserName, password))
            {
                var viewModel = GetViewModel<MainViewModel>();
                viewModel.ShowAsync(NavigationConstants.IsDialog.ToValue(false));
                await CloseAsync(null);
                return;
            }
            var dialogService = IocContainer.Get<IDialogService>();
            await dialogService.ShowMessage("Login failed");
            EndBusy(id);
        }

        private void Register()
        {
            var viewModel = GetViewModel<RegisterViewModel>();
            viewModel.ShowAsync(NavigationConstants.IsDialog.ToValue(false));
            CloseAsync(null);
        }

    }
}
