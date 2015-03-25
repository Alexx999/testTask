using System.Web.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tracker.Web.Tests
{
    static class TestConfig
    {
        public const string AppUrl = "http://localhost/";
        public const string TestUserEmail = "testuser@testdomain.test";
        public const string TestUserPassword = "TestPass!2";


        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}
