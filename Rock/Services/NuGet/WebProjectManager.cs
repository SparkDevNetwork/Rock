//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NuGet;

namespace Rock.Services.NuGet
{
    /// <summary>
    /// This is the service layer that handles installing, updating, removing Packages (aka Plugins)
    /// from the website / local filesystem and the Rock Quarry (our NuGet server).
    /// </summary>
    public class WebProjectManager
    {
        private readonly IProjectManager _projectManager;

        /// <summary>
        /// Creates a WebProjectManager service.
        /// </summary>
        /// <param name="remoteSource">URL of the NuGet server API (ex, http://nuget.org/api/v2 ).</param>
        /// <param name="siteRoot">The physical path to the web root.</param>
        public WebProjectManager( string remoteSource, string siteRoot )
        {
            string webRepositoryDirectory = GetWebRepositoryDirectory( siteRoot );
            var fileSystem = new PhysicalFileSystem( webRepositoryDirectory );
            var packagePathResolver = new RockPackagePathResolver( fileSystem );
            _projectManager = new ProjectManager( sourceRepository: PackageRepositoryFactory.Default.CreateRepository( remoteSource ),
                                       pathResolver: packagePathResolver,
                                       localRepository: new LocalPackageRepository( packagePathResolver, fileSystem ),
                                       project: new WebProjectSystem( siteRoot ) );
        }

        /// <summary>
        /// Represents the local NuGet package repository.
        /// </summary>
        public IPackageRepository LocalRepository
        {
            get
            {
                return _projectManager.LocalRepository;
            }
        }

        /// <summary>
        /// Represents the remote/source NuGet package repository.
        /// </summary>
        public IPackageRepository SourceRepository
        {
            get
            {
                return _projectManager.SourceRepository;
            }
        }

        /// <summary>
        /// Gets the newest/latest package or all of them matching the given keyword search.
        /// </summary>
        /// <param name="searchTerms">a string of space delimited search terms</param>
        /// <param name="includeAllVersions">if true, returns all matching packages; otherwise only the latest</param>
        /// <returns>
        /// a list of packages
        /// </returns>
        public IQueryable<IPackage> GetLatestRemotePackages( string searchTerms, bool includeAllVersions )
        {
            var packages = GetPackages( SourceRepository, searchTerms );
            return packages.Where( x => x.IsLatestVersion || includeAllVersions );
        }

        /// <summary>
        /// Gets all matching packages for the given keyword search.
        /// </summary>
        /// <param name="searchTerms">a string of space delimited search terms</param>
        /// <returns>a list of packages</returns>
        public IQueryable<IPackage> GetRemotePackages( string searchTerms )
        {
            return GetPackages( SourceRepository, searchTerms );
        }

        /// <summary>
        /// Gets all matching installed packages for the given keyword search.
        /// </summary>
        /// <param name="searchTerms">a string of space delimited search terms</param>
        /// <returns>a list of packages</returns>
        public IQueryable<IPackage> GetInstalledPackages( string searchTerms )
        {
            return GetPackages( LocalRepository, searchTerms );
        }

        // <summary>
        // Gets all matching installed packages for the given keyword search for which updates are available.
        // </summary>
        // <param name="searchTerms">a string of space delimited search terms</param>
        // <returns>a list of packages</returns>
        //public IQueryable<IPackage> GetPackagesWithUpdates( string searchTerms )
        //{
        //    IQueryable<IPackage> packages = LocalRepository.GetUpdates( SourceRepository.GetPackages(), includePrerelease: false, includeAllVersions: true, targetFramework: null ).AsQueryable();
        //    return GetPackages( packages, searchTerms );
        //}

        /// <summary>
        /// Gets the locally installed package for the given id.
        /// </summary>
        /// <param name="packageId">the Id of a package</param>
        /// <returns>a package</returns>
        public IPackage GetInstalledPackage( string packageId )
        {
            return LocalRepository.FindPackage( packageId );
        }

        /// <summary>
        /// Installs and adds a package reference to the project
        /// </summary>
        /// <returns>Warnings encountered when installing the package.</returns>
        public IEnumerable<string> InstallPackage( IPackage package )
        {
            return PerformLoggedAction( () =>
            {
                _projectManager.AddPackageReference( package.Id, package.Version, ignoreDependencies: false, allowPrereleaseVersions: false );
            } );
        }

        /// <summary>
        /// Updates a package reference. Installs the package to the App_Data repository if it does not already exist.
        /// </summary>
        /// <returns>Warnings encountered when updating the package.</returns>
        public IEnumerable<string> UpdatePackage( IPackage package )
        {
            return PerformLoggedAction( () =>
            {
                _projectManager.UpdatePackageReference( package.Id, package.Version, updateDependencies: true, allowPrereleaseVersions: false);
            } );
        }
        /// <summary>
        /// Removes a package reference and uninstalls the package
        /// </summary>
        /// <returns>Warnings encountered when uninstalling the package.</returns>
        public IEnumerable<string> UninstallPackage( IPackage package, bool removeDependencies )
        {
            return PerformLoggedAction( () =>
            {
                _projectManager.RemovePackageReference( package.Id, forceRemove: false, removeDependencies: removeDependencies );
            } );
        }

        /// <summary>
        /// Will let you know if this exact package is installed locally
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        public bool IsPackageInstalled( IPackage package )
        {
            return LocalRepository.Exists( package );
        }

