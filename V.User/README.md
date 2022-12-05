# V.User

木叉一人工作室封装的用户模块，包括手机号、邮箱登录以及 OAuth 授权登录等

## Nuget

[V.User](https://www.nuget.org/packages/V.User)

## 官方示例项目
[V.TouristGuide](https://github.com/venyowong/V.TouristGuide)

## 初始化数据库
本项目目前只支持 PostgreSQL，首先在你的项目数据库中执行 user 表初始化脚本 init_user.sql
```
su postgres
psql -d dbName
\i init_user.sql
```
并且在代码里将 IDbConnection 注入到容器中

## 如何使用

```
public void ConfigureServices(IServiceCollection services)
{
    services.AddJwt("JwtSecret")
        .AddUserModule(new UserModuleCallback(builder.Configuration), c =>
        {
            c.ServiceCode = "自定义服务唯一编码";
            c.ServiceName = "自定义服务名称";
            c.NeedVerificationForSignUp = true; // 注册时是否需要验证，否则使用密码直接注册
            c.AccountMode = 1; // 账户主体 0 Mail 1 Mobile
            c.TencentSmsSecretId = "腾讯云短信服务 SecretId";
            c.TencentSmsSecretKey = "腾讯云短信服务 SecretKey";
            c.TencentSmsAppId = "腾讯云短信服务 AppId";
            c.TencentSmsSignName = "腾讯云短信已审核过的签名";
            c.TencentSmsTemplateId = "腾讯云短信已审核过的模板id";
        });

    // 使用 SqlKata 来访问数据库，这样后续就可以支持多种数据库而不用改代码了
    services.AddTransient(_ =>
    {
        var db = new QueryFactory(new NpgsqlConnection("Host=127.0.0.1;Username=postgres;Password=xxx;Database=postgres"), new PostgresCompiler());
        return db;
    });
}

public void Configure(IApplicationBuilder app)
{
    app.UseUserModule(true);
}

public class UserModuleCallback : IUserModuleCallback
{
    private IConfiguration config;

    public UserModuleCallback(IConfiguration config)
    {
        this.config = config;
    }

    public Task OnOAuthLogin(HttpContext context, UserModel user)
    {
        context.Response.Redirect($"{this.config["OAuthCallbackUrl"]}?token={user.Token}");
        return Task.CompletedTask;
    }
}
```
AddUserModule 方法有两个参数，第一个参数为 `Action<Configuration>` 用于修改用户模块配置；第二个参数为 IUserModuleCallback 接口，该参数用于接收 OAuth 授权登录回调，若该参数为空则不启用 OAuth

UseUserModule 方法有一个可选参数 useOAuth 表示是否启用 OAuth，若调用 AddUserModule 方法时没有传第二个参数，则这里的 useOAuth 应为 false，否则服务启动时会报依赖的服务不存在

## 接口

### POST /usermodule/signup
注册账号，若 NeedVerificationForSignUp 为 true，则会发送验证邮件或短信
```
Request：
{
    "mail": "", // 若账户主体为邮箱则必传
    "mobile": "", // 若账户主体为手机号则必传
    "password": "" // 若 NeedVerificationForSignUp 为 false，则密码必传
}
Response：
{
    "code": 0, // 0 成功 -1 失败
    "msg": "", // 失败原因
    "data": "contextId", // 在使用验证码注册的场景下会返回上下文 id，用于后续接口调用
    "data": { // 若使用密码注册成功时直接返回用户信息
        "id": 6,
        "name": "Venyo Wong",
        "avatar": "https://avatars.githubusercontent.com/u/5902200?v=4",
        "gender": 0,
        "source": "github",
        "sourceName": null,
        "platformId": "5902200",
        "mail": "venyowong@163.com",
        "location": "Shanghai",
        "company": "",
        "bio": "Hou tui, wo yao kai shi zhuang bi le.",
        "maskMobile": null,
        "md5Mobile": null,
        "encryptedMobile": null,
        "createTime": "2022-09-24T05:20:04.044308Z",
        "updateTime": "2022-10-04T07:30:12.864247Z",
        "token": null,
        "canSetPwd": false // 是否可以设置密码
    }
}
```

### POST /usermodule/login
登录接口
```
Request：
{
    "mail": "", // 若账户主体为邮箱则必传
    "mobile": "", // 若账户主体为手机号则必传
    "password": "" // 若密码为空，则使用验证码登录
}
Response：
{
    "code": 0, // 0 成功 -1 失败
    "msg": "", // 失败原因
    "data": "contextId", // 在使用验证码登录的场景下会返回上下文 id，用于后续接口调用
    "data": { // 若使用密码登录成功时直接返回用户信息
        "id": 6,
        "name": "Venyo Wong",
        "avatar": "https://avatars.githubusercontent.com/u/5902200?v=4",
        "gender": 0,
        "source": "github",
        "sourceName": null,
        "platformId": "5902200",
        "mail": "venyowong@163.com",
        "location": "Shanghai",
        "company": "",
        "bio": "Hou tui, wo yao kai shi zhuang bi le.",
        "maskMobile": null,
        "md5Mobile": null,
        "encryptedMobile": null,
        "createTime": "2022-09-24T05:20:04.044308Z",
        "updateTime": "2022-10-04T07:30:12.864247Z",
        "token": null,
        "canSetPwd": false // 是否可以设置密码
    }
}
```

### POST /usermodule/setpassword
设置密码接口，仅支持在用户密码为空时，例如使用验证码注册或 OAuth 登录
```
Request：
{
    "token": "", // 登录或注册后获取到的 token，该参数可放在 queryString、form、headers、body 中
    "password": ""
}
Response：
{
    "code": 0, // 0 成功 -1 失败
    "msg": "" // 失败原因
}
```

### POST /usermodule/resetpassword
使用旧密码重置密码接口
```
Request：
{
    "token": "", // 登录或注册后获取到的 token，该参数可放在 queryString、form、headers、body 中
    "oldPwd": "",
    "newPwd": ""
}
Response：
{
    "code": 0, // 0 成功 -1 失败
    "msg": "" // 失败原因
}
```

### POST /usermodule/forgetpassword
忘记密码时，使用验证码重置密码接口
```
Request：
{
    "mail": "", // 若账户主体为邮箱则必传
    "mobile": "" // 若账户主体为手机号则必传
}
Response：
{
    "code": 0, // 0 成功 -1 失败
    "msg": "", // 失败原因
    "data": "contextId" // 上下文 id，用于后续接口调用
}
```

### POST /usermodule/verifycode
验证码校验接口，所有验证码场景都可以调用该接口进行校验
```
Request：
{
    "contextId": "", // 上下文 id
    "code": "" // 验证码
}
Response：
{
    "code": 0, // 0 成功 -1 失败
    "msg": "", // 失败原因
    "data": { // 仅在注册成功、重置密码成功、登录成功的场景下会返回该字段
        "id": 6,
        "name": "Venyo Wong",
        "avatar": "https://avatars.githubusercontent.com/u/5902200?v=4",
        "gender": 0,
        "source": "github",
        "sourceName": null,
        "platformId": "5902200",
        "mail": "venyowong@163.com",
        "location": "Shanghai",
        "company": "",
        "bio": "Hou tui, wo yao kai shi zhuang bi le.",
        "maskMobile": null,
        "md5Mobile": null,
        "encryptedMobile": null,
        "createTime": "2022-09-24T05:20:04.044308Z",
        "updateTime": "2022-10-04T07:30:12.864247Z",
        "token": null,
        "canSetPwd": false // 是否可以设置密码
    }
}
```

### POST /usermodule/updateinfo
更新用户信息接口
```
Request：
{
    "token": "", // 登录或注册后获取到的 token，该参数可放在 queryString、form、headers、body 中
    "name": "", // 昵称
    "avatar": "", // 头像
    "mail": "", // 邮箱，在用户主体为邮箱时，该参数不为空时，将作为更换邮箱操作，其他字段不起作用
    "location": "", // 地址
    "company": "", // 公司
    "bio": "", // 简介
    "mobile": "" // 手机号，在用户主体为手机号时，该参数不为空时，将作为更换手机号操作，其他字段不起作用
}
Response：
{
    "code": 0, // 0 成功 -1 失败
    "msg": "", // 失败原因
    "data": "contextId" // 仅在更换邮箱、手机号时会返回该字段
}
```

### /usermodule/info
获取用户信息
```
Request：
{
    "token": "" // 登录或注册后获取到的 token，该参数可放在 queryString、form、headers、body 中
}
Response：
{
    "id": 6,
    "name": "Venyo Wong",
    "avatar": "https://avatars.githubusercontent.com/u/5902200?v=4",
    "gender": 0,
    "source": "github",
    "sourceName": null,
    "platformId": "5902200",
    "mail": "venyowong@163.com",
    "location": "Shanghai",
    "company": "",
    "bio": "Hou tui, wo yao kai shi zhuang bi le.",
    "maskMobile": null,
    "md5Mobile": null,
    "encryptedMobile": null,
    "createTime": "2022-09-24T05:20:04.044308Z",
    "updateTime": "2022-10-04T07:30:12.864247Z",
    "canSetPwd": false // 是否可以设置密码
}
```

## 启用 OAuth
调用 AddUserModule 方法时传入 IUserModuleCallback 参数，并且在调用 UseUserModule 方法时传入 true，即可启用 OAuth，具体配置方法可参考 V.User.OAuth 项目

值得注意的是，若用户在使用 OAuth 登录后修改的用户信息，可能在下次 OAuth 登录时被覆盖为三方平台数据

## 注
该用户模块目前只有账号主体为手机号的场景详细测试过，若使用过程中遇到问题，请提交 issue 或 PR

## 如果这个项目有帮助到你，不妨支持一下

![](https://raw.githubusercontent.com/venyowong/V.ClassLibrary/main/imgs/%E5%BE%AE%E4%BF%A1%E6%94%B6%E6%AC%BE%E7%A0%81.jpg)
![](https://raw.githubusercontent.com/venyowong/V.ClassLibrary/main/imgs/%E6%94%AF%E4%BB%98%E5%AE%9D%E6%94%B6%E6%AC%BE%E7%A0%81.jpg)