using System.Windows.Input;
using MugenMvvmToolkit;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;
using Tracker.Core.Services;
using Tracker.Models.Account;

namespace Tracker.Core.ViewModels
{
    public class RegisterViewModel : CloseableViewModel
    {
        private string _name;
        private string _email;

        public ICommand RegisterCommand { get; private set; }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public string Email
        {
            get { return _email; }
            set
            {
                _email = value;
                OnPropertyChanged();
            }
        }

        public RegisterViewModel()
        {
            RegisterCommand = new RelayCommand<string>(Register);
            Closing += (sender, args) =>
            {
                var viewModel = GetViewModel<LoginViewModel>();
                viewModel.ShowAsync(NavigationConstants.IsDialog.ToValue(false));
            };
        }

        private async void Register(string password)
        {
            if (IsBusy) return;
            var id = BeginBusy();
            var server = IocContainer.Get<IServerService>();
            if (await server.Register(new RegisterModel{Email = Email, Name = Name, Password = password}))
            {
                await CloseAsync(null);
                return;
            }
            var dialogService = IocContainer.Get<IDialogService>();
            await dialogService.ShowMessage("Registration failed");
            EndBusy(id);
        }
    }
}
