using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using V.User.Models;

namespace V.User.Services
{
    public interface IAccountService
    {
        Task<object> SignUp(string account, string password, HttpContext context);

        Task<object> Login(string account, string password, HttpContext context);

        Task<object> ResetPassword(string account, HttpContext context);

        Task<object> VerifyCode(string contextId, string code, HttpContext context);

        Task<object> UpdateUserInfo(long userId, ModifiableUserInfo user, HttpContext context);
    }
}
