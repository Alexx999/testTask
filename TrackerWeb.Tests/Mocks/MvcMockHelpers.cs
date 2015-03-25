using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;

namespace Tracker.Web.Tests.Mocks
{
    /// <summary>
    /// This helper class can be used to set up Moq mocks of MVC3/4 controllers.
    /// Slightly modified from Original URL https://gist.github.com/1578697 
    /// which is slightly modified from the original version from Scott Hanselman's blog:
    /// http://www.hanselman.com/blog/ASPNETMVCSessionAtMix08TDDAndMvcMockHelpers.aspx
    /// Added a mechanism to return all mocked objects if you want to act on them
    /// </summary>
    public static class MvcMockHelpers
    {
        public static Mock<T> GetMock<T>(this Dictionary<Type,Mock> list) where T : class
        {
            return  (Mock<T>) list[typeof(T)];
        }

        public static Mock<T> GetMockedObjectUsing<T>(this Controller controller, Dictionary<Type, Mock> mockedList) where T : class
        {
            if (mockedList == null) throw new ArgumentNullException("mockedList");
            return mockedList.GetMock<T>();
        }


        public static Type GetGenericType<T>(this T @object)
        {
            Type type = typeof(T);
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Mock<>))
            {
                return type.GetGenericArguments()[0]; 
            }
            return null;
        }

        public static void Add<T>(this Dictionary<Type, Mock> dictionary, Mock<T> @object) where T : class
        {
            dictionary.Add(@object.GetGenericType(), @object);
        }


        public static HttpContextBase FakeHttpContext(out Dictionary<Type, Mock> mockedList)
        {
            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            var response = new Mock<HttpResponseBase>();
            var session = new Mock<HttpSessionStateBase>();
            var server = new Mock<HttpServerUtilityBase>();

            mockedList = new Dictionary<Type, Mock>(); // This to create a dictionary of all mocked objects 
            mockedList.Add(context);
            mockedList.Add(request);
            mockedList.Add(response);
            mockedList.Add(session);
            mockedList.Add(server);

            context.Setup(ctx => ctx.Request).Returns(request.Object);
            context.Setup(ctx => ctx.Response).Returns(response.Object);
            context.SetupGet(ctx => ctx.Response).Returns(response.Object);
            context.Setup(ctx => ctx.Session).Returns(session.Object);
            context.Setup(ctx => ctx.Server).Returns(server.Object);
            context.SetupProperty(ctx => ctx.User);

            return context.Object;
        }

        public static HttpContextBase FakeHttpContext()
        {
            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            var response = new Mock<HttpResponseBase>();
            var session = new Mock<HttpSessionStateBase>();
            var server = new Mock<HttpServerUtilityBase>();

            context.Setup(ctx => ctx.Request).Returns(request.Object);
            context.Setup(ctx => ctx.Response).Returns(response.Object);
            context.SetupGet(ctx => ctx.Response).Returns(response.Object);
            context.Setup(ctx => ctx.Session).Returns(session.Object);
            context.Setup(ctx => ctx.Server).Returns(server.Object);
            context.SetupProperty(ctx => ctx.User);

            return context.Object;
        }

        public static HttpContextBase FakeHttpContext(string url, out Dictionary<Type, Mock> mockedList)
        {
            var context = FakeHttpContext(out mockedList);
            context.Request.SetupRequestUrl(url);
            return context;
        }

        public static HttpContextBase FakeHttpContext(string url)
        {
            var context = FakeHttpContext();
            context.Request.SetupRequestUrl(url);
            return context;
        }

        public static Dictionary<Type, Mock> SetFakeControllerContextAndReturnMocksList(this Controller controller, string requestUrl = "~/Nowhere/")
        {
            Dictionary<Type, Mock> mockedList;
            var httpContext = FakeHttpContext(requestUrl, out mockedList);
            var requestContext = new RequestContext(httpContext, new RouteData());
            var context = new ControllerContext(requestContext, controller);
            controller.ControllerContext = context;
            controller.Url = new UrlHelper(requestContext);
            return mockedList;
        }

        public static void SetFakeControllerContext(this Controller controller, string requestUrl = "~/Nowhere/")
        {
            var httpContext = FakeHttpContext(requestUrl);
            var requestContext = new RequestContext(httpContext, new RouteData());
            var context = new ControllerContext(requestContext, controller);
            controller.ControllerContext = context;
            controller.Url = new UrlHelper(requestContext);
        }

        public static void SetFakeSatusCode(this Controller controller, int statusCode)
        {
            controller.Response.StatusCode = statusCode;
        }

        private static string GetUrlFileName(string url)
        {
            return url.Contains("?") ? url.Substring(0, url.IndexOf("?", StringComparison.Ordinal)) : url;
        }

        private static NameValueCollection GetQueryStringParameters(string url)
        {
            if (url.Contains("?"))
            {
                var parameters = new NameValueCollection();

                var parts = url.Split("?".ToCharArray());
                var keys = parts[1].Split("&".ToCharArray());

                foreach (var key in keys)
                {
                    var part = key.Split("=".ToCharArray());
                    parameters.Add(part[0], part[1]);
                }

                return parameters;
            }
            return null;
        }

        public static void SetHttpMethodResult(this HttpRequestBase request, string httpMethod)
        {
            Mock.Get(request)
                .Setup(req => req.HttpMethod)
                .Returns(httpMethod);
        }

        public static void SetupRequestUrl(this HttpRequestBase request, string url)
        {
            if (url == null)
                throw new ArgumentNullException("url");

            if (!url.StartsWith("~/"))
                throw new ArgumentException("Sorry, we expect a virtual url starting with \"~/\".");

            var mock = Mock.Get(request);

            mock.Setup(req => req.QueryString)
                .Returns(GetQueryStringParameters(url));
            mock.Setup(req => req.AppRelativeCurrentExecutionFilePath)
                .Returns(GetUrlFileName(url));
            mock.Setup(req => req.PathInfo)
                .Returns(string.Empty);
            mock.SetupGet(x => x.Url).Returns(new Uri(url.Replace("~/", "http://www.nowhere.com/")));
        }
    }
}
