using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Results;
using Microsoft.AspNet.Identity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrackerWeb.Controllers.API;
using Tracker.Models.Account;
using TrackerWeb.Tests.Mocks;

namespace TrackerWeb.Tests
{
    [TestClass]
    public class AccountApiTest : ApiTestBase
    {
        private AccountController _controller;

        [TestInitialize]
        public new void Init()
        {
            base.Init();
            _controller = new AccountController(UserManager);
            SetupControllerForTests(_controller);
        }

        [TestMethod]
        public async Task TestRegister()
        {
            var user = await UserManager.FindByEmailAsync(TestConfig.TestUserEmail);
            await UserManager.DeleteAsync(user);
            var model = new RegisterModel {Email = TestConfig.TestUserEmail, Password = TestConfig.TestUserPassword, Name = "Test User"};
            var result = await _controller.PostAccount(model);
            Assert.IsInstanceOfType(result, typeof(OkResult));
            var requestResult = await result.ExecuteAsync(new CancellationToken());
            Assert.IsTrue(requestResult.IsSuccessStatusCode);
            Assert.IsNotNull(UserManager.FindByEmail(TestConfig.TestUserEmail));
        }

        [TestMethod]
        public void TestUserManagerGetter()
        {
            var controller = new AccountController();
            SetupControllerForTests(controller);
            Assert.AreSame(UserManager, controller.UserManager);
        }

        [TestMethod]
        public async Task TestRegisterExisting()
        {
            var model = new RegisterModel { Email = TestConfig.TestUserEmail, Password = TestConfig.TestUserPassword, Name = "Test User" };
            await _controller.PostAccount(model);
            Assert.IsNotNull(UserManager.FindByEmail(TestConfig.TestUserEmail));
            var result = await _controller.PostAccount(model);
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
            var result = await _controller.PostAccount(model);
            Assert.IsInstanceOfType(result, typeof(InvalidModelStateResult));
            var requestResult = await result.ExecuteAsync(new CancellationToken());
            Assert.IsFalse(requestResult.IsSuccessStatusCode);
            Assert.IsNull(UserManager.FindByEmail(badEmail));
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
            var validator = UserManager.PasswordValidator;

            Assert.IsFalse((await validator.ValidateAsync("!wrT1")).Succeeded);
            Assert.IsFalse((await validator.ValidateAsync("qwerty")).Succeeded);
            Assert.IsFalse((await validator.ValidateAsync("!wert1")).Succeeded);
            Assert.IsFalse((await validator.ValidateAsync("!werTy")).Succeeded);
            Assert.IsFalse((await validator.ValidateAsync("qwerT1")).Succeeded);
            Assert.IsTrue((await validator.ValidateAsync("!werT1")).Succeeded);
        }

        [TestCleanup]
        public override void Cleanup()
        {
            _controller.Dispose();
            base.Cleanup();
        }

        private static bool Validate(RegisterModel model)
        {
            var context = new ValidationContext(model, null, null);
            var results = new List<ValidationResult>();
            return Validator.TryValidateObject(model, context, results, true);
        }

        protected override string GetControllerPath()
        {
            return "http://localhost/api/Account";
        }

    }
}
