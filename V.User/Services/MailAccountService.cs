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
    public class MailAccountService : IAccountService
    {
        private Configuration config;
        private UserService service;
        private MailService mailService;
        private CacheService cacheService;

        public MailAccountService(Configuration config, UserService service, 
            MailService mailService, CacheService cacheService)
        {
            this.config = config;
            this.service = service;
            this.mailService = mailService;
            this.cacheService = cacheService;
        }

        public async Task<object> Login(string mail, string password, HttpContext context)
        {
            if (string.IsNullOrWhiteSpace(mail))
            {
                return new Result { Code = -1, Msg = "邮箱不能为空" };
            }
            var user = await this.service.GetUserByMail(mail);
            if (user == null)
            {
                return new Result { Code = -1, Msg = "该邮箱未注册" };
            }

            if (!string.IsNullOrWhiteSpace(password))
            {
                if ($"{password}{user.Salt}".Md5() != user.Password)
                {
                    return new Result { Code = -1, Msg = "密码错误" };
                }

                return Result.Success(context.GetLoginedUser(user));
            }

            // 验证码登录
            var counterKey = "V:User:Services:MailAccountService:Counter:" + mail;
            var counter = this.cacheService.StringGet(counterKey).ToObj<Counter>();
            if (counter == null)
            {
                counter = new Counter();
            }
            if (counter.Over(this.config.MailTimesDaily))
            {
                return new Result { Code = -1, Msg = "该邮箱发送验证码次数已超过今日上限" };
            }

            var random = new Random($"{mail}{DateTime.Now.Ticks}".GetHashCode());
            var verificationCode = random.Next(899999) + 100000;
            var expiration = DateTime.Now.AddMinutes(this.config.MailEffectiveMinutes);
            var ctx = new Context
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = "LoginWithMail",
                Step = 130,
                VerificationCode = verificationCode.ToString(),
                UserId = user.Id,
                Expiration = expiration
            };

            var body = this.config.VerificationCodeMail4Login.Replace("{code}", ctx.VerificationCode)
                .Replace("{expiration}", expiration.ToString())
                .Replace("{serviceName}", this.config.ServiceName);
            if (!this.mailService.SendMail($"{this.config.ServiceName} 登录验证", body, mail))
            {
                return new Result { Code = -1, Msg = "邮件发送失败" };
            }

            counter.Inc();
            this.cacheService.StringSet(counterKey, counter.ToJson(), counter.Expiration - DateTime.Now);
            this.cacheService.StringSet("V:User:Services:MailAccountService:Context:" + ctx.Id,
                ctx.ToJson(), new TimeSpan(0, this.config.MailEffectiveMinutes, 0));
            return Result.Success(ctx.Id);
        }

        public async Task<object> ResetPassword(string mail, HttpContext context)
        {
            var user = await this.service.GetUserByMail(mail);
            if (user == null)
            {
                return new Result { Code = -1, Msg = "邮箱未注册" };
            }
            var counterKey = "V:User:Services:MailAccountService:Counter:" + mail;
            var counter = this.cacheService.StringGet(counterKey).ToObj<Counter>();
            if (counter == null)
            {
                counter = new Counter();
            }
            if (counter.Over(this.config.MailTimesDaily))
            {
                return new Result { Code = -1, Msg = "该邮箱发送验证码次数已超过今日上限" };
            }

            var random = new Random($"{mail}{DateTime.Now.Ticks}".GetHashCode());
            var verificationCode = random.Next(899999) + 100000;
            var expiration = DateTime.Now.AddMinutes(this.config.MailEffectiveMinutes);
            var ctx = new Context
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = "ResetPasswordWithMail",
                Step = 110,
                VerificationCode = verificationCode.ToString(),
                UserId = user.Id,
                Expiration = expiration
            };

            var body = this.config.VerificationCodeMail4ResetPwd.Replace("{code}", ctx.VerificationCode)
                .Replace("{expiration}", expiration.ToString())
                .Replace("{serviceName}", this.config.ServiceName);
            if (!this.mailService.SendMail($"{this.config.ServiceName} 重置密码验证", body, user.Mail))
            {
                return new Result { Code = -1, Msg = "邮件发送失败" };
            }

            counter.Inc();
            this.cacheService.StringSet(counterKey, counter.ToJson(), counter.Expiration - DateTime.Now);
            this.cacheService.StringSet("V:User:Services:MailAccountService:Context:" + ctx.Id,
                ctx.ToJson(), new TimeSpan(0, this.config.MailEffectiveMinutes, 0));
            return Result.Success(ctx.Id);
        }

        public async Task<object> SignUp(string mail, string password, HttpContext context)
        {
            if (string.IsNullOrWhiteSpace(mail))
            {
                return new Result { Code = -1, Msg = "邮箱不能为空" };
            }

            var user = await this.service.GetUserByMail(mail);
            if (user != null)
            {
                if (!string.IsNullOrWhiteSpace(user.SourceName))
                {
                    return Result<UserEntity>.Fail(msg: $"该邮箱已在 {user.SourceName} 注册过，请使用密码登录");
                }

                var serviceName = Utility.GetServiceProductName(user.Source);
                if (!string.IsNullOrWhiteSpace(serviceName))
                {
                    return Result<UserEntity>.Fail(msg: $"该邮箱已在 {serviceName} 注册过，请使用密码登录");
                }

                return Result<UserEntity>.Fail(msg: "该邮箱已注册过，请使用密码登录");
            }

            if (!this.config.NeedVerificationForSignUp)
            {
                #region 使用密码注册
                if (string.IsNullOrWhiteSpace(password))
                {
                    return new Result { Code = -1, Msg = "密码不能为空" };
                }

                var salt = Guid.NewGuid().ToString("N");
                user = new UserEntity
                {
                    Source = this.config.ServiceCode,
                    SourceName = this.config.ServiceName,
                    Mail = mail,
                    Salt = salt,
                    Password = $"{password}{salt}".Md5()
                };
                var id = await this.service.CreateUser(user);
                if (id <= 0)
                {
                    return Result<UserEntity>.Fail();
                }

                user.Id = id;
                return Result.Success(context.GetLoginedUser(user));
                #endregion
            }

            var counterKey = "V:User:Services:MailAccountService:Counter:" + mail;
            var counter = this.cacheService.StringGet(counterKey).ToObj<Counter>();
            if (counter == null)
            {
                counter = new Counter();
            }
            if (counter.Over(this.config.MailTimesDaily))
            {
                return new Result { Code = -1, Msg = "该邮箱发送验证码次数已超过今日上限" };
            }

            var random = new Random($"{mail}{DateTime.Now.Ticks}".GetHashCode());
            var verificationCode = random.Next(899999) + 100000;
            var expiration = DateTime.Now.AddMinutes(this.config.MailEffectiveMinutes);
            var ctx = new Context
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = "SignUpWithMail",
                Step = 100,
                VerificationCode = verificationCode.ToString(),
                Mail = mail,
                Expiration = expiration
            };

            var body = this.config.VerificationCodeMail4SignUp.Replace("{code}", ctx.VerificationCode)
                .Replace("{expiration}", expiration.ToString())
                .Replace("{serviceName}", this.config.ServiceName);
            if (!this.mailService.SendMail($"{this.config.ServiceName} 注册验证", body, mail))
            {
                return new Result { Code = -1, Msg = "邮件发送失败" };
            }

            counter.Inc();
            this.cacheService.StringSet(counterKey, counter.ToJson(), counter.Expiration - DateTime.Now);
            this.cacheService.StringSet("V:User:Services:MailAccountService:Context:" + ctx.Id,
                ctx.ToJson(), new TimeSpan(0, this.config.MailEffectiveMinutes, 0));
            return Result.Success(ctx.Id);
        }

        public async Task<object> UpdateUserInfo(long userId, ModifiableUserInfo user, HttpContext context)
        {
            var usr = await this.service.GetUser(userId);
            if (usr == null)
            {
                return new Result { Code = -1, Msg = "用户不存在" };
            }

            if (!string.IsNullOrWhiteSpace(user.Mail)) // 更换用户邮箱
            {
                if (user.Mail == usr.Mail)
                {
                    return new Result { Code = -1, Msg = "请设置不同的邮箱" };
                }

                var newUser = await this.service.GetUserByMail(user.Mail);
                if (newUser != null)
                {
                    return new Result { Code = -1, Msg = "该邮箱已被注册" };
                }
                var counterKey = "V:User:Services:MailAccountService:Counter:" + user.Mail;
                var counter = this.cacheService.StringGet(counterKey).ToObj<Counter>();
                if (counter == null)
                {
                    counter = new Counter();
                }
                if (counter.Over(this.config.MailTimesDaily))
                {
                    return new Result { Code = -1, Msg = "该邮箱发送验证码次数已超过今日上限" };
                }

                var random = new Random($"{user.Mail}{DateTime.Now.Ticks}".GetHashCode());
                var verificationCode = random.Next(899999) + 100000;
                var expiration = DateTime.Now.AddMinutes(this.config.MailEffectiveMinutes);
                var ctx = new Context
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Name = "ChangeMail",
                    Step = 120,
                    VerificationCode = verificationCode.ToString(),
                    UserId = userId,
                    Mail = user.Mail,
                    Expiration = expiration
                };

                var body = this.config.VerificationCodeMail4Change.Replace("{code}", ctx.VerificationCode)
                    .Replace("{expiration}", expiration.ToString())
                    .Replace("{serviceName}", this.config.ServiceName);
                if (!this.mailService.SendMail($"{this.config.ServiceName} 邮箱更换验证", body, user.Mail))
                {
                    return new Result { Code = -1, Msg = "邮件发送失败" };
                }

                counter.Inc();
                this.cacheService.StringSet(counterKey, counter.ToJson(), counter.Expiration - DateTime.Now);
                this.cacheService.StringSet("V:User:Services:MailAccountService:Context:" + ctx.Id,
                    ctx.ToJson(), new TimeSpan(0, this.config.MailEffectiveMinutes, 0));
                return Result.Success(ctx.Id);
            }

            if (!string.IsNullOrWhiteSpace(user.Name) && user.Name != usr.Name)
            {
                usr.Name = user.Name;
            }
            if (!string.IsNullOrWhiteSpace(user.Avatar) && user.Avatar != usr.Avatar)
            {
                usr.Avatar = user.Avatar;
            }
            if (!string.IsNullOrWhiteSpace(user.Location) && user.Location != usr.Location)
            {
                usr.Location = user.Location;
            }
            if (!string.IsNullOrWhiteSpace(user.Company) && user.Company != usr.Company)
            {
                usr.Company = user.Company;
            }
            if (!string.IsNullOrWhiteSpace(user.Bio) && user.Bio != usr.Bio)
            {
                usr.Bio = user.Bio;
            }
            if (!string.IsNullOrWhiteSpace(user.Mobile))
            {
                if (!user.Mobile.IsValidMobile())
                {
                    return new Result { Code = -1, Msg = "请输入正确的手机号" };
                }

                var md5Mobile = user.Mobile.Md5();
                if (md5Mobile != usr.Md5Mobile)
                {
                    usr.Md5Mobile = md5Mobile;
                    usr.MaskMobile = user.Mobile.MaskMobile();
                    usr.EncryptedMobile = user.Mobile.DESEncrypt();
                }
            }
            if (!await this.service.UpdateUser(usr))
            {
                return new Result { Code = -1 };
            }

            return new Result();
        }

        public async Task<object> VerifyCode(string contextId, string code, HttpContext context)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return new Result { Code = -1, Msg = "验证码不能为空" };
            }
            var ctxKey = "V:User:Services:MailAccountService:Context:" + contextId;
            var ctxJson = this.cacheService.StringGet(ctxKey);
            if (ctxJson == null)
            {
                return new Result { Code = -1, Msg = "验证码已过期" };
            }
            var ctx = ctxJson.ToObj<Context>();
            if (ctx == null)
            {
                return new Result { Code = -1 };
            }
            if (ctx.VerificationCode != code)
            {
                return new Result { Code = -1, Msg = "验证码错误" };
            }
            
            if (ctx.Step == 100) // 注册验证
            {
                var user = await this.service.GetUserByMail(ctx.Mail);
                if (user != null)
                {
                    return new Result { Code = -1, Msg = "该邮箱已注册" };
                }

                user = new UserEntity
                {
                    Source = this.config.ServiceCode,
                    SourceName = this.config.ServiceName,
                    Mail = ctx.Mail
                };
                var id = await this.service.CreateUser(user);
                if (id <= 0)
                {
                    return Result<UserEntity>.Fail();
                }

                user.Id = id;
                ctx.Step = 101;
                this.cacheService.StringSet(ctxKey, ctx.ToJson(), ctx.Expiration - DateTime.Now);
                return Result.Success(context.GetLoginedUser(user));
            }
            else if (ctx.Step == 110) // 重置密码验证
            {
                var user = await this.service.GetUser(ctx.UserId);
                user.Password = null;
                if (!await this.service.UpdateUser(user))
                {
                    return new Result { Code = -1 };
                }

                ctx.Step = 111;
                this.cacheService.StringSet(ctxKey, ctx.ToJson(), ctx.Expiration - DateTime.Now);
                return Result.Success(context.GetLoginedUser(user));
            }
            else if (ctx.Step == 120) // 更换邮箱验证
            {
                var user = await this.service.GetUserByMail(ctx.Mail);
                if (user != null)
                {
                    return new Result { Code = -1, Msg = "该邮箱已注册" };
                }

                user = await this.service.GetUser(ctx.UserId);
                user.Mail = ctx.Mail;
                if (!await this.service.UpdateUser(user))
                {
                    return new Result { Code = -1 };
                }

                ctx.Step = 121;
                this.cacheService.StringSet(ctxKey, ctx.ToJson(), ctx.Expiration - DateTime.Now);
                return new Result();
            }
            else if (ctx.Step == 130) // 登录验证
            {
                var user = await this.service.GetUser(ctx.UserId);
                ctx.Step = 131;
                this.cacheService.StringSet(ctxKey, ctx.ToJson(), ctx.Expiration - DateTime.Now);
                return Result.Success(context.GetLoginedUser(user));
            }

            return new Result { Code = -1, Msg = "无法识别" };
        }
    }
}
