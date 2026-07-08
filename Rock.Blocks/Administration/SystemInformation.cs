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
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Rock;
using Rock.Attribute;
using Rock.Configuration;
using Rock.Data;
using Rock.Enums.Configuration;
using Rock.Model;
using Rock.Security;
using Rock.Transactions;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Administration.SystemInformation;
using Rock.Web.Cache;
using Rock.WebFarm;

#if REVIEW_WEBFORMS
using System.Web.Routing;
#endif

namespace Rock.Blocks.Administration
{
    /// <summary>
    /// Displays system information on the installed version of Rock.
    /// </summary>
    /// <seealso cref="RockBlockType" />
    [DisplayName( "System Information" )]
    [Category( "Administration" )]
    [Description( "Displays system information on the installed version of Rock." )]
    [IconCssClass( "ti ti-info-circle" )]
    [SupportedSiteTypes( SiteType.Web )]
    [InitialBlockHeight( 220 )]

    [SystemGuid.EntityTypeGuid( "F52B2616-5D15-40BE-B84A-6039FE3EAB83" )]
    // was [SystemGuid.BlockTypeGuid( "CA91E091-AB61-4DB1-AA00-B931FE4A4AEB" )]
    [Rock.SystemGuid.BlockTypeGuid( "DE08EFD7-4CF9-4BD5-9F72-C0151FD08523" )]
    public class SystemInformation : RockBlockType
    {
        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return new CustomBlockBox<SystemInformationBag, SystemInformationOptionsBag>
            {
                Bag = new SystemInformationBag
                {
                    RockVersion = VersionInfo.VersionInfo.GetRockProductVersionFullName(),
                    RockVersionNumber = VersionInfo.VersionInfo.GetRockProductVersionNumber(),
                    ClientCulture = CultureInfo.CurrentCulture.ToString()
                },
                Options = new SystemInformationOptionsBag
                {
                    IsAdministrator = IsAdministrator()
                }
            };
        }

