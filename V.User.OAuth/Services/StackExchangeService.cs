using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using V.Common.Extensions;

namespace V.User.OAuth.Services
{
    public class StackExchangeService : IOAuthService
    {
        private IConfiguration config;
        private IHttpClientFactory clientFactory;

        private static readonly Regex _tokenRegex = new Regex("access_token=([^&]+)");

        public StackExchangeService(IHttpClientFactory clientFactory, IConfiguration config)
        {
            this.clientFactory = clientFactory;
            this.config = config;
        }

        public string Name => "stackexchange";

        public string GetAuthorizeUrl(HttpContext context)
        {
            var redirectUrl = this.config["OAuth:BaseUrl"];
            if (string.IsNullOrEmpty(redirectUrl))
            {
                redirectUrl = context.Request.GetAbsoluteUrl($"/usermodule/authorize{context.Request.QueryString}");
            }
            else
            {
                redirectUrl += $"/usermodule/authorize{context.Request.QueryString}";
            }
            redirectUrl = WebUtility.UrlEncode(redirectUrl);
            return $"https://stackoverflow.com/oauth?client_id={this.config["OAuth:Stackexchange:client_id"]}&redirect_uri={redirectUrl}";
        }

        public async Task<UserInfo> GetUserInfo(HttpContext context, string authCode)
        {
            var client = this.clientFactory.CreateClient();
            var queryString = string.Join("&", context.Request.Query.Where(x => x.Key != "code")
                .Select(x => $"{x.Key}={x.Value}")
                .ToArray());
            var redirectUrl = this.config["OAuth:BaseUrl"];
            if (string.IsNullOrEmpty(redirectUrl))
            {
                redirectUrl = context.Request.GetAbsoluteUrl($"/usermodule/authorize{context.Request.QueryString}");
            }
            else
            {
                redirectUrl += $"/usermodule/authorize{context.Request.QueryString}";
            }
            redirectUrl = WebUtility.UrlEncode(redirectUrl);
            var form = $"client_id={this.config["OAuth:Stackexchange:client_id"]}&client_secret={this.config["OAuth:Stackexchange:client_secret"]}&code={authCode}&redirect_uri={redirectUrl}";
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://stackoverflow.com/oauth/access_token");
            requestMessage.Content = new StringContent(form, Encoding.UTF8, "application/x-www-form-urlencoded");
            requestMessage.Headers.UserAgent.Add(new ProductInfoHeaderValue("V.User.OAuth", "1.0"));
            var response = await client.SendAsync(requestMessage);
            var tokenResult = await response.ReadAsString();
            var match = _tokenRegex.Match(tokenResult);
            if (!match.Success)
            {
                return null;
            }
            var token = match.Groups[1].Value;
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            response = await client.GetAsync($"https://api.stackexchange.com/2.3/me?key={this.config["OAuth:Stackexchange:key"]}&access_token={token}&site=stackoverflow");
            var json = await response.ReadAsDecompressedString();
            var result = json.ToObj<JObject>();
            if (result == null)
            {
                return null;
            }
            var items = result["items"] as JArray;
            if (items.IsNullOrEmpty())
            {
                return null;
            }
            var user = items[0];

            return new UserInfo
            {
                Id = user["user_id"].ToString(),
                Name = user["display_name"].ToString(),
                Avatar = user["profile_image"].ToString(),
                Source = "stackexchange",
                Url = user["link"].ToString()
            };
        }
    }
}
