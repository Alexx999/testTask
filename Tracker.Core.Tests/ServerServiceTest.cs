using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tracker.Core.Services;
using Tracker.Models.Account;

namespace Tracker.Core.Tests
{
    [TestClass]
    public class ServerServiceTest
    {
        private ServerService _serverService;
        private UserHelper _helper = new UserHelper();

        [TestInitialize]
        public void Init()
        {
            _serverService = new ServerService(TestConfig.AppUrl);
        }

        [TestMethod]
        public async Task RegisterUserTest()
        {
            await _helper.EnsureUserDontExist(TestConfig.TestUserEmail);

            var model = new RegisterModel
            {
                Email = TestConfig.TestUserEmail,
                Password = TestConfig.TestUserPassword,
                Name = "Test User"
            };

            var result = await _serverService.RegisterUser(model);

            Assert.IsTrue(result);

            var user = await _helper.UserExists(TestConfig.TestUserEmail);
            Assert.IsNotNull(user);


            await _helper.EnsureUserDontExist(TestConfig.TestUserEmail);
        }

        [TestMethod]
        public async Task LoginTest()
        {
            await _helper.EnsureUserExists(TestConfig.TestUserEmail);

            var result = await _serverService.Login(TestConfig.TestUserEmail, TestConfig.TestUserPassword);

            Assert.IsTrue(result);

            await _helper.EnsureUserDontExist(TestConfig.TestUserEmail);
        }
    }
}
