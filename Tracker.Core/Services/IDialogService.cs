using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracker.Core.Services
{
    public interface IDialogService
    {
        Task ShowMessage(string message);
    }
}
