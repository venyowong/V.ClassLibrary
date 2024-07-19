# 使用邮件推送

```
var mailService = new MailService(host, port, userName, password);
mailService.SendMail(title, content, "to1@mail.com", "to2@mail.com");
```

# 使用腾讯云短信服务

```
var smsService = new TencentSmsService(secretId, secretKey, region, appId, signName, templateId);
smsService.SendSms("1234567890", "param1", "param2");
```