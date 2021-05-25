using Quartz;

namespace Pootis_Bot.Jobs
{
    public class Job
    {
        internal Job(IJobDetail job, TriggerBuilder trigger, SimpleScheduleBuilder scheduleBuilder)
        {
            QuartzJob = job;
            QuartzTrigger = trigger;
            QuartzScheduleBuilder = scheduleBuilder;
        }
        
        internal IJobDetail QuartzJob { get; }
        internal TriggerBuilder QuartzTrigger { get; }
        internal SimpleScheduleBuilder QuartzScheduleBuilder { get; }
    }
}