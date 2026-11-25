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
using System.Diagnostics;
using System.Linq;
using System.Threading;

using Microsoft.Data.SqlClient;

namespace Rock.BaselineTool;

/// <summary>
/// Watches B's database while the site starts and reports when startup
/// has settled, so the person knows when to stop the site and run
/// <c>cleanup</c>. It only reads.
/// </summary>
/// <remarks>
/// Settled means two things. First, the definite signals <c>cleanup</c>
/// checks: no run-once job is left and every HotFix B runs is recorded.
/// Second, the registration tables have not changed for the quiet
/// period. The block type thread (<c>Global.asax.cs</c>) runs in the
/// background and writes nothing that marks it finished, so the quiet
/// period stands in for it. When the CLI runs the startup flow itself,
/// it will know when that finishes and the quiet period can go.
/// </remarks>
internal static class MonitorCommand
{
    /// <summary>
    /// The tables startup registration and the block type thread write.
    /// A change to any of them restarts the quiet period.
    /// </summary>
    private static readonly string[] WatchedTables =
    {
        "EntityType", "FieldType", "BlockType", "Block", "Attribute", "AttributeQualifier", "AttributeValue"
    };

    /// <summary>
    /// Runs monitor.
    /// </summary>
    /// <param name="options">The parsed options.</param>
    /// <returns>0 when startup settled, 1 when the timeout passed first.</returns>
    public static int Run( BaselineOptions options )
    {
        var site = new BaselineSite( options.RockWebPath );
        var source = new BaselineSource( options.SourcePath );
        var rockVersion = site.GetRockVersion();
        var timing = new MonitorTiming
        {
            QuietPeriod = TimeSpan.FromSeconds( options.QuietSeconds ),
            PollInterval = TimeSpan.FromSeconds( options.PollSeconds ),
            Timeout = TimeSpan.FromMinutes( options.TimeoutMinutes )
        };

        Console.WriteLine( $"B is Rock {rockVersion}. Watching for startup to settle (quiet period {options.QuietSeconds}s, poll every {options.PollSeconds}s, timeout {options.TimeoutMinutes} min)." );

        if ( !Monitor( BaselineSite.ResolveDatabase( options.Database, site ), source, rockVersion, timing, Console.WriteLine ) )
        {
            Console.WriteLine( "monitor: STOPPED. The timeout passed before startup settled." );
            return 1;
        }

        Console.WriteLine( "monitor: DONE. Stop the site, then run cleanup." );

        return 0;
    }

    /// <summary>
    /// Polls the database until startup settles or the timeout passes.
    /// </summary>
    /// <param name="connectionString">B's database.</param>
    /// <param name="source">B's source tree.</param>
    /// <param name="rockVersion">B's version.</param>
    /// <param name="timing">The quiet period, poll interval and timeout.</param>
    /// <param name="log">Receives status lines.</param>
    /// <returns><c>true</c> when startup settled.</returns>
    internal static bool Monitor( string connectionString, BaselineSource source, Version rockVersion, MonitorTiming timing, Action<string> log )
    {
        var elapsed = Stopwatch.StartNew();
        var quiet = Stopwatch.StartNew();
        string lastFingerprint = null;
        var lastOutstanding = string.Empty;

        while ( true )
        {
            var (outstanding, fingerprint) = Poll( connectionString, source, rockVersion );

            if ( fingerprint == null || fingerprint != lastFingerprint )
            {
                lastFingerprint = fingerprint;
                quiet.Restart();
            }

            // Lists the outstanding items only when they change, to keep the output readable.
            var outstandingText = string.Join( Environment.NewLine, outstanding.Select( o => $"OUTSTANDING  {o}" ) );
            if ( outstandingText != lastOutstanding && outstandingText.Length > 0 )
            {
                log( outstandingText );
            }

            lastOutstanding = outstandingText;

            if ( outstanding.Count == 0 && quiet.Elapsed >= timing.QuietPeriod )
            {
                log( $"[{elapsed.Elapsed:hh\\:mm\\:ss}] Settled: nothing outstanding and no registration changes for {quiet.Elapsed.TotalSeconds:0}s." );
                return true;
            }

            log( $"[{elapsed.Elapsed:hh\\:mm\\:ss}] {outstanding.Count} outstanding, registration tables unchanged for {quiet.Elapsed.TotalSeconds:0}s." );

            if ( elapsed.Elapsed >= timing.Timeout )
            {
                return false;
            }

            Thread.Sleep( timing.PollInterval );
        }
    }

    /// <summary>
    /// Reads the outstanding items and a fingerprint of the watched
    /// tables. Reads uncommitted data so startup's transactions never
    /// block the poll.
    /// </summary>
    /// <returns>The outstanding items, and the fingerprint or <c>null</c> when the database could not be read.</returns>
    private static (List<string> Outstanding, string Fingerprint) Poll( string connectionString, BaselineSource source, Version rockVersion )
    {
        try
        {
            using var database = new BaselineDatabase( connectionString );
            database.Execute( "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;" );

            if ( !database.TableExists( "ServiceJob" ) || !database.TableExists( "PluginMigration" ) )
            {
                return (new List<string> { "migrations have not created ServiceJob and PluginMigration yet" }, null);
            }

            return (CleanupCommand.FindOutstanding( database, source, rockVersion ), ReadFingerprint( database ));
        }
        catch ( SqlException ex )
        {
            return (new List<string> { $"the database could not be read: {ex.Message}" }, null);
        }
    }

    /// <summary>
    /// Describes the watched tables by row count, highest <c>Id</c> and
    /// latest <c>ModifiedDateTime</c>: new rows, deleted rows and updated
    /// rows all change it. Compared between polls, never with the clock.
    /// </summary>
    private static string ReadFingerprint( BaselineDatabase database )
    {
        var parts = new List<string>();

        foreach ( var table in WatchedTables.Where( database.TableExists ) )
        {
            var modified = database.ColumnExists( table, "ModifiedDateTime" ) ? "MAX( [ModifiedDateTime] )" : "NULL";
            var row = database.Query( $"SELECT COUNT_BIG(*), MAX( [Id] ), {modified} FROM [dbo].[{table}];" )[0];

            parts.Add( $"{table}:{row[0]}:{row[1]}:{( row[2] as DateTime? )?.ToString( "o" )}" );
        }

        return string.Join( "|", parts );
    }
}

/// <summary>
/// How <see cref="MonitorCommand"/> waits.
/// </summary>
internal sealed class MonitorTiming
{
    /// <summary>
    /// How long the watched tables must stay unchanged, once nothing is
    /// outstanding, for startup to count as settled.
    /// </summary>
    public TimeSpan QuietPeriod { get; set; } = TimeSpan.FromSeconds( 60 );

    /// <summary>
    /// The time between polls.
    /// </summary>
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds( 10 );

    /// <summary>
    /// How long to watch before giving up.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes( 120 );
}
