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
using System.Linq;

namespace Rock.BaselineTool;

/// <summary>
/// Prepares B's settled database for scripting: checks that startup
/// finished, stops on startup exceptions, resets the state startup wrote,
/// undoes the post-install job and records what <c>package</c> needs.
/// </summary>
/// <remarks>
/// Every table step skips a table that does not exist, so the lists can
/// name tables that only newer versions have. All changes run in one
/// transaction, so a failure leaves the database as it was.
/// </remarks>
internal static class CleanupCommand
{
    #region Lists

    /// <summary>
    /// The <c>PostInstallDataMigrations</c> job's <c>Guid</c>.
    /// </summary>
    private static readonly Guid PostInstallJobGuid = new Guid( "322984F1-A7A0-4D1B-AE6F-D7F043F66EB3" );

    /// <summary>
    /// The tables the post-install job fills, in delete order.
    /// <c>IdentityVerification</c> references <c>IdentityVerificationCode</c>.
    /// The job fills <c>AnalyticsSourcePostalCode</c> from RockWeb's
    /// census spreadsheet only when the table is empty, so kept rows would
    /// stop a new install from generating its own.
    /// </summary>
    private static readonly string[] PostInstallTables =
    {
        "AnalyticsSourceDate", "AnalyticsSourcePostalCode", "IdentityVerification", "IdentityVerificationCode"
    };

    /// <summary>
    /// The tables holding run data from startup, in delete order (children
    /// before the tables they reference). <c>ScheduleDate</c> holds each
    /// schedule's dates for two years either side of when it was written,
    /// so kept rows would anchor every new install to the build date;
    /// the nightly <c>RockCleanup</c> job fills it on a new install.
    /// <c>HistoryLogin</c> records each sign-in, such as the one needed to
    /// run the 2 AM run-once jobs.
    /// </summary>
    private static readonly string[] RunDataTables =
    {
        "ExceptionLog", "History", "HistoryLogin", "ScheduleDate",
        "Interaction", "InteractionSession", "InteractionComponent", "InteractionDeviceType",
        "AttendanceOccurrence", "ServiceJobHistory",
        "WebFarmNodeLog", "WebFarmNodeMetric", "WebFarmNode"
    };

    /// <summary>
    /// The <c>ServiceJob</c> columns that record a job's last run.
    /// </summary>
    private static readonly string[] JobRunStateColumns =
    {
        "LastRunDateTime", "LastSuccessfulRunDateTime", "LastRunDurationSeconds", "LastStatus", "LastStatusMessage", "LastRunSchedulerName"
    };

    /// <summary>
    /// System settings Rock creates at runtime that hold values of the
    /// site that wrote them. Kept rows would ship those values to every
    /// new install; without them Rock creates its own when first needed.
    /// <c>OpenIdConnectRsaKeys</c> is the OIDC signing key, encrypted with
    /// the build site's <c>DataEncryptionKey</c>. <c>core_RockSecuritySettings</c>
    /// holds a copy of the security roles' members, including the Admin
    /// person's <c>Guid</c>, which each install regenerates.
    /// </summary>
    private static readonly string[] PerSiteSettings =
    {
        "OpenIdConnectRsaKeys", "core_RockSecuritySettings"
    };

    #endregion Lists

