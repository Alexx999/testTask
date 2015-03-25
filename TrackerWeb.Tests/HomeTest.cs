using System;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrackerWeb.Controllers;

namespace TrackerWeb.Tests
{
    [TestClass]
    public class HomeTest
    {
        [TestMethod]
        public void TestHomeIndex()
        {
            var controller = new HomeController();
            var result = controller.Index();
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(string.Empty, viewResult.ViewName);
        }
    }
}
