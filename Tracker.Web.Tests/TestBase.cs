using System;
using System.Security.Claims;

namespace Tracker.Web.Tests
{
    public abstract class TestBase : IDisposable
    {
        protected ClaimsIdentity CreateClaimsIdentity(string userId, string userEmail, string userName)
        {
            return new ClaimsIdentity(new[]
                {
                    new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", userId, "String", "TestIssuer"),
                    new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", userEmail),
                    new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", userName)
                }, "TestAuth");
        }


        protected abstract string GetControllerPath();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            
        }
    }
}
