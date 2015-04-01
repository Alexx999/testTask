using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
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
                try
                {
                    _context.Expenses.RemoveRange(_context.Expenses.Where(e => e.ApplicationUserID == user.Id));
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                }
                try
                {
                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                }
            }
        }

        public async Task<ApplicationUser> EnsureUserExists(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser {Name = "Test User", UserName = email, Email = email};
                await _userManager.CreateAsync(user, TestConfig.TestUserPassword);
            }
            return user;
        }
        public async Task<List<ModelsFull::Tracker.Models.Expense>> GetExpenses(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return _context.Expenses.Where(e => e.ApplicationUserID == user.Id).ToList();
        }

        public async Task<ModelsFull::Tracker.Models.Expense> AddExpense(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var expense = new ModelsFull::Tracker.Models.Expense
            {
                Amount = 0,
                ApplicationUserID = user.Id,
                Comment = TestConfig.TestUserPassword,
                Date = DateTime.Now,
                Description = "This is Expense"
            };
            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();
            return expense;
        }
    }
}
