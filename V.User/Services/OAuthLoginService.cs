using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using V.Common.Extensions;
using V.User.Extensions;
using V.User.Models;
using V.User.OAuth;

namespace V.User.Services
{
    public class OAuthLoginService : ILoginService
    {
        private IUserModuleCallback callback;
        private UserService userService;
        private Configuration config;

        public OAuthLoginService(IUserModuleCallback callback, UserService userService, Configuration config)
        {
            this.callback = callback;
            this.userService = userService;
            this.config = config;
        }

        public async Task Login(HttpContext context, UserInfo user)
        {
            UserEntity usr = null;
            if (this.config.AccountMode  == 0)
            {
                if (!string.IsNullOrWhiteSpace(user.Mail))
                {
                    usr = await this.userService.GetUserByMail(user.Mail);
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(user.Mobile))
                {
                    var md5Mobile = user.Mobile.Md5();
                    usr = await this.userService.GetUserByMobile(md5Mobile);
                }
            }
            var usr2 = await this.userService.GetUserByPlatform(user.Source, user.Id);

            if (usr2 == null)
            {
                usr2 = new UserEntity
                {
                    Source = user.Source,
                    PlatformId = user.Id,
                    Name = user.Name,
                    Avatar = user.Avatar,
                    Location = user.Location,
                    Company = user.Company,
                    Bio = user.Bio
                };
                if (this.config.AccountMode != 0 || usr == null)
                {
                    usr2.Mail = user.Mail;
                }
                if ((this.config.AccountMode != 1 || usr == null) && !string.IsNullOrWhiteSpace(user.Mobile))
                {
                    usr2.Md5Mobile = user.Mobile.Md5();
                    usr2.MaskMobile = user.Mobile.MaskMobile();
                    usr2.EncryptedMobile = user.Mobile.DESEncrypt();
                }
                var id = await this.userService.CreateUser(usr2);
                if (id <= 0)
                {
                    context.Response.StatusCode = 500;
                    return;
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(user.Name))
                {
                    usr2.Name = user.Name;
                }
                if (!string.IsNullOrWhiteSpace(user.Avatar))
                {
                    usr2.Avatar = user.Avatar;
                }
                if (!string.IsNullOrWhiteSpace(user.Location))
                {
                    usr2.Location = user.Location;
                }
                if (!string.IsNullOrWhiteSpace(user.Company))
                {
                    usr2.Company = user.Company;
                }
                if (!string.IsNullOrWhiteSpace(user.Bio))
                {
                    usr2.Bio = user.Bio;
                }
                if (this.config.AccountMode != 0 || usr == null)
                {
                    usr2.Mail = user.Mail;
                }
                if ((this.config.AccountMode != 1 || usr == null) && !string.IsNullOrWhiteSpace(user.Mobile))
                {
                    usr2.Md5Mobile = user.Mobile.Md5();
                    usr2.MaskMobile = user.Mobile.MaskMobile();
                    usr2.EncryptedMobile = user.Mobile.DESEncrypt();
                }

                if (!await this.userService.UpdateUser(usr2))
                {
                    context.Response.StatusCode = 500;
                    return;
                }
            }

            await this.callback.OnOAuthLogin(context, context.GetLoginedUser(usr2));
        }
    }
}
