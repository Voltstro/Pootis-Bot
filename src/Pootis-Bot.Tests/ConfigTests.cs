using System.IO;
using NUnit.Framework;
using Pootis_Bot.Config;
using Pootis_Bot.Logging;

namespace Pootis_Bot.Tests
{
	public class ConfigTests
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

			//Delete configs
			if(File.Exists($"Config/{typeof(TestConfig1).Name}.json"))
				File.Delete($"Config/{typeof(TestConfig1).Name}.json");

			if(File.Exists($"Config/{typeof(TestConfig2).Name}.json"))
				File.Delete($"Config/{typeof(TestConfig2).Name}.json");
		}

		[Test]
		public void ConfigTest1()
		{
			//Create a new config
			TestConfig1 config = Config<TestConfig1>.Instance;
			Assert.AreEqual(string.Empty, config.StringTest);
			Assert.AreEqual(TestConfig1.TestEnum.Off, config.EnumTest);

			//Values update
			config.StringTest = "String test";
			config.EnumTest = TestConfig1.TestEnum.On;
			config.Save();

			string json = config.ToJson();

			//Read the file our self and check it
			Assert.AreEqual(json, File.ReadAllText($"Config/{typeof(TestConfig1).Name}.json"));

			//Reloads the file
			config.Reload();

			//Values should be our updated ones
			Assert.AreEqual("String test", config.StringTest);
			Assert.AreEqual(TestConfig1.TestEnum.On, config.EnumTest);
		}

		[Test]
		public void ConfigTest2()
		{
			TestConfig2 config = Config<TestConfig2>.Instance;
			Assert.AreEqual(false, config.BoolTest);

			//Values update
			config.BoolTest = true;
			config.Save();

			string json = config.ToJson();

			//Read the file our self and check it
			Assert.AreEqual(json, File.ReadAllText($"Config/{typeof(TestConfig2).Name}.json"));

			config.Reload();

			Assert.AreEqual(true, config.BoolTest);
		}

		private class TestConfig1 : Config<TestConfig1>
		{
			public enum TestEnum : byte
			{
				On,
				Off
			}

			public string StringTest { get; set; } = string.Empty;
			public TestEnum EnumTest { get; set; } = TestEnum.Off;
		}

		private class TestConfig2 : Config<TestConfig2>
		{
			public bool BoolTest { get; set; }
		}
	}
}