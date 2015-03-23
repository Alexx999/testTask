using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace TrackerWeb.Tests.Mocks
{
    public class TestUserStore<TUser> : IUserStore<TUser>, IUserEmailStore<TUser>, IUserPasswordStore<TUser>, IUserLockoutStore<TUser, string>, IUserTwoFactorStore<TUser, string>, IUserPhoneNumberStore<TUser>, IUserLoginStore<TUser>, IUserSecurityStampStore<TUser> where TUser : class, IUser<string>
    {
        private readonly Task _completedTask = Task.FromResult(false);
        private Dictionary<string, UserData<TUser>> _data = new Dictionary<string, UserData<TUser>>(); 

        public void Dispose()
        {
        }

        private UserData<TUser> GetOrCreate(TUser user)
        {
            UserData<TUser> result;
            lock (_data)
            {
                if (!_data.TryGetValue(user.Id, out result))
                {
                    result = new UserData<TUser>() {User = user};
                    _data[user.Id] = result;
                }
            }
            return result;
        }

        private UserData<TUser> Get(TUser user)
        {
            UserData<TUser> result;
            lock (_data)
            {
                if (_data.TryGetValue(user.Id, out result))
                {
                    return result;
                }
            }
            return null;
        }

        public Task CreateAsync(TUser user)
        {
            GetOrCreate(user).User = user;
            return _completedTask;
        }

        public Task UpdateAsync(TUser user)
        {
            GetOrCreate(user).User = user;
            return _completedTask;
        }

        public Task DeleteAsync(TUser user)
        {
            lock (_data)
            {
                if (_data.ContainsKey(user.Id))
                {
                    _data.Remove(user.Id);
                }
            }
            return _completedTask;
        }

        public Task<TUser> FindByIdAsync(string userId)
        {
            if (userId == null) return Task.FromResult((TUser)null);

            UserData<TUser> result;

            lock (_data)
            {
                if (!_data.TryGetValue(userId, out result))
                {
                    return Task.FromResult((TUser)null);
                }
            }
            return Task.FromResult(result.User);
        }

        public Task<TUser> FindByNameAsync(string userName)
        {
            lock (_data)
            {
                var result = _data.Values.FirstOrDefault(d => d.User.UserName == userName);
                if (result != null)
                {
                    return Task.FromResult(result.User);
                }
            }
            return Task.FromResult((TUser)null);
        }

        public Task SetEmailAsync(TUser user, string email)
        {
            var data = GetOrCreate(user);
            data.Email = email;
            return _completedTask;
        }

        public Task<string> GetEmailAsync(TUser user)
        {
            var data = Get(user);
            if (data != null)
            {
                return Task.FromResult(data.Email);
            }

            return Task.FromResult((string)null);
        }

        public Task<bool> GetEmailConfirmedAsync(TUser user)
        {
            var data = Get(user);
            if (data != null)
            {
                return Task.FromResult(data.EmailConfirmed);
            }

            return Task.FromResult(false);
        }

        public Task SetEmailConfirmedAsync(TUser user, bool confirmed)
        {
            var data = GetOrCreate(user);
            data.EmailConfirmed = confirmed;
            return _completedTask;
        }

        public Task<TUser> FindByEmailAsync(string email)
        {
            lock (_data)
            {
                var result = _data.Values.FirstOrDefault(d => d.Email == email);
                if (result != null)
                {
                    return Task.FromResult(result.User);
                }
            }
            return Task.FromResult((TUser)null);
        }

        public Task SetPasswordHashAsync(TUser user, string passwordHash)
        {
            var data = GetOrCreate(user);
            data.PasswordHash = passwordHash;
            if (user is IdentityUser)
            {
                var iu = user as IdentityUser;
                SetEmailAsync(user, iu.Email);
            }
            return _completedTask;
        }

        public Task<string> GetPasswordHashAsync(TUser user)
        {
            var data = Get(user);
            if (data != null)
            {
                return Task.FromResult(data.PasswordHash);
            }

            return Task.FromResult((string)null);
        }

        public Task<bool> HasPasswordAsync(TUser user)
        {
            var data = Get(user);
            if (data != null)
            {
                return Task.FromResult(!string.IsNullOrEmpty(data.PasswordHash));
            }

            return Task.FromResult(false);
        }

        public Task<DateTimeOffset> GetLockoutEndDateAsync(TUser user)
        {
            var data = Get(user);
            if (data != null)
            {
                return Task.FromResult(data.LockoutEnd);
            }

            return Task.FromResult(default(DateTimeOffset));
        }

        public Task SetLockoutEndDateAsync(TUser user, DateTimeOffset lockoutEnd)
        {
            var data = GetOrCreate(user);
            data.LockoutEnd = lockoutEnd;
            return _completedTask;
        }

        public Task<int> IncrementAccessFailedCountAsync(TUser user)
        {
            var data = GetOrCreate(user);
            data.FailedCount++;
            return Task.FromResult(data.FailedCount);
        }

        public Task ResetAccessFailedCountAsync(TUser user)
        {
            var data = GetOrCreate(user);
            data.FailedCount = 0;
            return _completedTask;
        }

        public Task<int> GetAccessFailedCountAsync(TUser user)
        {
            var data = Get(user);
            if (data != null)
            {
                return Task.FromResult(data.FailedCount);
            }

            return Task.FromResult(0);
        }

        public Task<bool> GetLockoutEnabledAsync(TUser user)
        {
            var data = Get(user);
            if (data != null)
            {
                return Task.FromResult(data.LockoutEnabled);
            }

            return Task.FromResult(false);
        }

        public Task SetLockoutEnabledAsync(TUser user, bool enabled)
        {
            var data = GetOrCreate(user);
            data.LockoutEnabled = enabled;
            return _completedTask;
        }

        public Task SetTwoFactorEnabledAsync(TUser user, bool enabled)
        {
            var data = GetOrCreate(user);
            data.TwoFactorEnabled = enabled;
            return _completedTask;
        }

        public Task<bool> GetTwoFactorEnabledAsync(TUser user)
        {
            var data = Get(user);
            if (data != null)
            {
                return Task.FromResult(data.TwoFactorEnabled);
            }

            return Task.FromResult(false);
        }

        public Task SetPhoneNumberAsync(TUser user, string phoneNumber)
        {
            var data = GetOrCreate(user);
            data.PhoneNumber = phoneNumber;
            return _completedTask;
        }

        public Task<string> GetPhoneNumberAsync(TUser user)
        {
            var data = Get(user);
            if (data != null)
            {
                return Task.FromResult(data.PhoneNumber);
            }

            return Task.FromResult((string)null);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(TUser user)
        {
            var data = Get(user);
            if (data != null)
            {
                return Task.FromResult(data.PhoneConfirmed);
            }

            return Task.FromResult(false);
        }

        public Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed)
        {
            var data = GetOrCreate(user);
            data.PhoneConfirmed = confirmed;
            return _completedTask;
        }

        public Task AddLoginAsync(TUser user, UserLoginInfo login)
        {
            var data = GetOrCreate(user);
            data.Logins.Add(login);
            return _completedTask;
        }

        public Task RemoveLoginAsync(TUser user, UserLoginInfo login)
        {
            var data = Get(user);
            if (data != null)
            {
                data.Logins.Remove(login);
            }

            return _completedTask;
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user)
        {
            var data = Get(user);
            if (data != null)
            {
                return Task.FromResult((IList<UserLoginInfo>)data.Logins);
            }

            return Task.FromResult((IList<UserLoginInfo>)null);
        }

        public Task<TUser> FindAsync(UserLoginInfo login)
        {
            UserData<TUser> data;
            lock (_data)
            {
                data = _data.Values.FirstOrDefault(d => d.Logins.Any(
                    l => login.LoginProvider == l.LoginProvider && login.ProviderKey == l.ProviderKey));
            }
            return data == null ? Task.FromResult((TUser) null) : Task.FromResult(data.User);
        }

        public Task SetSecurityStampAsync(TUser user, string stamp)
        {
            var data = GetOrCreate(user);
            data.SecurityStamp = stamp;
            return _completedTask;
        }

        public Task<string> GetSecurityStampAsync(TUser user)
        {
            var data = Get(user);
            if (data != null)
            {
                return Task.FromResult(data.SecurityStamp);
            }

            return Task.FromResult((string)null);
        }
    }

    internal class UserData<TUser>
    {
        public TUser User;
        public string Email;
        public bool EmailConfirmed;
        public string PasswordHash;
        public DateTimeOffset LockoutEnd;
        public int FailedCount;
        public bool LockoutEnabled;
        public bool TwoFactorEnabled;
        public string PhoneNumber;
        public bool PhoneConfirmed;
        public string SecurityStamp;
        public List<UserLoginInfo> Logins = new List<UserLoginInfo>();
    }
}
