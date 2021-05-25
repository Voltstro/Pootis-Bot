using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using Quartz.Logging;

namespace Pootis_Bot.Jobs
{
    public static class JobsSystem
    {
        private static StdSchedulerFactory factory;
        private static IScheduler scheduler;
        
        internal static void InitJobs()
        {
            LogProvider.SetCurrentLogProvider(new QuartzLogger());
            factory = new StdSchedulerFactory();
            scheduler = factory.GetScheduler().GetAwaiter().GetResult();
            scheduler.Start().GetAwaiter().GetResult();
        }

        internal static void Shutdown()
        {
            scheduler.Shutdown().GetAwaiter().GetResult();
        }

        public static Job CreateJob<T>() where T : IJob
        {
            IJobDetail job = JobBuilder.Create<T>()
                .WithIdentity($"{nameof(T)}-Job", "Jobs")
                .Build();

            SimpleScheduleBuilder scheduleBuilder = null;
            TriggerBuilder triggerBuilder = TriggerBuilder.Create()
                .WithIdentity($"{nameof(T)}-Trigger", "Jons")
                .WithSimpleSchedule(x => scheduleBuilder = x);
            return new Job(job, triggerBuilder, scheduleBuilder);
        }

        public static async Task ScheduleJob(Job job)
        {
            await scheduler.ScheduleJob(job.QuartzJob, job.QuartzTrigger.Build());
        }
    }
}