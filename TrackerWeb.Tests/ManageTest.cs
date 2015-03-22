using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Principal;
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

        [TestCleanup]
        protected override void Cleanup()
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
