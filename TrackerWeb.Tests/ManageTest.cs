using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrackerWeb.Controllers;

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
