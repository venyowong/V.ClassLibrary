using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TencentCloud.Common;
using TencentCloud.Common.Profile;
using TencentCloud.Sms.V20210111;
using TencentCloud.Sms.V20210111.Models;

namespace V.User.Services
{
    public class SmsService
    {
        private Configuration config;
        private Credential cred;
        private ClientProfile profile;

        public SmsService(Configuration config)
        {
            this.config = config;
            this.cred = new Credential
            {
                SecretId = config.TencentSmsSecretId,
                SecretKey = config.TencentSmsSecretKey
            };
            this.profile = new ClientProfile
            {
                SignMethod = ClientProfile.SIGN_TC3SHA256,
                HttpProfile = new HttpProfile
                {
                    ReqMethod = "GET",
                    Endpoint = config.TencentSmsEndpoint
                }
            };
        }

        public async Task<bool> SendSms(string mobile, params string[] paramSet)
        {
            var client = new SmsClient(this.cred, this.config.TencentSmsRegion, this.profile);
            var req = new SendSmsRequest
            {
                SmsSdkAppId = config.TencentSmsAppId,
                SignName = config.TencentSmsSignName,
                TemplateId = config.TencentSmsTemplateId,
                TemplateParamSet = paramSet,
                PhoneNumberSet = new string[] { "+86" + mobile }
            };
            var response = await client.SendSms(req);
            if (response?.SendStatusSet?.Any(x => x.Code?.Contains("Failed") ?? false) ?? false)
            {
                return false;
            }

            return true;
        }
    }
}
