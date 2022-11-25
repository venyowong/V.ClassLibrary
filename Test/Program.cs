using Quartz.Spi;
using Serilog;
using Test;
using V.Common.Extensions;
using V.Quartz;
using V.QueryParser;
using V.User.OAuth;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var query = new QueryExpression("key1.key2 like 'val'");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<IJobFactory, CustomJobFactory>();
builder.Services.AddHostedService<QuartzHostedService>();

builder.Services.AddTransientBothTypes<IScheduledJob, HelloJob>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<ILoginService, LoginService>()
    .AddOAuth();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.UseOAuth();

app.MapControllers();

app.Run();
