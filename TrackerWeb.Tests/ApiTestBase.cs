using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TrackerWeb.Models;
using TrackerWeb.Tests.Mocks;

namespace TrackerWeb.Tests
{
    public abstract class ApiTestBase : TestBase
    {
        protected ApplicationUserManager UserManager;
        protected ClaimsIdentity Identity;

        protected void Init()
        {
            UserManager = ApplicationUserManager.Create(new TestUserStore<ApplicationUser>());

            var user = new ApplicationUser()
            {
                Name = "Test User",
                Email = TestConfig.TestUserEmail,
                UserName = TestConfig.TestUserEmail
            };

            UserManager.CreateAsync(user, TestConfig.TestUserPassword).Wait();

            user = UserManager.FindByEmailAsync(TestConfig.TestUserEmail).Result;
            Identity = CreateClaimsIdentity(user.Id, user.Email, user.Name);
        }

        protected void SetupControllerForTests(ApiController controller)
        {
            var config = new HttpConfiguration();
            var request = new HttpRequestMessage(HttpMethod.Post, GetControllerPath());
            var route = config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{id}");
            var routeData = new HttpRouteData(route, new HttpRouteValueDictionary { { "controller", "Account" } });

            var principal = new Mock<IPrincipal>();

            principal.Setup(p => p.Identity).Returns(Identity);

            controller.ControllerContext = new HttpControllerContext(config, routeData, request);
            controller.RequestContext.Principal = principal.Object;
            controller.Request = request;
            controller.Request.SetConfiguration(config);
            var owinContext = new OwinContext();
            owinContext.Set(UserManager);
            request.Properties["MS_OwinContext"] = owinContext;
        }

        public virtual void Cleanup()
        {
            UserManager.Dispose();
        }
    }
}
