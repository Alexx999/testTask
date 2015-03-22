using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TrackerWeb.Controllers;
using TrackerWeb.Models;
using TrackerWeb.Tests.Mocks;

namespace TrackerWeb.Tests
{
    [TestClass]
    public class AccountTest
    {
        private ApplicationUserManager _userManager;
        private ApplicationSignInManager _signInManager;
        private AccountController _controller;
        private TestUserStore<ApplicationUser> _userStore;

        [TestInitialize]
        public void Init()
        {
            _userStore = new TestUserStore<ApplicationUser>();
            _userManager = ApplicationUserManager.Create(_userStore);
            var mockAuthenticationManager = new Mock<IAuthenticationManager>();
            mockAuthenticationManager.Setup(am => am.SignOut());
            mockAuthenticationManager.Setup(am => am.SignIn());
            _signInManager = new ApplicationSignInManager(_userManager, mockAuthenticationManager.Object);
            _controller = new AccountController(_userManager, _signInManager);
            SetupControllerForTests(_controller);
            var user = new ApplicationUser()
            {
                Email = TestConfig.TestUserEmail,
                Name = "Test User",
                UserName = TestConfig.TestUserEmail
            };
            _userManager.CreateAsync(user, TestConfig.TestUserPassword).Wait();
        }


        [TestMethod]
        public void TestAccountLoginView()
        {
            var result = _controller.Login("/Test");
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public async Task TestAccountLoginSuccess()
        {
            var viewModel = new LoginViewModel {Email = TestConfig.TestUserEmail, Password = TestConfig.TestUserPassword};
            var result = await _controller.Login(viewModel, "/Test");
            Assert.IsInstanceOfType(result, typeof(RedirectResult));
            var redirectResult = (RedirectResult) result;
            Assert.AreEqual(redirectResult.Url, "/Test");
        }

        [TestMethod]
        public async Task TestAccountLoginLockout()
        {
            var user = await _userManager.FindByEmailAsync(TestConfig.TestUserEmail);
            await _userManager.SetLockoutEnabledAsync(user.Id, true);
            await _userManager.SetLockoutEndDateAsync(user.Id, new DateTimeOffset(DateTime.UtcNow + new TimeSpan(1000,0,0,0)));

            var viewModel = new LoginViewModel {Email = TestConfig.TestUserEmail, Password = TestConfig.TestUserPassword};
            var result = await _controller.Login(viewModel, "/Test");
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(viewResult.ViewName, "Lockout");
        }

        [TestMethod]
        public async Task TestAccountLoginTwoFactor()
        {
            var user = await _userManager.FindByEmailAsync(TestConfig.TestUserEmail);
            await _userManager.SetTwoFactorEnabledAsync(user.Id, true);
            await _userManager.SetPhoneNumberAsync(user.Id, "+100000000000");
            await _userStore.SetPhoneNumberConfirmedAsync(user, true);

            var viewModel = new LoginViewModel {Email = TestConfig.TestUserEmail, Password = TestConfig.TestUserPassword};
            var result = await _controller.Login(viewModel, "/Test");
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var redirectResult = (RedirectToRouteResult)result;
            Assert.AreEqual(redirectResult.RouteName, "");
            Assert.AreEqual(redirectResult.RouteValues["action"], "SendCode");
        }

        [TestMethod]
        public async Task TestAccountLoginBadPass()
        {
            var viewModel = new LoginViewModel { Email = TestConfig.TestUserEmail, Password = TestConfig.TestUserPassword + " " };
            var result = await _controller.Login(viewModel, "/Test");
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.IsFalse(_controller.ModelState.IsValid);
        }

        [TestMethod]
        public async Task TestAccountLoginBadModelState()
        {
            var viewModel = new LoginViewModel { Email = TestConfig.TestUserEmail, Password = TestConfig.TestUserPassword + " " };
            _controller.ModelState.AddModelError("Error","Some Error");
            var result = await _controller.Login(viewModel, "/Test");
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestCleanup]
        public void Cleanup()
        {
            _controller.Dispose();
            _userManager.Dispose();
            _signInManager.Dispose();
        }

        [TestMethod]
        public void TestUserManagerGetter()
        {
            var controller = new AccountController();
            SetupControllerForTests(controller);
            Assert.AreSame(controller.UserManager, _userManager);
        }

        [TestMethod]
        public void TestSignInManagerGetter()
        {
            var controller = new AccountController();
            SetupControllerForTests(controller);
            Assert.AreSame(controller.SignInManager, _signInManager);
        }

        private void SetupControllerForTests(Controller controller)
        {
            var owinContext = new OwinContext();
            owinContext.Set(_userManager);
            owinContext.Set(_signInManager);

            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            var response = new Mock<HttpResponseBase>();
            var session = new Mock<HttpSessionStateBase>();
            var server = new Mock<HttpServerUtilityBase>();

            context.Setup(ctx => ctx.Request).Returns(request.Object);
            context.Setup(ctx => ctx.Response).Returns(response.Object);
            context.Setup(ctx => ctx.Session).Returns(session.Object);
            context.Setup(ctx => ctx.Server).Returns(server.Object);

            request.SetupGet(x => x.ApplicationPath).Returns("/");
            request.SetupGet(x => x.Url).Returns(new Uri("http://localhost/Account", UriKind.Absolute));
            request.SetupGet(x => x.ServerVariables).Returns(new NameValueCollection());

            response.Setup(x => x.ApplyAppPathModifier(It.IsAny<string>())).Returns<string>(x => x);

            context.SetupGet(x => x.Request).Returns(request.Object);
            context.SetupGet(x => x.Response).Returns(response.Object);
            context.SetupGet(x => x.Items).Returns(new Dictionary<object, object>() { { "owin.Environment", owinContext.Environment } });

            var helper = new UrlHelper(new RequestContext(MvcMockHelpers.FakeHttpContext(), new RouteData()), RouteTable.Routes);
            controller.Url = helper;
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);
        }
    }
}
