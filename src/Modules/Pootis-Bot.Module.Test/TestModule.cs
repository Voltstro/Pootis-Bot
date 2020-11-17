using System;
using Newtonsoft.Json;
using Pootis_Bot.Config;
using Pootis_Bot.Logging;
using Pootis_Bot.Modules;
using YoutubeExplode;

namespace Pootis_Bot.Module.Test
{
	public class TestModule : IModule
	{
		public ModuleInfo GetModuleInfo()
		{
			return new ModuleInfo("Test Module", new Version(1, 0),
				new ModuleNuGetPackage("YoutubeExplode", new Version(5, 1, 8), "YoutubeExplode"));
		}

		public void Init()
		{
			Logger.Info("Hello World!");

			TestThing testThing = Config<TestThing>.Instance;
			AnotherTestThing anotherTestTestThing = Config<AnotherTestThing>.Instance;

			YoutubeClient client = new YoutubeClient();

			Logger.Info(Config<TestThing>.Instance.Bruh);
			Logger.Info(Config<AnotherTestThing>.Instance.Voltstro);

			anotherTestTestThing.EternalClickbait = "Is gay";
			anotherTestTestThing.Save();
		}

		public void Dispose()
		{
			Logger.Info("Shutdown stuff!");
		}

		public class TestThing : Config<TestThing>
		{
			public string Bruh { get; set; } = "Bruh Moment";
		}

		public class AnotherTestThing : Config<AnotherTestThing>
		{
			public string Voltstro { get; set; } = "Is the best.";
			public string EternalClickbait { get; set; } = "Is cool.";
		}
	}
}