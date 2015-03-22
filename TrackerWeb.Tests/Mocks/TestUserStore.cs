using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace TrackerWeb.Tests.Mocks
{
    internal class TestUserStore<T> : IUserStore<T>, IUserEmailStore<T>, IUserPasswordStore<T>, IUserLockoutStore<T, string>, IUserTwoFactorStore<T, string>, IUserPhoneNumberStore<T> where T : class, IUser<string>
    {
        private readonly Task CompletedTask = Task.FromResult(false);
        private List<T> _users = new List<T>();
        private Dictionary<string, EmailData> _emails = new Dictionary<string, EmailData>();
        private Dictionary<string, string> _passwords = new Dictionary<string, string>();
        private Dictionary<string, LockoutData> _lockouts = new Dictionary<string, LockoutData>();
        private Dictionary<string, bool> _twoFactor = new Dictionary<string, bool>();
        private Dictionary<string, PhoneData> _phones = new Dictionary<string, PhoneData>();

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

        public Task<DateTimeOffset> GetLockoutEndDateAsync(T user)
        {
            lock (_lockouts)
            {
                LockoutData data;
                if (_lockouts.TryGetValue(user.Id, out data))
                {
                    return Task.FromResult(data.LockoutOffset);
                }
            }
            return Task.FromResult(DateTimeOffset.UtcNow);
        }

        public Task SetLockoutEndDateAsync(T user, DateTimeOffset lockoutEnd)
        {
            lock (_lockouts)
            {
                LockoutData data;
                if (!_lockouts.TryGetValue(user.Id, out data))
                {
                    data = new LockoutData();
                    _lockouts[user.Id] = data;
                }
                data.LockoutOffset = lockoutEnd;
            }
            return CompletedTask;
        }

        public Task<int> IncrementAccessFailedCountAsync(T user)
        {
            lock (_lockouts)
            {
                LockoutData data;
                if (!_lockouts.TryGetValue(user.Id, out data))
                {
                    data = new LockoutData();
                    _lockouts[user.Id] = data;
                }
                data.FailedCount++;
                return Task.FromResult(data.FailedCount);
            }
        }

        public Task ResetAccessFailedCountAsync(T user)
        {
            lock (_lockouts)
            {
                LockoutData data;
                if (!_lockouts.TryGetValue(user.Id, out data))
                {
                    data = new LockoutData();
                    _lockouts[user.Id] = data;
                }
                data.FailedCount = 0;
            }
            return CompletedTask;
        }

        public Task<int> GetAccessFailedCountAsync(T user)
        {
            lock (_lockouts)
            {
                LockoutData data;
                if (_lockouts.TryGetValue(user.Id, out data))
                {
                    return Task.FromResult(data.FailedCount);
                }
            }
            return Task.FromResult(0);
        }

        public Task<bool> GetLockoutEnabledAsync(T user)
        {
            lock (_lockouts)
            {
                LockoutData data;
                if (_lockouts.TryGetValue(user.Id, out data))
                {
                    return Task.FromResult(data.LockoutEnabled);
                }
            }
            return Task.FromResult(false);
        }

        public Task SetLockoutEnabledAsync(T user, bool enabled)
        {
            lock (_lockouts)
            {
                LockoutData data;
                if (!_lockouts.TryGetValue(user.Id, out data))
                {
                    data = new LockoutData();
                    _lockouts[user.Id] = data;
                }
                data.LockoutEnabled = enabled;
            }
            return CompletedTask;
        }

        private class LockoutData
        {
            public DateTimeOffset LockoutOffset;
            public int FailedCount;
            public bool LockoutEnabled;
        }

        public Task SetTwoFactorEnabledAsync(T user, bool enabled)
        {
            lock (_twoFactor)
            {
                _twoFactor[user.Id] = enabled;
            }
            return CompletedTask;
        }

        public Task<bool> GetTwoFactorEnabledAsync(T user)
        {
            bool result;
            lock (_twoFactor)
            {
                _twoFactor.TryGetValue(user.Id, out result);
            }
            return Task.FromResult(result);
        }

        public Task SetPhoneNumberAsync(T user, string phoneNumber)
        {
            lock (_phones)
            {
                PhoneData data;
                if (!_phones.TryGetValue(user.Id, out data))
                {
                    data = new PhoneData();
                    _phones[user.Id] = data;
                }
                data.PhoneNumber = phoneNumber;
            }
            return CompletedTask;
        }

        public Task<string> GetPhoneNumberAsync(T user)
        {
            lock (_phones)
            {
                PhoneData data;
                if (_phones.TryGetValue(user.Id, out data))
                {
                    return Task.FromResult(data.PhoneNumber);
                }
            }
            return Task.FromResult((string)null);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(T user)
        {
            lock (_phones)
            {
                PhoneData data;
                if (_phones.TryGetValue(user.Id, out data))
                {
                    return Task.FromResult(data.Confirmed);
                }
            }
            return Task.FromResult(false);
        }

        public Task SetPhoneNumberConfirmedAsync(T user, bool confirmed)
        {
            lock (_phones)
            {
                PhoneData data;
                if (!_phones.TryGetValue(user.Id, out data))
                {
                    data = new PhoneData();
                    _phones[user.Id] = data;
                }
                data.Confirmed = confirmed;
            }
            return CompletedTask;
        }

        private class PhoneData
        {
            public string PhoneNumber;
            public bool Confirmed;
        }
    }
}