        /// <summary>
        /// Determines whether the current person is authorized to administrate the block.
        /// </summary>
        /// <returns><c>true</c> if the current person can administrate the block; otherwise <c>false</c>.</returns>
        private bool IsAdministrator()
        {
            return BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Builds the full set of diagnostics information for the Diagnostics tab.
        /// </summary>
        /// <returns>A populated <see cref="SystemDiagnosticsBag"/>.</returns>
        private SystemDiagnosticsBag GetSystemDiagnosticsBag()
        {
            var hostingSettings = RockApp.Current.HostingSettings;
            var ( lastCoreMigration, pluginMigrations ) = GetMigrationData();
            var isCacheStatisticsEnabled = Rock.Web.SystemSettings.GetValueFromWebConfig( Rock.SystemKey.SystemSetting.CACHE_MANAGER_ENABLE_STATISTICS ).AsBoolean();

            return new SystemDiagnosticsBag
            {
                Database = GetDatabaseInformation(),
                LavaEngineName = RockApp.Current.GetCurrentLavaEngineName(),
                SystemDateTime = new DateTimeOffset( RockDateTime.SystemDateTime ).ToString(),
                RockTime = RockDateTime.Now.ToRockDateTimeOffset().ToString(),
                ProcessStartTime = GetProcessStartTime(),
                RockApplicationStartTime = new DateTimeOffset( hostingSettings.ApplicationStartDateTime ).ToString(),
                InstallDateTime = Rock.Web.SystemSettings.GetRockInstallationDateTime().ToRockDateTimeOffset().ToString(),
                MachineName = hostingSettings.MachineName,
                ExecutingLocation = Assembly.GetExecutingAssembly().Location,
                WebRootPath = hostingSettings.WebRootPath,
                LastCoreMigration = lastCoreMigration,
                PluginMigrations = pluginMigrations,
                TransactionQueue = GetTransactionQueueStats(),
                Routes = GetRoutes(),
                IsCacheStatisticsEnabled = isCacheStatisticsEnabled,
                CacheStatistics = isCacheStatisticsEnabled ? GetCacheStatistics() : new List<CacheStatisticBag>(),
                Threads = GetThreadInformation()
            };
        }

        /// <summary>
        /// Gets the formatted start time of the current process, or "-" when unavailable.
        /// </summary>
        /// <returns>The formatted process start time.</returns>
        private static string GetProcessStartTime()
        {
            try
            {
                var process = System.Diagnostics.Process.GetCurrentProcess();
                return new DateTimeOffset( process.StartTime ).ToString();
            }
            catch
            {
                // Intentionally ignored: process start time is not readable in some hosting environments.
                return "-";
            }
        }

        /// <summary>
        /// Gets information about the current Rock database.
        /// </summary>
        /// <returns>A populated <see cref="DatabaseInformationBag"/>.</returns>
        private DatabaseInformationBag GetDatabaseInformation()
        {
            try
            {
                var config = RockApp.Current.GetDatabaseConfiguration();
                var isAzure = config.Platform == DatabasePlatform.AzureSql;

                var bag = new DatabaseInformationBag
                {
                    Name = config.DatabaseName,
                    ServerName = config.ServerName,
                    Version = config.Version,
                    FriendlyVersion = isAzure ? null : config.GetVersionFriendlyName(),
                    CompatibilityVersion = config.GetCompatibilityLevelFriendlyName(),
                    DatabaseSizeMb = config.GetDatabaseSize(),
                    LogSizeMb = config.GetLogSize(),
                    RecoveryModel = config.RecoveryModel,
                    AllowSnapshotIsolation = config.IsSnapshotIsolationAllowed,
                    IsReadCommittedSnapshotOn = config.IsReadCommittedSnapshotEnabled,
                    IsAzure = isAzure,
                    ServiceObjective = isAzure ? config.ServiceObjective : null
                };

                // The read-only context is optional, so only report its status when configured.
                if ( System.Configuration.ConfigurationManager.ConnectionStrings["RockContextReadOnly"] != null )
                {
                    var readOnlyContext = new RockContextReadOnly();
                    bag.ReadOnlyContextStatus = readOnlyContext.Database
                        .SqlQuery<string>( "SELECT DATABASEPROPERTYEX(DB_NAME(), 'Updateability')" )
                        .FirstOrDefault();
                }

                return bag;
            }
            catch ( Exception ex )
            {
                return new DatabaseInformationBag
                {
                    ErrorMessage = $"Unable to read database system information: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Gets the last core migration identifier and the most recent migration for each plugin assembly.
        /// </summary>
        /// <returns>A tuple of the last core migration name and the list of plugin migrations.</returns>
        private (string LastCoreMigration, List<PluginMigrationBag> PluginMigrations) GetMigrationData()
        {
            var lastCoreMigration = DbService.ExecuteScalar(
                "SELECT TOP 1 [MigrationId] FROM [__MigrationHistory] ORDER BY [MigrationId] DESC",
                CommandType.Text,
                null ) as string;

            var pluginMigrations = new List<PluginMigrationBag>();

            var pluginTable = DbService.GetDataTable( @"
WITH summary AS
(
    SELECT p.[PluginAssemblyName], p.[MigrationName], p.[MigrationNumber], ROW_NUMBER()
        OVER( PARTITION BY p.[PluginAssemblyName] ORDER BY p.[MigrationNumber] DESC ) AS section
    FROM [PluginMigration] p
)
SELECT s.[PluginAssemblyName], s.[MigrationName], s.[MigrationNumber]
FROM summary s
WHERE s.section = 1", CommandType.Text, null );

            if ( pluginTable != null )
            {
                foreach ( DataRow row in pluginTable.Rows )
                {
                    pluginMigrations.Add( new PluginMigrationBag
                    {
                        PluginAssemblyName = row[0].ToStringSafe(),
                        MigrationName = row[1].ToStringSafe(),
                        MigrationNumber = row[2].ToStringSafe()
                    } );
                }
            }

            return (lastCoreMigration, pluginMigrations);
        }

        /// <summary>
        /// Gets the counts of the standard queued transactions, grouped by type.
        /// </summary>
        /// <returns>The list of transaction queue statistics.</returns>
        private List<TransactionQueueStatBag> GetTransactionQueueStats()
        {
            return RockQueue.GetStandardQueuedTransactions()
                .GroupBy( t => t.GetType().Name )
                .Select( g => new TransactionQueueStatBag { Name = g.Key, Count = g.Count() } )
                .ToList();
        }

        /// <summary>
        /// Gets the per-cache statistics for every configured cache.
        /// </summary>
        /// <returns>The list of cache statistics.</returns>
        private List<CacheStatisticBag> GetCacheStatistics()
        {
            var statistics = new List<CacheStatisticBag>();

            foreach ( var cacheItemStat in RockCache.GetAllStatistics().OrderBy( s => s.Name ) )
            {
                foreach ( var handleStat in cacheItemStat.HandleStats )
                {
                    statistics.Add( new CacheStatisticBag
                    {
                        Name = cacheItemStat.Name,
                        Statistics = handleStat.Stats
                            .Select( s => $"{s.CounterType.ConvertToString()}: {s.Count:N0}" )
                            .ToList()
                    } );
                }
            }

            return statistics;
        }

        /// <summary>
        /// Gets worker thread pool usage details.
        /// </summary>
        /// <returns>A populated <see cref="ThreadInformationBag"/>.</returns>
        private ThreadInformationBag GetThreadInformation()
        {
            ThreadPool.GetMaxThreads( out var maxWorkerThreads, out _ );
            ThreadPool.GetAvailableThreads( out var availableWorkerThreads, out _ );

            var threadsInUse = maxWorkerThreads - availableWorkerThreads;

            // Decide the badge severity from the raw fraction (not the rounded percent) so the
            // boundary behavior matches exactly: >10% warning, >=30% danger, otherwise none.
            var usageFraction = maxWorkerThreads > 0 ? ( float ) threadsInUse / maxWorkerThreads : 0f;
            var badgeCssClass = string.Empty;
            if ( usageFraction > 0.1f )
            {
                badgeCssClass = usageFraction < 0.3f ? "badge badge-warning" : "badge badge-danger";
            }

            return new ThreadInformationBag
            {
                ThreadsInUse = threadsInUse,
                MaxThreads = maxWorkerThreads,
                PercentInUse = ( int ) Math.Ceiling( usageFraction * 100 ),
                BadgeCssClass = badgeCssClass
            };
        }

        /// <summary>
        /// Gets the registered routes and the pages each one serves.
        /// </summary>
        /// <returns>The list of route information.</returns>
        private List<RouteInformationBag> GetRoutes()
        {
            var routes = new List<RouteInformationBag>();

#if REVIEW_WEBFORMS
            /*
                6/17/26 - MSE

                Route enumeration reads the live, in-memory ASP.NET route table, which only
                exists under the System.Web hosting model. This must be reimplemented against
                .NET Core endpoint routing (or rebuilt from the PageRoute data) when Rock no
                longer targets .NET Framework.

                Reason: System.Web.Routing.RouteTable has no cross-platform equivalent.
            */
            var pageLookup = new PageService( RockContext )
                .Queryable()
                .Select( p => new { p.Id, p.InternalName } )
                .ToDictionary( p => p.Id, p => p.InternalName );

            var distinctRoutes = new Dictionary<string, Route>();
            foreach ( var route in RouteTable.Routes.OfType<Route>() )
            {
                if ( !distinctRoutes.ContainsKey( route.Url ) )
                {
                    distinctRoutes.Add( route.Url, route );
                }
            }

            foreach ( var routeItem in distinctRoutes )
            {
                var pages = routeItem.Value.PageIds()
                    .Where( id => pageLookup.ContainsKey( id ) )
                    .Select( id => $"{pageLookup[id]} ({id})" )
                    .ToList();

                routes.Add( new RouteInformationBag
                {
                    Route = routeItem.Key,
                    Pages = pages
                } );
            }
#endif

            return routes;
        }

        /// <summary>
        /// Restarts the web application, falling back to touching the web.config file
        /// to trigger an application pool recycle.
        /// </summary>
        private void RestartWebApplication()
        {
#if REVIEW_WEBFORMS
            /*
                6/17/26 - MSE

                Restarting the application requires host-specific APIs that only exist in the
                System.Web (WebForms) hosting model. This must be reimplemented against the
                hosting abstraction when Rock no longer targets .NET Framework.

                Reason: HttpRuntime.UnloadAppDomain has no cross-platform equivalent.
            */
            try
            {
                // Preferred approach (requires full trust).
                System.Web.HttpRuntime.UnloadAppDomain();
                return;
            }
            catch
            {
                // Intentionally ignored: fall back to touching web.config below.
            }

            try
            {
                var configPath = RockApp.Current.MapPath( "~/web.config" );
                File.SetLastWriteTimeUtc( configPath, RockDateTime.Now.ToUniversalTime() );
            }
            catch
            {
                // Intentionally ignored: restart is best-effort; the request was already logged and broadcast.
            }
#endif
        }

        /// <summary>
        /// Builds the plain-text diagnostics document used by the download action.
        /// </summary>
        /// <returns>The diagnostics file contents.</returns>
        private string BuildDiagnosticsFileText()
        {
            var diagnostics = GetSystemDiagnosticsBag();
            var sb = new StringBuilder();

            var version = $"{VersionInfo.VersionInfo.GetRockProductVersionFullName()} ({VersionInfo.VersionInfo.GetRockProductVersionNumber()})";
            AppendDiagnostic( sb, "Version", version );
            AppendDiagnostic( sb, "Database", FormatDatabaseForFile( diagnostics.Database ) );
            AppendDiagnostic( sb, "Execution Location", $"Machine Name: {diagnostics.MachineName}{Environment.NewLine}{diagnostics.ExecutingLocation}{Environment.NewLine}{diagnostics.WebRootPath}" );
            AppendDiagnostic( sb, "Migrations", FormatMigrationsForFile( diagnostics ) );
            AppendDiagnostic( sb, "Cache", FormatCacheForFile( diagnostics.CacheStatistics ) );
            AppendDiagnostic( sb, "Routes", FormatRoutesForFile( diagnostics.Routes ) );
            AppendDiagnostic( sb, "Threads", $"{diagnostics.Threads.ThreadsInUse} out of {diagnostics.Threads.MaxThreads} worker threads in use ({diagnostics.Threads.PercentInUse}%)" );

#if REVIEW_WEBFORMS
            /*
                6/17/26 - MSE

                The server-variables dump reads the System.Web request server variables, which
                are only available under the System.Web hosting model. This section must be
                reimplemented (or dropped) when Rock no longer targets .NET Framework.

                Reason: HttpRequest.ServerVariables has no cross-platform equivalent.
            */
            AppendDiagnostic( sb, "Server Variables", string.Empty );

            var request = System.Web.HttpContext.Current?.Request;
            if ( request != null )
            {
                foreach ( string key in request.ServerVariables )
                {
                    var isCookieOrPassword = key.Equals( "HTTP_COOKIE", StringComparison.OrdinalIgnoreCase )
                        || key.Equals( "AUTH_PASSWORD", StringComparison.OrdinalIgnoreCase );
                    if ( isCookieOrPassword )
                    {
                        continue;
                    }

                    var isSensitiveData = key.IndexOf( "ALL_HTTP", StringComparison.OrdinalIgnoreCase ) >= 0
                        || key.IndexOf( "ALL_RAW", StringComparison.OrdinalIgnoreCase ) >= 0;
                    if ( isSensitiveData )
                    {
                        var sanitized = System.Text.RegularExpressions.Regex.Replace( request.ServerVariables[key], @"ASP.NET_SessionId=\S*;|\.ROCK=\S*;", string.Empty, System.Text.RegularExpressions.RegexOptions.Multiline );
                        AppendDiagnostic( sb, key, sanitized );
                    }
                    else
                    {
                        AppendDiagnostic( sb, key, request.ServerVariables[key] );
                    }
                }
            }
#endif

            return sb.ToString();
        }

        /// <summary>
        /// Appends a "key: value" line, followed by a newline, to the diagnostics document.
        /// </summary>
        /// <param name="sb">The builder to append to.</param>
        /// <param name="key">The label.</param>
        /// <param name="value">The value.</param>
        private static void AppendDiagnostic( StringBuilder sb, string key, string value )
        {
            sb.Append( $"{key}: {value}{Environment.NewLine}" );
        }

        /// <summary>
        /// Formats the database information as plain text for the diagnostics download.
        /// </summary>
        /// <param name="database">The database information.</param>
        /// <returns>The formatted text.</returns>
        private static string FormatDatabaseForFile( DatabaseInformationBag database )
        {
            if ( database == null )
            {
                return string.Empty;
            }

            if ( database.ErrorMessage.IsNotNullOrWhiteSpace() )
            {
                return database.ErrorMessage;
            }

            var lines = new List<string>
            {
                $"Name: {database.Name}",
                $"Server: {database.ServerName}",
                $"Database Version: {database.Version}"
            };

            if ( database.FriendlyVersion.IsNotNullOrWhiteSpace() )
            {
                lines.Add( $"Database Friendly Version: {database.FriendlyVersion}" );
            }

            lines.Add( $"Database Compatibility Version: {database.CompatibilityVersion}" );
            lines.Add( $"Database Size: {database.DatabaseSizeMb} MB" );
            lines.Add( $"Log File Size: {database.LogSizeMb} MB" );
            lines.Add( $"Recovery Model: {database.RecoveryModel}" );
            lines.Add( $"Allow Snapshot Isolation: {database.AllowSnapshotIsolation.ToYesNo()}" );
            lines.Add( $"Is Read Committed Snapshot On: {database.IsReadCommittedSnapshotOn.ToYesNo()}" );

            if ( database.IsAzure )
            {
                lines.Add( $"Azure Service Tier Objective: {database.ServiceObjective}" );
            }

            if ( database.ReadOnlyContextStatus.IsNotNullOrWhiteSpace() )
            {
                lines.Add( $"RockContextReadOnly: {database.ReadOnlyContextStatus}" );
            }

            return string.Join( Environment.NewLine, lines );
        }

        /// <summary>
        /// Formats the migration information as plain text for the diagnostics download.
        /// </summary>
        /// <param name="diagnostics">The diagnostics data.</param>
        /// <returns>The formatted text.</returns>
        private static string FormatMigrationsForFile( SystemDiagnosticsBag diagnostics )
        {
            var lines = new List<string>();

            if ( diagnostics.LastCoreMigration.IsNotNullOrWhiteSpace() )
            {
                lines.Add( $"Last Core Migration: {diagnostics.LastCoreMigration}" );
            }

            lines.AddRange( diagnostics.PluginMigrations
                .Select( m => $"{m.PluginAssemblyName} - {m.MigrationName} ({m.MigrationNumber})" ) );

            return string.Join( Environment.NewLine, lines );
        }

        /// <summary>
        /// Formats the cache statistics as plain text for the diagnostics download.
        /// </summary>
        /// <param name="cacheStatistics">The cache statistics.</param>
        /// <returns>The formatted text.</returns>
        private static string FormatCacheForFile( List<CacheStatisticBag> cacheStatistics )
        {
            if ( cacheStatistics == null || !cacheStatistics.Any() )
            {
                return string.Empty;
            }

            return string.Join( Environment.NewLine,
                cacheStatistics.Select( c => $"{c.Name}: {string.Join( ", ", c.Statistics )}" ) );
        }

        /// <summary>
        /// Formats the route information as plain text for the diagnostics download.
        /// </summary>
        /// <param name="routes">The routes.</param>
        /// <returns>The formatted text.</returns>
        private static string FormatRoutesForFile( List<RouteInformationBag> routes )
        {
            if ( routes == null || !routes.Any() )
            {
                return string.Empty;
            }

            return string.Join( Environment.NewLine,
                routes.Select( r => $"{r.Route}: {string.Join( ", ", r.Pages )}" ) );
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the diagnostics information for the Diagnostics tab. Loaded on demand.
        /// </summary>
        /// <returns>The diagnostics information.</returns>
        [BlockAction]
        public BlockActionResult GetDiagnostics()
        {
            if ( !IsAdministrator() )
            {
                return ActionForbidden( "You are not authorized to view diagnostics." );
            }

            return ActionOk( GetSystemDiagnosticsBag() );
        }

        /// <summary>
        /// Clears all cached items, re-registers types, and deletes cached files.
        /// </summary>
        /// <returns>The clear-cache result, including any non-fatal warning.</returns>
        [BlockAction]
        public BlockActionResult ClearCache()
        {
            if ( !IsAdministrator() )
            {
                return ActionForbidden( "You are not authorized to clear the cache." );
            }

            var messages = RockCache.ClearAllCachedItems();

            // Flush today's check-in codes.
            AttendanceCodeService.FlushTodaysCodes();

            var webAppPath = RockApp.Current.MapPath( "~" );

            // Re-register any unregistered entity types, field types, and block types.
            EntityTypeService.RegisterEntityTypes();
            FieldTypeService.RegisterFieldTypes();
            BlockTypeService.FlushRegistrationCache();
            BlockTypeService.RegisterBlockTypes( webAppPath, false );

            messages.Add( "EntityTypes, FieldTypes, BlockTypes have been re-registered" );

            // Delete all cached files. The cache itself was already cleared above, so a
            // file-deletion failure is surfaced as a warning rather than failing the action.
            var isWarning = false;

            try
            {
                var cachePath = Path.Combine( webAppPath, "App_Data/Cache" );
                if ( Directory.Exists( cachePath ) )
                {
                    var directoryInfo = new DirectoryInfo( cachePath );

                    foreach ( var childDirectory in directoryInfo.GetDirectories() )
                    {
                        childDirectory.Delete( true );
                    }

                    foreach ( var file in directoryInfo.GetFiles().Where( f => f.Name != ".gitignore" ) )
                    {
                        file.Delete();
                    }
                }

                messages.Add( "Cached files have been deleted" );
            }
            catch ( Exception ex )
            {
                isWarning = true;
                messages.Add( $"The following error occurred when attempting to delete cached files: {ex.Message}" );
            }

            return ActionOk( new ClearCacheResultBag
            {
                Messages = messages,
                IsWarning = isWarning
            } );
        }

        /// <summary>
        /// Restarts the Rock application.
        /// </summary>
        /// <returns>An empty success result.</returns>
        [BlockAction]
        public BlockActionResult RestartRock()
        {
            if ( !IsAdministrator() )
            {
                return ActionForbidden( "You are not authorized to restart Rock." );
            }

            RockWebFarm.OnRestartRequested( RequestContext.CurrentPerson );
            RestartWebApplication();

            return ActionOk();
        }

        /// <summary>
        /// Drains the standard transaction queue immediately and returns refreshed diagnostics.
        /// </summary>
        /// <returns>The refreshed diagnostics information.</returns>
        [BlockAction]
        public BlockActionResult DrainQueue()
        {
            if ( !IsAdministrator() )
            {
                return ActionForbidden( "You are not authorized to drain the queue." );
            }

            // Drain the queue immediately, then wait up to 2 seconds so quickly-drained work shows progress.
            var task = Task.Run( () => RockQueue.Drain( ex => ExceptionLogService.LogException( ex ) ) );
            task.Wait( 2000 );

            return ActionOk( GetSystemDiagnosticsBag() );
        }

        /// <summary>
        /// Generates a plain-text diagnostics file for download.
        /// </summary>
        /// <returns>A file download result.</returns>
        [BlockAction]
        public BlockActionResult DownloadDiagnostics()
        {
            if ( !IsAdministrator() )
            {
                return ActionForbidden( "You are not authorized to download diagnostics." );
            }

            var content = BuildDiagnosticsFileText();
            var stream = new MemoryStream( Encoding.UTF8.GetBytes( content ) );
            var fileName = $"RockDiagnostics-{RockApp.Current.HostingSettings.MachineName}.txt";

            return new FileBlockActionResult( stream, "text/plain", fileName );
        }

        #endregion
    }
}
