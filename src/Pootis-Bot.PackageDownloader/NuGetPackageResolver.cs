using System;
using System.Collections.Generic;
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
	/// <summary>
	///     A class for downloading NuGet packages
	/// </summary>
	public sealed class NuGetPackageResolver : IDisposable
	{
		private readonly SourceCacheContext cache;
		private readonly ILogger nugetLogger;
		private readonly string packagesDir;
		private NuGetFramework framework;
		private IEnumerable<SourceRepository> repositories;
		private ISettings settings;

		/// <summary>
		///     Creates a new <see cref="NuGetPackageResolver" /> instance
		/// </summary>
		/// <param name="framework">Whats the target framework</param>
		/// <param name="packagesDirectory">Where is the location of our packages</param>
		public NuGetPackageResolver(string framework, string packagesDirectory = "Packages/")
		{
			settings = Settings.LoadDefaultSettings(null);
			SourceRepositoryProvider sourceRepositoryProvider =
				new SourceRepositoryProvider(new PackageSourceProvider(settings), Repository.Provider.GetCoreV3());
			repositories = sourceRepositoryProvider.GetRepositories();

			nugetLogger = new NullLogger();
			cache = new SourceCacheContext();
			this.framework = NuGetFramework.Parse(framework);
			packagesDir = packagesDirectory;
		}

		/// <summary>
		///     Destroy this instance
		/// </summary>
		public void Dispose()
		{
			ReleaseResources();
			GC.SuppressFinalize(this);
		}

		~NuGetPackageResolver()
		{
			ReleaseResources();
		}

		private void ReleaseResources()
		{
			repositories = null;
			settings = null;
			framework = null;
			cache?.Dispose();
		}

		/// <summary>
		///     Downloads a package
		/// </summary>
		/// <param name="packageId">What package to download</param>
		/// <param name="version">What version of the package to download</param>
		/// <param name="cancellationToken">Cancellation token to use</param>
		/// <returns>Returns a list of locations of all the .Dlls</returns>
		public async Task<List<string>> DownloadPackage(string packageId, Version version,
			CancellationToken cancellationToken = default)
		{
			PackageIdentity package = new PackageIdentity(packageId, new NuGetVersion(version));
			HashSet<SourcePackageDependencyInfo> availablePackages =
				new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);

			//Get our package's dependencies
			await GetPackageDependencies(package, availablePackages);

			//Setup our resolver
			PackageResolverContext resolverContext = new PackageResolverContext(
				DependencyBehavior.Lowest,
				new[] {packageId},
				Enumerable.Empty<string>(),
				Enumerable.Empty<PackageReference>(),
				Enumerable.Empty<PackageIdentity>(),
				availablePackages,
				repositories.Select(s => s.PackageSource),
				NullLogger.Instance);
			PackageResolver resolver = new PackageResolver();
			PackagePathResolver packagePathResolver = new PackagePathResolver(packagesDir);

			//Setup package extraction
			PackageExtractionContext packageExtractionContext = new PackageExtractionContext(PackageSaveMode.Defaultv3,
				XmlDocFileSaveMode.None, ClientPolicyContext.GetClientPolicy(settings, nugetLogger), nugetLogger);
			FrameworkReducer frameworkReducer = new FrameworkReducer();

			//Get all the packages we need to install
			IEnumerable<SourcePackageDependencyInfo> packagesToInstall = resolver
				.Resolve(resolverContext, cancellationToken)
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
					DownloadResource downloadResource =
						await packageToInstall.Source.GetResourceAsync<DownloadResource>(cancellationToken);
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
				IEnumerable<NuGetFramework> possibleFrameworks = libItems.Select(x => x.TargetFramework);
				NuGetFramework nearest = frameworkReducer.GetNearest(framework, possibleFrameworks);
				foreach (FrameworkSpecificGroup frameworkGroup in libItems)
				{
					Console.WriteLine(frameworkGroup.TargetFramework.ToString());

					if (frameworkGroup.TargetFramework.Equals(nearest))
					{
						foreach (string item in frameworkGroup.Items)
						{
							dlls.Add($"{installedPath}/{item}");
						}
					}
				}
			}

			return dlls;
		}

		/// <summary>
		///     Gets all of a package's dependencies
		/// </summary>
		/// <param name="package">The <see cref="PackageIdentity" /> to use</param>
		/// <param name="availablePackages"></param>
		/// <returns></returns>
		public async Task GetPackageDependencies(PackageIdentity package,
			ISet<SourcePackageDependencyInfo> availablePackages)
		{
			if (availablePackages.Contains(package)) return;

			foreach (SourceRepository sourceRepository in repositories)
			{
				DependencyInfoResource dependencyInfoResource =
					await sourceRepository.GetResourceAsync<DependencyInfoResource>();
				SourcePackageDependencyInfo dependencyInfo = await dependencyInfoResource.ResolvePackage(
					package, framework, cache, nugetLogger, CancellationToken.None);

				if (dependencyInfo == null) continue;

				availablePackages.Add(dependencyInfo);
				foreach (PackageDependency dependency in dependencyInfo.Dependencies)
					await GetPackageDependencies(
						new PackageIdentity(dependency.Id, dependency.VersionRange.MinVersion), availablePackages);
			}
		}
	}
}