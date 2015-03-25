using Microsoft.AspNet.Identity;

namespace Tracker.Web.Tests.Mocks
{
    public class TestIdentityResult : IdentityResult
    {
        public TestIdentityResult(bool success)
            : base(success)
        {
        }
    }
}