    /// <summary>
    /// Runs cleanup.
    /// </summary>
    /// <param name="options">The parsed options.</param>
    /// <returns>0 on success, 1 when a check stopped it.</returns>
    public static int Run( BaselineOptions options )
    {
        var site = new BaselineSite( options.RockWebPath );
        var source = new BaselineSource( options.SourcePath );
        var rockVersion = site.GetRockVersion();

        Console.WriteLine( $"B is Rock {rockVersion}." );

        using var database = new BaselineDatabase( BaselineSite.ResolveDatabase( options.Database, site ) );
        if ( !CheckSettled( database, source, rockVersion ) || !CheckExceptions( database, options.IsAcceptExceptions ) )
        {
            return 1;
        }

        database.Transaction = database.Connection.BeginTransaction();

        var work = new BaselineWork
        {
            RockVersion = rockVersion.ToString(),
            IncludesThroughMigrationId = ReadIncludesThroughMigrationId( database ),
            CleanedDateTime = DateTime.Now
        };

        ResetStartupState( database );
        DeletePerSiteSettings( database );
        UndoPostInstallJob( database );

        foreach ( var table in RunDataTables )
        {
            DeleteRows( database, table );
        }

        if ( database.ColumnExists( "UserLogin", "IsOnLine" ) )
        {
            Report( "UserLogin marked offline", database.Execute( "UPDATE [dbo].[UserLogin] SET [IsOnLine] = 0 WHERE [IsOnLine] = 1;" ) );
        }

        // Recorded last, after every insert and delete above.
        work.IdentityValues = ReadIdentityValues( database );

        database.Transaction.Commit();

        Console.WriteLine( $"Recorded {work.IdentityValues.Count} identity values, migration {work.IncludesThroughMigrationId} as the last one included, to {work.Save( options.WorkPath )}." );
        Console.WriteLine( "cleanup: DONE." );

        return 0;
    }

    #region Checks

    /// <summary>
    /// Checks that startup settled: no run-once job from either
    /// <c>DataMigrationsStartup</c> list is left (the post-install job is
    /// on the startup list, so it must have run too), and every HotFix B
    /// runs is recorded.
    /// </summary>
    private static bool CheckSettled( BaselineDatabase database, BaselineSource source, Version rockVersion )
    {
        var outstanding = FindOutstanding( database, source, rockVersion );

        Console.WriteLine( $"Settle check: {source.GetRunOnceJobGuids().Count} run-once jobs in DataMigrationsStartup, {source.GetHotFixNumbers( rockVersion ).Count} HotFixes B runs." );

        foreach ( var item in outstanding )
        {
            Console.WriteLine( $"OUTSTANDING  {item}" );
        }

        if ( outstanding.Count > 0 )
        {
            Console.WriteLine( "cleanup: STOPPED. Startup has not settled. Start the site again and wait, or run the named jobs from Rock, then rerun cleanup." );
        }

        return outstanding.Count == 0;
    }

    /// <summary>
    /// Lists what startup has not finished: run-once jobs from either
    /// <c>DataMigrationsStartup</c> list still present, and HotFixes B runs
    /// that are not recorded. Empty when startup has settled.
    /// </summary>
    /// <param name="database">B's database.</param>
    /// <param name="source">B's source tree.</param>
    /// <param name="rockVersion">B's version, which decides the HotFixes it runs.</param>
    /// <returns>One line per outstanding item.</returns>
    internal static List<string> FindOutstanding( BaselineDatabase database, BaselineSource source, Version rockVersion )
    {
        var jobs = source.GetRunOnceJobGuids();
        var remainingJobs = database.Query( "SELECT [Guid], [Name] FROM [dbo].[ServiceJob];" )
            .Where( r => jobs.ContainsValue( ( Guid ) r[0] ) )
            .Select( r => $"job '{r[1]}' ({r[0]}) has not run" );

        var recordedHotFixes = new HashSet<int>( database.Query( "SELECT [MigrationNumber] FROM [dbo].[PluginMigration] WHERE [PluginAssemblyName] = 'Rock';" ).Select( r => ( int ) r[0] ) );
        var missingHotFixes = source.GetHotFixNumbers( rockVersion )
            .Where( n => !recordedHotFixes.Contains( n ) )
            .Select( n => $"HotFix {n} is not recorded in PluginMigration" );

        return remainingJobs.Concat( missingHotFixes ).ToList();
    }

