using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using V.Common.Extensions;

namespace V.User.OAuth.Services
{
    public class GiteeService : IOAuthService
    {
        private IConfiguration config;
        private IHttpClientFactory clientFactory;

        public GiteeService(IHttpClientFactory clientFactory, IConfiguration config)
        {
            this.clientFactory = clientFactory;
            this.config = config;
        }

        public string Name => "gitee";

        public string GetAuthorizeUrl(HttpContext context)
        {
            var redirectUrl = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}/usermodule/authorize?service=gitee";
            redirectUrl = WebUtility.UrlEncode(redirectUrl);
            return $"https://gitee.com/oauth/authorize?client_id={this.config["Oauth:Gitee:client_id"]}&redirect_uri={redirectUrl}&response_type=code&scope=user_info%20emails";
        }

        public async Task<UserInfo> GetUserInfo(HttpContext context, string authCode)
        {
            var client = this.clientFactory.CreateClient();
            var redirectUrl = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}/usermodule/authorize?service=gitee";
            var tokenRequest = new
            {
                client_id = this.config["Oauth:Gitee:client_id"],
                client_secret = this.config["Oauth:Gitee:client_secret"],
                code = authCode,
                redirect_uri = redirectUrl
            };
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://gitee.com/oauth/token?grant_type=authorization_code");
            requestMessage.Content = new StringContent(tokenRequest.ToJson(), Encoding.UTF8, "application/json");
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await client.SendAsync(requestMessage);
            var tokenResult = await response.ReadAsObj<JObject>();

            if (tokenResult == null)
            {
                return null;
            }
            var token = tokenResult["access_token"]?.ToString();
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            requestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://gitee.com/api/v5/user?access_token={token}");
            response = await client.SendAsync(requestMessage);
            if (!response.IsSuccessStatusCode || response.Content == null)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = json.ToObj<JObject>();
            if (result == null)
            {
                return null;
            }

            return new UserInfo
            {
                Id = result["id"].ToString(),
                Name = result["name"].ToString(),
                Avatar = result["avatar_url"].ToString(),
                Source = "gitee",
                Mail = result["email"]?.ToString() ?? string.Empty,
                Url = result["url"].ToString(),
                Blog = result["blog"]?.ToString() ?? string.Empty,
                Bio = result["bio"]?.ToString() ?? string.Empty
            };
        }
    }
}
