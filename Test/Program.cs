using Quartz.Spi;
using Serilog;
using Test;
using V.Common.Extensions;
using V.Quartz;
using V.QueryParser;
using V.User.OAuth;
using V.User.Services;
using V.User.Extensions;
using SqlKata.Compilers;
using SqlKata.Execution;
using Npgsql;
using V.User;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var query = new QueryExpression("(sizeLevel == 'B' || sizeLevel == 'KB') && (creationDate >= '2022-12-25' && creationDate <= '2023-05-01')");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<IJobFactory, CustomJobFactory>();
builder.Services.AddHostedService<QuartzHostedService>();

builder.Services.AddTransientBothTypes<IScheduledJob, HelloJob>();

builder.Services.AddTransient(_ =>
{
    var db = new QueryFactory(new NpgsqlConnection("Host=127.0.0.1;Username=postgres;Password=venyo283052;Database=postgres"), new PostgresCompiler());
    db.Logger = sql =>
    {
        Log.Information(sql.Sql);
    };
    return db;
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<ILoginService, LoginService>()
    .AddOAuth()
    .AddJwt(builder.Configuration["Jwt:Secret"])
    .AddUserModule(c =>
    {
        c.ServiceCode = "TouristGuide";
        c.ServiceName = "¬√”Œ÷∏ƒœ";
        c.NeedVerificationForSignUp = true;
        c.AccountMode = 1;
        c.TencentSmsSecretId = builder.Configuration["Tencent:Sms:SecretId"];
        c.TencentSmsSecretKey = builder.Configuration["Tencent:Sms:SecretKey"];
        c.TencentSmsAppId = builder.Configuration["Tencent:Sms:AppId"];
        c.TencentSmsSignName = builder.Configuration["Tencent:Sms:SignName"];
        c.TencentSmsTemplateId = builder.Configuration["Tencent:Sms:TemplateId"];
    }, new UserModuleCallback());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseUserModule(true);

app.UseAuthorization();

app.UseOAuth();

app.MapControllers();

app.Run();
