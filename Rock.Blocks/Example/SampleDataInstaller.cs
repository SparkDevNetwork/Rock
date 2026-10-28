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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.Configuration;
using Rock.Model;
using Rock.RealTime;
using Rock.RealTime.Topics;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Example.SampleDataInstaller;
using Rock.Web.Cache;

using RockSampleData.Loader;
using RockSampleData.Loader.Packages;

namespace Rock.Blocks.Example
{
    /// <summary>
    /// Installs a hosted sample data package into a fresh Rock database. Only
    /// members of the Rock Administrators role can use it.
    /// </summary>
    [DisplayName( "Sample Data Installer" )]
    [Category( "Examples" )]
    [Description( "Installs a sample data package into a fresh Rock database." )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [BooleanField( "Include Prerelease Packages",
        Description = "Also offer prerelease sample data packages, for testing a sample data release before it is final.",
        Key = AttributeKey.IncludePrereleasePackages,
        DefaultBooleanValue = false,
        Order = 0 )]

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.OBSIDIAN_EXAMPLE_SAMPLE_DATA_INSTALLER )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.OBSIDIAN_EXAMPLE_SAMPLE_DATA_INSTALLER )]
    public class SampleDataInstaller : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string IncludePrereleasePackages = "IncludePrereleasePackages";
        }

        #endregion Keys

        #region Fields

        private static readonly string NotAdministratorMessage = "Only members of the Rock Administrators role can install sample data.";

        private static readonly string PackagesUnavailableMessage = "Sample data packages aren't available to download yet.";

        /*
            10/27/26 - CLAUDE

            The package location is fixed in code, not a block setting. Anyone
            who can administrate the block could change a setting, and the
            server fetches the index whenever an administrator opens the page,
            so a setting could point the server at internal addresses or at a
            package that is not Spark's. Downloads also refuse redirects and
            stop at a size and time limit.

            Reason: Only download sample data from Spark's package storage.
        */
        private static readonly Uri PackageFolderUri = new Uri( "https://storage.rockrms.com/sampledata/packages/" );

        private static readonly string StableIndexFileName = "index.json";

        private static readonly string PrereleaseIndexFileName = "index-prerelease.json";

        private const long MaxIndexBytes = 1024 * 1024;

        private const long MaxPackageBytes = 1024L * 1024 * 1024;

        private const int IndexTimeoutMilliseconds = 30 * 1000;

        private const int PackageTimeoutMilliseconds = 10 * 60 * 1000;

        #endregion Fields

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<SampleDataInstallerBag, SampleDataInstallerOptionsBag>();

            if ( !IsCurrentPersonAdministrator() )
            {
                box.ErrorMessage = NotAdministratorMessage;
                return box;
            }

            var readiness = DatasetLoader.CheckDatabase( GetConnectionString() );
            var defaults = new DatasetLoadOptions();
            var virtualRootPath = RockApp.Current.HostingSettings.VirtualRootPath;

            box.Bag = new SampleDataInstallerBag
            {
                DatabaseProblems = readiness.Problems.ToList(),
                HistoryYears = defaults.HistoryYears,
                AnchorDate = defaults.Anchor ?? RockDateTime.Today,
                Seed = defaults.Seed,
                PersonSearchUrl = $"{virtualRootPath.TrimEnd( '/' )}/Person/Search/name/?SearchTerm=Decker"
            };

            box.Options = new SampleDataInstallerOptionsBag
            {
                Parts = SampleDataParts.All
                    .Select( part => new SampleDataInstallerPartBag
                    {
                        Key = part.Part.ToString(),
                        Name = part.Name,
                        Description = part.Description,
                        DependsOn = part.DependsOn.Select( dependency => dependency.ToString() ).ToList()
                    } )
                    .ToList()
            };

            return box;
        }

        /// <summary>
        /// Determines whether the current person is in the Rock Administrators
        /// role. Every entry point checks this on the server.
        /// </summary>
        /// <returns><c>true</c> if the current person is an administrator; otherwise <c>false</c>.</returns>
        private bool IsCurrentPersonAdministrator()
        {
            var administrators = RoleCache.Get( Rock.SystemGuid.Group.GROUP_ADMINISTRATORS.AsGuid() );

            return administrators?.IsPersonInRole( RequestContext.CurrentPerson?.Guid ) == true;
        }

        /// <summary>
        /// Gets the connection string of the Rock database.
        /// </summary>
        /// <returns>The connection string.</returns>
        private static string GetConnectionString()
        {
            return RockApp.Current.InitializationSettings.ConnectionString;
        }

        /// <summary>
        /// Downloads every package from Spark's package indexes: the stable
        /// index, plus the prerelease index when the block setting asks for
        /// it. An index that is not published yet (a 404) adds nothing.
        /// </summary>
        /// <returns>Every package in the indexes read.</returns>
        private List<SampleDataPackageIndexEntry> GetPackageIndexEntries()
        {
            var fileNames = new List<string> { StableIndexFileName };

            if ( GetAttributeValue( AttributeKey.IncludePrereleasePackages ).AsBoolean() )
            {
                fileNames.Add( PrereleaseIndexFileName );
            }

            var entries = new List<SampleDataPackageIndexEntry>();

            foreach ( var fileName in fileNames )
            {
                try
                {
                    using ( var index = new MemoryStream() )
                    {
                        DownloadFromPackageStorage( new Uri( PackageFolderUri, fileName ), index, MaxIndexBytes, IndexTimeoutMilliseconds );
                        entries.AddRange( SampleDataPackageIndex.Parse( Encoding.UTF8.GetString( index.ToArray() ) ).Packages );
                    }
                }
                catch ( Exception ex ) when ( IsNotFound( ex ) )
                {
                    // Intentionally ignored: the index is not published yet.
                }
            }

            return entries;
        }

        /// <summary>
        /// Downloads a file from Spark's package storage into a stream. The
        /// file must answer directly with 200 OK: redirects are refused, and
        /// the download stops past the size or time limit.
        /// </summary>
        /// <param name="uri">The file's address, which must be in Spark's package storage.</param>
        /// <param name="destination">The stream that receives the file.</param>
        /// <param name="maxBytes">The largest file accepted.</param>
        /// <param name="timeoutMilliseconds">The longest the whole download may take.</param>
        private static void DownloadFromPackageStorage( Uri uri, Stream destination, long maxBytes, int timeoutMilliseconds )
        {
            if ( uri.Scheme != Uri.UriSchemeHttps || !string.Equals( uri.Authority, PackageFolderUri.Authority, StringComparison.OrdinalIgnoreCase ) )
            {
                throw new InvalidDataException( $"{uri} is not in Spark's sample data package storage, so it was not downloaded." );
            }

            var request = ( HttpWebRequest ) WebRequest.Create( uri );
            request.AllowAutoRedirect = false;
            request.Timeout = timeoutMilliseconds;
            request.ReadWriteTimeout = timeoutMilliseconds;

            var stopwatch = Stopwatch.StartNew();
            HttpWebResponse httpResponse;

            try
            {
                httpResponse = ( HttpWebResponse ) request.GetResponse();
            }
            catch ( WebException ex )
            {
                // An error status still holds an open response and its pooled
                // connection, so close it here. A 404 becomes a
                // FileNotFoundException, which callers treat as "not published yet".
                var isNotFound = ( ex.Response as HttpWebResponse )?.StatusCode == HttpStatusCode.NotFound;
                ex.Response?.Dispose();

                if ( isNotFound )
                {
                    throw new FileNotFoundException( $"{uri} is not published yet.", ex );
                }

                throw;
            }

            using ( var response = httpResponse )
            {
                if ( response.StatusCode != HttpStatusCode.OK )
                {
                    throw new InvalidDataException( $"{uri} answered {( int ) response.StatusCode} instead of sending the file. Redirects are not followed." );
                }

                if ( response.ContentLength > maxBytes )
                {
                    throw new InvalidDataException( $"{uri} is larger than the {maxBytes:N0} bytes allowed." );
                }

                using ( var source = response.GetResponseStream() )
                {
                    var buffer = new byte[81920];
                    long totalBytes = 0;
                    int bytesRead;

                    while ( ( bytesRead = source.Read( buffer, 0, buffer.Length ) ) > 0 )
                    {
                        totalBytes += bytesRead;

                        if ( totalBytes > maxBytes )
                        {
                            throw new InvalidDataException( $"{uri} is larger than the {maxBytes:N0} bytes allowed." );
                        }

                        if ( stopwatch.ElapsedMilliseconds > timeoutMilliseconds )
                        {
                            throw new TimeoutException( $"Downloading {uri} took longer than {timeoutMilliseconds / 1000:N0} seconds." );
                        }

                        destination.Write( buffer, 0, bytesRead );
                    }
                }
            }
        }

        /// <summary>
        /// Gets the version of this Rock, for example "21.0.2". A package
        /// serves one Rock major release, so this decides which packages can
        /// be installed here.
        /// </summary>
        /// <returns>The semantic version of this Rock.</returns>
        private static string GetRockVersion()
        {
            return Rock.VersionInfo.VersionInfo.GetRockSemanticVersionNumber();
        }

        /// <summary>
        /// Gets every package in Spark's package indexes. A problem reading
        /// an index is logged; an index that is not published yet is not a
        /// problem.
        /// </summary>
        /// <param name="entries">When this returns <c>true</c>, contains every package in the index.</param>
        /// <returns><c>true</c> if the index was read; otherwise <c>false</c>.</returns>
        private bool TryGetPackageIndexEntries( out List<SampleDataPackageIndexEntry> entries )
        {
            entries = null;

            try
            {
                entries = GetPackageIndexEntries();
                return true;
            }
            catch ( Exception ex )
            {
                // Intentionally caught: the person sees that packages are not
                // available instead of an error.
                ExceptionLogService.LogException( ex );

                return false;
            }
        }

        /// <summary>
        /// Determines whether an exception means the file is not in package
        /// storage. <see cref="DownloadFromPackageStorage"/> turns a 404 into a
        /// <see cref="FileNotFoundException"/>.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <returns><c>true</c> if the server said the file does not exist; otherwise <c>false</c>.</returns>
        private static bool IsNotFound( Exception ex )
        {
            return ex is FileNotFoundException;
        }

        /// <summary>
        /// Gets the entry with the highest semantic version. A prerelease
        /// sorts below its release, and a version that does not parse sorts
        /// below every version that does.
        /// </summary>
        /// <param name="entries">The index entries to choose from.</param>
        /// <returns>The newest entry, or <c>null</c> when there are none.</returns>
        private static SampleDataPackageIndexEntry GetNewestEntry( IEnumerable<SampleDataPackageIndexEntry> entries )
        {
            return entries
                .OrderByDescending( entry => RockSemanticVersion.TryParse( entry.Version, out var version ) ? version : null )
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets one package per dataset, in dataset key order. Each is the
        /// newest package that can be installed on this Rock. A dataset with
        /// no such package shows its newest package, which cannot be
        /// installed here.
        /// </summary>
        /// <param name="entries">Every package in the index.</param>
        /// <param name="rockVersion">The version of this Rock, for example "21.0.2".</param>
        /// <returns>The packages as dataset cards.</returns>
        private static List<SampleDataInstallerPackageBag> GetDatasetPackages( IEnumerable<SampleDataPackageIndexEntry> entries, string rockVersion )
        {
            return entries
                .GroupBy( entry => entry.Dataset, StringComparer.Ordinal )
                .OrderBy( dataset => dataset.Key, StringComparer.Ordinal )
                .Select( dataset =>
                {
                    var installableEntry = GetNewestEntry( dataset.Where( entry => entry.IsInstallableOn( rockVersion ) ) );
                    var entry = installableEntry ?? GetNewestEntry( dataset );

                    return new SampleDataInstallerPackageBag
                    {
                        Dataset = entry.Dataset,
                        Name = entry.Name,
                        Summary = entry.Description,
                        BestFor = entry.BestFor,
                        Highlights = entry.Highlights.ToList(),
                        People = entry.People,
                        Households = entry.Households,
                        Campuses = entry.Campuses,
                        TypicalInstallSeconds = entry.TypicalInstallSeconds,
                        IsInstallable = installableEntry != null
                    };
                } )
                .ToList();
        }

        /// <summary>
        /// Downloads a hosted package to a temporary file, checks it against
        /// the SHA-256 in the index, and opens it. The temporary file is
        /// deleted before this returns.
        /// </summary>
        /// <param name="entry">The index entry for the package. Its URL is relative to the package folder.</param>
        /// <returns>The opened package.</returns>
        private static SampleDataPackage DownloadHostedPackage( SampleDataPackageIndexEntry entry )
        {
            // An absolute URL in the index would replace the folder's address;
            // DownloadFromPackageStorage refuses anything outside package storage.
            var packageUri = new Uri( PackageFolderUri, entry.Url );

            var tempFolder = RockApp.Current.MapPath( "~/App_Data/SampleDataInstaller" );
            var tempFile = Path.Combine( tempFolder, $"{Guid.NewGuid()}.zip" );

            Directory.CreateDirectory( tempFolder );

            try
            {
                using ( var file = File.Create( tempFile ) )
                {
                    DownloadFromPackageStorage( packageUri, file, MaxPackageBytes, PackageTimeoutMilliseconds );
                }

                var actualSha256 = GetSha256( tempFile );
                if ( !string.Equals( actualSha256, entry.Sha256, StringComparison.OrdinalIgnoreCase ) )
                {
                    throw new InvalidDataException( $"The downloaded {entry.Name} package does not match its checksum in the package index. The download may be damaged. Please try again." );
                }

                return SampleDataPackage.Open( tempFile );
            }
            finally
            {
                if ( File.Exists( tempFile ) )
                {
                    File.Delete( tempFile );
                }
            }
        }

        /// <summary>
        /// Computes the SHA-256 of a file as lowercase hexadecimal.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <returns>The hexadecimal SHA-256 hash.</returns>
        private static string GetSha256( string path )
        {
            using ( var sha256 = SHA256.Create() )
            using ( var stream = File.OpenRead( path ) )
            {
                return BitConverter.ToString( sha256.ComputeHash( stream ) ).Replace( "-", string.Empty ).ToLowerInvariant();
            }
        }

        /// <summary>
        /// Parses the part keys sent by the browser.
        /// </summary>
        /// <param name="keys">The part keys.</param>
        /// <param name="parts">When this returns <c>true</c>, contains the parsed parts.</param>
        /// <returns><c>true</c> if every key names a known part; otherwise <c>false</c>.</returns>
        private static bool TryParseParts( IEnumerable<string> keys, out List<SampleDataPart> parts )
        {
            parts = new List<SampleDataPart>();

            foreach ( var key in keys ?? Enumerable.Empty<string>() )
            {
                if ( !Enum.TryParse<SampleDataPart>( key, out var part ) || !Enum.IsDefined( typeof( SampleDataPart ), part ) )
                {
                    return false;
                }

                parts.Add( part );
            }

            return true;
        }

        /// <summary>
        /// Gets the messages of an exception and its inner exceptions, so a
        /// person can read why the install stopped.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <returns>One message per exception, outermost first.</returns>
        private static List<string> GetExceptionMessages( Exception ex )
        {
            var messages = new List<string>();

            for ( var current = ex; current != null; current = current.InnerException )
            {
                messages.Add( current.Message );
            }

            return messages;
        }

        /// <summary>
        /// Joins the person's RealTime connection to a new channel and
        /// creates the progress reporter for one install. Done before the
        /// install starts, so a connection that can't join fails the request
        /// instead of leaving the browser waiting for progress that never comes.
        /// </summary>
        /// <param name="realTimeConnectionId">The RealTime connection that receives progress.</param>
        /// <returns>The progress reporter for the install.</returns>
        private static async Task<TaskActivityProgress> CreateInstallProgressAsync( string realTimeConnectionId )
        {
            var taskChannelName = $"SampleDataInstall:{Guid.NewGuid()}";
            var topic = RealTimeHelper.GetTopicContext<ITaskActivityProgress>();

            await topic.Channels.AddToChannelAsync( realTimeConnectionId, taskChannelName );

            return new TaskActivityProgress( topic.Clients.Channel( taskChannelName ), "Sample Data Install" )
            {
                StartNotificationDelayMilliseconds = 0
            };
        }

        /// <summary>
        /// Loads the package in the background and reports progress over the
        /// TaskActivityProgress RealTime topic.
        /// </summary>
        /// <param name="progress">The progress reporter for this install. Disposed when the install ends.</param>
        /// <param name="openPackage">Opens the package to load. Called in the background because a download can be slow.</param>
        /// <param name="options">The load options, without <see cref="DatasetLoadOptions.Progress"/>.</param>
        private static void StartInstall( TaskActivityProgress progress, Func<SampleDataPackage> openPackage, DatasetLoadOptions options )
        {
            var connectionString = GetConnectionString();

            Task.Run( async () =>
            {
                // Small delay so the browser can render and start listening.
                await Task.Delay( 1000 );

                try
                {
                    progress.StartTask( "Opening the sample data package..." );

                    options.Progress = new SynchronousProgress<LoadProgressReport>( report => ReportLoadProgress( progress, report ) );

                    DatasetLoader.Load( connectionString, openPackage(), options );

                    /*
                        10/27/26 - CLAUDE

                        The load has committed by now, so a failure below must
                        not be reported as a failed install: the data is saved,
                        and a retry would be refused because the database now
                        has people. Each step logs its own exception and adds a
                        warning to the success instead.

                        Reason: Never tell the person nothing was saved after the load committed.
                    */
                    var warnings = new List<string>();

                    try
                    {
                        // The loader bulk-writes past Rock's services, so every
                        // in-memory cache is stale once the load commits.
                        RockCache.ClearAllCachedItems();
                    }
                    catch ( Exception cacheException )
                    {
                        ExceptionLogService.LogException( cacheException );
                        warnings.Add( "Rock's caches could not be cleared. Restart Rock to see all of the sample data." );
                    }

                    try
                    {
                        // Lets other parts of Rock, such as the impact statistics,
                        // know this instance holds sample data.
                        Web.SystemSettings.SetValue( SystemKey.SystemSetting.SAMPLEDATA_DATE, RockDateTime.Now.ToString( "s" ) );
                    }
                    catch ( Exception settingException )
                    {
                        ExceptionLogService.LogException( settingException );
                        warnings.Add( "Rock could not record the date the sample data was installed." );
                    }

                    progress.StopTask( "Sample data installed.", warnings: warnings );
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex );

                    progress.StopTask( "The install stopped.", GetExceptionMessages( ex ) );
                }
                finally
                {
                    progress.Dispose();
                }
            } );
        }

        /// <summary>
        /// Sends one loader progress report to the browser. A step report
        /// starts a step and moves the progress bar. Any other report is a
        /// detail line within the current step.
        /// </summary>
        /// <param name="progress">The task progress reporter.</param>
        /// <param name="report">The loader progress report.</param>
        private static void ReportLoadProgress( TaskActivityProgress progress, LoadProgressReport report )
        {
            if ( report.StepName == null )
            {
                progress.LogMessage( report.Message );
            }
            else if ( report.TotalSteps > 0 )
            {
                // A step report carries the steps finished before this one starts.
                progress.ReportProgressUpdate( report.CompletedSteps, report.TotalSteps, $"{report.StepName} (step {report.CompletedSteps + 1} of {report.TotalSteps})" );
            }
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets one hosted package per dataset, to show as dataset cards.
        /// </summary>
        /// <returns>A list of <see cref="SampleDataInstallerPackageBag"/> objects.</returns>
        [BlockAction]
        public BlockActionResult GetHostedPackages()
        {
            if ( !IsCurrentPersonAdministrator() )
            {
                return ActionForbidden( NotAdministratorMessage );
            }

            if ( !TryGetPackageIndexEntries( out var entries ) || entries.Count == 0 )
            {
                return ActionBadRequest( PackagesUnavailableMessage );
            }

            return ActionOk( GetDatasetPackages( entries, GetRockVersion() ) );
        }

        /// <summary>
        /// Starts installing a package as a background task and returns
        /// immediately. Progress is reported via the TaskActivityProgress
        /// RealTime topic.
        /// </summary>
        /// <param name="bag">The choices the person made.</param>
        /// <returns>A result indicating whether the install was started.</returns>
        [BlockAction]
        public async Task<BlockActionResult> Install( SampleDataInstallerInstallBag bag )
        {
            if ( !IsCurrentPersonAdministrator() )
            {
                return ActionForbidden( NotAdministratorMessage );
            }

            if ( bag == null || bag.Dataset.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Please choose a dataset to install." );
            }

            if ( bag.RealTimeConnectionId.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "A real-time connection is required. Please wait a moment and try again." );
            }

            if ( !TryParseParts( bag.Parts, out var parts ) )
            {
                return ActionBadRequest( "One of the chosen parts is not a part this installer knows." );
            }

            if ( bag.HistoryYears < 1 )
            {
                return ActionBadRequest( "History years must be at least 1." );
            }

            // The page checks this when it loads, but the database may have
            // changed since, and a request can skip the page. The loader checks
            // again inside its transaction, under an application lock.
            var readiness = DatasetLoader.CheckDatabase( GetConnectionString() );

            if ( readiness.Problems.Any() )
            {
                return ActionBadRequest( string.Join( " ", readiness.Problems ) );
            }

            if ( !TryGetPackageIndexEntries( out var entries ) )
            {
                return ActionBadRequest( PackagesUnavailableMessage );
            }

            var rockVersion = GetRockVersion();
            var entry = GetNewestEntry( entries.Where( e => e.Dataset == bag.Dataset && e.IsInstallableOn( rockVersion ) ) );

            if ( entry == null )
            {
                return ActionBadRequest( "That dataset is not available for this version of Rock yet." );
            }

            var options = new DatasetLoadOptions
            {
                Parts = parts,
                HistoryYears = bag.HistoryYears,
                Anchor = bag.AnchorDate?.Date,
                RockVersion = rockVersion,
                PasswordKey = RockApp.Current.InitializationSettings.PasswordKey,
                DataEncryptionKey = RockApp.Current.InitializationSettings.DataEncryptionKey
            };

            if ( bag.Seed.IsNotNullOrWhiteSpace() )
            {
                options.Seed = bag.Seed.Trim();
            }

            TaskActivityProgress progress;

            try
            {
                progress = await CreateInstallProgressAsync( bag.RealTimeConnectionId );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                return ActionBadRequest( "The live progress connection could not be set up. Reload the page and try again." );
            }

            StartInstall( progress, () => DownloadHostedPackage( entry ), options );

            return ActionOk();
        }

        #endregion Block Actions

        #region Support Classes

        /// <summary>
        /// Reports progress on the thread that raised it, so reports reach
        /// the browser in the order the loader made them.
        /// </summary>
        /// <remarks>
        /// <see cref="Progress{T}"/> posts each report to the thread pool when
        /// there is no synchronization context, which can reorder them.
        /// </remarks>
        /// <typeparam name="T">The type of progress value.</typeparam>
        private sealed class SynchronousProgress<T> : IProgress<T>
        {
            private readonly Action<T> _handler;

            /// <summary>
            /// Initializes a new instance of the <see cref="SynchronousProgress{T}"/> class.
            /// </summary>
            /// <param name="handler">The handler called for each report.</param>
            public SynchronousProgress( Action<T> handler )
            {
                _handler = handler;
            }

            /// <inheritdoc/>
            public void Report( T value )
            {
                _handler( value );
            }
        }

        #endregion Support Classes
    }
}
