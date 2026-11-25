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
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using Microsoft.Data.SqlClient;

namespace Rock.Data;

/// <summary>
/// Base class for Rock's SQL migrations. A migration only emits SQL;
/// every statement runs immediately, in order, on the runner's
/// connection and the migration's transaction. Declare the ID and
/// target version with <see cref="SqlMigrationAttribute"/>.
/// </summary>
internal abstract class Migration : IMigration
{
    #region Baseline

    /// <summary>
    /// The baseline's migration ID: the first 14 digits of EF6's
    /// <c>201311251734059_CreateDatabase</c>. Every squash keeps it. The
    /// runner finds the baseline by this ID and handles it before every
    /// other migration; its target version is only a label.
    /// </summary>
    internal const long BaselineMigrationId = 20131125173405;

    /*
        10/28/2026 - CLAUDE

        This value changes at every squash, along with the baseline's target
        version. Paste the lines the baseline package tool prints.

        A database that is not empty must have run this migration, or it is
        older than the baseline and the runner stops before running anything.
        It is checked against [RockMigration] on every start, so the same rule
        covers the EF6 cutover and every later squash.

        Reason: Stop a database that skipped a squash before it is damaged further.
    */

    /// <summary>
    /// The ID of the last migration the current baseline replaces.
    /// </summary>
    internal static readonly long BaselineIncludesThroughMigrationId = 20260512231528;

    #endregion Baseline

    /// <summary>
    /// The command timeout, matching the EF6 migrations configuration.
    /// </summary>
    private const int CommandTimeoutSeconds = 300;

    /// <summary>
    /// Splits a script into batches on lines that contain only <c>GO</c>,
    /// as SSMS writes them.
    /// </summary>
    private static readonly Regex BatchSeparator = new Regex( @"^[ \t]*GO[ \t]*\r?$", RegexOptions.Multiline | RegexOptions.IgnoreCase );

    private MigrationHelper _rockMigrationHelper;

    /// <summary>
    /// The connection the runner opened. Set by the runner before <see cref="Up"/>.
    /// </summary>
    internal SqlConnection Connection { get; set; }

    /// <summary>
    /// The migration's transaction. Every migration runs in one. Set by
    /// the runner before <see cref="Up"/>.
    /// </summary>
    internal SqlTransaction Transaction { get; set; }

    /// <summary>
    /// The helper that builds Rock's common migration SQL.
    /// </summary>
    public MigrationHelper RockMigrationHelper => _rockMigrationHelper ?? ( _rockMigrationHelper = new MigrationHelper( this ) );

    /// <summary>
    /// Applies the migration.
    /// </summary>
    public abstract void Up();

