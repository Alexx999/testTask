using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;
using Tracker.Models;
using Tracker.Web;
using Tracker.Web.Models;

namespace Tracker.Core.Tests
{
    extern alias ModelsFull;

    public class UserHelper
    {
        private ApplicationDbContext _context = ApplicationDbContext.Create();
        private ApplicationUserManager _userManager;

        public UserHelper()
        {
            _userManager = ApplicationUserManager.Create(new UserStore<ApplicationUser>(_context));
        }

        public async Task<bool> UserExists(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user != null;
        }

        public async Task EnsureUserDontExist(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
        }

        public async Task EnsureUserExists(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser {Name = "Test User", UserName = email, Email = email};
                await _userManager.CreateAsync(user, TestConfig.TestUserPassword);
            }
        }

        public async Task AddExpense(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            _context.Expenses.Add(new ModelsFull::Tracker.Models.Expense { Amount = 0, ApplicationUserID = user.Id, Comment = TestConfig.TestUserPassword, Date = DateTime.Now, Description = "This is Expense"});
            await _context.SaveChangesAsync();
        }
    }
}
