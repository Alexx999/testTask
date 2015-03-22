using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TrackerWeb.Controllers;
using TrackerWeb.Models;
using TrackerWeb.Results;
using TrackerWeb.Tests.Mocks;

namespace TrackerWeb.Tests
{
    [TestClass]
    public class AccountTest : MvcTestBase
    {
        private AccountController _controller;

        [TestInitialize]
        public new void Init()
        {
            base.Init();
            _controller = new AccountController(UserManager, SignInManager);
            SetupControllerForTests(_controller);
        }


        [TestMethod]
        public void TestAccountLoginView()
        {
            var result = _controller.Login("/Test");
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(viewResult.ViewName, "");
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
            var user = await UserManager.FindByEmailAsync(TestConfig.TestUserEmail);
            await UserManager.SetLockoutEnabledAsync(user.Id, true);
            await UserManager.SetLockoutEndDateAsync(user.Id, new DateTimeOffset(DateTime.UtcNow + new TimeSpan(1000,0,0,0)));

            var viewModel = new LoginViewModel {Email = TestConfig.TestUserEmail, Password = TestConfig.TestUserPassword};
            var result = await _controller.Login(viewModel, "/Test");
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(viewResult.ViewName, "Lockout");
        }

        [TestMethod]
        public async Task TestAccountLoginTwoFactor()
        {
            var user = await UserManager.FindByEmailAsync(TestConfig.TestUserEmail);
            await UserManager.SetTwoFactorEnabledAsync(user.Id, true);
            await UserManager.SetPhoneNumberAsync(user.Id, "+100000000000");
            await UserStore.SetPhoneNumberConfirmedAsync(user, true);

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
            Assert.AreSame(controller.UserManager, UserManager);
        }

        [TestMethod]
        public void TestSignInManagerGetter()
        {
            var controller = new AccountController();
            SetupControllerForTests(controller);
            Assert.AreSame(controller.SignInManager, SignInManager);
        }

        [TestMethod]
        public void TestAuthorize()
        {
            var result = _controller.Authorize();
            Assert.IsInstanceOfType(result, typeof(EmptyResult));
            AuthMock.Verify(auth => auth.SignIn(It.IsAny<ClaimsIdentity[]>()), Times.Once);
        }

