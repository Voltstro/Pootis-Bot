namespace Pootis_Bot.Jobs
{
    public static class JobsExtensions
    {
        public static Job WithIntervalInSeconds(this Job job, int seconds)
        {
            job.QuartzScheduleBuilder.WithIntervalInSeconds(seconds);
            return job;
        }
        
        public static Job RepeatForever(this Job job)
        {
            job.QuartzScheduleBuilder.RepeatForever();
            return job;
        }

        public static Job WithRepeatCount(this Job job, int repeatCount)
        {
            job.QuartzScheduleBuilder.WithRepeatCount(repeatCount);
            return job;
        }
    }
}