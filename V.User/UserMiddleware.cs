using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using V.Common.Extensions;
using V.User.Extensions;
using V.User.Models;
using V.User.Services;

namespace V.User
{
    public class UserMiddleware
    {
        private RequestDelegate requestDelegate;
        private IAccountService accountService;
        private Configuration config;
        private JwtService jwtService;
        private UserService userService;

        private const string _base_path = "/usermodule";

        public UserMiddleware(RequestDelegate requestDelegate, IAccountService accountService,
            Configuration config, JwtService jwtService, UserService userService)
        {
            this.requestDelegate = requestDelegate;
            this.accountService = accountService;
            this.config = config;
            this.jwtService = jwtService;
            this.userService = userService;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.Path.HasValue)
            {
                await this.requestDelegate.Invoke(context);
                return;
            }

            var path = context.Request.Path.Value.ToLower();
            var index = path.IndexOf(_base_path);
            if (index < 0)
            {
                await this.requestDelegate.Invoke(context);
                return;
            }

            path = path.Substring(index + _base_path.Length);
            if (string.IsNullOrEmpty(path))
            {
                await requestDelegate.Invoke(context);
                return;
            }

            JObject param = null;
            if (context.Request.Method == "POST")
            {
                var body = await context.Request.ReadBody();
                if (!string.IsNullOrWhiteSpace(body))
                {
                    param = JObject.Parse(body);
                }
            }

            var account = this.config.AccountMode == 0 ? param?["mail"]?.ToString() : param?["mobile"]?.ToString();
            var password = param?["password"]?.ToString();
            var routes = path.Trim('/').Split('/');
            object result = null;
            long userId;
            context.Response.ContentType = "application/json;charset=utf-8";
            switch (routes[0].ToLower())
            {
                case "signup":
                    result = await this.accountService.SignUp(account, password, context);
                    await context.Response.WriteAsync(result.ToJson());
                    return;
                case "login":
                    result = await this.accountService.Login(account, password, context);
                    await context.Response.WriteAsync(result.ToJson());
                    return;
                case "setpassword":
                    userId = this.GetUserIdFromContext(context);
                    if (userId <= 0)
                    {
                        context.Response.StatusCode = 403;
                        return;
                    }

                    result = await context.SetPassword(userId, password);
                    await context.Response.WriteAsync(result.ToJson());
                    return;
                case "resetpassword":
                    userId = this.GetUserIdFromContext(context);
                    if (userId <= 0)
                    {
                        context.Response.StatusCode = 403;
                        return;
                    }

                    var oldPwd = param?["oldPwd"]?.ToString();
                    var newPwd = param?["newPwd"]?.ToString();
                    result = await context.ResetPassword(userId, oldPwd, newPwd);
                    await context.Response.WriteAsync(result.ToJson());
                    return;
                case "forgetpassword":
                    result = await this.accountService.ResetPassword(account, context);
                    await context.Response.WriteAsync(result.ToJson());
                    return;
                case "verifycode":
                    result = await this.accountService.VerifyCode(param?["contextId"]?.ToString(), param?["code"]?.ToString(), context);
                    await context.Response.WriteAsync(result.ToJson());
                    return;
                case "updateinfo":
                    userId = this.GetUserIdFromContext(context);
                    if (userId <= 0)
                    {
                        context.Response.StatusCode = 403;
                        return;
                    }

                    result = await this.accountService.UpdateUserInfo(userId, param?.ToObject<ModifiableUserInfo>(), context);
                    await context.Response.WriteAsync(result.ToJson());
                    return;
                case "info":
                    userId = this.GetUserIdFromContext(context);
                    if (userId <= 0)
                    {
                        context.Response.StatusCode = 403;
                        return;
                    }

                    result = new UserModel(await this.userService.GetUser(userId));
                    await context.Response.WriteAsync(result.ToJson());
                    return;
            }

            await requestDelegate.Invoke(context);
            return;
        }

        private long GetUserIdFromContext(HttpContext context)
        {
            var claims = this.jwtService.GetTokenClaimsFromContext(context);
            if (claims == null)
            {
                return 0;
            }
            if (!claims.ContainsKey("userId"))
            {
                return 0;
            }
            var id = claims["userId"]?.ToString();
            if (string.IsNullOrWhiteSpace(id))
            {
                return 0;
            }

            long.TryParse(id, out long result);
            return result;
        }
    }
}
