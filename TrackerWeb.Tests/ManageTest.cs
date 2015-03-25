using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TrackerWeb.Controllers;
using TrackerWeb.Models;
using TrackerWeb.Results;
using TrackerWeb.Tests.Mocks;

namespace TrackerWeb.Tests
{
    [TestClass]
    public class ManageTest : MvcTestBase
    {
        private ManageController _controller;

        [TestInitialize]
        public new void Init()
        {
            base.Init();
            _controller = new ManageController(UserManager, SignInManager);
            SetupControllerForTests(_controller, true);
        }

        [TestMethod]
        public void TestUserManagerGetter()
        {
            var controller = new ManageController();
            SetupControllerForTests(controller);
            Assert.AreSame(UserManager, controller.UserManager);
        }

        [TestMethod]
        public void TestSignInManagerGetter()
        {
            var controller = new ManageController();
            SetupControllerForTests(controller);
            Assert.AreSame(SignInManager, controller.SignInManager);
        }

        [TestMethod]
        public async Task TestIndexView()
        {
            var result = await _controller.Index(null);
            Assert.AreEqual(string.Empty, _controller.ViewBag.StatusMessage);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(string.Empty, viewResult.ViewName);
        }

        [TestMethod]
        public async Task TestIndexViewMessages()
        {
            var values = Enum.GetValues(typeof(ManageController.ManageMessageId)).Cast<ManageController.ManageMessageId>();
            foreach (var value in values)
            {
                await _controller.Index(value);
                Assert.AreNotEqual("", _controller.ViewBag.StatusMessage, string.Format("Message for enum value {0} must not be empty", value));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task TestIndexBadUserId()
        {
            var controller = new ManageController(UserManager, SignInManager);
            Identity = CreateClaimsIdentity("qwerty", "qwerty", "qwerty");
            SetupControllerForTests(controller, true);

            await controller.Index(null);
        }

        [TestMethod]
        public async Task TestRemoveLoginError()
        {
            var userManager = new Mock<ApplicationUserManager>(UserStore) { CallBase = true };
            userManager.Setup(
                si => si.RemoveLoginAsync(It.IsAny<string>(), It.IsAny<UserLoginInfo>()))
                .ReturnsAsync(new TestIdentityResult(false));
            var signInManeger = new ApplicationSignInManager(userManager.Object, GetAuthenticationManager(false, true));
            var controller = new ManageController(userManager.Object, signInManeger);
            SetupControllerForTests(controller);

            var result = await controller.RemoveLogin("", "");
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var redirectResult = (RedirectToRouteResult)result;
            Assert.AreEqual("ManageLogins", redirectResult.RouteValues["action"]);
            Assert.AreEqual(ManageController.ManageMessageId.Error, redirectResult.RouteValues["Message"]);
        }

        [TestMethod]
        public async Task TestRemoveLoginSuccess()
        {
            var result = await _controller.RemoveLogin("", "");
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var redirectResult = (RedirectToRouteResult)result;
            Assert.AreEqual("ManageLogins", redirectResult.RouteValues["action"]);
            Assert.AreEqual(ManageController.ManageMessageId.RemoveLoginSuccess, redirectResult.RouteValues["Message"]);
        }

        [TestMethod]
        public void TestAddPhoneNumberView()
        {
            var result = _controller.AddPhoneNumber();
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(string.Empty, viewResult.ViewName);
        }

        [TestMethod]
        public async Task TestAddPhoneNumberBadModelState()
        {
            var model = new AddPhoneNumberViewModel();

            _controller.ModelState.AddModelError("Error", "Some Error");
            var result = await _controller.AddPhoneNumber(model);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(string.Empty, viewResult.ViewName);
        }

        [TestMethod]
        public async Task TestAddPhoneNumberSuccess()
        {
            var model = new AddPhoneNumberViewModel {Number = "0000000000"};

            var result = await _controller.AddPhoneNumber(model);
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var redirectResult = (RedirectToRouteResult)result;
            Assert.AreEqual("VerifyPhoneNumber", redirectResult.RouteValues["action"]);
        }

        [TestMethod]
        public async Task TestEnableTwoFactorAuthentication()
        {
            var result = await _controller.EnableTwoFactorAuthentication();
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var redirectResult = (RedirectToRouteResult)result;
            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);
            Assert.AreEqual("Manage", redirectResult.RouteValues["controller"]);
        }

        [TestMethod]
        public async Task TestDisableTwoFactorAuthentication()
        {
            var result = await _controller.DisableTwoFactorAuthentication();
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var redirectResult = (RedirectToRouteResult)result;
            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);
            Assert.AreEqual("Manage", redirectResult.RouteValues["controller"]);
        }

