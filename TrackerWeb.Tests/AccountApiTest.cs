using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Microsoft.AspNet.Identity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrackerWeb.Models;
using TrackerWeb.Controllers.API;
using Tracker.Models.Account;
using TrackerWeb.Tests.Mock;

namespace TrackerWeb.Tests
{
    [TestClass]
    public class AccountApiTest
    {
        private ApplicationUserManager _userManager = ApplicationUserManager.Create(new TestUserStore<ApplicationUser>());

        [TestInitialize]
        public void Init()
        {
        }

        [TestMethod]
        public async Task TestRegister()
        {
            var controller = new AccountController(_userManager);
            SetupControllerForTests(controller);
            var model = new RegisterModel {Email = TestConfig.TestUserEmail, Password = TestConfig.TestUserPassword, Name = "Test User"};
            var result = await controller.Register(model);
            var requestResult = await result.ExecuteAsync(new CancellationToken());
            Assert.IsTrue(requestResult.IsSuccessStatusCode);
            Assert.IsNotNull(_userManager.FindByEmail(TestConfig.TestUserEmail));
        }

        [TestMethod]
        public async Task TestRegisterExisting()
        {
            var controller = new AccountController(_userManager);
            SetupControllerForTests(controller);
            var model = new RegisterModel { Email = TestConfig.TestUserEmail, Password = TestConfig.TestUserPassword, Name = "Test User" };
            await controller.Register(model);
            Assert.IsNotNull(_userManager.FindByEmail(TestConfig.TestUserEmail));
            var result = await controller.Register(model);
            var requestResult = await result.ExecuteAsync(new CancellationToken());
            Assert.IsFalse(requestResult.IsSuccessStatusCode);
        }

        [TestMethod]
        public async Task TestRegisterBadViewState()
        {
            var badEmail = "badEmail_bademail.com";
            var controller = new AccountController(_userManager);
            SetupControllerForTests(controller);
            controller.ModelState.AddModelError("Email", "Wrong Email");
            var model = new RegisterModel { Email = badEmail, Password = TestConfig.TestUserPassword, Name = "Test User" };
            var result = await controller.Register(model);
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

        private static bool Validate(RegisterModel model)
        {
            var context = new ValidationContext(model, null, null);
            var results = new List<ValidationResult>();
            return Validator.TryValidateObject(model, context, results, true);
        }

        private static void SetupControllerForTests(ApiController controller)
        {
            var config = new HttpConfiguration();
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/api/Account");
            var route = config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{id}");
            var routeData = new HttpRouteData(route, new HttpRouteValueDictionary { { "controller", "Account" } });

            controller.ControllerContext = new HttpControllerContext(config, routeData, request);
            controller.Request = request;
            controller.Request.SetConfiguration(config);
        }
    }
}
