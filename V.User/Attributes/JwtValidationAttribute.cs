using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using V.User.Services;

namespace V.User.Attributes
{
    public class JwtValidationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var service = context.HttpContext.RequestServices.GetService(typeof(JwtService)) as JwtService;
            if (service == null)
            {
                context.HttpContext.Response.StatusCode = 500;
                context.HttpContext.Response.WriteAsync("未配置 JwtService").Wait();
                return;
            }

            var claims = service.GetTokenClaimsFromContext(context.HttpContext);
            if (claims == null)
            {
                context.Result = new StatusCodeResult(403);
                return;
            }

            foreach (var item in claims)
            {
                context.HttpContext.Items.Add(item.Key, item.Value);
            }

            base.OnActionExecuting(context);
        }
    }
}