        [TestMethod]
        public void TestVerifyPhoneNumberViewError()
        {
            var result = _controller.VerifyPhoneNumber((string)null);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual("Error", viewResult.ViewName);
        }

        [TestMethod]
        public void TestVerifyPhoneNumberView()
        {
            var result = _controller.VerifyPhoneNumber("0000000");
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(string.Empty, viewResult.ViewName);
        }

        [TestMethod]
        public async Task TestVerifyPhoneNumberBadModelState()
        {
            var model = new VerifyPhoneNumberViewModel();

            _controller.ModelState.AddModelError("Error", "Some Error");
            var result = await _controller.VerifyPhoneNumber(model);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(string.Empty, viewResult.ViewName);
        }

        [TestMethod]
        public async Task TestVerifyPhoneNumberFailure()
        {
            var model = new VerifyPhoneNumberViewModel();

            var result = await _controller.VerifyPhoneNumber(model);
            Assert.IsFalse(_controller.ModelState.IsValid);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(string.Empty, viewResult.ViewName);
        }

        [TestMethod]
        public async Task TestVerifyPhoneNumberSuccess()
        {
            var userManager = new Mock<ApplicationUserManager>(UserStore) { CallBase = true };
            userManager.Setup(
                si => si.ChangePhoneNumberAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new TestIdentityResult(true));
            var signInManeger = new ApplicationSignInManager(userManager.Object, GetAuthenticationManager(false, true));
            var controller = new ManageController(userManager.Object, signInManeger);
            SetupControllerForTests(controller, true);

            var model = new VerifyPhoneNumberViewModel();

            var result = await controller.VerifyPhoneNumber(model);
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var redirectResult = (RedirectToRouteResult)result;
            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);
            Assert.AreEqual(ManageController.ManageMessageId.AddPhoneSuccess, redirectResult.RouteValues["Message"]);
        }

