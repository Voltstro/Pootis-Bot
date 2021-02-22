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
		private string packagesPath;

		[OneTimeSetUp]
		public void Setup()
		{
			packagesPath = $"{Path.GetDirectoryName(typeof(PackageDownloaderTests).Assembly.Location)}/Packages/";
			packageResolver = new NuGetPackageResolver("netstandard2.1", packagesPath);
		}

		[Test]
		public void NewtonSoft1203DownloadTest()
		{
			List<string> excepted = new List<string>
			{
				Path.GetFullPath($"{packagesPath}/Newtonsoft.Json.12.0.3/lib/netstandard2.0/Newtonsoft.Json.dll")
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
				Path.GetFullPath($"{packagesPath}/Newtonsoft.Json.12.0.3/lib/netstandard2.0/Newtonsoft.Json.dll"),
				Path.GetFullPath($"{packagesPath}/Wiki.Net.3.0.0/lib/netstandard2.0/Wiki.Net.dll")
			};
			List<string> dlls = packageResolver.DownloadPackage("Wiki.Net", new Version(3, 0, 0)).GetAwaiter()
				.GetResult();
			Assert.AreEqual(excepted, dlls);
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			packageResolver.Dispose();
		}
	}
}