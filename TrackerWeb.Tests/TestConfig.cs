using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TrackerWeb.Tests
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
