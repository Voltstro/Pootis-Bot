using NUnit.Framework;
using Pootis_Bot.Logging;

namespace Pootis_Bot.Tests
{
    [SetUpFixture]
    public class PootisTests
    {
        [OneTimeSetUp]
        public void Setup()
        {
            Logger.Init();
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            Logger.Shutdown();
        }
    }
}