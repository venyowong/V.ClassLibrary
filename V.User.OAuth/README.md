# V.User.OAuth

木叉一人工作室封装的 OAuth 类库，支持授权登录成功后进行自定义操作

适用于 asp.net core 的 OAuth 实现，框架支持在授权后自定义操作，比如用户入库、生成JWT便于后续鉴权等

## Nuget

[V.User.OAuth](https://www.nuget.org/packages/V.User.OAuth)

## 官方示例项目
[V.TouristGuide](https://github.com/venyowong/V.TouristGuide)

## 支持的第三方平台

### Github

```
"OAuth": {
    "Github": {
        "client_id": "xxx",
        "client_secret": "xxx"
    }
}
```
[Github OAuth 应用申请链接](https://github.com/settings/developers)

Github 应用设置页面回调地址格式：`https://localhost:7167/usermodule`

### Gitee

```
"OAuth": {
    "Gitee": {
        "client_id": "xxx",
        "client_secret": "xxx"
    }
}
```
[Gitee OAuth 应用申请链接](https://gitee.com/oauth/applications)

Gitee 应用回调地址格式：`https://localhost:7167/usermodule/authorize?service=gitee`

### Baidu

```
"OAuth": {
    "Baidu": {
        "client_id": "xxx",
        "client_secret": "xxx"
    }
}
```

[Baidu OAuth 应用申请链接](http://developer.baidu.com/console#app/project)

Baidu 应用回调地址格式：`https://localhost:7167/usermodule/authorize?service=baidu`

百度估计很久没维护 OAuth 功能了，应用申请页面比较拉跨，授权得到的用户信息也很少

## 在 asp.net core 项目中使用

1. 创建自定义类型实现 `ILoginService` 接口
    ```
    public class LoginService : ILoginService
    {
        public async Task Login(HttpContext context, UserInfo user)
        {
            if (user.Source == "github")
            {
                // do something
            }
            if (user.Source == "gitee")
            {
                // do something
            }
            await context.Response.WriteAsJsonAsync(user);
        }
    }
    ```
2. 往容器内注入服务 `services.AddTransient<ILoginService, LoginService>()`
3. 启用 OAuth
    ```
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddOAuth();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseOAuth();
    }
    ```
4. OAuth 授权登录链接： https://localhost:7167/usermodule/oauth?service=github

    注意: 如果你使用的是 Github 授权登录，你可以在 `https://localhost:7167/usermodule/oauth?service=github` 之后添加任何参数, 例如 `https://localhost:7167/usermodule/oauth?service=github&key1=value1&key2=value2`, 你将能够在 ILoginService 的回调方法中通过 context 获取到这些参数
    
    但是如果用的是 Gitee，在 url 添加的参数将无法在 Login 方法中获取到，因为 Gitee 的回调链接必须是固定配置的，不接受动态参数。因此，若在使用 Gitee 授权结束后，希望能有不同的处理流程，需要在后续流程进行处理，比如需要区分平台，打开不同的页面，则可以在 Login 方法中返回一个中间页面，处理跳转逻辑。

### 其他三方平台还在努力支持中...

## 如果这个项目有帮助到你，不妨支持一下

![](https://raw.githubusercontent.com/venyowong/V.ClassLibrary/main/imgs/%E5%BE%AE%E4%BF%A1%E6%94%B6%E6%AC%BE%E7%A0%81.jpg)
![](https://raw.githubusercontent.com/venyowong/V.ClassLibrary/main/imgs/%E6%94%AF%E4%BB%98%E5%AE%9D%E6%94%B6%E6%AC%BE%E7%A0%81.jpg)