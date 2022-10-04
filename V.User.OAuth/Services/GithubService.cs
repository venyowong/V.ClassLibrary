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
    public class GithubService : IOAuthService
    {
        private IConfiguration config;
        private IHttpClientFactory clientFactory;

        public GithubService(IHttpClientFactory clientFactory, IConfiguration config)
        {
            this.clientFactory = clientFactory;
            this.config = config;
        }

        public string Name => "github";

        public string GetAuthorizeUrl(HttpContext context)
        {
            var redirectUrl = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}/usermodule/authorize{context.Request.QueryString}";
            redirectUrl = WebUtility.UrlEncode(redirectUrl);
            return $"https://github.com/login/oauth/authorize?client_id={this.config["Oauth:Github:client_id"]}&redirect_uri={redirectUrl}";
        }

        public async Task<UserInfo> GetUserInfo(HttpContext context, string authCode)
        {
            var client = this.clientFactory.CreateClient();
            var tokenRequest = new
            {
                client_id = this.config["Oauth:Github:client_id"],
                client_secret = this.config["Oauth:Github:client_secret"],
                code = authCode
            };
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://github.com/login/oauth/access_token");
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

            requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user");
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            requestMessage.Headers.Add("User-Agent", ".net core");
            requestMessage.Headers.Add("Accept", "application/json");
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
                Source = "github",
                Mail = result["email"]?.ToString() ?? string.Empty,
                Url = result["url"].ToString(),
                Location = result["location"]?.ToString() ?? string.Empty,
                Company = result["company"]?.ToString() ?? string.Empty,
                Blog = result["blog"]?.ToString() ?? string.Empty,
                Bio = result["bio"]?.ToString() ?? string.Empty
            };
        }
    }
}
