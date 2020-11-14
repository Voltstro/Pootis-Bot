using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Pootis_Bot.PackageDownloader;

namespace Pootis_Bot.Tests
{
	public class PackageDownloaderTests
	{
		private NuGetPackageResolver packageResolver;

		[SetUp]
		public void Setup()
		{
			packageResolver = new NuGetPackageResolver("net5.0");
		}

		[Test]
		public void NewtonSoft1203DownloadTest()
		{
			List<string> excepted = new List<string>
			{
				Path.GetFullPath("Packages/Newtonsoft.Json.12.0.3/lib/netstandard2.0/Newtonsoft.Json.dll")
			};
			List<string> dlls = packageResolver.DownloadPackage("Newtonsoft.Json", new Version(12, 0, 3)).GetAwaiter()
				.GetResult();
			Assert.AreEqual(excepted, dlls);
		}

		[Test]
		public void WikiNet300DownloadTest()
		{
			List<string> excepted = new List<string>
			{
				Path.GetFullPath("Packages/Newtonsoft.Json.12.0.3/lib/netstandard2.0/Newtonsoft.Json.dll"),
				Path.GetFullPath("Packages/Wiki.Net.3.0.0/lib/netstandard2.0/Wiki.Net.dll")
			};
			List<string> dlls = packageResolver.DownloadPackage("Wiki.Net", new Version(3, 0, 0)).GetAwaiter()
				.GetResult();
			Assert.AreEqual(excepted, dlls);
		}
	}
}