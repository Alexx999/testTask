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
        private ApplicationUserManager _userManager = ApplicationUserManager.Create(new UserStore<ApplicationUser>(new ApplicationDbContext()));

        [TestMethod]
        public async Task RegisterUser()
        {
            await EnsureUserDontExist();
            var request = WebRequest.CreateHttp(TestConfig.AppUrl + "api/Account/Register");
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

            using (var dataStream = request.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
            }


            using (var response = (HttpWebResponse)request.GetResponse())
            {
                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
                Assert.AreEqual(response.ContentLength, 0);
            }
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
