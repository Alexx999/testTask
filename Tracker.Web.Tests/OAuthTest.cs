using System;
using System.Collections.Generic;
using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tracker.Web.Providers;

namespace Tracker.Web.Tests
{
    [TestClass]
    public class OAuthTest
    {
        [TestMethod]
        public void TestConstructor()
        {
            var provider = new ApplicationOAuthProvider("");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestConstructorArgNull()
        {
            var provider = new ApplicationOAuthProvider(null);
        }

        [TestMethod]
        public void TestValidateWrongId()
        {
            var provider = new ApplicationOAuthProvider("1");
            var context = new OAuthValidateClientRedirectUriContext(new OwinContext(),
                new OAuthAuthorizationServerOptions(), "2", "/Test");
            provider.ValidateClientRedirectUri(context);
            Assert.IsFalse(context.IsValidated);
        }

        [TestMethod]
        public void TestValidateFail()
        {
            var provider = new ApplicationOAuthProvider("1");
            var dict = new Dictionary<string, object>()
            {
                {"owin.RequestScheme", "http"},
                {"Host", "localhost"},
                {"owin.RequestPathBase", "/"},
                {"owin.RequestPath", ""},
                {"owin.RequestQueryString", ""},
                {"owin.RequestHeaders", new Dictionary<string, string[]>()}
            };

            var context = new OAuthValidateClientRedirectUriContext(new OwinContext(dict),
                new OAuthAuthorizationServerOptions(), "1", "/Test");
            provider.ValidateClientRedirectUri(context);
            Assert.IsFalse(context.IsValidated);
        }

        [TestMethod]
        public void TestValidateFail2()
        {
            var provider = new ApplicationOAuthProvider("web");
            var dict = new Dictionary<string, object>()
            {
                {"owin.RequestScheme", "http"},
                {"Host", "localhost"},
                {"owin.RequestPathBase", ""},
                {"owin.RequestPath", "/Test"},
                {"owin.RequestQueryString", ""},
                {"owin.RequestHeaders", new Dictionary<string, string[]>()}
            };

            var context = new OAuthValidateClientRedirectUriContext(new OwinContext(dict),
                new OAuthAuthorizationServerOptions(), "web", "http://localhost");
            provider.ValidateClientRedirectUri(context);
            Assert.IsFalse(context.IsValidated);
        }

        [TestMethod]
        public void TestValidateFail3()
        {
            var provider = new ApplicationOAuthProvider("1");
            var dict = new Dictionary<string, object>()
            {
                {"owin.RequestScheme", "http"},
                {"Host", "localhost"},
                {"owin.RequestPathBase", "/"},
                {"owin.RequestPath", ""},
                {"owin.RequestQueryString", ""},
                {"owin.RequestHeaders", new Dictionary<string, string[]>()}
            };

            var context = new OAuthValidateClientRedirectUriContext(new OwinContext(dict),
                new OAuthAuthorizationServerOptions(), "1", "");
            provider.ValidateClientRedirectUri(context);
            Assert.IsFalse(context.IsValidated);
        }

        [TestMethod]
        public void TestValidateSuccess()
        {
            var provider = new ApplicationOAuthProvider("1");
            var dict = new Dictionary<string, object>()
            {
                {"owin.RequestScheme", "http"},
                {"Host", "localhost"},
                {"owin.RequestPathBase", "/"},
                {"owin.RequestPath", ""},
                {"owin.RequestQueryString", ""},
                {"owin.RequestHeaders", new Dictionary<string, string[]>()}
            };

            var context = new OAuthValidateClientRedirectUriContext(new OwinContext(dict),
                new OAuthAuthorizationServerOptions(), "1", "http://localhost/");
            provider.ValidateClientRedirectUri(context);
            Assert.IsTrue(context.IsValidated);
        }

        [TestMethod]
        public void TestValidateWeb()
        {
            var provider = new ApplicationOAuthProvider("web");
            var dict = new Dictionary<string, object>()
            {
                {"owin.RequestScheme", "http"},
                {"Host", "localhost"},
                {"owin.RequestPathBase", "/"},
                {"owin.RequestPath", ""},
                {"owin.RequestQueryString", ""},
                {"owin.RequestHeaders", new Dictionary<string, string[]>()}
            };

            var context = new OAuthValidateClientRedirectUriContext(new OwinContext(dict),
                new OAuthAuthorizationServerOptions(), "web", "");
            provider.ValidateClientRedirectUri(context);
            Assert.IsTrue(context.IsValidated);
        }
    }
}
