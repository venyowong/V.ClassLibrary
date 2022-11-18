using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using V.User.OAuth;

namespace V.User.Extensions
{
    public static class ApplicationBuilderExtension
    {
        public static IApplicationBuilder UseUserModule(this IApplicationBuilder app, bool useOAuth = false)
        {
            app.UseMiddleware<UserMiddleware>();
            if (useOAuth)
            {
                app.UseOAuth();
            }
            return app;
        }
    }
}
