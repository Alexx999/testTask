using System;
using MugenMvvmToolkit.ViewModels;
using Tracker.Models;

namespace Tracker.Core.ViewModels
{
    public class ExpenseViewModel : ViewModelBase
    {
        private Expense _model;

        public ExpenseViewModel(Expense model)
        {
            _model = model;
        }

        public ExpenseViewModel():this(new Expense())
        {
        }

        public DateTime Time
        {
            get
            {
                return _model.Date;
            }
            set
            {
                var date = _model.Date;
                var newDate = new DateTime(date.Year, date.Month, date.Day, value.Hour, value.Minute, value.Second);
                _model.Date = newDate;
                OnPropertyChanged();
            }
        }

        public DateTime Date
        {
            get
            {
                return _model.Date;
            }
            set
            {
                var time = Time;
                _model.Date = value;
                Time = time;
                OnPropertyChanged();
            }
        }

        public string Description
        {
            get
            {
                return _model.Description;
            }
            set
            {
                _model.Description = value;
                OnPropertyChanged();
            }
        }

        public decimal Amount
        {
            get
            {
                return _model.Amount;
            }
            set
            {
                _model.Amount = value;
                OnPropertyChanged();
            }
        }

        public string Comment
        {
            get
            {
                return _model.Comment;
            }
            set
            {
                _model.Comment = value;
                OnPropertyChanged();
            }
        }
    }
}