using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using V.User.OAuth.Services;

namespace V.User.OAuth
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddOAuth(this IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddTransient<IOAuthService, GithubService>()
                .AddTransient<IOAuthService, GiteeService>()
                .AddTransient<IOAuthService, StackExchangeService>();
            return services;
        }

        public static IApplicationBuilder UseOAuth(this IApplicationBuilder app)
        {
            app.UseMiddleware<OAuthMiddleware>();
            return app;
        }
    }
}
