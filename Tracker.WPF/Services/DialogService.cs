using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Tracker.Core.Services;

namespace Tracker.WPF.Services
{
    class DialogService : IDialogService
    {
        public Task ShowMessage(string message)
        {
            MessageBox.Show(message);
            return Task.FromResult(false);
        }
    }
}
