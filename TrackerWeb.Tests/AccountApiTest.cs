using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
using System.Web.Http.Routing;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrackerWeb.Models;
using TrackerWeb.Controllers.API;
using Tracker.Models.Account;
using TrackerWeb.Tests.Mocks;

namespace TrackerWeb.Tests
{
    [TestClass]
    public class AccountApiTest
    {
        private ApplicationUserManager _userManager;
        private AccountController _controller;

        [TestInitialize]
        public void Init()
        {
            _userManager = ApplicationUserManager.Create(new TestUserStore<ApplicationUser>());
            _controller = new AccountController(_userManager);
            SetupControllerForTests(_controller);
        }

        [TestMethod]
        public async Task TestRegister()
        {
            var model = new RegisterModel {Email = TestConfig.TestUserEmail, Password = TestConfig.TestUserPassword, Name = "Test User"};
            var result = await _controller.Register(model);
            Assert.IsInstanceOfType(result, typeof(OkResult));
            var requestResult = await result.ExecuteAsync(new CancellationToken());
            Assert.IsTrue(requestResult.IsSuccessStatusCode);
            Assert.IsNotNull(_userManager.FindByEmail(TestConfig.TestUserEmail));
        }

        [TestMethod]
        public void TestUserManagerGetter()
        {
            var controller = new AccountController();
            SetupControllerForTests(controller);
            Assert.AreSame(controller.UserManager, _userManager);
        }

        [TestMethod]
        public async Task TestRegisterExisting()
        {
            var model = new RegisterModel { Email = TestConfig.TestUserEmail, Password = TestConfig.TestUserPassword, Name = "Test User" };
            await _controller.Register(model);
            Assert.IsNotNull(_userManager.FindByEmail(TestConfig.TestUserEmail));
            var result = await _controller.Register(model);
            Assert.IsInstanceOfType(result, typeof(InvalidModelStateResult));
            var requestResult = await result.ExecuteAsync(new CancellationToken());
            Assert.IsFalse(requestResult.IsSuccessStatusCode);
        }

        [TestMethod]
        public async Task TestRegisterBadViewState()
        {
            const string badEmail = "badEmail_bademail.com";
            _controller.ModelState.AddModelError("Email", "Wrong Email");
            var model = new RegisterModel { Email = badEmail, Password = TestConfig.TestUserPassword, Name = "Test User" };
            var result = await _controller.Register(model);
            Assert.IsInstanceOfType(result, typeof(InvalidModelStateResult));
            var requestResult = await result.ExecuteAsync(new CancellationToken());
            Assert.IsFalse(requestResult.IsSuccessStatusCode);
            Assert.IsNull(_userManager.FindByEmail(badEmail));
        }

        [TestMethod]
        public void TestValidation()
        {
            var model = new RegisterModel { Email = TestConfig.TestUserEmail, Password = TestConfig.TestUserPassword, Name = "Test User" };

            Assert.IsTrue(Validate(model));
        }

        [TestMethod]
        public void TestEmailValidation()
        {
            const string badEmail = "badEmail_bademail.com";
            var model = new RegisterModel { Email = badEmail, Password = TestConfig.TestUserPassword, Name = "Test User" };

            Assert.IsFalse(Validate(model));
        }

        [TestMethod]
        public void TestPasswordLengthValidation()
        {
            const string badPassword = "qwert";
            var model = new RegisterModel { Email = TestConfig.TestUserEmail, Password = badPassword, Name = "Test User" };

            Assert.IsFalse(Validate(model));
        }

        [TestMethod]
        public void TestErrorHelper()
        {
            var nullResult = _controller.GetErrorResult(null);
            Assert.IsInstanceOfType(nullResult, typeof(InternalServerErrorResult));

            var validResult = _controller.GetErrorResult(new TestIdentityResult(true));
            Assert.IsNull(validResult);

            var unknownErrorResult = _controller.GetErrorResult(new TestIdentityResult(false));
            Assert.IsInstanceOfType(unknownErrorResult, typeof(BadRequestResult));

            var knownErrorResult = _controller.GetErrorResult(new IdentityResult("Some Error Text"));
            Assert.IsInstanceOfType(knownErrorResult, typeof(InvalidModelStateResult));
        }

        [TestMethod]
        public async Task TestPasswordComplexityValidation()
        {
            var validator = _userManager.PasswordValidator;

            Assert.IsFalse((await validator.ValidateAsync("!wrT1")).Succeeded);
            Assert.IsFalse((await validator.ValidateAsync("qwerty")).Succeeded);
            Assert.IsFalse((await validator.ValidateAsync("!wert1")).Succeeded);
            Assert.IsFalse((await validator.ValidateAsync("!werTy")).Succeeded);
            Assert.IsFalse((await validator.ValidateAsync("qwerT1")).Succeeded);
            Assert.IsTrue((await validator.ValidateAsync("!werT1")).Succeeded);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _controller.Dispose();
            _userManager.Dispose();
        }

        private static bool Validate(RegisterModel model)
        {
            var context = new ValidationContext(model, null, null);
            var results = new List<ValidationResult>();
            return Validator.TryValidateObject(model, context, results, true);
        }

        private void SetupControllerForTests(ApiController controller)
        {
            var config = new HttpConfiguration();
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/api/Account");
            var route = config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{id}");
            var routeData = new HttpRouteData(route, new HttpRouteValueDictionary { { "controller", "Account" } });

            controller.ControllerContext = new HttpControllerContext(config, routeData, request);
            controller.Request = request;
            controller.Request.SetConfiguration(config);
            var owinContext = new OwinContext();
            owinContext.Set(_userManager);
            request.Properties["MS_OwinContext"] = owinContext;
        }

        private class TestIdentityResult:IdentityResult
        {
            public TestIdentityResult(bool success)
                : base(success)
            {
            }
        }
    }
}
