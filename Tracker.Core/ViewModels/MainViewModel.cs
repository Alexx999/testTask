using System.Collections.ObjectModel;
using System.Linq;
using MugenMvvmToolkit;
using MugenMvvmToolkit.ViewModels;
using Tracker.Core.Services;
using Tracker.Models;
using Tracker.Models.Account;

namespace Tracker.Core.ViewModels
{
    public class MainViewModel : CloseableViewModel
    {
        public ObservableCollection<ExpenseViewModel> Expenses { get; private set; }

        public MainViewModel()
        {
            Expenses = new ObservableCollection<ExpenseViewModel>();
            Init();
        }

        private async void Init()
        {
            if (IsBusy) return;
            var id = BeginBusy();
            var server = IocContainer.Get<IServerService>();
            var expenses = await server.GetExpenses();
            Expenses.AddRange(expenses.Select(e => new ExpenseViewModel(e)));
            EndBusy(id);
        }
    }
}
