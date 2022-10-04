using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using V.User.Services;
using Microsoft.AspNetCore.Builder;
using V.User.OAuth;

namespace V.User.Extensions
{
    public static class ServiceCollectionExtension
    {
        /// <summary>
        /// 添加 JwtService 到容器中
        /// <para>jwt token 参数必须放在请求的 querystring 或 form 或 body 或 header 中，参数名为 token</para>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="secret">jwt 密钥</param>
        /// <returns></returns>
        public static IServiceCollection AddJwt(this IServiceCollection services, string secret)
        {
            services.AddSingleton(sp => new JwtService(secret));
            return services;
        }

        /// <summary>
        /// 添加用户模块
        /// <para>必须同时添加 Jwt 模块</para>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IServiceCollection AddUserModule(this IServiceCollection services, Action<Configuration> config, IUserModuleCallback callback = null)
        {
            var configuration = new Configuration();
            if (config != null)
            {
                config(configuration);
            }
            if (string.IsNullOrWhiteSpace(configuration.ServiceCode))
            {
                throw new Exception("使用 V.User 模块时，必须配置 ServiceCode");
            }
            if (string.IsNullOrWhiteSpace(configuration.ServiceName))
            {
                throw new Exception("使用 V.User 模块时，必须配置 ServiceName");
            }
            if (configuration.CacheMode == 1 && string.IsNullOrWhiteSpace(configuration.RedisConnectionString))
            {
                throw new Exception("缓存方式设置为 redis 时，必须配置 RedisConnectionString");
            }
            if (configuration.AccountMode == 0)
            {
                if (string.IsNullOrWhiteSpace(configuration.SmtpServer))
                {
                    throw new Exception("使用邮箱作为账号主体时，必须配置 SmtpServer");
                }
                if (string.IsNullOrWhiteSpace(configuration.AdmMailAccount))
                {
                    throw new Exception("使用邮箱作为账号主体时，必须配置 AdmMailAccount");
                }
                if (string.IsNullOrWhiteSpace(configuration.AdmMailPwd))
                {
                    throw new Exception("使用邮箱作为账号主体时，必须配置 AdmMailPwd");
                }
            }

            services.AddSingleton(configuration)
                .AddTransient<CacheService>()
                .AddTransient<MailService>()
                .AddTransient<UserDao>()
                .AddTransient<UserService>()
                .AddTransient<SmsService>();
            if (callback != null)
            {
                services.AddSingleton(callback)
                    .AddTransient<ILoginService, OAuthLoginService>()
                    .AddOAuth();
            }
            if (configuration.AccountMode == 0)
            {
                services.AddTransient<IAccountService, MailAccountService>();
            }
            else
            {
                services.AddTransient<IAccountService, MobileAccountService>();
            }
            return services;
        }
    }
}
