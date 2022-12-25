using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using V.Common.Extensions;
using V.SwitchableCache;
using V.User.Extensions;
using V.User.Models;
using V.User.OAuth;

namespace V.User.Services
{
    public class MobileAccountService : IAccountService
    {
        private SmsService smsService;
        private UserService service;
        private Configuration config;
        private ICacheService cacheService;

        public MobileAccountService(SmsService smsService, UserService service,
            Configuration config, ICacheService cacheService)
        {
            this.smsService = smsService;
            this.service = service;
            this.config = config;
            this.cacheService = cacheService;
        }

        public async Task<object> Login(string mobile, string password, HttpContext context)
        {
            if (string.IsNullOrWhiteSpace(mobile))
            {
                return new Result { Code = -1, Msg = "手机号不能为空" };
            }
            var md5Mobile = mobile.Md5();
            var user = await this.service.GetUserByMobile(md5Mobile);
            if (user == null)
            {
                return new Result { Code = -1, Msg = "该手机号未注册" };
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
            var counterKey = "V:User:Services:MobileAccountService:Counter:" + mobile;
            var counter = this.cacheService.StringGet(counterKey).ToObj<Counter>();
            if (counter == null)
            {
                counter = new Counter();
            }
            if (counter.Over(this.config.SmsTimesDaily))
            {
                return new Result { Code = -1, Msg = "该手机号发送短信验证码次数已超过今日上限" };
            }

            var random = new Random($"{mobile}{DateTime.Now.Ticks}".GetHashCode());
            var verificationCode = random.Next(899999) + 100000;
            var expiration = DateTime.Now.AddMinutes(this.config.SmsEffectiveMinutes);
            var ctx = new Context
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = "LoginWithMobile",
                Step = 230,
                VerificationCode = verificationCode.ToString(),
                UserId = user.Id,
                Expiration = expiration
            };

            if (!await this.smsService.SendSms(mobile, ctx.VerificationCode))
            {
                return new Result { Code = -1, Msg = "短信验证码发送失败" };
            }

            counter.Inc();
            this.cacheService.StringSet(counterKey, counter.ToJson(), counter.Expiration - DateTime.Now);
            this.cacheService.StringSet("V:User:Services:MobileAccountService:Context:" + ctx.Id,
                ctx.ToJson(), new TimeSpan(0, this.config.SmsEffectiveMinutes, 0));
            return Result.Success(ctx.Id);
        }

        public async Task<object> ResetPassword(string mobile, HttpContext context)
        {
            var md5Mobile = mobile.Md5();
            var user = await this.service.GetUserByMobile(md5Mobile);
            if (user == null)
            {
                return new Result { Code = -1, Msg = "手机号未注册" };
            }
            var counterKey = "V:User:Services:MobileAccountService:Counter:" + mobile;
            var counter = this.cacheService.StringGet(counterKey).ToObj<Counter>();
            if (counter == null)
            {
                counter = new Counter();
            }
            if (counter.Over(this.config.SmsTimesDaily))
            {
                return new Result { Code = -1, Msg = "该手机号发送短信验证码次数已超过今日上限" };
            }

            var random = new Random($"{mobile}{DateTime.Now.Ticks}".GetHashCode());
            var verificationCode = random.Next(899999) + 100000;
            var expiration = DateTime.Now.AddMinutes(this.config.SmsEffectiveMinutes);
            var ctx = new Context
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = "ResetPasswordWithMobile",
                Step = 210,
                VerificationCode = verificationCode.ToString(),
                UserId = user.Id,
                Expiration = expiration
            };

            if (!await this.smsService.SendSms(mobile, ctx.VerificationCode))
            {
                return new Result { Code = -1, Msg = "短信验证码发送失败" };
            }

            counter.Inc();
            this.cacheService.StringSet(counterKey, counter.ToJson(), counter.Expiration - DateTime.Now);
            this.cacheService.StringSet("V:User:Services:MobileAccountService:Context:" + ctx.Id,
                ctx.ToJson(), new TimeSpan(0, this.config.SmsEffectiveMinutes, 0));
            return Result.Success(ctx.Id);
        }

