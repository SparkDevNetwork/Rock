// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
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
            _projectManager = new RockProjectManager( sourceRepository: PackageRepositoryFactory.Default.CreateRepository( remoteSource ),
                                       pathResolver: packagePathResolver,
                                       localRepository: new LocalPackageRepository( packagePathResolver, fileSystem ),
                                       project: new WebProjectSystem( siteRoot ) );

            // Add event for reference files added (See note below)
            _projectManager.PackageReferenceAdded += ProjectManager_PackageReferenceAdded;
        }

        /// <summary>
        /// NOTE ***************************************************************************
        /// This event handler is needed because the current version of NuGet.Core has a bug 
        /// when calling WebProjectSsyte.AddReferences() method. It is passing a null stream 
        /// object resulting in zero byte bin files getting written.
        /// 
        /// SEE: 
        ///     http://nuget.codeplex.com/discussions/253750
        ///     http://nuget.codeplex.com/discussions/479191
        ///     http://nuget.codeplex.com/workitem/4029
        /// ********************************************************************************
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PackageOperationEventArgs"/> instance containing the event data.</param>
        void ProjectManager_PackageReferenceAdded( object sender, PackageOperationEventArgs e )
        {
            foreach ( var assemblyReference in e.Package.AssemblyReferences )
            {
                string referencePath = Path.Combine( ( (ProjectManager)_projectManager ).PathResolver.GetInstallPath( e.Package ), assemblyReference.Path );
                string relativeReferencePath = PathUtility.GetRelativePath( _projectManager.Project.Root, referencePath );
                string fileName = Path.GetFileName( relativeReferencePath );
                string fullPath = _projectManager.Project.GetFullPath( Path.Combine( "bin", fileName ) );
                _projectManager.Project.AddFile( fullPath, assemblyReference.GetStream() );
            }
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
        /// Gets a package for the given id from the source repository.
        /// </summary>
        /// <param name="packageId">the Id of a package</param>
        /// <returns>a package</returns>
        public IPackage GetRemotePackage( string packageId )
        {
            return SourceRepository.FindPackage( packageId );
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
        /// Updates a package reference. Installs the package to the App_Data repository if it does not already exist.
        /// </summary>
        /// <returns>Warnings encountered when updating the package.</returns>
        public IEnumerable<string> UpdatePackageAndBackup( IPackage package, IPackage oldPackage )
        {
            return PerformLoggedAction( () =>
            {
                Backup( oldPackage );

                try
                {
                    _projectManager.UpdatePackageReference( package.Id, package.Version, updateDependencies: true, allowPrereleaseVersions: false );
                }
                catch
                {
                    Restore( oldPackage );
                    throw;
                }
            } );
        }

        /// <summary>
        /// Make a backup of the given package.
        /// </summary>
        /// <param name="package"></param>
        protected void Backup( IPackage package )
        {
            var fileName =  string.Format( "{0}.{1}.nupkg", package.Id, package.Version );
            var backupDirectory = GetWebRepositoryRestoreDirectory( _projectManager.Project.Root );
            var sourcePackage = Path.Combine( GetWebRepositoryDirectory( _projectManager.Project.Root ), fileName );
            var targetPackage = Path.Combine( backupDirectory, fileName );
            if ( File.Exists( targetPackage ) )
            {
                File.Delete( targetPackage );
            }

            Directory.CreateDirectory( backupDirectory );
            File.Copy( sourcePackage, targetPackage );
        }

        /// <summary>
        /// Restore the backup copy of the given package.
        /// </summary>
        /// <param name="package"></param>
        protected void Restore( IPackage package )
        {
            var fileName = string.Format( "{0}.{1}.nupkg", package.Id, package.Version );
            var originalPackage = Path.Combine( GetWebRepositoryDirectory( _projectManager.Project.Root ), fileName );
            var backupPackage = Path.Combine( GetWebRepositoryRestoreDirectory( _projectManager.Project.Root ), fileName );
            try
            {
                File.Copy( backupPackage, originalPackage );
                File.Delete( backupPackage );
            }
            catch { }
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
            return SourceRepository.GetUpdates( LocalRepository.GetPackages(), includePrerelease: false, includeAllVersions: true, targetFrameworks: null ).FirstOrDefault( p => package.Id == p.Id && p.IsListed() );
        }

        /// <summary>
        /// Gets all listed versions of the given package.
        /// </summary>
        /// <param name="packageId">the Id of a package</param>
        /// <returns>
        /// a list of packages
        /// </returns>
        public IEnumerable<IPackage> GetUpdates( string packageId )
        {
            return SourceRepository.GetUpdates( LocalRepository.GetPackages(), includePrerelease: false, includeAllVersions: true, targetFrameworks: null ).Where( p => p.Id == packageId && p.IsListed() );
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

            InstallWalker walker = new InstallWalker( localRepository: localRepository, sourceRepository: sourceRepository, targetFramework: null, logger: NullLogger.Instance, ignoreDependencies: false, allowPrereleaseVersions: false, dependencyVersion: DependencyVersion.Highest );
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
        /// Gets the location of web repository backup directory.
        /// </summary>
        /// <param name="siteRoot"></param>
        /// <returns></returns>
        internal static string GetWebRepositoryRestoreDirectory( string siteRoot )
        {
            return Path.Combine( siteRoot, "App_Data", "PackageRestore" );
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

            public FileConflictResolution ResolveFileConflict( string message )
            {
                throw new NotImplementedException();
            }
        }
    }
}