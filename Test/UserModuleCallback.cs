using V.User.Models;
using V.User.Services;

namespace Test
{
    public class UserModuleCallback : IUserModuleCallback
    {
        public Task OnOAuthLogin(HttpContext context, UserModel user)
        {
            context.Response.WriteAsync(user.ToString());
            return Task.CompletedTask;
        }
    }
}
