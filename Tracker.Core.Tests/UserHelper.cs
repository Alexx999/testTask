using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;
using Tracker.Web;
using Tracker.Web.Models;

namespace Tracker.Core.Tests
{
    public class UserHelper
    {
        private ApplicationUserManager _userManager = ApplicationUserManager.Create(new UserStore<ApplicationUser>(ApplicationDbContext.Create()));

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
    }
}