        [TestMethod]
        public async Task TestVerifyCodeView()
        {
            var signInManager = new ApplicationSignInManager(UserManager, GetAuthenticationManager(true));
            var controller = new AccountController(UserManager, signInManager);
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
            var signIn = new Mock<ApplicationSignInManager>(UserManager, GetAuthenticationManager(true));
            signIn.Setup(
                si => si.TwoFactorSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(SignInStatus.Success);
            var controller = new AccountController(UserManager, signIn.Object);
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
            var signIn = new Mock<ApplicationSignInManager>(UserManager, GetAuthenticationManager(true));
            signIn.Setup(
                si => si.TwoFactorSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(SignInStatus.LockedOut);
            var controller = new AccountController(UserManager, signIn.Object);
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
            var viewResult = (ViewResult)result;
            Assert.AreEqual(viewResult.ViewName, "");
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
            await UserManager.DeleteAsync(await UserManager.FindByNameAsync(TestConfig.TestUserEmail));
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
            var user = await UserManager.FindByNameAsync(TestConfig.TestUserEmail);
            var userManager = new Mock<ApplicationUserManager>(UserStore);
            userManager.Setup(
                si => si.ConfirmEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new TestIdentityResult(false));
            var controller = new AccountController(userManager.Object, SignInManager);
            SetupControllerForTests(controller);

            var result = await controller.ConfirmEmail(user.Id, "");
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(viewResult.ViewName, "Error");
        }

        [TestMethod]
        public async Task TestConfirmEmailSuccess()
        {
            var userManager = new Mock<ApplicationUserManager>(UserStore);
            userManager.Setup(
                si => si.ConfirmEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new TestIdentityResult(true));
            var controller = new AccountController(userManager.Object, SignInManager);
            SetupControllerForTests(controller);

            var result = await controller.ConfirmEmail("", "");
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(viewResult.ViewName, "ConfirmEmail");
        }

        [TestMethod]
        public void TestForgotPasswordView()
        {
            var result = _controller.ForgotPassword();
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(viewResult.ViewName, "");
        }

        [TestMethod]
        public async Task TestForgotPasswordBadModelState()
        {
            var model = new ForgotPasswordViewModel();
            _controller.ModelState.AddModelError("Error", "SomeError");
            var result = await _controller.ForgotPassword(model);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(viewResult.ViewName, "");
        }

        //TODO: Password recovery not implemented yet
        [Ignore]
        [TestMethod]
        public async Task TestForgotPasswordSuccessForExisting()
        {
            var user = await UserManager.FindByEmailAsync(TestConfig.TestUserEmail);
            await UserStore.SetEmailConfirmedAsync(user, true);

            var model = new ForgotPasswordViewModel {Email = TestConfig.TestUserEmail};

            var result = await _controller.ForgotPassword(model);
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var redirectResult = (RedirectToRouteResult)result;
            Assert.AreEqual(redirectResult.RouteValues["action"], "ForgotPasswordConfirmation");
        }

        [TestMethod]
        public async Task TestForgotPasswordSuccessForNonConfirmed()
        {
            var model = new ForgotPasswordViewModel {Email = TestConfig.TestUserEmail};

            var result = await _controller.ForgotPassword(model);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(viewResult.ViewName, "ForgotPasswordConfirmation");
        }

        [TestMethod]
        public async Task TestForgotPasswordSuccessForNonExistent()
        {
            var model = new ForgotPasswordViewModel {Email = ""};

            var result = await _controller.ForgotPassword(model);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(viewResult.ViewName, "ForgotPasswordConfirmation");
        }

        [TestMethod]
        public void TestForgotPasswordConfirmationView()
        {
            var result = _controller.ForgotPasswordConfirmation();
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(viewResult.ViewName, "");
        }

        [TestMethod]
        public void TestResetPasswordView()
        {
            var result = _controller.ResetPassword("");
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(viewResult.ViewName, "");
        }

        [TestMethod]
        public void TestResetPasswordViewError()
        {
            var result = _controller.ResetPassword((string)null);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(viewResult.ViewName, "Error");
        }

        [TestMethod]
        public async Task TestResetPasswordBadModelState()
        {
            var model = new ResetPasswordViewModel();
            _controller.ModelState.AddModelError("Error", "SomeError");
            var result = await _controller.ResetPassword(model);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(viewResult.ViewName, "");
        }

        [TestMethod]
        public async Task TestResetPasswordSuccessForNonExistent()
        {
            var model = new ResetPasswordViewModel {Email = "", Code = "", Password = "", ConfirmPassword = ""};
            var result = await _controller.ResetPassword(model);
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var redirectResult = (RedirectToRouteResult)result;
            Assert.AreEqual(redirectResult.RouteValues["action"], "ResetPasswordConfirmation");
        }

        [TestMethod]
        public async Task TestResetPasswordSuccess()
        {
            var userManager = new Mock<ApplicationUserManager>(UserStore) {CallBase = true};
            userManager.Setup(
                si => si.ResetPasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new TestIdentityResult(true));
            var controller = new AccountController(userManager.Object, SignInManager);
            SetupControllerForTests(controller);

            var model = new ResetPasswordViewModel
            {
                Email = TestConfig.TestUserEmail,
                Code = "",
                Password = "",
                ConfirmPassword = ""
            };

            var result = await controller.ResetPassword(model);
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var redirectResult = (RedirectToRouteResult)result;
            Assert.AreEqual(redirectResult.RouteValues["action"], "ResetPasswordConfirmation");
        }

        [TestMethod]
        public async Task TestResetPasswordFail()
        {
            var userManager = new Mock<ApplicationUserManager>(UserStore) {CallBase = true};
            userManager.Setup(
                si => si.ResetPasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new TestIdentityResult(false));
            var controller = new AccountController(userManager.Object, SignInManager);
            SetupControllerForTests(controller);

            var model = new ResetPasswordViewModel
            {
                Email = TestConfig.TestUserEmail,
                Code = "",
                Password = "",
                ConfirmPassword = ""
            };

            var result = await controller.ResetPassword(model);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(viewResult.ViewName, "");
        }

        [TestMethod]
        public void TestResetPasswordConfirmationView()
        {
            var result = _controller.ResetPasswordConfirmation();
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(viewResult.ViewName, "");
        }

        [TestMethod]
        public void TestExternalLogin()
        {
            var result = _controller.ExternalLogin("", "");
            Assert.IsInstanceOfType(result, typeof(ChallengeResult));
            var challenge = (ChallengeResult) result;
            challenge.ExecuteResult(_controller.ControllerContext);
        }

        [TestMethod]
        public async Task TestSendCodeViewError()
        {
            var result = await _controller.SendCode("", false);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(viewResult.ViewName, "Error");
        }

        [TestMethod]
        public async Task TestSendCodeView()
        {
            UserManager.RegisterTwoFactorProvider("prov", new DataProtectorTokenProvider<ApplicationUser, string>(new TestDataProtection()));
            var signInManager = new ApplicationSignInManager(UserManager, GetAuthenticationManager(true));
            var controller = new AccountController(UserManager, signInManager);
            SetupControllerForTests(controller);

            var result = await controller.SendCode("", false);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(viewResult.ViewName, "");
        }

        [TestMethod]
        public async Task TestSendCodeBadModelState()
        {
            var model = new SendCodeViewModel();
            _controller.ModelState.AddModelError("Error", "Some Error");
            var result = await _controller.SendCode(model);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(viewResult.ViewName, "");
        }

        [TestMethod]
        public async Task TestSendCodeError()
        {
            var model = new SendCodeViewModel();
            var result = await _controller.SendCode(model);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(viewResult.ViewName, "Error");
        }

        [TestMethod]
        public async Task TestSendCodeSuccess()
        {
            var signIn = new Mock<ApplicationSignInManager>(UserManager, GetAuthenticationManager(true));
            signIn.Setup(
                si => si.SendTwoFactorCodeAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            var controller = new AccountController(UserManager, signIn.Object);
            SetupControllerForTests(controller);

            var model = new SendCodeViewModel();
            var result = await controller.SendCode(model);
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var redirectResult = (RedirectToRouteResult)result;
            Assert.AreEqual(redirectResult.RouteValues["action"], "VerifyCode");
        }

        [TestMethod]
        public async Task TestExternalLoginCallbackNoInfo()
        {
            var result = await _controller.ExternalLoginCallback("");
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var redirectResult = (RedirectToRouteResult)result;
            Assert.AreEqual(redirectResult.RouteValues["action"], "Login");
        }

        [TestMethod]
        public async Task TestExternalLoginCallbackNewUser()
        {
            var signInManager = new ApplicationSignInManager(UserManager, GetAuthenticationManager(false, true));
            var controller = new AccountController(UserManager, signInManager);
            SetupControllerForTests(controller);

            var result = await controller.ExternalLoginCallback("");
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(viewResult.ViewName, "ExternalLoginConfirmation");
        }

        [TestMethod]
        public async Task TestExternalLoginCallbackSuccess()
        {
            var user = await UserManager.FindByEmailAsync(TestConfig.TestUserEmail);
            var loginInfo = new UserLoginInfo("TestIssuer", user.Id);
            await UserManager.AddLoginAsync(user.Id, loginInfo);
            var signInManager = new ApplicationSignInManager(UserManager, GetAuthenticationManager(false, true));
            var controller = new AccountController(UserManager, signInManager);
            SetupControllerForTests(controller);

            var result = await controller.ExternalLoginCallback("/Test");
            Assert.IsInstanceOfType(result, typeof(RedirectResult));
            var redirectResult = (RedirectResult) result;
            Assert.AreEqual(redirectResult.Url, "/Test");
        }

        [TestMethod]
        public async Task TestExternalLoginCallbackLockout()
        {
            var user = await UserManager.FindByEmailAsync(TestConfig.TestUserEmail);
            var loginInfo = new UserLoginInfo("TestIssuer", user.Id);
            await UserManager.AddLoginAsync(user.Id, loginInfo);
            await UserManager.SetLockoutEnabledAsync(user.Id, true);
            await UserManager.SetLockoutEndDateAsync(user.Id, new DateTimeOffset(DateTime.UtcNow + new TimeSpan(1000, 0, 0, 0)));
            var signInManager = new ApplicationSignInManager(UserManager, GetAuthenticationManager(false, true));
            var controller = new AccountController(UserManager, signInManager);
            SetupControllerForTests(controller);

            var result = await controller.ExternalLoginCallback("/Test");
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(viewResult.ViewName, "Lockout");
        }

        [TestMethod]
        public async Task TestExternalLoginCallbackRequiresVerification()
        {
            var user = await UserManager.FindByEmailAsync(TestConfig.TestUserEmail);
            var loginInfo = new UserLoginInfo("TestIssuer", user.Id);
            await UserManager.AddLoginAsync(user.Id, loginInfo);
            await UserStore.SetTwoFactorEnabledAsync(user, true);
            UserManager.RegisterTwoFactorProvider("prov", new DataProtectorTokenProvider<ApplicationUser, string>(new TestDataProtection()));
            var signInManager = new ApplicationSignInManager(UserManager, GetAuthenticationManager(false, true));
            var controller = new AccountController(UserManager, signInManager);
            SetupControllerForTests(controller);

            var result = await controller.ExternalLoginCallback("/Test");
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var redirectResult = (RedirectToRouteResult)result;
            Assert.AreEqual(redirectResult.RouteValues["action"], "SendCode");
        }

        [TestMethod]
        public async Task TestExternalLoginConfirmationAuthenticatedUser()
        {
            var model = new ExternalLoginConfirmationViewModel();

            var controller = new AccountController(UserManager, SignInManager);
            SetupControllerForTests(controller, true);

            var result = await controller.ExternalLoginConfirmation(model, "/Test");
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var redirectResult = (RedirectToRouteResult)result;
            Assert.AreEqual(redirectResult.RouteValues["controller"], "Manage");
            Assert.AreEqual(redirectResult.RouteValues["action"], "Index");
        }

        [TestMethod] 
        public async Task TestExternalLoginConfirmationBadModelState()
        {
            var model = new ExternalLoginConfirmationViewModel();

            _controller.ModelState.AddModelError("Error", "Some Error");
            var result = await _controller.ExternalLoginConfirmation(model, "/Test");
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(viewResult.ViewName, "");
        }

        [TestMethod] 
        public async Task TestExternalLoginConfirmationFailure()
        {
            var model = new ExternalLoginConfirmationViewModel();

            var result = await _controller.ExternalLoginConfirmation(model, "/Test");
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(viewResult.ViewName, "ExternalLoginFailure");
        }

        [TestMethod] 
        public async Task TestExternalLoginConfirmationCreateUserFailure()
        {
            var userManager = new Mock<ApplicationUserManager>(UserStore) { CallBase = true };
            userManager.Setup(
                si => si.CreateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new TestIdentityResult(false));
            var signInManeger = new ApplicationSignInManager(userManager.Object, GetAuthenticationManager(false, true));
            var controller = new AccountController(userManager.Object, signInManeger);
            SetupControllerForTests(controller);

            var model = new ExternalLoginConfirmationViewModel();

            var result = await controller.ExternalLoginConfirmation(model, "/Test");
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(viewResult.ViewName, "");
        }

        [TestMethod]
        public async Task TestExternalLoginConfirmationAddLoginFailure()
        {
            var userManager = new Mock<ApplicationUserManager>(UserStore) { CallBase = true };
            userManager.Setup(
                si => si.CreateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new TestIdentityResult(true));
            userManager.Setup(
                si => si.AddLoginAsync(It.IsAny<string>(), It.IsAny<UserLoginInfo>()))
                .ReturnsAsync(new TestIdentityResult(false));
            var signInManeger = new ApplicationSignInManager(userManager.Object, GetAuthenticationManager(false, true));
            var controller = new AccountController(userManager.Object, signInManeger);
            SetupControllerForTests(controller);

            var model = new ExternalLoginConfirmationViewModel();

            var result = await controller.ExternalLoginConfirmation(model, "/Test");
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(viewResult.ViewName, "");
        }

        [TestMethod]
        public async Task TestExternalLoginConfirmationSuccess()
        {
            var userManager = new Mock<ApplicationUserManager>(UserStore) { CallBase = true };
            userManager.Setup(
                si => si.CreateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new TestIdentityResult(true));
            userManager.Setup(
                si => si.AddLoginAsync(It.IsAny<string>(), It.IsAny<UserLoginInfo>()))
                .ReturnsAsync(new TestIdentityResult(true));
            userManager.Setup(
                si => si.GetSecurityStampAsync(It.IsAny<string>()))
                .ReturnsAsync("TestStamp");
            var signInManeger = new ApplicationSignInManager(userManager.Object, GetAuthenticationManager(false, true));
            var controller = new AccountController(userManager.Object, signInManeger);
            SetupControllerForTests(controller);

            var model = new ExternalLoginConfirmationViewModel {Email = TestConfig.TestUserEmail, Name = "Test User"};

            var result = await controller.ExternalLoginConfirmation(model, "/Test");
            Assert.IsInstanceOfType(result, typeof(RedirectResult));
            var redirectResult = (RedirectResult)result;
            Assert.AreEqual(redirectResult.Url, "/Test");
        }

        [TestMethod]
        public void TestExternalLoginFailureView()
        {
            var result = _controller.ExternalLoginFailure();
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(viewResult.ViewName, "");
        }

        [TestMethod]
        public void TestLogOff()
        {
            var result = _controller.LogOff();
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var redirectResult = (RedirectToRouteResult)result;
            Assert.AreEqual(redirectResult.RouteValues["controller"], "Home");
            Assert.AreEqual(redirectResult.RouteValues["action"], "Index");
            AuthMock.Verify(auth => auth.SignOut(), Times.Once);
        }

        [TestMethod]
        public void TestLogOffDefault()
        {
            var controller = new AccountController();
            SetupControllerForTests(controller);
            var result = controller.LogOff();
            var redirectResult = (RedirectToRouteResult)result;
            Assert.AreEqual(redirectResult.RouteValues["controller"], "Home");
            Assert.AreEqual(redirectResult.RouteValues["action"], "Index");
        }

        [TestCleanup]
        public override void Cleanup()
        {
            _controller.Dispose();
            base.Cleanup();
        }

        protected override string GetControllerPath()
        {
            return "http://localhost/Account";
        }
    }
}
