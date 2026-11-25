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

using Microsoft.Data.SqlClient;

namespace Rock.Data;

/// <summary>
/// Applies Rock's SQL migrations. Set-based: every migration not recorded
/// in <c>[RockMigration]</c> runs, in order of target version then ID,
/// each in its own transaction with its history row. Web farm nodes are
/// serialized with a database application lock.
/// </summary>
/// <remarks>
/// Uses plain ADO.NET through <c>Microsoft.Data.SqlClient</c> and no EF
/// types, so it builds unchanged for the .NET Core host.
/// </remarks>
internal static class SqlMigrationRunner
{
    /// <summary>
    /// The application lock resource every node waits on.
    /// </summary>
    private const string LockResource = "Rock Migrations";

    /// <summary>
    /// The application name on the runner's connection, so a lock holder
    /// is recognizable from another node or in SSMS.
    /// </summary>
    private const string ApplicationName = "Rock Migrations";

    /// <summary>
    /// SQL Server's error number for "Cannot open database".
    /// </summary>
    private const int CannotOpenDatabaseErrorNumber = 4060;

    /// <summary>
    /// The <c>ContextKey</c> EF6 wrote on every row of Rock's own history:
    /// the full name of the <c>Rock.Migrations</c> configuration class.
    /// </summary>
    private const string RockEf6ContextKey = "Rock.Migrations.Configuration";

    #region SQL

    private const string CreateHistoryTableSql = @"
IF OBJECT_ID( N'[dbo].[RockMigration]', N'U' ) IS NULL
BEGIN
    CREATE TABLE [dbo].[RockMigration] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [MigrationId] BIGINT NOT NULL,
        [TargetVersion] NVARCHAR(50) NULL,
        [AppliedVersion] NVARCHAR(50) NULL,
        [MigrationName] NVARCHAR(250) NOT NULL,
        [AppliedDateTime] DATETIME NOT NULL,
        [DurationMilliseconds] INT NULL,
        CONSTRAINT [PK_dbo.RockMigration] PRIMARY KEY CLUSTERED ( [Id] )
    );

    CREATE UNIQUE INDEX [IX_MigrationId] ON [dbo].[RockMigration] ( [MigrationId] );
END";

    private const string InsertHistorySql = @"
INSERT INTO [dbo].[RockMigration] ( [MigrationId], [TargetVersion], [AppliedVersion], [MigrationName], [AppliedDateTime], [DurationMilliseconds] )
VALUES ( @MigrationId, @TargetVersion, @AppliedVersion, @MigrationName, @AppliedDateTime, @DurationMilliseconds );";

    private const string AcquireLockSql = @"
DECLARE @Result INT;
EXEC @Result = [sp_getapplock] @Resource = @Resource, @LockMode = 'Exclusive', @LockOwner = 'Session', @LockTimeout = @LockTimeout;
SELECT @Result;";

    private const string LockHolderSql = @"
SELECT TOP 1 [s].[session_id], [s].[host_name], [s].[program_name], [s].[login_time]
FROM [sys].[dm_tran_locks] AS [l]
INNER JOIN [sys].[dm_exec_sessions] AS [s] ON [s].[session_id] = [l].[request_session_id]
WHERE [l].[resource_type] = 'APPLICATION'
    AND [l].[resource_database_id] = DB_ID()
    AND [l].[request_status] = 'GRANT'
    AND [l].[request_session_id] <> @@SPID
    AND [l].[resource_description] LIKE '%[[]' + @Resource + ']%';";

    /// <summary>
    /// Reads Rock's EF6 history rows that are not yet recorded, in EF6 ID
    /// order. Rows of other EF6 contexts (a plugin's own code-first
    /// migrations) are not Rock's history, so they are left out.
    /// </summary>
    private const string ReadUnimportedEf6HistorySql = @"