        public async Task<object> SignUp(string mobile, string password, HttpContext context)
        {
            if (string.IsNullOrWhiteSpace(mobile))
            {
                return new Result { Code = -1, Msg = "手机号不能为空" };
            }

            var md5Mobile = mobile.Md5();
            var user = await this.service.GetUserByMobile(md5Mobile);
            if (user != null)
            {
                if (!string.IsNullOrWhiteSpace(user.SourceName))
                {
                    return Result<UserEntity>.Fail(msg: $"该手机号已在 {user.SourceName} 注册过，请使用密码登录");
                }

                var serviceName = Utility.GetServiceProductName(user.Source);
                if (!string.IsNullOrWhiteSpace(serviceName))
                {
                    return Result<UserEntity>.Fail(msg: $"该手机号已在 {serviceName} 注册过，请使用密码登录");
                }

                return Result<UserEntity>.Fail(msg: "该手机号已注册过，请使用密码登录");
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
                    Md5Mobile = md5Mobile,
                    MaskMobile = mobile.MaskMobile(),
                    EncryptedMobile = mobile.DESEncrypt(),
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

            var counterKey = "V:User:Services:MobileAccountService:Counter:" + mobile;
            var counter = this.cacheService.StringGet(counterKey).ToObj<Counter>();
            if (counter == null)
            {
                counter = new Counter();
            }
            if (counter.Over(this.config.SmsTimesDaily))
            {
                return new Result { Code = -1, Msg = "该手机号发送短信验证码次数已超过今日上限" };
            }

            var random = new Random($"{mobile}{DateTime.Now.Ticks}".GetHashCode());
            var verificationCode = random.Next(899999) + 100000;
            var expiration = DateTime.Now.AddMinutes(this.config.SmsEffectiveMinutes);
            var ctx = new Context
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = "SignUpWithMobile",
                Step = 200,
                VerificationCode = verificationCode.ToString(),
                Mobile = mobile,
                Expiration = expiration
            };

            if (!await this.smsService.SendSms(mobile, ctx.VerificationCode))
            {
                return new Result { Code = -1, Msg = "短信验证码发送失败" };
            }

            counter.Inc();
            this.cacheService.StringSet(counterKey, counter.ToJson(), counter.Expiration - DateTime.Now);
            this.cacheService.StringSet("V:User:Services:MobileAccountService:Context:" + ctx.Id,
                ctx.ToJson(), new TimeSpan(0, this.config.SmsEffectiveMinutes, 0));
            return Result.Success(ctx.Id);
        }

