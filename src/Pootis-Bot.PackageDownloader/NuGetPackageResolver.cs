using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Packaging.Signing;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;
using NuGet.Versioning;

namespace Pootis_Bot.PackageDownloader
{
	public class NuGetPackageResolver : IDisposable
	{
		private IEnumerable<SourceRepository> repositories;
		private ISettings settings;
		private ILogger nugetLogger;
		private SourceCacheContext cache;
		private NuGetFramework framework;
		private string packagesDir;

		public NuGetPackageResolver(string framework, string packagesDirectory = "Packages/")
		{
			settings = Settings.LoadDefaultSettings(null);
			SourceRepositoryProvider sourceRepositoryProvider = new SourceRepositoryProvider(new PackageSourceProvider(settings), Repository.Provider.GetCoreV3());
			repositories = sourceRepositoryProvider.GetRepositories();

			nugetLogger = new NullLogger();
			cache = new SourceCacheContext();
			this.framework = NuGetFramework.Parse(framework);
			packagesDir = Path.GetFullPath(packagesDirectory);
		}

		public async Task<List<string>> DownloadPackage(string packageId, Version version, CancellationToken cancellationToken = default)
		{
			PackageIdentity package = new PackageIdentity(packageId, new NuGetVersion(version));
			HashSet<SourcePackageDependencyInfo> availablePackages = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);

			//Get our package's dependencies
			await GetPackageDependencies(package, availablePackages);

			//Setup our resolver
			PackageResolverContext resolverContext = new PackageResolverContext(
				DependencyBehavior.Lowest,
				new[] { packageId },
				Enumerable.Empty<string>(),
				Enumerable.Empty<PackageReference>(),
				Enumerable.Empty<PackageIdentity>(),
				availablePackages,
				repositories.Select(s => s.PackageSource),
				NullLogger.Instance);
			PackageResolver resolver = new PackageResolver();
			PackagePathResolver packagePathResolver = new PackagePathResolver(packagesDir);

			//Setup package extraction
			PackageExtractionContext packageExtractionContext = new PackageExtractionContext(PackageSaveMode.Defaultv3, XmlDocFileSaveMode.None, ClientPolicyContext.GetClientPolicy(settings, nugetLogger), nugetLogger);
			FrameworkReducer frameworkReducer = new FrameworkReducer();

			//Get all the packages we need to install
			IEnumerable<SourcePackageDependencyInfo> packagesToInstall = resolver.Resolve(resolverContext, cancellationToken)
				.Select(p => availablePackages.Single(x => PackageIdentityComparer.Default.Equals(x, p)));

			List<string> dlls = new List<string>();

			//Now to actually extract it
			foreach (SourcePackageDependencyInfo packageToInstall in packagesToInstall)
			{
				PackageReaderBase packageReader;
				string installedPath = packagePathResolver.GetInstalledPath(packageToInstall);

				//If the package is already installed or not
				if (installedPath == null)
				{
					DownloadResource downloadResource = await packageToInstall.Source.GetResourceAsync<DownloadResource>(cancellationToken);
					DownloadResourceResult downloadResult = await downloadResource.GetDownloadResourceResultAsync(
						packageToInstall,
						new PackageDownloadContext(cache),
						SettingsUtility.GetGlobalPackagesFolder(settings),
						nugetLogger, cancellationToken);

					await PackageExtractor.ExtractPackageAsync(
						downloadResult.PackageSource,
						downloadResult.PackageStream,
						packagePathResolver,
						packageExtractionContext,
						cancellationToken);

					packageReader = downloadResult.PackageReader;
				}
				else
				{
					packageReader = new PackageFolderReader(installedPath);
				}

				IEnumerable<FrameworkSpecificGroup> libItems = await packageReader.GetLibItemsAsync(cancellationToken);
				NuGetFramework nearest = frameworkReducer.GetNearest(framework, libItems.Select(x => x.TargetFramework));
				dlls.AddRange(from @group in libItems.Where(x => x.TargetFramework.Equals(nearest)) from item in @group.Items where item.Contains(".dll") select Path.GetFullPath($"{installedPath}/{item}"));
			}

			return dlls;
		}

		public async Task GetPackageDependencies(PackageIdentity package, ISet<SourcePackageDependencyInfo> availablePackages)
		{
			if (availablePackages.Contains(package)) return;

			foreach (SourceRepository sourceRepository in repositories)
			{
				DependencyInfoResource dependencyInfoResource = await sourceRepository.GetResourceAsync<DependencyInfoResource>();
				SourcePackageDependencyInfo dependencyInfo = await dependencyInfoResource.ResolvePackage(
					package, framework, cache, nugetLogger, CancellationToken.None);

				if (dependencyInfo == null) continue;

				availablePackages.Add(dependencyInfo);
				foreach (PackageDependency dependency in dependencyInfo.Dependencies)
				{
					await GetPackageDependencies(
						new PackageIdentity(dependency.Id, dependency.VersionRange.MinVersion), availablePackages);
				}
			}
		}

		public void Dispose()
		{
			repositories = null;
			settings = null;
			framework = null;
			cache?.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}