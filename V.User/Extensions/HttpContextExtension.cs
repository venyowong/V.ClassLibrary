using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using V.Common.Extensions;
using V.User.Models;
using V.User.Services;

namespace V.User.Extensions
{
    public static class HttpContextExtension
    {
        public static async Task<object> SetPassword(this HttpContext context, long userId, string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return new Result { Code = -1, Msg = "密码不能为空" };
            }
            var service = context.RequestServices.GetService(typeof(UserService)) as UserService;
            var user = await service.GetUser(userId);
            if (user == null)
            {
                return new Result { Code = -1, Msg = "用户不存在" };
            }
            if (!string.IsNullOrWhiteSpace(user.Password))
            {
                return new Result { Code = -1, Msg = "已设置过密码" };
            }

            user.Salt = Guid.NewGuid().ToString("N");
            user.Password = $"{password}{user.Salt}".Md5();
            if (!await service.UpdateUser(user))
            {
                return new Result { Code = -1 };
            }

            return new Result();
        }

        public static async Task<object> ResetPassword(this HttpContext context, long userId, string oldPwd, string newPwd)
        {
            if (string.IsNullOrWhiteSpace(newPwd))
            {
                return new Result { Code = -1, Msg = "新密码不能为空" };
            }
            var service = context.RequestServices.GetService(typeof(UserService)) as UserService;
            var user = await service.GetUser(userId);
            if (user == null)
            {
                return new Result { Code = -1, Msg = "用户不存在" };
            }
            if ($"{oldPwd}{user.Salt}".Md5() != user.Password)
            {
                return new Result { Code = -1, Msg = "旧密码不正确" };
            }

            user.Password = $"{newPwd}{user.Salt}".Md5();
            if (!await service.UpdateUser(user))
            {
                return new Result { Code = -1 };
            }

            return new Result();
        }

        public static UserModel GetLoginedUser(this HttpContext context, UserEntity user)
        {
            var config = context.RequestServices.GetService(typeof(Configuration)) as Configuration;
            var jwtService = context.RequestServices.GetService(typeof(JwtService)) as JwtService;

            var userModel = new UserModel(user);
            TimeSpan? expiration = null;
            if (config.TokenEffectiveMinutes > 0)
            {
                expiration = new TimeSpan(0, config.TokenEffectiveMinutes, 0);
            }
            userModel.Token = jwtService.GenerateToken(new Dictionary<string, string> { { "userId", userModel.Id.ToString() } }, expiration);
            return userModel;
        }
    }
}
