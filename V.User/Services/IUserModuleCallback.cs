using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using V.User.Models;

namespace V.User.Services
{
    public interface IUserModuleCallback
    {
        Task OnOAuthLogin(HttpContext context, UserModel user);
    }
}
