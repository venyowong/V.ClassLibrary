using Quartz;
using Serilog;
using V.Quartz;

namespace Test
{
    public class HelloJob : IJob, IScheduledJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            Log.Information("Hello");
            return Task.CompletedTask;
        }

        public IJobDetail GetJobDetail()
        {
            return JobBuilder.Create<HelloJob>()
                .WithIdentity("HelloJob", "V.Quartz")
                .StoreDurably()
                .Build();
        }

        public IEnumerable<ITrigger> GetTriggers()
        {
            yield return TriggerBuilder.Create()
                .WithIdentity("HelloJob_Trigger1", "V.Quartz")
                .WithCronSchedule("*/30 * * * * ?")
                .ForJob("HelloJob", "V.Quartz")
                .Build();

            yield return TriggerBuilder.Create()
                .WithIdentity("HelloJob_RightNow", "V.Quartz")
                .StartNow()
                .ForJob("HelloJob", "V.Quartz")
                .Build();
        }
    }
}
