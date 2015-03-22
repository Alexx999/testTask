using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Moq;
using TrackerWeb.Models;
using TrackerWeb.Tests.Mocks;

namespace TrackerWeb.Tests
{
    public abstract class MvcTestBase
    {
        protected ApplicationUserManager UserManager;
        protected ApplicationSignInManager SignInManager;
        protected TestUserStore<ApplicationUser> UserStore;
        protected Mock<IAuthenticationManager> AuthMock;
        protected ClaimsIdentity Identity;


        protected void Init()
        {
            UserStore = new TestUserStore<ApplicationUser>();
            UserManager = ApplicationUserManager.Create(UserStore);
            var user = new ApplicationUser()
            {
                Email = TestConfig.TestUserEmail,
                Name = "Test User",
                UserName = TestConfig.TestUserEmail
            };
            UserManager.CreateAsync(user, TestConfig.TestUserPassword).Wait();

            AuthMock = GetAuthenticationManagerMock(false, false);
            SignInManager = new ApplicationSignInManager(UserManager, AuthMock.Object);
        }

        protected IAuthenticationManager GetAuthenticationManager(bool hasAuthenticatedUser, bool hasExternalLoginInfo = false)
        {
            return GetAuthenticationManagerMock(hasAuthenticatedUser, hasExternalLoginInfo).Object;
        }

        protected Mock<IAuthenticationManager> GetAuthenticationManagerMock(bool hasAuthenticatedUser, bool hasExternalLoginInfo)
        {
            var mockAuthenticationManager = new Mock<IAuthenticationManager>();
            mockAuthenticationManager.Setup(am => am.SignOut()).Verifiable();
            mockAuthenticationManager.Setup(am => am.SignIn(It.IsAny<ClaimsIdentity[]>())).Verifiable();

            var user = UserManager.FindByEmailAsync(TestConfig.TestUserEmail).Result;
            Identity = CreateClaimsIdentity(user.Id, user.Email, user.Name);
            var authenticateResult = new AuthenticateResult(Identity, new AuthenticationProperties(), new AuthenticationDescription());
            if (hasAuthenticatedUser)
            {
                mockAuthenticationManager.Setup(am => am.AuthenticateAsync("TwoFactorCookie")).ReturnsAsync(authenticateResult);
            }
            if (hasExternalLoginInfo)
            {
                mockAuthenticationManager.Setup(am => am.AuthenticateAsync("ExternalCookie")).ReturnsAsync(authenticateResult);
            }
            return mockAuthenticationManager;
        }

        protected ClaimsIdentity CreateClaimsIdentity(string userId, string userEmail, string userName)
        {
            return new ClaimsIdentity(new[]
                {
                    new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", userId, "String", "TestIssuer"),
                    new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", userEmail),
                    new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", userName)
                }, "TestAuth");
        }


        protected virtual void SetupControllerForTests(Controller controller, bool identityAuthenticated = false)
        {
            var owinContext = new OwinContext();
            owinContext.Set(UserManager);
            owinContext.Set(SignInManager);

            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            var response = new Mock<HttpResponseBase>();
            var session = new Mock<HttpSessionStateBase>();
            var server = new Mock<HttpServerUtilityBase>();
            var principal = new Mock<IPrincipal>();
            var identity = new Mock<IIdentity>();
            
            if (identityAuthenticated)
            {
                principal.Setup(p => p.Identity).Returns(Identity);
            }
            else
            {
                principal.Setup(p => p.Identity).Returns(identity.Object);
            }

            context.Setup(ctx => ctx.Request).Returns(request.Object);
            context.Setup(ctx => ctx.Response).Returns(response.Object);
            context.Setup(ctx => ctx.Session).Returns(session.Object);
            context.Setup(ctx => ctx.Server).Returns(server.Object);
            context.Setup(ctx => ctx.User).Returns(principal.Object);

            request.SetupGet(x => x.ApplicationPath).Returns("/");
            request.SetupGet(x => x.Url).Returns(new Uri(GetControllerPath(), UriKind.Absolute));
            request.SetupGet(x => x.ServerVariables).Returns(new NameValueCollection());

            response.Setup(x => x.ApplyAppPathModifier(It.IsAny<string>())).Returns<string>(x => x);

            context.SetupGet(x => x.Request).Returns(request.Object);
            context.SetupGet(x => x.Response).Returns(response.Object);
            context.SetupGet(x => x.Items).Returns(new Dictionary<object, object>() { { "owin.Environment", owinContext.Environment } });

            var helper = new UrlHelper(new RequestContext(MvcMockHelpers.FakeHttpContext(), new RouteData()), RouteTable.Routes);
            controller.Url = helper;
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);
        }

        protected abstract string GetControllerPath();

        public virtual void Cleanup()
        {
            UserManager.Dispose();
            SignInManager.Dispose();
        }
    }
}