    /// <summary>
    /// Lists startup exceptions and stops unless they were accepted.
    /// </summary>
    private static bool CheckExceptions( BaselineDatabase database, bool isAccepted )
    {
        if ( !database.TableExists( "ExceptionLog" ) )
        {
            return true;
        }

        var groups = database.Query( @"
SELECT COUNT(*), ISNULL( [ExceptionType], '' ), LEFT( ISNULL( [Description], '' ), 200 )
FROM [dbo].[ExceptionLog]
GROUP BY [ExceptionType], LEFT( ISNULL( [Description], '' ), 200 )
ORDER BY COUNT(*) DESC;" );

        if ( groups.Count == 0 )
        {
            return true;
        }

        foreach ( var group in groups )
        {
            Console.WriteLine( $"EXCEPTION  x{group[0]} {group[1]}: {group[2]}" );
        }

        if ( isAccepted )
        {
            Console.WriteLine( "Exceptions accepted with --accept-exceptions; they are pruned." );
            return true;
        }

        Console.WriteLine( "cleanup: STOPPED. Startup logged exceptions. Review them, then rerun with --accept-exceptions if they are expected (for example one-time block conversion errors)." );

        return false;
    }

    #endregion Checks

    #region Changes

    /// <summary>
    /// Resets <c>RockInstanceId</c>'s <c>CreatedDateTime</c>, which marks
    /// an existing installation, and every job's last-run columns.
    /// </summary>
    private static void ResetStartupState( BaselineDatabase database )
    {
        Report( "RockInstanceId CreatedDateTime cleared", database.Execute( "UPDATE [dbo].[Attribute] SET [CreatedDateTime] = NULL WHERE [Key] = 'RockInstanceId' AND [EntityTypeQualifierColumn] = 'SystemSetting';" ) );

        var columns = JobRunStateColumns.Where( c => database.ColumnExists( "ServiceJob", c ) ).ToList();

        if ( columns.Count > 0 )
        {
            Report( "ServiceJob run state cleared", database.Execute( $"UPDATE [dbo].[ServiceJob] SET {string.Join( ", ", columns.Select( c => $"[{c}] = NULL" ) )};" ) );
        }
    }

    /// <summary>
    /// Deletes the system settings in <see cref="PerSiteSettings"/>.
    /// </summary>
    private static void DeletePerSiteSettings( BaselineDatabase database )
    {
        foreach ( var key in PerSiteSettings )
        {
            Report( $"{key} system setting deleted", database.Execute( $"DELETE FROM [dbo].[Attribute] WHERE [Key] = '{key}' AND [EntityTypeQualifierColumn] = 'SystemSetting';" ) );
        }
    }

    /// <summary>
    /// Deletes what the post-install job generated and inserts its job
    /// row again, so every new install runs it.
    /// </summary>
    /// <remarks>
    /// The row is the one in the 2021 baseline script (and no migration
    /// has changed it since), with its original <c>Id</c> of 64, so the
    /// table's identity values match a database built through EF6.
    /// </remarks>
    private static void UndoPostInstallJob( BaselineDatabase database )
    {
        foreach ( var table in PostInstallTables )
        {
            DeleteRows( database, table );
        }

        if ( database.Scalar( "SELECT 1 FROM [dbo].[ServiceJob] WHERE [Guid] = @Guid;", ("@Guid", PostInstallJobGuid) ) != null )
        {
            Console.WriteLine( "  PostInstallDataMigrations job row already present" );
            return;
        }

        var occupant = database.Scalar( "SELECT [Name] FROM [dbo].[ServiceJob] WHERE [Id] = 64;" );

        if ( occupant != null )
        {
            throw new InvalidOperationException( $"ServiceJob Id 64 belongs to '{occupant}', so the PostInstallDataMigrations row cannot be restored with its original Id." );
        }

        var columns = new List<(string Name, object Value)>
        {
            ("Id", 64),
            ("IsSystem", true),
            ("IsActive", true),
            ("Name", "Runs data updates that need to occur after a new installation."),
            ("Description", "This job will take care of any data migrations that need to occur after a new installation. After all the operations are done, this job will delete itself."),
            ("Class", "Rock.Jobs.PostInstallDataMigrations"),
            ("CronExpression", "0 0 2 1/1 * ? *"),
            ("NotificationStatus", 1),
            ("Guid", PostInstallJobGuid)
        };

        columns.AddRange( new (string Name, object Value)[] { ("EnableHistory", false), ("HistoryCount", 0) }.Where( c => database.ColumnExists( "ServiceJob", c.Name ) ) );

        database.Execute( $@"
SET IDENTITY_INSERT [dbo].[ServiceJob] ON;
INSERT INTO [dbo].[ServiceJob] ( {string.Join( ", ", columns.Select( c => $"[{c.Name}]" ) )} )
VALUES ( {string.Join( ", ", columns.Select( c => $"@{c.Name}" ) )} );
SET IDENTITY_INSERT [dbo].[ServiceJob] OFF;", columns.Select( c => ($"@{c.Name}", c.Value) ).ToArray() );

        Console.WriteLine( "  PostInstallDataMigrations job row restored with Id 64" );
    }

    /// <summary>
    /// Deletes every row of a table, skipping a table that does not exist.
    /// </summary>
    private static void DeleteRows( BaselineDatabase database, string table )
    {
        if ( !database.TableExists( table ) )
        {
            Console.WriteLine( $"  {table}: not in this version, skipped" );
            return;
        }

        Report( table, database.Execute( $"DELETE FROM [dbo].[{table}];" ) );
    }

    #endregion Changes

    #region Recorded State

    /// <summary>
    /// Reads the highest migration ID from <c>__MigrationHistory</c> and
    /// <c>[RockMigration]</c>, truncated to 14 digits.
    /// </summary>
    private static long ReadIncludesThroughMigrationId( BaselineDatabase database )
    {
        var ids = new List<long>();

        if ( database.TableExists( "__MigrationHistory" ) )
        {
            var ef6Id = ( string ) database.Scalar( "SELECT MAX( [MigrationId] ) FROM [dbo].[__MigrationHistory];" );

            if ( ef6Id != null )
            {
                ids.Add( long.Parse( ef6Id.Substring( 0, 14 ) ) );
            }
        }

        if ( database.TableExists( "RockMigration" ) && database.Scalar( "SELECT MAX( [MigrationId] ) FROM [dbo].[RockMigration];" ) is long rockMigrationId )
        {
            ids.Add( rockMigrationId );
        }

        if ( ids.Count == 0 )
        {
            throw new InvalidOperationException( "Neither __MigrationHistory nor [RockMigration] has any rows, so the last migration B includes is unknown." );
        }

        return ids.Max();
    }

    /// <summary>
    /// Reads the last identity value of every table that has issued one.
    /// Tables that never issued a value are left out; their seed is unchanged.
    /// </summary>
    internal static Dictionary<string, long> ReadIdentityValues( BaselineDatabase database )
    {
        return database.Query( @"
SELECT QUOTENAME( [s].[name] ) + '.' + QUOTENAME( [t].[name] ), CONVERT( BIGINT, [ic].[last_value] )
FROM [sys].[identity_columns] AS [ic]
INNER JOIN [sys].[tables] AS [t] ON [t].[object_id] = [ic].[object_id]
INNER JOIN [sys].[schemas] AS [s] ON [s].[schema_id] = [t].[schema_id]
WHERE [t].[is_ms_shipped] = 0
    AND [ic].[last_value] IS NOT NULL
ORDER BY 1;" ).ToDictionary( r => ( string ) r[0], r => ( long ) r[1] );
    }

    #endregion Recorded State

    /// <summary>
    /// Prints how many rows a step changed.
    /// </summary>
    private static void Report( string step, int rowCount )
    {
        Console.WriteLine( $"  {step}: {rowCount} rows" );
    }
}
