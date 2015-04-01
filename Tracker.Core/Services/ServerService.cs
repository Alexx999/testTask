using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tracker.Models;
using Tracker.Models.Account;

namespace Tracker.Core.Services
{
    public class ServerService : IServerService
    {
        private const string Json = "application/json; charset=utf-8";
        private const string UrlEncoded = "application/x-www-form-urlencoded; charset=utf-8";
        private readonly string _url;
        private string _authorization;

        public ServerService(string url)
        {
            _url = url;
        }

        public async Task<bool> Register(RegisterModel model)
        {
            var request = SendRequest("api/Account", "POST", JsonConvert.SerializeObject(model), Json);
            using (var response = await request)
            {
                return response.Success;
            }
        }

        public async Task<bool> Login(string username, string password)
        {
            var sb = new StringBuilder("grant_type=password&username=");
            sb.Append(username);
            sb.Append("&password=");
            sb.Append(password);
            var request = SendRequest("Token", "POST", sb.ToString(), UrlEncoded);
            using (var result = await request)
            {
                if (!result.Success) return false;
                var response = result.Response;

                string data;
                using (var stream = response.GetResponseStream())
                {
                    data = await new StreamReader(stream).ReadToEndAsync();
                }

                var obj = JObject.Parse(data);
                JToken token;
                if (!obj.TryGetValue("access_token", out token)) return false;
                var key = token.ToObject<string>();
                _authorization = "Bearer " + key;
                return true;
            }
        }

        public async Task<List<Expense>> GetExpenses()
        {
            if (_authorization == null)
            {
                throw new InvalidOperationException("Must login successfully before getting data");
            }
            var request = SendRequest("api/Expenses", "GET", null, Json);
            using (var result = await request)
            {
                if (!result.Success) return null;
                var response = result.Response;

                string data;
                using (var stream = response.GetResponseStream())
                {
                    data = await new StreamReader(stream).ReadToEndAsync();
                }

                return JsonConvert.DeserializeObject<List<Expense>>(data);
            }
        }

        public async Task<Expense> CreateExpense(Expense model)
        {
            var request = SendRequest("api/Expenses", "POST", JsonConvert.SerializeObject(model), Json);
            using (var result = await request)
            {
                if (!result.Success) return null;
                var response = result.Response;

                string data;
                using (var stream = response.GetResponseStream())
                {
                    data = await new StreamReader(stream).ReadToEndAsync();
                }

                return JsonConvert.DeserializeObject<Expense>(data);
            }
        }

        public async Task<Expense> RemoveExpense(Guid targetGuid)
        {
            var request = SendRequest("api/Expenses/" + targetGuid.ToString(), "DELETE", null, Json);
            using (var result = await request)
            {
                if (!result.Success) return null;
                var response = result.Response;

                string data;
                using (var stream = response.GetResponseStream())
                {
                    data = await new StreamReader(stream).ReadToEndAsync();
                }

                return JsonConvert.DeserializeObject<Expense>(data);
            }
        }

        public async Task<bool> UpdateExpense(Expense target)
        {
            var request = SendRequest("api/Expenses/" + target.ExpenseId.ToString(), "PUT", JsonConvert.SerializeObject(target), Json);
            using (var result = await request)
            {
                return result.Success;
            }
        }

        private async Task<HttpResult> SendRequest(string endpoint, string method, string postData, string contentType)
        {
            var url = _url + endpoint;
            var request = WebRequest.CreateHttp(url);
            request.Method = method;
            request.ContentType = contentType;
            request.Headers[HttpRequestHeader.Authorization] = _authorization;

            if (postData != null)
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                using (var dataStream = await request.GetRequestStreamAsync())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }
            }

            HttpWebResponse result;
            var success = false;
            try
            {
                result = (HttpWebResponse)await request.GetResponseAsync();
                success = true;
            }
            catch (WebException ex)
            {
                result = (HttpWebResponse)ex.Response;
            }
            return new HttpResult(result, success);
        }

        private struct HttpResult : IDisposable
        {
            public bool Success;
            public HttpWebResponse Response;

            public HttpResult(HttpWebResponse response, bool success)
            {
                Success = success;
                Response = response;
            }

            public void Dispose()
            {
                if (Response != null)
                {
                    Response.Dispose();
                }
            }
        }
    }
}
