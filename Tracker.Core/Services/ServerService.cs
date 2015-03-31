using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tracker.Models.Account;

namespace Tracker.Core.Services
{
    public class ServerService : IServerService
    {
        private const string Json = "application/json; charset=utf-8";
        private const string UrlEncoded = "application/x-www-form-urlencoded; charset=utf-8";
        private readonly string _url;
        private string _token;

        public ServerService(string url)
        {
            _url = url;
        }

        public async Task<bool> RegisterUser(RegisterModel model)
        {
            var request = SendRequest("api/Account", "POST", JsonConvert.SerializeObject(model), Json);
            using (var response = await request)
            {
                return response.StatusCode == HttpStatusCode.OK;
            }
        }

        public async Task<bool> Login(string username, string password)
        {
            var sb = new StringBuilder("grant_type=password&username=");
            sb.Append(username);
            sb.Append("&password=");
            sb.Append(password);
            var request = SendRequest("Token", "POST", sb.ToString(), UrlEncoded);
            try
            {
                using (var response = await request)
                {
                    if (response.StatusCode != HttpStatusCode.OK) return false;

                    var data = await new StreamReader(response.GetResponseStream()).ReadToEndAsync();
                    var obj = JObject.Parse(data);
                    JToken token;
                    if (!obj.TryGetValue("access_token", out token)) return false;
                    _token = token.ToObject<string>();
                    return true;
                }
            }
            catch (WebException ex)
            {
                
                throw;
            }
        }

        private async Task<HttpWebResponse> SendRequest(string endpoint, string method, string postData, string contentType)
        {
            var url = _url + endpoint;
            var request = WebRequest.CreateHttp(url);
            request.Method = method;

            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentType = contentType;

            using (var dataStream = await request.GetRequestStreamAsync())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
            }

            return (HttpWebResponse) await request.GetResponseAsync();
        }
    }

    public interface IServerService
    {
        Task<bool> RegisterUser(RegisterModel model);
    }
}
