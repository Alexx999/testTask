using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.AspNet.Identity;
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
            _signInManager = new ApplicationSignInManager(_userManager, GetAuthenticationManager(false));
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

        private IAuthenticationManager GetAuthenticationManager(bool hasAuthenticatedUser)
        {
            var mockAuthenticationManager = new Mock<IAuthenticationManager>();
            mockAuthenticationManager.Setup(am => am.SignOut());
            mockAuthenticationManager.Setup(am => am.SignIn());
            if (hasAuthenticatedUser)
            {
                var identity = new ClaimsIdentity(new []{ new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "userId")});
                var result = new AuthenticateResult(identity, new AuthenticationProperties(), new AuthenticationDescription());
                mockAuthenticationManager.Setup(am => am.AuthenticateAsync(It.IsAny<string>())).ReturnsAsync(result);
            }
            return mockAuthenticationManager.Object;
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
        public async Task TestAccountLoginBadRedirectUrl()
        {
            var viewModel = new LoginViewModel { Email = TestConfig.TestUserEmail, Password = TestConfig.TestUserPassword };
            var result = await _controller.Login(viewModel, "http://nowhere.net/");
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var redirectResult = (RedirectToRouteResult)result;
            Assert.AreEqual(redirectResult.RouteValues["controller"], "Home");
            Assert.AreEqual(redirectResult.RouteValues["action"], "Index");
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
            Assert.IsNull(redirectResult.RouteValues["controller"]);
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

        [TestMethod]
        public void TestAuthorize()
        {
            var result = _controller.Authorize();
            Assert.IsInstanceOfType(result, typeof(EmptyResult));
        }

        [TestMethod]
        public async Task TestVerifyCodeView()
        {
            var signInManager = new ApplicationSignInManager(_userManager, GetAuthenticationManager(true));
            var controller = new AccountController(_userManager, signInManager);
            SetupControllerForTests(controller);

            var result = await controller.VerifyCode("", "", false);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult) result;
            Assert.AreEqual(viewResult.ViewName, "");
        }

        [TestMethod]
        public async Task TestVerifyCodeViewError()
        {
            var result = await _controller.VerifyCode("", "", false);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult) result;
            Assert.AreEqual(viewResult.ViewName, "Error");
        }

        [TestMethod]
        public async Task TestVerifyCodeError()
        {
            var verifyViewModel = new VerifyCodeViewModel();
            var result = await _controller.VerifyCode(verifyViewModel);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult) result;
            Assert.IsFalse(_controller.ModelState.IsValid);
            Assert.AreEqual(viewResult.ViewName, "");
        }

        [TestMethod]
        public async Task TestVerifyCodeBadModelState()
        {
            var verifyViewModel = new VerifyCodeViewModel();
            _controller.ModelState.AddModelError("Error", "Some error");
            var result = await _controller.VerifyCode(verifyViewModel);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult) result;
            Assert.AreEqual(viewResult.ViewName, "");
        }

        [TestMethod]
        public async Task TestVerifyCodeSuccess()
        {
            var signIn = new Mock<ApplicationSignInManager>(_userManager, GetAuthenticationManager(true));
            signIn.Setup(
                si => si.TwoFactorSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(SignInStatus.Success);
            var controller = new AccountController(_userManager, signIn.Object);
            SetupControllerForTests(controller);

            var verifyViewModel = new VerifyCodeViewModel();
            var result = await controller.VerifyCode(verifyViewModel);
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var redirectResult = (RedirectToRouteResult)result;
            Assert.AreEqual(redirectResult.RouteValues["controller"], "Home");
            Assert.AreEqual(redirectResult.RouteValues["action"], "Index");
        }

        [TestMethod]
        public async Task TestVerifyCodeLockout()
        {
            var signIn = new Mock<ApplicationSignInManager>(_userManager, GetAuthenticationManager(true));
            signIn.Setup(
                si => si.TwoFactorSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(SignInStatus.LockedOut);
            var controller = new AccountController(_userManager, signIn.Object);
            SetupControllerForTests(controller);

            var verifyViewModel = new VerifyCodeViewModel();
            var result = await controller.VerifyCode(verifyViewModel);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(viewResult.ViewName, "Lockout");
        }

        [TestMethod]
        public void TestRegisterView()
        {
            var result = _controller.Register();
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public async Task TestRegisterBadModelState()
        {
            var viewModel = new RegisterViewModel();
            _controller.ModelState.AddModelError("Error", "Some Error");
            var result = await _controller.Register(viewModel);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public async Task TestRegisterSuccess()
        {
            await _userManager.DeleteAsync(await _userManager.FindByNameAsync(TestConfig.TestUserEmail));
            var viewModel = new RegisterViewModel
            {
                Email = TestConfig.TestUserEmail,
                Password = TestConfig.TestUserPassword,
                ConfirmPassword = TestConfig.TestUserPassword,
                Name = "Test User"
            };
            var result = await _controller.Register(viewModel);
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var redirectResult = (RedirectToRouteResult)result;
            Assert.AreEqual(redirectResult.RouteValues["controller"], "Home");
            Assert.AreEqual(redirectResult.RouteValues["action"], "Index");
        }

        [TestMethod]
        public async Task TestRegisterDuplicateUser()
        {
            var viewModel = new RegisterViewModel
            {
                Email = TestConfig.TestUserEmail,
                Password = TestConfig.TestUserPassword,
                ConfirmPassword = TestConfig.TestUserPassword,
                Name = "Test User"
            };
            var result = await _controller.Register(viewModel);
            Assert.IsFalse(_controller.ModelState.IsValid);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(viewResult.ViewName, "");
        }

        [TestMethod]
        public async Task TestConfirmEmailError()
        {
            var result = await _controller.ConfirmEmail(null, null);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(viewResult.ViewName, "Error");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task TestConfirmEmailError2()
        {
            await _controller.ConfirmEmail("", "");
        }

        [TestMethod]
        public async Task TestConfirmEmailError3()
        {
            var user = await _userManager.FindByNameAsync(TestConfig.TestUserEmail);
            var userManager = new Mock<ApplicationUserManager>(_userStore);
            userManager.Setup(
                si => si.ConfirmEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new TestIdentityResult(false));
            var controller = new AccountController(userManager.Object, _signInManager);
            SetupControllerForTests(controller);

            var result = await controller.ConfirmEmail(user.Id, "");
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(viewResult.ViewName, "Error");
        }

        [TestMethod]
        public async Task TestConfirmEmailSuccess()
        {
            var userManager = new Mock<ApplicationUserManager>(_userStore);
            userManager.Setup(
                si => si.ConfirmEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new TestIdentityResult(true));
            var controller = new AccountController(userManager.Object, _signInManager);
            SetupControllerForTests(controller);

            var result = await controller.ConfirmEmail("", "");
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(viewResult.ViewName, "ConfirmEmail");
        }

        [TestCleanup]
        public void Cleanup()
        {
            _controller.Dispose();
            _userManager.Dispose();
            _signInManager.Dispose();
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
            var principal = new Mock<IPrincipal>();

            context.Setup(ctx => ctx.Request).Returns(request.Object);
            context.Setup(ctx => ctx.Response).Returns(response.Object);
            context.Setup(ctx => ctx.Session).Returns(session.Object);
            context.Setup(ctx => ctx.Server).Returns(server.Object);
            context.Setup(ctx => ctx.User).Returns(principal.Object);

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
