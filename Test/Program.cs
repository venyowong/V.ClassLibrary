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
using V.Finance.Services;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var financialService = new FinancialService();
var rate = await financialService.GetRiskFreeRate();
var fundService = new FundService();
var navs = await fundService.GetFundNavs("007994");
var begin = DateTime.Now.Date.AddYears(-3);
navs = navs.FindAll(x => x.Date >= begin);
var yieldAnnual = financialService.CalcYieldAnnual(navs);
var fisk = financialService.CalcDownsideRisk(navs);
var sortino = await financialService.CalcSortinoRatio(navs, rate);
//var result = fundService.GetFundManagerChangeList("010430");

var query = new QueryExpression("(sizeLevel == 'B' || sizeLevel == 'KB') && (creationDate >= '2022-12-25' && creationDate <= '2023-05-01')");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<IJobFactory, CustomJobFactory>();
builder.Services.AddHostedService<QuartzHostedService>();

builder.Services.AddTransientBothTypes<IScheduledJob, HelloJob>();

builder.Services.AddTransient(_ =>
{
    var db = new QueryFactory(new NpgsqlConnection("Host=127.0.0.1;Username=postgres;Password=venyo283052;Database=vbranch"), new PostgresCompiler());
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
        c.ServiceCode = "V.Coox";
        c.ServiceName = "Coox";
        c.NeedVerificationForSignUp = true;
        c.AccountMode = 0;
        c.CacheMode = 1;
        c.RedisConnectionString = builder.Configuration["Redis:ConnectionString"];
        c.RedisDb = int.Parse(builder.Configuration["Redis:Db"]);
        c.SmtpServer = builder.Configuration["Mail:Smtp:Server"];
        c.SmtpPort = int.Parse(builder.Configuration["Mail:Smtp:Port"]);
        c.AdmMailAccount = builder.Configuration["Mail:Admin:Account"];
        c.AdmMailPwd = builder.Configuration["Mail:Admin:Password"];
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
