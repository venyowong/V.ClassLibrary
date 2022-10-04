using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace V.User.OAuth.Services
{
    public interface IOAuthService
    {
        string Name { get; }

        string GetAuthorizeUrl(HttpContext context);

        Task<UserInfo> GetUserInfo(HttpContext context, string authCode);
    }
}
