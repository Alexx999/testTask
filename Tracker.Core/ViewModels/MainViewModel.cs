using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using MugenMvvmToolkit;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;
using Tracker.Core.Services;

namespace Tracker.Core.ViewModels
{
    public class MainViewModel : PrintViewModel
    {
        public ICommand NavigateToPrint { get; private set; }

        public MainViewModel()
        {
            NavigateToPrint = new RelayCommand(() => GetViewModel<PrintViewModel>().ShowAsync());
        }

        protected override sealed async Task Init()
        {
            if (IsBusy) return;
            await base.Init();
            Expenses.ForEach(Track);
            EnableChangeTracking();
        }

        private void Track(ExpenseViewModel item)
        {
            item.PropertyChanged += ExpenseViewModelOnPropertyChanged;
        }

        private async void ExpenseViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            var expense = (ExpenseViewModel) sender;
            if(expense.IsPending) return;
            var server = IocContainer.Get<IServerService>();
            if (!expense.IsCreated && expense.IsValid)
            {
                await CreateExpense(expense);
            }
            if (expense.IsDirty && expense.IsCreated && expense.IsValid)
            {
                expense.IsDirty = false;
                expense.IsPending = true;
                await server.UpdateExpense(expense.Model);
                expense.IsPending = false;
            }
        }

        private async Task CreateExpense(ExpenseViewModel expense)
        {
            var server = IocContainer.Get<IServerService>();
            expense.IsDirty = false;
            expense.IsPending = true;
            var model = await server.CreateExpense(expense.Model);
            if (model != null)
            {
                expense.Model.ApplicationUserID = model.ApplicationUserID;
                expense.Model.ExpenseId = model.ExpenseId;
                expense.IsCreated = true;
            }
            else
            {
                expense.IsDirty = true;
            }
            expense.IsPending = false;
        }

        private void Untrack(ExpenseViewModel item)
        {
            item.PropertyChanged -= ExpenseViewModelOnPropertyChanged;
        }

        private void EnableChangeTracking()
        {
            Expenses.CollectionChanged += ExpensesOnCollectionChanged;
        }

        private  async void ExpensesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            var server = IocContainer.Get<IServerService>();
            if (args.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var newItem in args.NewItems)
                {
                    var item = (ExpenseViewModel)newItem;
                    if (item.IsValid)
                    {
                        await CreateExpense(item);
                    }
                    Track(item);
                }
            }
            if (args.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var oldItem in args.OldItems)
                {
                    var item = (ExpenseViewModel) oldItem;
                    await server.RemoveExpense(item.Model.ExpenseId);
                    Untrack(item);
                }
            }
        }

        protected override void OnDispose(bool disposing)
        {
            base.OnDispose(disposing);
            if (disposing)
            {
                Expenses.ForEach(e => e.Dispose());
            }
        }
    }
}