    /*
        11/18/26 - CLAUDE

        Sql() runs its text as one batch and never splits on GO lines. EF6's
        Sql() did split, but in all of Rock's EF6 history (develop and
        release-20.0) only one migration relied on it, from 2022, and the
        baseline already contains it. HotFix Sql() never split. Splitting
        here would break any data value with "Go" alone on a line, such as
        HTML or Lava passed through a RockMigrationHelper call. Text that
        needs batches goes in separate Sql() calls or in a script run by
        SqlResource(), which does split.

        Reason: Keep data values intact; scripts opt in to batches.
    */
    /// <summary>
    /// Runs SQL as a single batch. The text is sent to SQL Server exactly
    /// as given: lines that contain only <c>GO</c> are <b>not</b> treated
    /// as batch separators, so <c>GO</c> is a syntax error here. To run
    /// several batches, make one <see cref="Sql"/> call per batch, or put
    /// the script in a file and run it with <see cref="SqlResource()"/>,
    /// which does split on <c>GO</c> lines.
    /// </summary>
    /// <param name="sql">The SQL, one batch.</param>
    public void Sql( string sql )
    {
        using var command = CreateCommand( sql );
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Runs a SQL statement and returns the first column of the first row.
    /// It sees every earlier statement of this migration.
    /// </summary>
    /// <param name="sql">The SQL.</param>
    /// <returns>The value, or <c>null</c> when there are no rows.</returns>
    public object SqlScalar( string sql )
    {
        using var command = CreateCommand( sql );
        return command.ExecuteScalar();
    }

    /// <summary>
    /// Runs this migration's unnamed script, <c>{id}.sql</c>. The script
    /// is split into batches on lines that contain only <c>GO</c> (in any
    /// case, as SSMS writes them), and each batch runs in order. Unlike
    /// <see cref="Sql"/>, this means a data value inside the script must
    /// not have <c>GO</c> alone on a line.
    /// </summary>
    protected void SqlResource()
    {
        RunScript( null );
    }

    /// <summary>
    /// Loads the rows of one of this migration's JSON data files,
    /// <c>{id}-{table}.json</c>, into the <c>dbo</c> table of that name
    /// with a bulk copy, keeping identity values. See
    /// <see cref="MigrationBulkLoader"/> for the format.
    /// </summary>
    /// <param name="table">The table name, which is also the file name's suffix.</param>
    protected void BulkResource( string table )
    {
        var fileName = $"{GetMigrationId()}-{table}.json";

        using var stream = OpenResource( fileName );
        using var gzip = new GZipStream( stream, CompressionMode.Decompress );
        using var json = new MemoryStream();
        gzip.CopyTo( json );
        MigrationBulkLoader.Load( Connection ?? throw NoConnection(), Transaction, table, json.ToArray(), CommandTimeoutSeconds );
    }

    /// <summary>
    /// Runs one of this migration's named scripts, <c>{id}-{suffix}.sql</c>.
    /// The script is split into batches on lines that contain only
    /// <c>GO</c> (in any case, as SSMS writes them), and each batch runs in
    /// order. Unlike <see cref="Sql"/>, this means a data value inside the
    /// script must not have <c>GO</c> alone on a line.
    /// </summary>
    /// <param name="suffix">The part of the file name after the ID and <c>-</c>.</param>
    protected void SqlResource( string suffix )
    {
        RunScript( suffix );
    }

    /// <summary>
    /// Loads a script embedded by the build as <c>.sql.gz</c>, decompresses
    /// it and runs each batch.
    /// </summary>
    private void RunScript( string suffix )
    {
        var id = GetMigrationId();
        var fileName = suffix == null ? $"{id}.sql" : $"{id}-{suffix}.sql";

        string script;

        using var stream = OpenResource( fileName );
        using var gzip = new GZipStream( stream, CompressionMode.Decompress );
        using var reader = new StreamReader( gzip );
        script = reader.ReadToEnd();

        foreach ( var batch in SplitBatches( script ) )
        {
            Sql( batch );
        }
    }

    /// <summary>
    /// The ID from this migration's <see cref="SqlMigrationAttribute"/>,
    /// which every one of its file names starts with.
    /// </summary>
    private long GetMigrationId()
    {
        return GetType().GetCustomAttribute<SqlMigrationAttribute>()?.Id
            ?? throw new InvalidOperationException( $"Migration {GetType().FullName} has no SqlMigration attribute, so its scripts cannot be found." );
    }

    /// <summary>
    /// Opens a file the build compressed and embedded as <c>{fileName}.gz</c>.
    /// </summary>
    private Stream OpenResource( string fileName )
    {
        return GetType().Assembly.GetManifestResourceStream( $"{fileName}.gz" )
            ?? throw new InvalidOperationException( $"Migration {GetType().FullName} could not find its script '{fileName}'. Check that the file exists in the migrations project with that name." );
    }

    /// <summary>
    /// Splits a script on <c>GO</c> lines and drops empty batches.
    /// </summary>
    /// <param name="script">The script.</param>
    /// <returns>The batches in order.</returns>
    internal static List<string> SplitBatches( string script )
    {
        return BatchSeparator.Split( script )
            .Where( b => !string.IsNullOrWhiteSpace( b ) )
            .ToList();
    }

    /// <summary>
    /// Creates a command on the runner's connection and the migration's transaction.
    /// </summary>
    private SqlCommand CreateCommand( string sql )
    {
        if ( Connection == null )
        {
            throw NoConnection();
        }

        return new SqlCommand( sql, Connection, Transaction )
        {
            CommandTimeout = CommandTimeoutSeconds
        };
    }

    /// <summary>
    /// The error for running a migration outside the runner.
    /// </summary>
    private InvalidOperationException NoConnection()
    {
        return new InvalidOperationException( $"Migration {GetType().FullName} has no connection. Migrations can only run through the migration runner." );
    }
}
