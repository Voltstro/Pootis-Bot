﻿using System;
using Pootis_Bot.Config;
using Pootis_Bot.Logging;
using Pootis_Bot.Modules;
using YoutubeExplode;

namespace Pootis_Bot.Module.Test
{
	public class TestModule : Modules.Module
	{
		public override ModuleInfo GetModuleInfo()
		{
			return new ModuleInfo("Test Module", new Version(1, 0),
				new ModuleDependency("YoutubeExplode", new Version(5, 1, 8), "YoutubeExplode"),
				new ModuleDependency("TestModule2"));
		}

		public override void Init()
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

		public class TestThing : Config<TestThing>
		{
			public TestThing()
			{
				ExpectedConfigVersion = 2;
			}

			public string Bruh { get; set; } = "Bruh Moment";
		}

		public class AnotherTestThing : Config<AnotherTestThing>
		{
			public string Voltstro { get; set; } = "Is the best.";
			public string EternalClickbait { get; set; } = "Is cool.";
		}
	}
}