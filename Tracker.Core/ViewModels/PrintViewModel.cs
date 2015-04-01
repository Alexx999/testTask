using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MugenMvvmToolkit;
using MugenMvvmToolkit.ViewModels;
using Tracker.Core.Services;

namespace Tracker.Core.ViewModels
{
    public class PrintViewModel : CloseableViewModel
    {
        private DateTime? _startDate;
        private DateTime? _endDate;
        private decimal _average;
        private decimal _total;
        private ObservableCollection<ExpenseViewModel> _originalData;
        private ObservableCollection<ExpenseViewModel> _expenses;

        public DateTime? StartDate
        {
            get { return _startDate; }
            set
            {
                _startDate = value;
                OnPropertyChanged();
                Filter();
            }
        }

        public DateTime? EndDate
        {
            get { return _endDate; }
            set
            {
                _endDate = value;
                OnPropertyChanged();
                Filter();
            }
        }

        public decimal Total
        {
            get { return _total; }
            set
            {
                _total = value;
                OnPropertyChanged();
            }
        }

        public decimal Average
        {
            get { return _average; }
            set
            {
                _average = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ExpenseViewModel> Expenses
        {
            get { return _expenses; }
            private set
            {
                _expenses = value;
                OnPropertyChanged();
                UpdateTotal();
            }
        }

        private void UpdateTotal()
        {
            if (Expenses.Count == 0)
            {
                Total = 0;
                Average = 0;
                return;
            }

            Total = Expenses.Sum(e => e.Amount);
            Average = Total / Expenses.Count;
        }

        public PrintViewModel()
        {
            Expenses = new ObservableCollection<ExpenseViewModel>();
            Init();
        }

        protected virtual async Task Init()
        {
            if (IsBusy) return;
            var id = BeginBusy();
            var server = IocContainer.Get<IServerService>();
            var expenses = await server.GetExpenses();
            Expenses.AddRange(expenses.Select(e => new ExpenseViewModel(e)));
            UpdateTotal();
            EndBusy(id);
        }

        private void Filter()
        {
            if (IsBusy) return;
            if (_startDate == null || _endDate == null) return;
            if (_originalData == null)
            {
                _originalData = new ObservableCollection<ExpenseViewModel>(Expenses.Select(e => e));
            }
            var startDate = _startDate.Value;
            var endDate = _endDate.Value;

            Expenses = new ObservableCollection<ExpenseViewModel>(_originalData.Where(e => e.Date >= startDate && e.Date <= endDate));
        }
    }
}
