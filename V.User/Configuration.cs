using System;
using System.Collections.Generic;
using System.Text;

namespace V.User
{
    public class Configuration
    {
        public string ServiceCode { get; set; }

        public string ServiceName { get; set; }

        /// <summary>
        /// 缓存方式 0 Memory 1 Redis
        /// </summary>
        public int CacheMode { get; set; }

        public string RedisConnectionString { get; set; }

        public int RedisDb { get; set; }

        public string VerificationCodeMail4SignUp { get; set; } = "您正在注册 {serviceName} 账号，请在 {expiration} 前，使用验证码 {code} 完成注册";

        public string VerificationCodeMail4Login { get; set; } = "您正在登录 {serviceName} 账号，请在 {expiration} 前，使用验证码 {code} 完成登录";

        public string VerificationCodeMail4ResetPwd { get; set; } = "您正在重置 {serviceName} 账号的密码，请在 {expiration} 前，使用验证码 {code} 完成密码重置";

        public string VerificationCodeMail4Change { get; set; } = "您正在更换 {serviceName} 账号的邮箱，请在 {expiration} 前，使用验证码 {code} 完成邮箱更换";

        public string SignUpSMSTemplate { get; set; } = "";

        /// <summary>
        /// 账户主体 0 Mail 1 Mobile
        /// </summary>
        public int AccountMode { get; set; }

        /// <summary>
        /// 注册时是否需要验证，否则使用密码直接注册
        /// </summary>
        public bool NeedVerificationForSignUp { get; set; }

        /// <summary>
        /// 短信验证码有效分钟数
        /// </summary>
        public int SmsEffectiveMinutes { get; set; } = 5;

        /// <summary>
        /// 验证邮件有效分钟数
        /// </summary>
        public int MailEffectiveMinutes { get; set; } = 30;

        /// <summary>
        /// 同一个手机号每日发送短信次数限制
        /// </summary>
        public int SmsTimesDaily { get; set; } = 5;

        /// <summary>
        /// 同一个邮箱每日发送邮件次数限制
        /// </summary>
        public int MailTimesDaily { get; set; } = 10;

        /// <summary>
        /// jwt token 有效分钟数
        /// <para><= 0 则为永久有效</para>
        /// </summary>
        public int TokenEffectiveMinutes { get; set; } = 0;

        public string SmtpServer { get; set; }

        public int SmtpPort { get; set; }

        public string AdmMailAccount { get; set; }

        public string AdmMailPwd { get; set; }

        /// <summary>
        /// 腾讯云短信 secretid
        /// </summary>
        public string TencentSmsSecretId { get; set; }

        /// <summary>
        /// 腾讯云短信 secretkey
        /// </summary>
        public string TencentSmsSecretKey { get; set; }

        /// <summary>
        /// 腾讯云短信域名
        /// </summary>
        public string TencentSmsEndpoint { get; set; } = "sms.tencentcloudapi.com";

        /// <summary>
        /// 腾讯云短信地域
        /// </summary>
        public string TencentSmsRegion { get; set; } = "ap-guangzhou";

        public string TencentSmsAppId { get; set; }

        /// <summary>
        /// 腾讯云短信已审核过的签名
        /// </summary>
        public string TencentSmsSignName { get; set; }

        /// <summary>
        /// 腾讯云短信已审核过的模板id
        /// </summary>
        public string TencentSmsTemplateId { get; set; }
    }
}
