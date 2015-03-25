using System.Security.Claims;

namespace TrackerWeb.Tests
{
    public abstract class TestBase
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
    }
}
