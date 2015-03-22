using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace TrackerWeb.Tests.Mock
{
    internal class TestUserStore<T> : IUserStore<T>, IUserEmailStore<T>, IUserPasswordStore<T> where T : class, IUser<string>
    {
        private readonly Task CompletedTask = Task.FromResult(false);
        private List<T> _users = new List<T>();
        private Dictionary<string, EmailData> _emails = new Dictionary<string, EmailData>();
        private Dictionary<string, string> _passwords = new Dictionary<string, string>();

        public void Dispose()
        {
        }

        public Task CreateAsync(T user)
        {
            lock (_users)
            {
                _users.Add(user);
            }
            return CompletedTask;
        }

        public Task UpdateAsync(T user)
        {
            var existing = FindById(user.Id);
            if (!ReferenceEquals(user, existing))
            {
                lock (_users)
                {
                    _users.Remove(existing);
                    _users.Add(user);
                }
            }
            return CompletedTask;
        }

        public Task DeleteAsync(T user)
        {
            lock (_users)
            {
                _users.Remove(user);
            }
            return CompletedTask;
        }

        public Task<T> FindByIdAsync(string userId)
        {
            lock (_users)
            {
                var user = _users.FirstOrDefault(u => u.Id == userId);
                return Task.FromResult(user);
            }
        }

        public Task<T> FindByNameAsync(string userName)
        {
            lock (_users)
            {
                var user = _users.FirstOrDefault(u => u.UserName == userName);
                return Task.FromResult(user);
            }
        }

        public T FindById(string userId)
        {
            lock (_users)
            {
                return _users.FirstOrDefault(u => u.Id == userId);
            }
        }

        public T FindByName(string userName)
        {
            lock (_users)
            {
                return _users.FirstOrDefault(u => u.UserName == userName);
            }
        }

        public Task SetEmailAsync(T user, string email)
        {
            lock (_emails)
            {
                _emails[user.Id] = new EmailData(email, false);
            }
            return CompletedTask;
        }

        public Task<string> GetEmailAsync(T user)
        {
            EmailData data;

            lock (_emails)
            {
                if (_emails.TryGetValue(user.Id, out data))
                {
                    return Task.FromResult(data.Address);
                }
            }
            return Task.FromResult((string)null);
        }

        public Task<bool> GetEmailConfirmedAsync(T user)
        {
            EmailData data;

            lock (_emails)
            {
                if (_emails.TryGetValue(user.Id, out data))
                {
                    return Task.FromResult(data.Confirmed);
                }
            }
            return Task.FromResult(false);
        }

        public Task SetEmailConfirmedAsync(T user, bool confirmed)
        {
            EmailData data;

            lock (_emails)
            {
                if (_emails.TryGetValue(user.Id, out data))
                {
                    data.Confirmed = confirmed;
                    _emails[user.Id] = data;
                }
            }
            return CompletedTask;
        }

        public Task<T> FindByEmailAsync(string email)
        {
            lock (_emails)
            {
                if (_emails.Any(kvp => kvp.Value.Address == email))
                {
                    var id = _emails.First(kvp => kvp.Value.Address == email).Key;
                    return FindByIdAsync(id);
                }
            }
            return Task.FromResult((T) null);
        }

        private struct EmailData
        {
            public string Address;
            public bool Confirmed;

            public EmailData(string address, bool confirmed)
            {
                Address = address;
                Confirmed = confirmed;
            }
        }

        public Task SetPasswordHashAsync(T user, string passwordHash)
        {
            if (FindById(user.Id) == null)
            {
                CreateAsync(user);
                if (user is IdentityUser)
                {
                    var iu = user as IdentityUser;
                    SetEmailAsync(user, iu.Email);
                }
            }
            lock (_passwords)
            {
                _passwords[user.Id] = passwordHash;
            }
            return CompletedTask;
        }

        public Task<string> GetPasswordHashAsync(T user)
        {
            string result;
            lock (_passwords)
            {
                if (_passwords.TryGetValue(user.Id, out result))
                {
                    return Task.FromResult(result);
                }
            }
            return Task.FromResult((string)null);
        }

        public Task<bool> HasPasswordAsync(T user)
        {
            lock (_passwords)
            {
                return Task.FromResult(_passwords.ContainsKey(user.Id));
            }
        }
    }
}