        /// <summary>
        /// Will let you know if this package is installed locally. If anyVersion is true
        /// it does not matter which version of the package; otherwise only returns true if
        /// an exact match.
        /// </summary>
        /// <param name="package"></param>
        /// <param name="anyVersion"></param>
        /// <returns></returns>
        public bool IsPackageInstalled( IPackage package, bool anyVersion )
        {
            return (anyVersion) ? LocalRepository.Exists( package.Id ) : LocalRepository.Exists( package );
        }

        /// <summary>
        /// Gets the latest version of the given package.
        /// </summary>
        /// <param name="package">a package</param>
        /// <returns>a package; otherwise null if no package was found</returns>
        public IPackage GetUpdate( IPackage package )
        {
            return SourceRepository.GetUpdates( LocalRepository.GetPackages(), includePrerelease: false, includeAllVersions: true, targetFramework: null ).FirstOrDefault( p => package.Id == p.Id );
        }

        /// <summary>
        /// Gets all versions of the given package.
        /// </summary>
        /// <returns>
        /// a list of packages
        /// </returns>
        public IEnumerable<IPackage> GetUpdates()
        {
            return SourceRepository.GetUpdates( LocalRepository.GetPackages(), includePrerelease: false, includeAllVersions: true, targetFramework: null );
        }

        /// <summary>
        /// Performs the logged action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        private IEnumerable<string> PerformLoggedAction( Action action )
        {
            ErrorLogger logger = new ErrorLogger();
            _projectManager.Logger = logger;
            try
            {
                action();
            }
            finally
            {
                _projectManager.Logger = null;
            }
            return logger.Errors;
        }

        /// <summary>
        /// Gets the packages requiring license acceptance.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <returns></returns>
        internal IEnumerable<IPackage> GetPackagesRequiringLicenseAcceptance( IPackage package )
        {
            return GetPackagesRequiringLicenseAcceptance( package, localRepository: LocalRepository, sourceRepository: SourceRepository );
        }

        /// <summary>
        /// Gets the packages requiring license acceptance.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <param name="localRepository">The local repository.</param>
        /// <param name="sourceRepository">The source repository.</param>
        /// <returns></returns>
        internal static IEnumerable<IPackage> GetPackagesRequiringLicenseAcceptance( IPackage package, IPackageRepository localRepository, IPackageRepository sourceRepository )
        {
            var dependencies = GetPackageDependencies( package, localRepository, sourceRepository );

            return from p in dependencies
                   where p.RequireLicenseAcceptance
                   select p;
        }

        /// <summary>
        /// Gets the package dependencies.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <param name="localRepository">The local repository.</param>
        /// <param name="sourceRepository">The source repository.</param>
        /// <returns></returns>
        private static IEnumerable<IPackage> GetPackageDependencies( IPackage package, IPackageRepository localRepository, IPackageRepository sourceRepository )
        {
            InstallWalker walker = new InstallWalker( localRepository: localRepository, sourceRepository: sourceRepository, logger: NullLogger.Instance, ignoreDependencies: false, allowPrereleaseVersions: false, targetFramework: null );
            IEnumerable<PackageOperation> operations = walker.ResolveOperations( package );

            return from operation in operations
                   where operation.Action == PackageAction.Install
                   select operation.Package;
        }

        /// <summary>
        /// Gets the packages.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="searchTerm">The search term.</param>
        /// <returns></returns>
        internal static IQueryable<IPackage> GetPackages( IPackageRepository repository, string searchTerm )
        {
            return GetPackages( repository.GetPackages(), searchTerm );
        }

        /// <summary>
        /// Gets the packages.
        /// </summary>
        /// <param name="packages">The packages.</param>
        /// <param name="searchTerm">The search term.</param>
        /// <returns></returns>
        internal static IQueryable<IPackage> GetPackages( IQueryable<IPackage> packages, string searchTerm )
        {
            if ( !String.IsNullOrEmpty( searchTerm ) )
            {
                searchTerm = searchTerm.Trim();
                packages = packages.Find( searchTerm );
            }
            return packages;
        }

        /// <summary>
        /// Gets the web repository directory.
        /// </summary>
        /// <param name="siteRoot">The site root.</param>
        /// <returns></returns>
        internal static string GetWebRepositoryDirectory( string siteRoot )
        {
            return Path.Combine( siteRoot, "App_Data", "packages" );
        }

        /// <summary>
        /// 
        /// </summary>
        private class ErrorLogger : ILogger
        {
            /// <summary>
            /// 
            /// </summary>
            private readonly IList<string> _errors = new List<string>();

            /// <summary>
            /// Gets the errors.
            /// </summary>
            /// <value>
            /// The errors.
            /// </value>
            public IEnumerable<string> Errors
            {
                get
                {
                    return _errors;
                }
            }

            /// <summary>
            /// Logs the specified level.
            /// </summary>
            /// <param name="level">The level.</param>
            /// <param name="message">The message.</param>
            /// <param name="args">The args.</param>
            public void Log( MessageLevel level, string message, params object[] args )
            {
                if ( level == MessageLevel.Warning || level == MessageLevel.Error )
                {
                    _errors.Add( String.Format( CultureInfo.CurrentCulture, message, args ) );
                }
            }
        }
    }
}