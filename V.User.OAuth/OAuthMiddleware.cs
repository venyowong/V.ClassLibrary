using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.Common.Extensions;
using V.User.OAuth.Services;

namespace V.User.OAuth
{
    public class OAuthMiddleware
    {
        private RequestDelegate requestDelegate;
        private IEnumerable<IOAuthService> services;
        private ILogger logger;
        private ILoginService login;

        private const string _base_path = "/usermodule";

        public OAuthMiddleware(RequestDelegate requestDelegate, IEnumerable<IOAuthService> services,
            ILogger<OAuthMiddleware> logger, ILoginService login)
        {
            this.requestDelegate = requestDelegate;
            this.services = services;
            this.logger = logger;
            this.login = login;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.HasValue)
            {
                var path = context.Request.Path.Value.ToLower();
                var index = path.IndexOf(_base_path);
                if (index >= 0)
                {
                    path = path.Substring(index + _base_path.Length);
                    if (string.IsNullOrEmpty(path))
                    {
                        await requestDelegate.Invoke(context);
                        return;
                    }

                    IOAuthService service;
                    var routes = path.Trim('/').Split('/');
                    switch (routes[0])
                    {
                        case "oauth":
                            service = this.GetOauthService(context.Request.Query["service"]);
                            if (service != null)
                            {
                                context.Response.Redirect(service.GetAuthorizeUrl(context));
                                return;
                            }
                            break;
                        case "authorize":
                            service = this.GetOauthService(context.Request.Query["service"]);
                            if (service != null)
                            {
                                var authCode = context.Request.Query["code"];
                                var user = await service.GetUserInfo(context, authCode);
                                if (user == null)
                                {
                                    this.logger.LogWarning($"cannot get user info when request {context.Request.Path}{context.Request.QueryString}");
                                    context.Response.StatusCode = 500;
                                    return;
                                }

                                try
                                {
                                    await this.login.Login(context, user);
                                    return;
                                }
                                catch (Exception ex)
                                {
                                    this.logger.LogWarning(ex, $"catch an exception when call login method, user: {user.ToJson()}");
                                    context.Response.StatusCode = 500;
                                    return;
                                }
                            }
                            break;
                    }
                }
            }

            await requestDelegate.Invoke(context);
        }

        private IOAuthService GetOauthService(string name) => this.services.FirstOrDefault(s => s.Name == name);
    }
}
