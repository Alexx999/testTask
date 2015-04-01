using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
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
            Expenses.ForEach(Track);
            EnableChangeTracking();
            EndBusy(id);
        }

        private void Track(ExpenseViewModel item)
        {
            item.PropertyChanged += ExpenseViewModelOnPropertyChanged;
        }

        private async void ExpenseViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            var expense = (ExpenseViewModel) sender;
            var server = IocContainer.Get<IServerService>();
            if (!expense.IsCreated && expense.IsValid)
            {
                var model = await server.CreateExpense(expense.Model);
                if (model != null)
                {
                    expense.Model = model;
                    expense.IsCreated = true;
                }
            }
            await server.UpdateExpense(expense.Model);
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
                    var item = (ExpenseViewModel) newItem;
                    if (item.IsValid)
                    {
                        var model = await server.CreateExpense(item.Model);
                        if (model != null)
                        {
                            item.Model = model;
                            item.IsCreated = true;
                        }
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