        [TestMethod]
        public async Task TestRemovePhoneNumberSuccess()
        {
            var result = await _controller.RemovePhoneNumber();
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var redirectResult = (RedirectToRouteResult)result;
            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);
            Assert.AreEqual(ManageController.ManageMessageId.RemovePhoneSuccess, redirectResult.RouteValues["Message"]);
        }

        [TestMethod]
        public async Task TestRemovePhoneNumberFailure()
        {
            var userManager = new Mock<ApplicationUserManager>(UserStore) { CallBase = true };
            userManager.Setup(
                si => si.SetPhoneNumberAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new TestIdentityResult(false));
            var signInManeger = new ApplicationSignInManager(userManager.Object, GetAuthenticationManager(false, true));
            var controller = new ManageController(userManager.Object, signInManeger);
            SetupControllerForTests(controller, true);

            var result = await controller.RemovePhoneNumber();
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var redirectResult = (RedirectToRouteResult)result;
            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);
            Assert.AreEqual(ManageController.ManageMessageId.Error, redirectResult.RouteValues["Message"]);
        }

        [TestMethod]
        public void TestChangePasswordView()
        {
            var result = _controller.ChangePassword();
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(string.Empty, viewResult.ViewName);
        }

        [TestMethod]
        public async Task TestChangePasswordBadModelState()
        {
            var model = new ChangePasswordViewModel();

            _controller.ModelState.AddModelError("Error", "Some Error");
            var result = await _controller.ChangePassword(model);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(string.Empty, viewResult.ViewName);
        }

        [TestMethod]
        public async Task TestChangePasswordSuccess()
        {
            const string pass = "Q`1werty";
            var model = new ChangePasswordViewModel()
            {
                OldPassword = TestConfig.TestUserPassword,
                NewPassword = pass,
                ConfirmPassword = pass
            };

            var result = await _controller.ChangePassword(model);
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var redirectResult = (RedirectToRouteResult)result;
            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);
            Assert.AreEqual(ManageController.ManageMessageId.ChangePasswordSuccess, redirectResult.RouteValues["Message"]);
        }

        [TestMethod]
        public async Task TestChangePasswordFailure()
        {
            const string pass = "Q`1werty";
            var model = new ChangePasswordViewModel()
            {
                OldPassword = pass,
                NewPassword = pass,
                ConfirmPassword = pass
            };

            var result = await _controller.ChangePassword(model);
            Assert.IsFalse(_controller.ModelState.IsValid);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(string.Empty, viewResult.ViewName);
        }

        [TestMethod]
        public void TestSetPasswordView()
        {
            var result = _controller.SetPassword();
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(string.Empty, viewResult.ViewName);
        }

        [TestMethod]
        public async Task TestSetPasswordBadModelState()
        {
            var model = new SetPasswordViewModel();

            _controller.ModelState.AddModelError("Error", "Some Error");
            var result = await _controller.SetPassword(model);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(string.Empty, viewResult.ViewName);
        }

        [TestMethod]
        public async Task TestSetPasswordFailure()
        {
            const string pass = "Q`1werty";
            var model = new SetPasswordViewModel()
            {
                NewPassword = pass,
                ConfirmPassword = pass
            };

            var result = await _controller.SetPassword(model);
            Assert.IsFalse(_controller.ModelState.IsValid);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(string.Empty, viewResult.ViewName);
        }

        [TestMethod]
        public async Task TestSetPasswordSuccess()
        {
            var userManager = new Mock<ApplicationUserManager>(UserStore) { CallBase = true };
            userManager.Setup(
                si => si.AddPasswordAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new TestIdentityResult(true));
            var signInManeger = new ApplicationSignInManager(userManager.Object, GetAuthenticationManager(false, true));
            var controller = new ManageController(userManager.Object, signInManeger);
            SetupControllerForTests(controller, true);

            var model = new SetPasswordViewModel();

            var result = await controller.SetPassword(model);
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var redirectResult = (RedirectToRouteResult)result;
            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);
            Assert.AreEqual(ManageController.ManageMessageId.SetPasswordSuccess, redirectResult.RouteValues["Message"]);
        }

        [TestMethod]
        public async Task TestManageLoginsViewError()
        {
            var controller = new ManageController(UserManager, SignInManager);
            SetupControllerForTests(controller);

            var result = await controller.ManageLogins(null);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual("Error", viewResult.ViewName);
        }

        [TestMethod]
        public async Task TestManageLoginsViewMessages()
        {
            await _controller.ManageLogins(null);
            Assert.AreEqual(string.Empty, _controller.ViewBag.StatusMessage);
            var values = Enum.GetValues(typeof(ManageController.ManageMessageId)).Cast<ManageController.ManageMessageId>();
            foreach (var value in values)
            {
                await _controller.Index(value);
                Assert.AreNotEqual("", _controller.ViewBag.StatusMessage, string.Format("Message for enum value {0} must not be empty", value));
            }
        }

        [TestMethod]
        public async Task TestManageLoginsViewSuccess()
        {
            var user = await UserManager.FindByEmailAsync(TestConfig.TestUserEmail);
            await UserManager.AddLoginAsync(user.Id, new UserLoginInfo("Prov1", "key1"));

            var result = await _controller.ManageLogins(null);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(string.Empty, viewResult.ViewName);
        }

        [TestMethod]
        public async Task TestManageLoginsViewDefault()
        {
            var controller = new ManageController();
            SetupControllerForTests(controller, true);

            var result = await controller.ManageLogins(null);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(string.Empty, viewResult.ViewName);
        }

        [TestMethod]
        public void TestLinkLoginView()
        {
            var result = _controller.LinkLogin("");
            Assert.IsInstanceOfType(result, typeof(ChallengeResult));
        }

        [TestMethod]
        public async Task TestLinkLoginCallbackFailure()
        {
            var result = await _controller.LinkLoginCallback();
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var redirectResult = (RedirectToRouteResult)result;
            Assert.AreEqual("ManageLogins", redirectResult.RouteValues["action"]);
            Assert.AreEqual(ManageController.ManageMessageId.Error, redirectResult.RouteValues["Message"]);
        }

        [TestMethod]
        public async Task TestLinkLoginCallbackSuccess()
        {
            var signInManager = new ApplicationSignInManager(UserManager, GetAuthenticationManager(false, true));
            var controller = new ManageController(UserManager, signInManager);
            SetupControllerForTests(controller, true);

            var result = await controller.LinkLoginCallback();
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var redirectResult = (RedirectToRouteResult)result;
            Assert.AreEqual("ManageLogins", redirectResult.RouteValues["action"]);
        }

        [TestMethod]
        public async Task TestLinkLoginCallbackAddLoginFailure()
        {
            var userManager = new Mock<ApplicationUserManager>(UserStore) { CallBase = true };
            userManager.Setup(
                si => si.AddLoginAsync(It.IsAny<string>(), It.IsAny<UserLoginInfo>()))
                .ReturnsAsync(new TestIdentityResult(false));
            var signInManeger = new ApplicationSignInManager(userManager.Object, GetAuthenticationManager(false, true));
            var controller = new ManageController(userManager.Object, signInManeger);
            SetupControllerForTests(controller, true);

            var result = await controller.LinkLoginCallback();
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var redirectResult = (RedirectToRouteResult)result;
            Assert.AreEqual("ManageLogins", redirectResult.RouteValues["action"]);
            Assert.AreEqual(ManageController.ManageMessageId.Error, redirectResult.RouteValues["Message"]);
        }

        [TestCleanup]
        public override void Cleanup()
        {
            _controller.Dispose();
            base.Cleanup();
        }

        protected override string GetControllerPath()
        {
            return "http://localhost/Manage";
        }
    }
}
