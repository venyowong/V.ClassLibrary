using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using V.Common.Extensions;

namespace V.User.OAuth.Services
{
    public class BaiduService : IOAuthService
    {
        private IConfiguration config;
        private IHttpClientFactory clientFactory;

        public BaiduService(IHttpClientFactory clientFactory, IConfiguration config)
        {
            this.clientFactory = clientFactory;
            this.config = config;
        }

        public string Name => "baidu";

        public string GetAuthorizeUrl(HttpContext context)
        {
            var redirectUrl = this.config["OAuth:BaseUrl"];
            if (string.IsNullOrEmpty(redirectUrl))
            {
                redirectUrl = context.Request.GetAbsoluteUrl("/usermodule/authorize?service=baidu");
            }
            else
            {
                redirectUrl += "/usermodule/authorize?service=baidu";
            }
            redirectUrl = WebUtility.UrlEncode(redirectUrl);
            return $"http://openapi.baidu.com/oauth/2.0/authorize?response_type=code&client_id={this.config["OAuth:Baidu:client_id"]}&redirect_uri={redirectUrl}&scope=basic&display=popup";
        }

        public async Task<UserInfo> GetUserInfo(HttpContext context, string authCode)
        {
            var client = this.clientFactory.CreateClient();
            var redirectUrl = context.Request.GetAbsoluteUrl("/usermodule/authorize?service=baidu");
            redirectUrl = WebUtility.UrlEncode(redirectUrl);
            var response = await client.GetAsync($"https://openapi.baidu.com/oauth/2.0/token?grant_type=authorization_code&code={authCode}&client_id={this.config["OAuth:Baidu:client_id"]}&client_secret={this.config["OAuth:Baidu:client_secret"]}&redirect_uri={redirectUrl}");
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

            response = await client.GetAsync($"https://openapi.baidu.com/rest/2.0/passport/users/getInfo?access_token={token}");
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
                Avatar = $"http://tb.himg.baidu.com/sys/portrait/item/{result["portrait"]}",
                Name = result["username"]?.ToString(),
                Source = "baidu",
                Id = result["openid"]?.ToString()
            };
        }
    }
}
