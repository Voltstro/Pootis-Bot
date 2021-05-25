using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Pootis_Bot.Jobs;
using Pootis_Bot.Logging;
using Quartz;

namespace Pootis_Bot.Tests
{
    public class JobsTest
    {
        private static int counter;
        
        [OneTimeSetUp]
        public void Setup()
        {
            Logger.Init();
            JobsSystem.InitJobs();
            counter = 0;
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            JobsSystem.Shutdown();
            Logger.Shutdown();
        }

        [Test]
        public void CreateJobsTest()
        {
            ManualResetEvent pause = new ManualResetEvent(false);
            
            Job testJob = JobsSystem.CreateJob<TestJob>();
            testJob.WithIntervalInSeconds(1);
            testJob.WithRepeatCount(4);
            JobsSystem.ScheduleJob(testJob);
            
            Assert.False(pause.WaitOne(10000));
            
            Assert.AreEqual(5, counter);
        }

        private class TestJob : IJob
        {
            public Task Execute(IJobExecutionContext context)
            {
                counter++;
                return Task.CompletedTask;
            }
        }
    }
}