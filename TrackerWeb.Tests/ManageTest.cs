using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TrackerWeb.Controllers;
using TrackerWeb.Models;
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
            Assert.AreSame(controller.UserManager, UserManager);
        }

        [TestMethod]
        public void TestSignInManagerGetter()
        {
            var controller = new ManageController();
            SetupControllerForTests(controller);
            Assert.AreSame(controller.SignInManager, SignInManager);
        }

        [TestMethod]
        public async Task TestIndexView()
        {
            var result = await _controller.Index(null);
            Assert.AreEqual(_controller.ViewBag.StatusMessage, "");
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(viewResult.ViewName, "");
        }

        [TestMethod]
        public async Task TestIndexViewMessages()
        {
            var values = Enum.GetValues(typeof(ManageController.ManageMessageId)).Cast<ManageController.ManageMessageId>();
            foreach (var value in values)
            {
                await _controller.Index(value);
                Assert.AreNotEqual(_controller.ViewBag.StatusMessage, "", string.Format("Message for enum value {0} must not be empty", value));
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
            Assert.AreEqual(redirectResult.RouteValues["action"], "ManageLogins");
            Assert.AreEqual(redirectResult.RouteValues["Message"], ManageController.ManageMessageId.Error);
        }

        [TestMethod]
        public async Task TestRemoveLoginSuccess()
        {
            var result = await _controller.RemoveLogin("", "");
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var redirectResult = (RedirectToRouteResult)result;
            Assert.AreEqual(redirectResult.RouteValues["action"], "ManageLogins");
            Assert.AreEqual(redirectResult.RouteValues["Message"], ManageController.ManageMessageId.RemoveLoginSuccess);
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
