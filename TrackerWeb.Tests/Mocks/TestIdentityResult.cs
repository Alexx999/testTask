using Microsoft.AspNet.Identity;

namespace TrackerWeb.Tests.Mocks
{
    public class TestIdentityResult : IdentityResult
    {
        public TestIdentityResult(bool success)
            : base(success)
        {
        }
    }
}