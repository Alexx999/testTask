using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Tracker.Models.Account;

namespace Tracker.Core.Services
{
    public class ServerService : IServerService
    {
        private readonly string _url;

        public ServerService(string url)
        {
            _url = url;
        }

        public async Task<bool> RegisterUser(RegisterModel model)
        {
            var request = SendRequest(_url + "api/Account", "POST", JsonConvert.SerializeObject(model));
            using (var response = await request)
            {
                return response.StatusCode == HttpStatusCode.OK;
            }
        }

        private async Task<HttpWebResponse> SendRequest(string url, string method, string postData)
        {
            var request = WebRequest.CreateHttp(url);
            request.Method = method;

            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentType = "application/json; charset=utf-8";

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
