using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Tracker.Models;
using TrackerWeb.Models;

namespace TrackerWeb.Controllers.API
{
    [Authorize]
    public class ExpensesController : ApiController
    {
        private ApplicationUserManager _userManager;
        private ApplicationDbContext _context;

        public ExpensesController()
        {
        }

        public ExpensesController(ApplicationUserManager userManager, ApplicationDbContext context)
        {
            UserManager = userManager;
            Context = context;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ApplicationDbContext Context
        {
            get
            {
                return _context ?? Request.GetOwinContext().Get<ApplicationDbContext>();
            }
            private set
            {
                _context = value;
            }
        }


        private async Task<ApplicationUser> GetCurrentUser()
        {
            return await UserManager.FindByIdAsync(User.Identity.GetUserId());
        }

        // GET: api/Expenses
        public async Task<ICollection<Expense>> GetExpenses()
        {
            var user = await GetCurrentUser();
            return user.Expenses;
        }

        // GET: api/Expenses/5
        [ResponseType(typeof(Expense))]
        public async Task<IHttpActionResult> GetExpense(Guid id)
        {
            Expense expense = await Context.Expenses.FindAsync(id);
            if (expense == null || expense.ApplicationUserID != User.Identity.GetUserId())
            {
                return NotFound();
            }

            return Ok(expense);
        }

        // PUT: api/Expenses/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutExpense(Guid id, Expense expense)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != expense.ExpenseId)
            {
                return BadRequest();
            }

            Expense existingExpense = await Context.Expenses.FindAsync(id);
            if (existingExpense == null || existingExpense.ApplicationUserID != User.Identity.GetUserId())
            {
                return NotFound();
            }

            expense.ApplicationUserID = existingExpense.ApplicationUserID;
            Context.SetModified(expense);

            await Context.SaveChangesAsync();

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Expenses
        [ResponseType(typeof(Expense))]
        public async Task<IHttpActionResult> PostExpense(Expense expense)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            expense.ApplicationUserID = User.Identity.GetUserId();
            Context.Expenses.Add(expense);

            try
            {
                await Context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ExpenseExists(expense.ExpenseId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = expense.ExpenseId }, expense);
        }

        // DELETE: api/Expenses/5
        [ResponseType(typeof(Expense))]
        public async Task<IHttpActionResult> DeleteExpense(Guid id)
        {
            Expense expense = await Context.Expenses.FindAsync(id);
            if (expense == null || expense.ApplicationUserID != User.Identity.GetUserId())
            {
                return NotFound();
            }

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

            return Ok(expense);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _context != null)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ExpenseExists(Guid id)
        {
            var expense = Context.Expenses.Find(id);
            return expense != null && expense.ApplicationUserID == User.Identity.GetUserId();
        }
    }
}