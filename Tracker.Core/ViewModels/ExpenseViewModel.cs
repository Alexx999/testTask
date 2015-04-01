using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MugenMvvmToolkit.ViewModels;
using Tracker.Models;
using Tracker.Models.Account;

namespace Tracker.Core.ViewModels
{
    public class ExpenseViewModel : ViewModelBase
    {
        private Expense _model;

        public ExpenseViewModel(Expense model)
        {
            _model = model;
            IsCreated = true;
        }

        public ExpenseViewModel():this(new Expense())
        {
            IsCreated = false;
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

        public Expense Model
        {
            get { return _model; }
            set
            {
                _model = value;
                OnPropertyChanged("");
            }
        }

        public bool IsValid
        {
            get { return Validate(_model); }
        }

        private static bool Validate(Expense model)
        {
            var context = new ValidationContext(model);
            return Validator.TryValidateObject(model, context, null, true);
        }

        public bool IsCreated { get; set; }
    }
}