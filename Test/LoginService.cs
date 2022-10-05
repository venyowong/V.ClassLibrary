using V.User.OAuth;

namespace Test
{
    public class LoginService : ILoginService
    {
        public async Task Login(HttpContext context, UserInfo user)
        {
            await context.Response.WriteAsJsonAsync(user);
        }
    }
}
