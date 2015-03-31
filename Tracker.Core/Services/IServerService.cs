using System.Collections.Generic;
using System.Threading.Tasks;
using Tracker.Models;
using Tracker.Models.Account;

namespace Tracker.Core.Services
{
    public interface IServerService
    {
        Task<bool> Register(RegisterModel model);
        Task<bool> Login(string username, string password);
        Task<List<Expense>> GetExpenses();
    }
}