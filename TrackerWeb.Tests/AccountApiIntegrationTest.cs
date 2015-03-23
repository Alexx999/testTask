using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Tracker.Models.Account;
using TrackerWeb.Models;

namespace TrackerWeb.Tests
{
    [TestClass]
    public class AccountApiIntegrationTest
    {
        private ApplicationUserManager _userManager = ApplicationUserManager.Create(new UserStore<ApplicationUser>(ApplicationDbContext.Create()));

        [TestInitialize]
        public void Init()
        {
            EnsureUserDontExist().Wait();
        }

        [TestMethod]
        public async Task RegisterUser()
        {
            var request = WebRequest.CreateHttp(TestConfig.AppUrl + "api/Account");
            request.Method = "POST";

            var model = new RegisterModel
            {
                Email = TestConfig.TestUserEmail,
                Password = TestConfig.TestUserPassword,
                Name = "Test User"
            };

            string postData = JsonConvert.SerializeObject(model);
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentType = "application/json; charset=utf-8";
            request.ContentLength = byteArray.Length;

            using (var dataStream = await request.GetRequestStreamAsync())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
            }


            using (var response = (HttpWebResponse)await request.GetResponseAsync())
            {
                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
                Assert.AreEqual(response.ContentLength, 0);
            }

            var user = await _userManager.FindByEmailAsync(TestConfig.TestUserEmail);
            Assert.IsNotNull(user);
        }

        [TestCleanup]
        public void Cleanup()
        {
            EnsureUserDontExist().Wait();

            _userManager.Dispose();
        }

        private async Task EnsureUserDontExist()
        {
            var user = await _userManager.FindByEmailAsync(TestConfig.TestUserEmail);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
        }
    }
}