IF OBJECT_ID( N'[dbo].[__MigrationHistory]', N'U' ) IS NOT NULL
    SELECT [h].[MigrationId]
    FROM [dbo].[__MigrationHistory] AS [h]
    WHERE [h].[ContextKey] = @ContextKey
        AND NOT EXISTS (
            SELECT 1
            FROM [dbo].[RockMigration] AS [r]
            WHERE [r].[MigrationId] = TRY_CONVERT( BIGINT, LEFT( [h].[MigrationId], 14 ) )
        )
    ORDER BY [h].[MigrationId];";

    #endregion SQL

    /// <summary>
    /// Applies every pending migration.
    /// </summary>
    /// <param name="options">What the host supplies.</param>
    /// <returns>What the run did.</returns>
    /// <exception cref="SqlMigrationException">Thrown when startup must stop: invalid migrations, a missing database outside development, a lock timeout, a database older than the baseline, or a failed migration.</exception>
    public static SqlMigrationRunResult Run( SqlMigrationRunnerOptions options )
    {
        var result = new SqlMigrationRunResult();
        var migrations = GetMigrations( options );

        if ( migrations.Count == 0 )
        {
            return result;
        }

        // The baseline is found by its fixed ID and handled before everything
        // else; its target version is only a label, so it is not sorted.
        var baseline = migrations.SingleOrDefault( m => m.Id == Migration.BaselineMigrationId )
            ?? throw new SqlMigrationException( $"No baseline migration (ID {Migration.BaselineMigrationId}) was found, so a new database cannot be built." );
        var sortedMigrations = OrderForProduction( migrations.Where( m => m != baseline ) );

        var connectionString = CreateConnectionString( options.ConnectionString );

        EnsureDatabaseExists( connectionString, options );

        using var connection = new SqlConnection( connectionString );
        // Messages SQL Server returns without an error (PRINT output,
        // "the module depends on the missing object" and similar), which
        // EF6 also logged as warnings.
        connection.InfoMessage += ( sender, e ) => options.Log( $"WARNING: {e.Message}" );
        connection.Open();

        AcquireLock( connection, options );

        try
        {
            ExecuteNonQuery( connection, null, CreateHistoryTableSql );

            var appliedIds = ReadAppliedIds( connection );

            result.ImportedCount = ImportEf6History( connection, migrations, appliedIds );

            if ( result.ImportedCount > 0 )
            {
                options.Log( $"Imported {result.ImportedCount} EntityFramework migration history rows." );
            }

            ApplyOrCheckBaseline( connection, baseline, appliedIds, options, result );

            result.ImportedHotFixCount = ImportHotFixHistory( connection, migrations, appliedIds );

            if ( result.ImportedHotFixCount > 0 )
            {
                options.Log( $"Recorded {result.ImportedHotFixCount} migrations whose HotFix already ran." );
            }

            var pending = sortedMigrations
                .Where( m => !appliedIds.Contains( m.Id ) )
                .Where( m => m.MinimumRockVersion == null || m.MinimumRockVersion <= options.RockVersion )
                .ToList();

            if ( pending.Count > 0 )
            {
                options.Log( $"Applying {pending.Count} migrations." );
            }

            foreach ( var migration in pending )
            {
                Apply( connection, migration, options );
                result.AppliedMigrationNames.Add( migration.Name );
            }
        }
        finally
        {
            ReleaseLock( connection );
        }

        return result;
    }

    #region Discovery

    /// <summary>
    /// Orders migrations the way the runner applies them: the baseline
    /// first, then by target version, then by ID. The commute check uses
    /// it too, so it always compares against the runner's real order.
    /// </summary>
    /// <param name="migrations">The migrations, with valid target versions.</param>
    /// <returns>The migrations in the order they are applied.</returns>
    internal static List<SqlMigrationDescriptor> OrderForProduction( IEnumerable<SqlMigrationDescriptor> migrations )
    {
        return migrations
            .OrderBy( m => m.Id == Migration.BaselineMigrationId ? 0 : 1 )
            .ThenBy( m => Version.Parse( m.TargetVersion ) )
            .ThenBy( m => m.Id )
            .ToList();
    }

    /// <summary>
    /// Finds the migrations and checks that their IDs are unique.
    /// </summary>
    private static List<SqlMigrationDescriptor> GetMigrations( SqlMigrationRunnerOptions options )
    {
        var migrations = ( options.Migrations ?? options.Assemblies
            .SelectMany( a => Rock.Reflection.SearchAssembly( a, typeof( Migration ) ).Values )
            .Select( SqlMigrationDescriptor.FromType ) )
            .ToList();

        var duplicate = migrations.GroupBy( m => m.Id ).FirstOrDefault( g => g.Count() > 1 );

        if ( duplicate != null )
        {
            throw new SqlMigrationException( $"The migrations {string.Join( " and ", duplicate.Select( m => m.FullName ) )} share the ID {duplicate.Key}. Every migration needs a unique ID." );
        }

        var duplicateHotFix = migrations
            .Where( m => m.ReplacesHotFixNumber.HasValue )
            .GroupBy( m => m.ReplacesHotFixNumber.Value )
            .FirstOrDefault( g => g.Count() > 1 );

        if ( duplicateHotFix != null )
        {
            throw new SqlMigrationException( $"The migrations {string.Join( " and ", duplicateHotFix.Select( m => m.FullName ) )} both replace HotFix {duplicateHotFix.Key}. A HotFix is replaced by one migration." );
        }

        return migrations;
    }

    #endregion Discovery

    #region Database and Lock

    /// <summary>
    /// Builds the runner's connection string: trusts the server certificate
    /// only where EF6's <c>System.Data.SqlClient</c> would not check it (see
    /// <see cref="SqlConnectionStringDefaults"/>), and names the application.
    /// </summary>
    private static string CreateConnectionString( string connectionString )
    {
        var builder = new SqlConnectionStringBuilder( connectionString )
        {
            ApplicationName = ApplicationName
        };

        SqlConnectionStringDefaults.TrustServerCertificateUnlessEncryptionRequested( builder );

        return builder.ConnectionString;
    }

    /// <summary>
    /// Creates the database when it is missing and this is a development
    /// environment, as EF6's <c>DbMigrator</c> did. Elsewhere a missing
    /// database stops startup.
    /// </summary>
    private static void EnsureDatabaseExists( string connectionString, SqlMigrationRunnerOptions options )
    {
        // SqlClient treats "Cannot open database" as transient and retries it
        // after 10 seconds, which only delays the answer here.
        var probeConnectionString = new SqlConnectionStringBuilder( connectionString ) { ConnectRetryCount = 0 }.ConnectionString;

        using var connection = new SqlConnection( probeConnectionString );
        try
        {
            connection.Open();
            return;
        }
        catch ( SqlException ex ) when ( ex.Number == CannotOpenDatabaseErrorNumber )
        {
            // The database is missing (or this login cannot open it), handled below.
        }

        var builder = new SqlConnectionStringBuilder( connectionString );
        var databaseName = builder.InitialCatalog;

        if ( !options.IsDevelopmentEnvironment )
        {
            throw new SqlMigrationException( $"Cannot open the database '{databaseName}'. Check that it exists and that the 'RockContext' connection string in web.ConnectionStrings.config is correct." );
        }

        options.Log( $"Creating the development database '{databaseName}'." );

        builder.InitialCatalog = "master";

        using var masterConnection = new SqlConnection( builder.ConnectionString );
        using var command = new SqlCommand( "IF DB_ID( @Name ) IS NULL BEGIN DECLARE @Sql NVARCHAR(400) = N'CREATE DATABASE ' + QUOTENAME( @Name ); EXEC( @Sql ); END", masterConnection );
        masterConnection.Open();
        command.Parameters.AddWithValue( "@Name", databaseName );
        command.ExecuteNonQuery();

        // The failed open leaves the pool in its blocking period, which
        // would replay the error on the next open.
        SqlConnection.ClearPool( connection );
    }

    /// <summary>
    /// Takes the session-owned application lock, waiting up to
    /// <see cref="SqlMigrationRunnerOptions.LockTimeout"/> and logging who
    /// holds it while waiting.
    /// </summary>
    private static void AcquireLock( SqlConnection connection, SqlMigrationRunnerOptions options )
    {
        if ( TryAcquireLock( connection, TimeSpan.Zero ) )
        {
            return;
        }

        var seconds = ( int ) options.LockTimeout.TotalSeconds;

        options.Log( $"Waiting up to {seconds} seconds for another Rock instance to finish migrating the database. {DescribeLockHolder( connection )}" );

        if ( TryAcquireLock( connection, options.LockTimeout ) )
        {
            options.Log( "Acquired the migration lock." );
            return;
        }

        var holder = DescribeLockHolder( connection );

        options.Log( $"Timed out waiting for the migration lock. {holder}" );

        throw new SqlMigrationException( $"Timed out after {seconds} seconds waiting for the database migration lock held by another Rock instance. {holder}" );
    }

    /// <summary>
    /// Calls <c>sp_getapplock</c> and returns whether the lock was granted.
    /// </summary>
    private static bool TryAcquireLock( SqlConnection connection, TimeSpan timeout )
    {
        using var command = new SqlCommand( AcquireLockSql, connection );
        command.Parameters.AddWithValue( "@Resource", LockResource );
        command.Parameters.AddWithValue( "@LockTimeout", ( int ) timeout.TotalMilliseconds );
        command.CommandTimeout = ( int ) timeout.TotalSeconds + 30;

        // 0 and 1 mean granted; negative values mean timeout, deadlock or error.
        return ( int ) command.ExecuteScalar() >= 0;
    }

    /// <summary>
    /// Releases the lock. Closing the connection would also release it.
    /// </summary>
    private static void ReleaseLock( SqlConnection connection )
    {
        if ( connection.State != System.Data.ConnectionState.Open )
        {
            return;
        }

        using var command = new SqlCommand( "EXEC [sp_releaseapplock] @Resource = @Resource, @LockOwner = 'Session';", connection );
        command.Parameters.AddWithValue( "@Resource", LockResource );
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Describes the session holding the lock, best effort. Reading other
    /// sessions' locks needs <c>VIEW SERVER STATE</c>, which Rock's login may not have.
    /// </summary>
    private static string DescribeLockHolder( SqlConnection connection )
    {
        const string unknown = "The lock holder could not be identified (that needs the VIEW SERVER STATE permission).";

        try
        {
            using var command = new SqlCommand( LockHolderSql, connection );
            command.Parameters.AddWithValue( "@Resource", LockResource );

            using var reader = command.ExecuteReader();
            if ( !reader.Read() )
            {
                return unknown;
            }

            return $"The lock is held by session {reader.GetInt16( 0 )} from host '{reader[1]}', program '{reader[2]}', logged in at {reader.GetDateTime( 3 ):s}.";
        }
        catch ( SqlException )
        {
            return unknown;
        }
    }

    #endregion Database and Lock

    #region History

    /// <summary>
    /// Reads the IDs of every applied migration.
    /// </summary>
    private static HashSet<long> ReadAppliedIds( SqlConnection connection )
    {
        var ids = new HashSet<long>();

        using var command = new SqlCommand( "SELECT [MigrationId] FROM [dbo].[RockMigration];", connection );
        using var reader = command.ExecuteReader();
        while ( reader.Read() )
        {
            ids.Add( reader.GetInt64( 0 ) );
        }

        return ids;
    }

    /*
        11/17/26 - CLAUDE

        This runs on every start, not only at the cutover: a developer can
        switch to a release branch, where EF6 still adds history rows, and
        then back to develop. The database filters out rows already
        recorded, so a normal start reads nothing and opens no transaction.

        Only Rock's own context is imported. Every Rock database checked
        (seven, including long-running sites back to CreateDatabase) has
        all of its rows under that key. If one somehow did not, the
        baseline check would stop startup before anything runs, which is
        safer than recording a plugin's EF6 migration as a Rock migration.

        Reason: Import only Rock's EF6 history, and only what is new.
    */
    /// <summary>
    /// Copies every row of Rock's <c>__MigrationHistory</c> whose truncated
    /// ID is not yet recorded, in EF6 ID order, so <c>[Id]</c> order matches
    /// the order EF6 applied them. <c>__MigrationHistory</c> is never written.
    /// </summary>
    /// <returns>The number of rows copied.</returns>
    private static int ImportEf6History( SqlConnection connection, List<SqlMigrationDescriptor> migrations, HashSet<long> appliedIds )
    {
        var ef6Ids = new List<string>();

        using ( var command = new SqlCommand( ReadUnimportedEf6HistorySql, connection ) )
        {
            command.Parameters.AddWithValue( "@ContextKey", RockEf6ContextKey );

            using var reader = command.ExecuteReader();
            while ( reader.Read() )
            {
                ef6Ids.Add( reader.GetString( 0 ) );
            }
        }

        if ( ef6Ids.Count == 0 )
        {
            return 0;
        }

        var targetVersionsById = migrations.ToDictionary( m => m.Id, m => m.TargetVersion );
        var importedCount = 0;

        using var transaction = connection.BeginTransaction();
        foreach ( var ef6Id in ef6Ids )
        {
            if ( ef6Id.Length < 14 || !long.TryParse( ef6Id.Substring( 0, 14 ), out var migrationId ) || appliedIds.Contains( migrationId ) )
            {
                continue;
            }

            var separatorIndex = ef6Id.IndexOf( '_' );
            var name = separatorIndex >= 0 ? ef6Id.Substring( separatorIndex + 1 ) : ef6Id;

            targetVersionsById.TryGetValue( migrationId, out var targetVersion );
            InsertHistory( connection, transaction, migrationId, targetVersion, null, name, null );

            appliedIds.Add( migrationId );
            importedCount++;
        }

        transaction.Commit();

        return importedCount;
    }

    /// <summary>
    /// Records, without running it, every migration converted from a HotFix
    /// that already ran as a plugin migration (a <c>[PluginMigration]</c> row
    /// for the <c>Rock</c> assembly). HotFixes were not written to be run
    /// twice, so the converted copy must not run where the HotFix did.
    /// Runs after the baseline check, so it never adds history to a
    /// database the check would refuse.
    /// </summary>
    /// <returns>The number of migrations recorded.</returns>
    private static int ImportHotFixHistory( SqlConnection connection, List<SqlMigrationDescriptor> migrations, HashSet<long> appliedIds )
    {
        var candidates = migrations
            .Where( m => m.ReplacesHotFixNumber.HasValue && !appliedIds.Contains( m.Id ) )
            .OrderBy( m => m.ReplacesHotFixNumber.Value )
            .ToList();

        if ( candidates.Count == 0 )
        {
            return 0;
        }

        var ranHotFixNumbers = new HashSet<int>();

        using ( var command = new SqlCommand( "IF OBJECT_ID( N'[dbo].[PluginMigration]', N'U' ) IS NOT NULL SELECT [MigrationNumber] FROM [dbo].[PluginMigration] WHERE [PluginAssemblyName] = N'Rock';", connection ) )
        using ( var reader = command.ExecuteReader() )
        {
            while ( reader.Read() )
            {
                ranHotFixNumbers.Add( reader.GetInt32( 0 ) );
            }
        }

        var importedCount = 0;

        using var transaction = connection.BeginTransaction();
        foreach ( var migration in candidates.Where( m => ranHotFixNumbers.Contains( m.ReplacesHotFixNumber.Value ) ) )
        {
            InsertHistory( connection, transaction, migration.Id, migration.TargetVersion, null, migration.Name, null );

            appliedIds.Add( migration.Id );
            importedCount++;
        }

        transaction.Commit();

        return importedCount;
    }

    /// <summary>
    /// Writes a history row.
    /// </summary>
    private static void InsertHistory( SqlConnection connection, SqlTransaction transaction, long migrationId, string targetVersion, string appliedVersion, string name, int? durationMilliseconds )
    {
        using var command = new SqlCommand( InsertHistorySql, connection, transaction );
        command.Parameters.AddWithValue( "@MigrationId", migrationId );
        command.Parameters.AddWithValue( "@TargetVersion", ( object ) targetVersion ?? DBNull.Value );
        command.Parameters.AddWithValue( "@AppliedVersion", ( object ) appliedVersion ?? DBNull.Value );
        command.Parameters.AddWithValue( "@MigrationName", name );
        command.Parameters.AddWithValue( "@AppliedDateTime", RockDateTime.Now );
        command.Parameters.AddWithValue( "@DurationMilliseconds", ( object ) durationMilliseconds ?? DBNull.Value );
        command.ExecuteNonQuery();
    }

    #endregion History

    #region Applying

    /// <summary>
    /// Runs the baseline on an empty database. On any other database,
    /// checks on every start that the history includes the last migration
    /// the baseline replaces, and stops startup before anything runs if
    /// it does not: the database is older than the baseline.
    /// </summary>
    private static void ApplyOrCheckBaseline( SqlConnection connection, SqlMigrationDescriptor descriptor, HashSet<long> appliedIds, SqlMigrationRunnerOptions options, SqlMigrationRunResult result )
    {
        if ( appliedIds.Count == 0 )
        {
            var hasPersonTable = ( int ) ExecuteScalar( connection, "SELECT CASE WHEN OBJECT_ID( N'[dbo].[Person]', N'U' ) IS NULL THEN 0 ELSE 1 END;" ) == 1;

            if ( hasPersonTable )
            {
                throw new SqlMigrationException( "The database has Rock tables but no migration history, so this version of Rock cannot tell what state it is in. Rock cannot run against this database. Restore the database and the Rock files from before this version was installed." );
            }

            options.Log( "New installation: running the baseline." );
            Apply( connection, descriptor, options, isBaseline: true );
            appliedIds.Add( descriptor.Id );
            appliedIds.Add( Migration.BaselineIncludesThroughMigrationId );
            result.IsBaselineRun = true;
            result.AppliedMigrationNames.Add( descriptor.Name );

            return;
        }

        if ( !appliedIds.Contains( Migration.BaselineIncludesThroughMigrationId ) )
        {
            throw new SqlMigrationException( $"This database is older than this version of Rock can upgrade. It has not run migration {Migration.BaselineIncludesThroughMigrationId}, the last one included in the Rock {descriptor.TargetVersion} baseline, so a squash was skipped. No migrations were run, but Rock cannot run against this database. Restore the database and the Rock files from before this version was installed." );
        }

        // EF6 databases record the baseline by importing CreateDatabase. This only
        // matters for a database whose history somehow lacks it; the baseline must
        // never run against a database that has history.
        if ( !appliedIds.Contains( descriptor.Id ) )
        {
            using var transaction = connection.BeginTransaction();
            InsertHistory( connection, transaction, descriptor.Id, descriptor.TargetVersion, options.RockVersion.ToString(), descriptor.Name, null );
            transaction.Commit();

            appliedIds.Add( descriptor.Id );
            result.IsBaselineRecorded = true;
        }
    }

    /*
        11/16/26 - CLAUDE

        Every migration runs in a transaction, with its history row written
        on the same transaction, so a crash or a failed statement leaves
        neither. There is deliberately no way to opt out: a migration outside
        a transaction can leave a half-applied change and must be safe to
        rerun, and nothing needs it today. Statements SQL Server refuses in a
        transaction (full-text indexes, ALTER DATABASE) are not supported in
        migrations. Adding an opt-out needs an approved decision first.

        Reason: Keep every migration atomic with its history row.
    */
    /// <summary>
    /// Runs one migration and records it in the same transaction. For the
    /// baseline, a second row records the last migration it replaces, so a
    /// database built from the baseline passes the same check as one that
    /// ran those migrations.
    /// </summary>
    private static void Apply( SqlConnection connection, SqlMigrationDescriptor descriptor, SqlMigrationRunnerOptions options, bool isBaseline = false )
    {
        var stopwatch = Stopwatch.StartNew();
        var transaction = connection.BeginTransaction();

        try
        {
            // Database access that bypasses this transaction (a RockContext,
            // DbService) throws while the migration is created and while Up()
            // runs, so field initializers and constructors are covered too.
            // See MigrationExecutionScope.
            using ( MigrationExecutionScope.Begin( descriptor.FullName ) )
            {
                var migration = descriptor.Create();
                migration.Connection = connection;
                migration.Transaction = transaction;

                migration.Up();
            }

            InsertHistory( connection, transaction, descriptor.Id, descriptor.TargetVersion, options.RockVersion.ToString(), descriptor.Name, ( int ) stopwatch.ElapsedMilliseconds );

            if ( isBaseline )
            {
                InsertHistory( connection, transaction, Migration.BaselineIncludesThroughMigrationId, null, options.RockVersion.ToString(), "IncludedInBaseline", null );
            }

            transaction.Commit();
        }
        catch ( Exception ex )
        {
            RollbackQuietly( transaction );

            throw new SqlMigrationException( $"## Migration error occurred in {descriptor.Id}, {descriptor.FullName}##", ex );
        }
        finally
        {
            transaction?.Dispose();
        }

        options.Log( $"Applied migration {descriptor.Id} {descriptor.Name} in {stopwatch.ElapsedMilliseconds} ms." );
    }

    /// <summary>
    /// Rolls back a transaction that may already be finished.
    /// </summary>
    private static void RollbackQuietly( SqlTransaction transaction )
    {
        try
        {
            transaction?.Rollback();
        }
        catch ( InvalidOperationException )
        {
            // Intentionally ignored: SQL Server already rolled the transaction
            // back (or it committed), and the original exception is the one to report.
        }
        catch ( SqlException )
        {
            // Intentionally ignored for the same reason.
        }
    }

    #endregion Applying

    #region Helpers

    /// <summary>
    /// Runs a statement on the runner's connection, outside any migration.
    /// </summary>
    /// <param name="connection">The runner's open connection.</param>
    /// <param name="transaction">The transaction to run in, or <c>null</c> for none.</param>
    /// <param name="sql">The SQL.</param>
    private static void ExecuteNonQuery( SqlConnection connection, SqlTransaction transaction, string sql )
    {
        using var command = new SqlCommand( sql, connection, transaction );
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Runs a query on the runner's connection, outside any migration, and
    /// returns the first column of the first row.
    /// </summary>
    /// <param name="connection">The runner's open connection.</param>
    /// <param name="sql">The SQL.</param>
    /// <returns>The value, or <c>null</c> when there are no rows.</returns>
    private static object ExecuteScalar( SqlConnection connection, string sql )
    {
        using var command = new SqlCommand( sql, connection );
        return command.ExecuteScalar();
    }

    #endregion Helpers
}