        public async Task<object> UpdateUserInfo(long userId, ModifiableUserInfo user, HttpContext context)
        {
            var usr = await this.service.GetUser(userId);
            if (usr == null)
            {
                return new Result { Code = -1, Msg = "用户不存在" };
            }

            if (!string.IsNullOrWhiteSpace(user.Mobile))
            {
                if (!user.Mobile.IsValidMobile())
                {
                    return new Result { Code = -1, Msg = "请输入正确的手机号" };
                }

                var md5Mobile = user.Mobile.Md5();
                if (md5Mobile == usr.Md5Mobile)
                {
                    return new Result { Code = -1, Msg = "请设置不同的手机号" };
                }

                var newUser = await this.service.GetUserByMobile(md5Mobile);
                if (newUser != null)
                {
                    return new Result { Code = -1, Msg = "该手机号已被注册" };
                }
                var counterKey = "V:User:Services:MobileAccountService:Counter:" + user.Mobile;
                var counter = this.cacheService.StringGet(counterKey).ToObj<Counter>();
                if (counter == null)
                {
                    counter = new Counter();
                }
                if (counter.Over(this.config.SmsTimesDaily))
                {
                    return new Result { Code = -1, Msg = "该手机号发送短信验证码次数已超过今日上限" };
                }

                var random = new Random($"{user.Mobile}{DateTime.Now.Ticks}".GetHashCode());
                var verificationCode = random.Next(899999) + 100000;
                var expiration = DateTime.Now.AddMinutes(this.config.SmsEffectiveMinutes);
                var ctx = new Context
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Name = "ChangeMobile",
                    Step = 220,
                    VerificationCode = verificationCode.ToString(),
                    UserId = userId,
                    Mobile = user.Mobile,
                    Expiration = expiration
                };

                if (!await this.smsService.SendSms(user.Mobile, ctx.VerificationCode))
                {
                    return new Result { Code = -1, Msg = "短信验证码发送失败" };
                }

                counter.Inc();
                this.cacheService.StringSet(counterKey, counter.ToJson(), counter.Expiration - DateTime.Now);
                this.cacheService.StringSet("V:User:Services:MobileAccountService:Context:" + ctx.Id,
                    ctx.ToJson(), new TimeSpan(0, this.config.SmsEffectiveMinutes, 0));
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
            if (!string.IsNullOrWhiteSpace(user.Mail))
            {
                usr.Mail = user.Mail;
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
            var ctxKey = "V:User:Services:MobileAccountService:Context:" + contextId;
            var ctxJson = this.cacheService.StringGet(ctxKey);
            if (ctxJson == null)
            {
                return new Result { Code = -1, Msg = "验证码已过期" };
            }
            var ctx = ctxJson.ToObj<Context>();
            if (ctx.VerificationCode != code)
            {
                return new Result { Code = -1, Msg = "验证码错误" };
            }

            if (ctx.Step == 200) // 注册验证
            {
                var md5Mobile = ctx.Mobile.Md5();
                var user = await this.service.GetUserByMobile(md5Mobile);
                if (user != null)
                {
                    return new Result { Code = -1, Msg = "该手机号已注册" };
                }

                user = new UserEntity
                {
                    Source = this.config.ServiceCode,
                    SourceName = this.config.ServiceName,
                    Md5Mobile = md5Mobile,
                    MaskMobile = ctx.Mobile.MaskMobile(),
                    EncryptedMobile = ctx.Mobile.DESEncrypt()
                };
                var id = await this.service.CreateUser(user);
                if (id <= 0)
                {
                    return Result<UserEntity>.Fail();
                }

                user.Id = id;
                ctx.Step = 201;
                this.cacheService.StringSet(ctxKey, ctx.ToJson(), ctx.Expiration - DateTime.Now);
                return Result.Success(context.GetLoginedUser(user));
            }
            else if (ctx.Step == 210) // 重置密码验证
            {
                var user = await this.service.GetUser(ctx.UserId);
                user.Password = null;
                if (!await this.service.UpdateUser(user))
                {
                    return new Result { Code = -1 };
                }

                ctx.Step = 211;
                this.cacheService.StringSet(ctxKey, ctx.ToJson(), ctx.Expiration - DateTime.Now);
                return Result.Success(context.GetLoginedUser(user));
            }
            else if (ctx.Step == 220) // 更换手机号验证
            {
                var md5Mobile = ctx.Mobile.Md5();
                var user = await this.service.GetUserByMobile(md5Mobile);
                if (user != null)
                {
                    return new Result { Code = -1, Msg = "该手机号已注册" };
                }

                user = await this.service.GetUser(ctx.UserId);
                user.Md5Mobile = md5Mobile;
                user.MaskMobile = ctx.Mobile.MaskMobile();
                user.EncryptedMobile = ctx.Mobile.DESEncrypt();
                if (!await this.service.UpdateUser(user))
                {
                    return new Result { Code = -1 };
                }

                ctx.Step = 221;
                this.cacheService.StringSet(ctxKey, ctx.ToJson(), ctx.Expiration - DateTime.Now);
                return new Result();
            }
            else if (ctx.Step == 230) // 登录验证
            {
                var user = await this.service.GetUser(ctx.UserId);
                ctx.Step = 231;
                this.cacheService.StringSet(ctxKey, ctx.ToJson(), ctx.Expiration - DateTime.Now);
                return Result.Success(context.GetLoginedUser(user));
            }

            return new Result { Code = -1, Msg = "无法识别" };
        }
    }
}
