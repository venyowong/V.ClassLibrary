using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace V.User.OAuth
{
    public interface ILoginService
    {
        /// <summary>
        /// login callback
        /// <para>should set http response in this method</para>
        /// </summary>
        /// <param name="context"></param>
        /// <param name="user"></param>
        Task Login(HttpContext context, UserInfo user);
    }
}
