using System;
using Newtonsoft.Json;
using Pootis_Bot.Core.Logging;
using Pootis_Bot.Modules;

namespace Pootis_Bot.Module.Test
{
	public class TestModule : IModule
	{
		public ModuleInfo GetModuleInfo()
		{
			return new ModuleInfo("Test Module", new Version(1, 0));
		}

		public void Init()
		{
			Logger.Info("Hello World!");

			TestThing testThing = new TestThing();
			Logger.Info(JsonConvert.SerializeObject(testThing, Formatting.Indented));
		}

		public void Dispose()
		{
			Logger.Info("Shutdown stuff!");
		}

		public class TestThing
		{
			public string Bruh { get; set; } = "Bruh Moment";
		}
	}
}