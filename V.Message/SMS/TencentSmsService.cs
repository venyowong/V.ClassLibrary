using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TencentCloud.Common.Profile;
using TencentCloud.Common;
using TencentCloud.Sms.V20210111;
using TencentCloud.Sms.V20210111.Models;
using System.Linq;

namespace V.Message.SMS
{
    public class TencentSmsService
    {
        private Credential cred;
        private ClientProfile profile;
        private string region;
        private string appId;
        private string signName;
        private string templateId;

        public TencentSmsService(string secretId, string secretKey, string region, string appId, 
            string signName, string templateId)
        {
            this.cred = new Credential
            {
                SecretId = secretId,
                SecretKey = secretKey
            };
            this.profile = new ClientProfile
            {
                SignMethod = ClientProfile.SIGN_TC3SHA256,
                HttpProfile = new HttpProfile
                {
                    ReqMethod = "GET",
                    Endpoint = "sms.tencentcloudapi.com"
                }
            };
            this.region = region;
            this.appId = appId;
            this.signName = signName;
            this.templateId = templateId;
        }

        public async Task<bool> SendSms(string mobile, params string[] paramSet)
        {
            var client = new SmsClient(this.cred, this.region, this.profile);
            var req = new SendSmsRequest
            {
                SmsSdkAppId = this.appId,
                SignName = this.signName,
                TemplateId = this.templateId,
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
