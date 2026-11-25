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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Data;
using Rock.Tests.Integration.TestFramework.Database;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Data.SqlMigrations;

/// <summary>
/// Runs the SQL migration runner against scratch databases.
/// </summary>
[TestClass]
public class SqlMigrationRunnerTests
{
    public TestContext TestContext { get; set; }

    /// <summary>
    /// Makes RockApp's database unusable for each test, so a test migration
    /// that reaches it outside the runner fails instead of using the shared
    /// test database. See <see cref="TestMigrationRunner"/>.
    /// </summary>
    private TestHelper.RockAppScope _rockAppScope;

    [TestInitialize]
    public void MakeRockAppDatabaseUnusable()
    {
        _rockAppScope = TestHelper.CreateScopedRockAppWithUnusableDatabase();
    }

    [TestCleanup]
    public void RestoreRockAppDatabase()
    {
        _rockAppScope?.Dispose();
    }

    #region Ordering and Set-Based Runs

    [TestMethod]
    public async Task Run_EmptyDatabase_RunsBaselineThenVersionThenIdOrder()
    {
        using var database = await ScratchDatabase.CreateAsync();
        var result = SqlMigrationRunner.Run( Options( database, typeof( V21First ), typeof( V20Second ), typeof( V19Fix ), typeof( TestBaseline ), typeof( V20First ) ) );

        Assert.IsTrue( result.IsBaselineRun );
        CollectionAssert.AreEqual( new[] { "TestBaseline", "V19Fix", "V20First", "V20Second", "V21First" }, result.AppliedMigrationNames );
        CollectionAssert.AreEqual( new[]
        {
            "20131125173405 TestBaseline 19.7 21.0",
            "20260512231528 IncludedInBaseline (null) 21.0",
            "20260601000000 V19Fix 19.1 21.0",
            "20250601000000 V20First 20.0 21.0",
            "20250701000000 V20Second 20.0 21.0",
            "20250101000000 V21First 21.0 21.0"
        }, ReadHistory( database ) );
    }

    [TestMethod]
    public async Task Run_MigrationMissingFromEarlierRun_IsAppliedNextRun()
    {
        using var database = await ScratchDatabase.CreateAsync();
        SqlMigrationRunner.Run( Options( database, typeof( TestBaseline ), typeof( V20First ), typeof( V21First ) ) );

        var secondRun = SqlMigrationRunner.Run( Options( database, typeof( TestBaseline ), typeof( V20First ), typeof( V20Second ), typeof( V21First ) ) );
        var thirdRun = SqlMigrationRunner.Run( Options( database, typeof( TestBaseline ), typeof( V20First ), typeof( V20Second ), typeof( V21First ) ) );

        CollectionAssert.AreEqual( new[] { "V20Second" }, secondRun.AppliedMigrationNames );
        CollectionAssert.AreEqual( new string[0], thirdRun.AppliedMigrationNames );
        Assert.AreEqual( "20250701000000 V20Second 20.0 21.0", ReadHistory( database ).Last() );
    }

    [TestMethod]
    public async Task Run_MigrationPrintsMessage_LogsItAsWarning()
    {
        using var database = await ScratchDatabase.CreateAsync();
        var log = new ConcurrentQueue<string>();
        var options = Options( database, typeof( TestBaseline ), typeof( PrintsMessage ) );
        options.Log = log.Enqueue;

        SqlMigrationRunner.Run( options );

        Assert.IsTrue( log.Contains( "WARNING: Hello from a migration." ), string.Join( Environment.NewLine, log ) );
    }

    [TestMethod]
    public async Task Run_NothingPending_LogsNothing()
    {
        using var database = await ScratchDatabase.CreateAsync();
        SqlMigrationRunner.Run( Options( database, typeof( TestBaseline ), typeof( V20First ) ) );

        var log = new ConcurrentQueue<string>();
        var options = Options( database, typeof( TestBaseline ), typeof( V20First ) );
        options.Log = log.Enqueue;

        SqlMigrationRunner.Run( options );

        Assert.AreEqual( 0, log.Count, string.Join( Environment.NewLine, log ) );
    }

    [TestMethod]
    public async Task Run_MinimumVersionAboveRunningVersion_LeavesMigrationPending()
    {
        using var database = await ScratchDatabase.CreateAsync();
        var result = SqlMigrationRunner.Run( Options( database, typeof( TestBaseline ), typeof( NeedsRock22 ) ) );

        CollectionAssert.AreEqual( new[] { "TestBaseline" }, result.AppliedMigrationNames );
    }

    #endregion Ordering and Set-Based Runs

    #region Failures

    [TestMethod]
    public async Task Run_MigrationFails_RollsBackItsWorkAndHistoryAndStops()
    {
        using var database = await ScratchDatabase.CreateAsync();
        var exception = Assert.ThrowsExactly<SqlMigrationException>( () => SqlMigrationRunner.Run( Options( database, typeof( TestBaseline ), typeof( CreatesTableThenFails ), typeof( V21First ) ) ) );

        StringAssert.Contains( exception.Message, "## Migration error occurred in 20250201000000, Rock.Tests.Integration.Data.SqlMigrations.SqlMigrationRunnerTests+CreatesTableThenFails##" );
        Assert.AreEqual( 0, Scalar( database, "SELECT COUNT(*) FROM [sys].[tables] WHERE [name] = 'CreatedBeforeFailure'" ) );
        CollectionAssert.AreEqual( new[] { "20131125173405 TestBaseline 19.7 21.0", "20260512231528 IncludedInBaseline (null) 21.0" }, ReadHistory( database ) );
    }

    [TestMethod]
    public async Task Run_DuplicateIds_FailsBeforeRunningAnything()
    {
        using var database = await ScratchDatabase.CreateAsync();
        var exception = Assert.ThrowsExactly<SqlMigrationException>( () => SqlMigrationRunner.Run( Options( database, typeof( TestBaseline ), typeof( V20First ), typeof( DuplicateOfV20First ) ) ) );

        StringAssert.Contains( exception.Message, "SqlMigrationRunnerTests+V20First and Rock.Tests.Integration.Data.SqlMigrations.SqlMigrationRunnerTests+DuplicateOfV20First share the ID 20250601000000" );
        Assert.AreEqual( 0, Scalar( database, "SELECT COUNT(*) FROM [sys].[tables]" ) );
    }

    [TestMethod]
    public async Task Run_NoBaseline_FailsBeforeRunningAnything()
    {
        using var database = await ScratchDatabase.CreateAsync();
        var exception = Assert.ThrowsExactly<SqlMigrationException>( () => SqlMigrationRunner.Run( Options( database, typeof( V20First ) ) ) );

        StringAssert.Contains( exception.Message, "No baseline migration (ID 20131125173405) was found" );
        Assert.AreEqual( 0, Scalar( database, "SELECT COUNT(*) FROM [sys].[tables]" ) );
    }

    #endregion Failures

    #region Transactions

    [TestMethod]
    public async Task Run_Migration_RunsInsideATransaction()
    {
        using var database = await ScratchDatabase.CreateAsync();
        TransactionCountRecorder.Counts.Clear();

        SqlMigrationRunner.Run( Options( database, typeof( TestBaseline ), typeof( RecordsTransactionCount ) ) );

        Assert.AreEqual( 1, TransactionCountRecorder.Counts["RecordsTransactionCount"] );
        CollectionAssert.AreEqual( new[]
        {
            "20131125173405 TestBaseline 19.7 21.0",
            "20260512231528 IncludedInBaseline (null) 21.0",
            "20250301000000 RecordsTransactionCount 20.0 21.0"
        }, ReadHistory( database ) );
    }

    [TestMethod]
    public async Task SqlScalar_AfterEarlierStatement_SeesItsChanges()
    {
        using var database = await ScratchDatabase.CreateAsync();
        TransactionCountRecorder.Counts.Clear();

        SqlMigrationRunner.Run( Options( database, typeof( TestBaseline ), typeof( ReadsItsOwnWrite ) ) );

        Assert.AreEqual( 42, TransactionCountRecorder.Counts["ReadsItsOwnWrite"] );
    }

    [TestMethod]
    public async Task Sql_ValueWithGoLine_IsNotSplit()
    {
        using var database = await ScratchDatabase.CreateAsync();

        SqlMigrationRunner.Run( Options( database, typeof( TestBaseline ), typeof( StoresGoLine ) ) );

        // A split would cut the string literal and fail with "Unclosed quotation mark".
        Assert.AreEqual( 1, Scalar( database, "SELECT COUNT(*) FROM [dbo].[GoValue] WHERE [Value] = N'<button>' + NCHAR(13) + NCHAR(10) + N'Go' + NCHAR(13) + NCHAR(10) + N'</button>'" ) );
    }

    [TestMethod]
    public async Task SqlResource_EmbeddedScripts_RunsUnnamedAndNamedScriptsInOrder()
    {
        using var database = await ScratchDatabase.CreateAsync();
        SqlMigrationRunner.Run( Options( database, typeof( TestBaseline ), typeof( RunsScripts ) ) );

        Assert.AreEqual( 2, Scalar( database, "SELECT COUNT(*) FROM [dbo].[ScriptedRow]" ) );
        Assert.AreEqual( 1, Scalar( database, "SELECT COUNT(*) FROM [dbo].[ScriptedRow] WHERE [Source] = N'Second_Part'" ) );
    }

    [TestMethod]
    public async Task BulkResource_EmbeddedJson_LoadsEveryValueKeepingIdentities()
    {
        using var database = await ScratchDatabase.CreateAsync();
        SqlMigrationRunner.Run( Options( database, typeof( TestBaseline ), typeof( RunsBulkLoad ) ) );

        Assert.AreEqual( 3, Scalar( database, "SELECT COUNT(*) FROM [dbo].[BulkRow]" ) );
        Assert.AreEqual( 1, Scalar( database, @"SELECT COUNT(*) FROM [dbo].[BulkRow]
WHERE [Id] = 5 AND [Name] = N'Comma, ""quoted"" Ünïcödé' AND [Code] = 'M' AND [Amount] = 1.50 AND [IsActive] = 1
    AND [Created] = '2026-10-28T13:45:00.1234567' AND [Data] = 0x01FF AND [Reference] = '6F9619FF-8B86-D011-B42D-00C04FC964FF'
    AND [Shape].STEquals( geography::Point( 33.4484, -112.074, 4326 ) ) = 1 AND [Shape].STSrid = 4326 AND [Doubled] = 10" ), "Quotes, commas, Unicode, geography and every other type load exactly." );
        Assert.AreEqual( 1, Scalar( database, @"SELECT COUNT(*) FROM [dbo].[BulkRow]
WHERE [Id] = 7 AND [Name] = N'' AND [Code] IS NULL AND [Amount] IS NULL AND [IsActive] = 0 AND [Created] IS NULL AND [Data] IS NULL AND [Reference] IS NULL AND [Shape] IS NULL" ), "An empty string stays empty; null is NULL." );
        Assert.AreEqual( 1, Scalar( database, "SELECT COUNT(*) FROM [dbo].[BulkRow] WHERE [Id] = 9 AND [Name] = N'Line one' + NCHAR(13) + NCHAR(10) + N'line two'" ), "An escaped line break loads as one." );
        Assert.AreEqual( 3, Scalar( database, "SELECT COUNT(DISTINCT [Guid]) FROM [dbo].[BulkRow] WHERE [Guid] <> '00000000-0000-0000-0000-000000000000'" ), "generateGuid gives every row a new Guid." );
        Assert.AreEqual( 0, Scalar( database, "SELECT COUNT(*) FROM [sys].[check_constraints] WHERE [is_not_trusted] = 1" ), "The load checks CHECK constraints, so they stay trusted." );
        Assert.AreEqual( 10, Scalar( database, "INSERT INTO [dbo].[BulkRow] ( [Guid] ) VALUES ( NEWID() ); SELECT CONVERT( INT, SCOPE_IDENTITY() );" ), "Identity values are kept, and the next one follows them." );
    }

    [TestMethod]
    public async Task BulkLoad_FileThatDoesNotMatchTheTable_FailsNamingTheProblem()
    {
        using var database = await ScratchDatabase.CreateAsync( "CREATE TABLE [dbo].[Lookup] ( [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, [Name] NVARCHAR(50) NULL, [Guid] UNIQUEIDENTIFIER NOT NULL );" );
        using var connection = new SqlConnection( database.ConnectionString );
        connection.Open();

        var cases = new[]
        {
            ( Json: "{ \"columns\": [ \"Id\", \"Name\" ], \"rows\": [ [ 1, \"a\" ] ] }", Message: "has no values for Guid" ),
            ( Json: "{ \"generateGuid\": true, \"columns\": [ \"Id\", \"Name\", \"Guid\" ], \"rows\": [] }", Message: "sets generateGuid but also has a Guid column" ),
            ( Json: "{ \"generateGuid\": true, \"columns\": [ \"Id\", \"Other\" ], \"rows\": [] }", Message: "names column 'Other'" ),
            ( Json: "{ \"generateGuid\": true, \"columns\": [ \"Id\", \"Name\" ], \"rows\": [ [ 1 ] ] }", Message: "has 1 values, but there are 2 columns" ),
            ( Json: "{ \"generateGuid\": true, \"columns\": [ \"Id\", \"Name\" ], \"rows\": [ [ \"one\", \"a\" ] ] }", Message: "has a value for Id that is not a valid Int32" )
        };

        foreach ( var (json, message) in cases )
        {
            var exception = Assert.ThrowsExactly<System.IO.InvalidDataException>( () => MigrationBulkLoader.Load( connection, null, "Lookup", System.Text.Encoding.UTF8.GetBytes( json ), 30 ), json );

            StringAssert.Contains( exception.Message, message, json );
        }
    }

    [TestMethod]
    public async Task SqlResource_MissingScript_FailsNamingTheExpectedFile()
    {
        using var database = await ScratchDatabase.CreateAsync();
        var exception = Assert.ThrowsExactly<SqlMigrationException>( () => SqlMigrationRunner.Run( Options( database, typeof( TestBaseline ), typeof( RunsMissingScript ) ) ) );

        StringAssert.Contains( exception.InnerException.Message, "could not find its script '20250902000000-NotThere.sql'" );
    }

    [TestMethod]
    public void SplitBatches_ScriptWithGoLines_SplitsOnlyOnWholeLineGo()
    {
        var batches = Migration.SplitBatches( "SELECT 1\r\nGO\r\nSELECT 'GO'\r\n  go  \r\nSELECT 2 -- GOOD\nGO\n" );

        CollectionAssert.AreEqual( new[] { "SELECT 1\r\n", "\nSELECT 'GO'\r\n", "\nSELECT 2 -- GOOD\n" }, batches );
    }

    #endregion Transactions

    #region History Import and Baseline Check

    /// <summary>
    /// EF6 history for a database at the baseline's version, plus one
    /// converted v20 migration that already ran through EF6, inserted out
    /// of order. CreateDatabase shares the baseline's ID, and
    /// 202605122315282 is the last migration the baseline includes.
    /// </summary>
    private const string Ef6DatabaseAtBaseline = @"
CREATE TABLE [dbo].[Person] ( [Id] INT NOT NULL );
CREATE TABLE [dbo].[__MigrationHistory] ( [MigrationId] NVARCHAR(150) NOT NULL, [ContextKey] NVARCHAR(300) NOT NULL );
INSERT INTO [dbo].[__MigrationHistory] ( [MigrationId], [ContextKey] ) VALUES
    ( N'202506010000001_V20First', N'Rock.Migrations.Configuration' ),
    ( N'202605122315282_UpdateExistingToCoreAzureBlobStorageProvider', N'Rock.Migrations.Configuration' ),
    ( N'201311251734059_CreateDatabase', N'Rock.Migrations.Configuration' ),
    ( N'202401010000001_BeforeBaseline', N'Rock.Migrations.Configuration' );";

    [TestMethod]
    public async Task Run_Ef6DatabaseAtBaseline_ImportsHistoryOnceWithoutRunningBaseline()
    {
        using var database = await ScratchDatabase.CreateAsync( Ef6DatabaseAtBaseline );
        var firstRun = SqlMigrationRunner.Run( Options( database, typeof( TestBaseline ), typeof( V20First ), typeof( V21First ) ) );
        var secondRun = SqlMigrationRunner.Run( Options( database, typeof( TestBaseline ), typeof( V20First ), typeof( V21First ) ) );

        Assert.AreEqual( 4, firstRun.ImportedCount );
        Assert.IsFalse( firstRun.IsBaselineRun );
        Assert.IsFalse( firstRun.IsBaselineRecorded );
        CollectionAssert.AreEqual( new[] { "V21First" }, firstRun.AppliedMigrationNames );
        Assert.AreEqual( 0, secondRun.ImportedCount );
        CollectionAssert.AreEqual( new string[0], secondRun.AppliedMigrationNames );
        Assert.AreEqual( 0, Scalar( database, "SELECT COUNT(*) FROM [sys].[tables] WHERE [name] = 'BaselineMarker'" ) );
        CollectionAssert.AreEqual( new[]
        {
            "20131125173405 CreateDatabase 19.7 (null)",
            "20240101000000 BeforeBaseline (null) (null)",
            "20250601000000 V20First 20.0 (null)",
            "20260512231528 UpdateExistingToCoreAzureBlobStorageProvider (null) (null)",
            "20250101000000 V21First 21.0 21.0"
        }, ReadHistory( database ) );
    }

    [TestMethod]
    public async Task Run_Ef6HistoryOfAnotherContext_IsNotImported()
    {
        // A plugin's own EF6 migration whose ID truncates to V21First's ID.
        var pluginHistory = @"
INSERT INTO [dbo].[__MigrationHistory] ( [MigrationId], [ContextKey] ) VALUES
    ( N'202501010000005_PluginTable', N'Acme.Plugin.Migrations.Configuration' );";
        using var database = await ScratchDatabase.CreateAsync( Ef6DatabaseAtBaseline + pluginHistory );

        var result = SqlMigrationRunner.Run( Options( database, typeof( TestBaseline ), typeof( V20First ), typeof( V21First ) ) );

        Assert.AreEqual( 4, result.ImportedCount );
        CollectionAssert.AreEqual( new[] { "V21First" }, result.AppliedMigrationNames, "The Rock migration ran instead of being taken as the plugin's." );
        Assert.IsFalse( ReadHistory( database ).Any( row => row.Contains( " PluginTable " ) ), "The plugin's history row was not imported." );
    }

    [TestMethod]
    public async Task Run_Ef6DatabaseOlderThanBaseline_StopsBeforeRunningAnything()
    {
        var olderDatabase = Ef6DatabaseAtBaseline.Replace( "( N'202605122315282_UpdateExistingToCoreAzureBlobStorageProvider', N'Rock.Migrations.Configuration' ),", string.Empty );

        using var database = await ScratchDatabase.CreateAsync( olderDatabase );
        var exception = Assert.ThrowsExactly<SqlMigrationException>( () => SqlMigrationRunner.Run( Options( database, typeof( TestBaseline ), typeof( V20First ), typeof( V21First ) ) ) );

        StringAssert.Contains( exception.Message, "This database is older than this version of Rock can upgrade. It has not run migration 20260512231528, the last one included in the Rock 19.7 baseline" );
        Assert.AreEqual( 0, Scalar( database, "SELECT COUNT(*) FROM [dbo].[RockMigration] WHERE [MigrationName] = 'V21First'" ) );
        Assert.AreEqual( 0, Scalar( database, "SELECT COUNT(*) FROM [sys].[tables] WHERE [name] = 'BaselineMarker'" ) );
    }

    [TestMethod]
    public async Task Run_RunnerBuiltDatabaseMissingIncludedMigration_Stops()
    {
        using var database = await ScratchDatabase.CreateAsync();
        SqlMigrationRunner.Run( Options( database, typeof( TestBaseline ), typeof( V20First ) ) );
        database.Execute( "DELETE FROM [dbo].[RockMigration] WHERE [MigrationId] = 20260512231528;" );

        var exception = Assert.ThrowsExactly<SqlMigrationException>( () => SqlMigrationRunner.Run( Options( database, typeof( TestBaseline ), typeof( V20First ), typeof( V21First ) ) ) );

        StringAssert.Contains( exception.Message, "It has not run migration 20260512231528" );
        Assert.AreEqual( 0, Scalar( database, "SELECT COUNT(*) FROM [dbo].[RockMigration] WHERE [MigrationName] = 'V21First'" ) );
    }

    [TestMethod]
    public async Task Run_DatabaseWithPersonTableButNoHistory_Stops()
    {
        using var database = await ScratchDatabase.CreateAsync( "CREATE TABLE [dbo].[Person] ( [Id] INT NOT NULL );" );
        var exception = Assert.ThrowsExactly<SqlMigrationException>( () => SqlMigrationRunner.Run( Options( database, typeof( TestBaseline ) ) ) );

        StringAssert.Contains( exception.Message, "The database has Rock tables but no migration history" );
        Assert.AreEqual( 0, Scalar( database, "SELECT COUNT(*) FROM [sys].[tables] WHERE [name] = 'BaselineMarker'" ) );
    }

    /// <summary>
    /// Plugin migration history where HotFix 320 already ran, and a plugin
    /// assembly other than Rock ran its own migration 321.
    /// </summary>
    private const string HotFixHistory = @"
CREATE TABLE [dbo].[PluginMigration] ( [Id] INT IDENTITY(1,1) NOT NULL, [PluginAssemblyName] NVARCHAR(512) NOT NULL, [MigrationNumber] INT NOT NULL, [MigrationName] NVARCHAR(100) NOT NULL );
INSERT INTO [dbo].[PluginMigration] ( [PluginAssemblyName], [MigrationNumber], [MigrationName] ) VALUES
    ( N'Rock', 320, N'MigrationRollupsForV21_0' ),
    ( N'com.example.Plugin', 321, N'SomethingElse' );";

    [TestMethod]
    public async Task Run_HotFixAlreadyRan_RecordsItsReplacementWithoutRunningIt()
    {
        using var database = await ScratchDatabase.CreateAsync( Ef6DatabaseAtBaseline + HotFixHistory );
        var log = new ConcurrentQueue<string>();
        var options = Options( database, typeof( TestBaseline ), typeof( V20First ), typeof( ReplacesHotFix320 ), typeof( ReplacesHotFix321 ) );
        options.Log = log.Enqueue;

        var firstRun = SqlMigrationRunner.Run( options );
        var secondRun = SqlMigrationRunner.Run( Options( database, typeof( TestBaseline ), typeof( V20First ), typeof( ReplacesHotFix320 ), typeof( ReplacesHotFix321 ) ) );

        Assert.AreEqual( 1, firstRun.ImportedHotFixCount );
        Assert.IsTrue( log.Contains( "Recorded 1 migrations whose HotFix already ran." ), string.Join( Environment.NewLine, log ) );
        CollectionAssert.AreEqual( new[] { "ReplacesHotFix321" }, firstRun.AppliedMigrationNames, "Only Rock's own HotFix numbers count." );
        Assert.AreEqual( 0, Scalar( database, "SELECT COUNT(*) FROM [sys].[tables] WHERE [name] = 'HotFix320Ran'" ) );
        Assert.AreEqual( 1, Scalar( database, "SELECT COUNT(*) FROM [sys].[tables] WHERE [name] = 'HotFix321Ran'" ) );
        Assert.AreEqual( 0, secondRun.ImportedHotFixCount );
        CollectionAssert.AreEqual( new string[0], secondRun.AppliedMigrationNames );
        CollectionAssert.AreEqual( new[]
        {
            "20260910160347 ReplacesHotFix320 21.0 (null)",
            "20260911123830 ReplacesHotFix321 21.0 21.0"
        }, ReadHistory( database ).Skip( 4 ).ToList() );
    }

    [TestMethod]
    public async Task Run_EmptyDatabase_RunsHotFixReplacement()
    {
        using var database = await ScratchDatabase.CreateAsync();
        var result = SqlMigrationRunner.Run( Options( database, typeof( TestBaseline ), typeof( ReplacesHotFix320 ) ) );

        Assert.AreEqual( 0, result.ImportedHotFixCount );
        CollectionAssert.AreEqual( new[] { "TestBaseline", "ReplacesHotFix320" }, result.AppliedMigrationNames );
    }

    [TestMethod]
    public async Task Run_Ef6DatabaseOlderThanBaselineWithHotFix_RecordsNothingForIt()
    {
        var olderDatabase = Ef6DatabaseAtBaseline.Replace( "( N'202605122315282_UpdateExistingToCoreAzureBlobStorageProvider', N'Rock.Migrations.Configuration' ),", string.Empty ) + HotFixHistory;

        using var database = await ScratchDatabase.CreateAsync( olderDatabase );
        Assert.ThrowsExactly<SqlMigrationException>( () => SqlMigrationRunner.Run( Options( database, typeof( TestBaseline ), typeof( ReplacesHotFix320 ) ) ) );

        Assert.AreEqual( 0, Scalar( database, "SELECT COUNT(*) FROM [dbo].[RockMigration] WHERE [MigrationName] = 'ReplacesHotFix320'" ) );
    }

    [TestMethod]
    public async Task Run_TwoMigrationsReplaceOneHotFix_FailsBeforeRunningAnything()
    {
        using var database = await ScratchDatabase.CreateAsync();
        var exception = Assert.ThrowsExactly<SqlMigrationException>( () => SqlMigrationRunner.Run( Options( database, typeof( TestBaseline ), typeof( ReplacesHotFix320 ), typeof( AlsoReplacesHotFix320 ) ) ) );

        StringAssert.Contains( exception.Message, "both replace HotFix 320" );
        Assert.AreEqual( 0, Scalar( database, "SELECT COUNT(*) FROM [sys].[tables]" ) );
    }

    #endregion History Import and Baseline Check

    #region Database Access Guard

    /*
        These tests only construct contexts and never query, so they pass or
        fail on the guard alone, not on the test database. There is no test of
        a cache lookup: whether one reaches the database depends on what is
        already cached, so it would prove nothing either way. Every cache miss
        builds a RockContext, which the tests below cover.
    */

    [TestMethod]
    public async Task Run_MigrationCreatesRockContext_FailsNamingItAndRollsBack()
    {
        using var database = await ScratchDatabase.CreateAsync();

        var exception = Assert.ThrowsExactly<SqlMigrationException>( () => SqlMigrationRunner.Run( Options( database, typeof( TestBaseline ), typeof( CreatesRockContext ), typeof( V21First ) ) ) );

        StringAssert.Contains( exception.Message, "## Migration error occurred in 20250501000000, Rock.Tests.Integration.Data.SqlMigrations.SqlMigrationRunnerTests+CreatesRockContext##" );
        Assert.IsInstanceOfType<InvalidOperationException>( exception.InnerException );
        StringAssert.Contains( exception.InnerException.Message, "SqlMigrationRunnerTests+CreatesRockContext created a RockContext, which opens a database connection outside the migration's transaction" );
        Assert.AreEqual( 0, Scalar( database, "SELECT COUNT(*) FROM [sys].[tables] WHERE [name] = 'CreatedBeforeRockContext'" ), "The migration's own change was rolled back." );
        CollectionAssert.AreEqual( new[] { "20131125173405 TestBaseline 19.7 21.0", "20260512231528 IncludedInBaseline (null) 21.0" }, ReadHistory( database ), "Neither it nor the next migration was recorded." );
        AssertScopeEnded();
    }

    [TestMethod]
    public async Task Run_MigrationReachesDatabaseIndirectly_Fails()
    {
        var cases = new[]
        {
            ( Migration: typeof( CallsCreateRockContext ), Access: "created a RockContext" ),
            ( Migration: typeof( CreatesRockContextInFieldInitializer ), Access: "created a RockContext" ),
            ( Migration: typeof( CallsDbService ), Access: "used DbService" )
        };

        foreach ( var (migration, access) in cases )
        {
            using var database = await ScratchDatabase.CreateAsync();

            var exception = Assert.ThrowsExactly<SqlMigrationException>( () => SqlMigrationRunner.Run( Options( database, typeof( TestBaseline ), migration ) ) );

            StringAssert.Contains( exception.InnerException?.Message, $"{migration.Name} {access}", migration.Name );
            AssertScopeEnded();
        }
    }

    [TestMethod]
    public async Task Run_MigrationStartsTaskThatCreatesRockContext_Fails()
    {
        using var database = await ScratchDatabase.CreateAsync();

        var exception = Assert.ThrowsExactly<SqlMigrationException>( () => SqlMigrationRunner.Run( Options( database, typeof( TestBaseline ), typeof( StartsTaskThatCreatesRockContext ) ) ) );

        StringAssert.Contains( exception.InnerException?.Message, "StartsTaskThatCreatesRockContext created a RockContext" );
        AssertScopeEnded();
    }

    [TestMethod]
    public async Task Run_OtherThreadDuringMigration_CanStillCreateRockContext()
    {
        using var database = await ScratchDatabase.CreateAsync();

        GuardSignals.MigrationStarted.Reset();
        GuardSignals.OtherThreadDone.Reset();

        var run = Task.Run( () => SqlMigrationRunner.Run( Options( database, typeof( TestBaseline ), typeof( WaitsForOtherThread ) ) ) );
        Exception otherThreadException = null;

        Assert.IsTrue( GuardSignals.MigrationStarted.Wait( TimeSpan.FromSeconds( 60 ) ), "The migration started." );

        try
        {
            using var rockContext = new RockContext();
        }
        catch ( Exception ex )
        {
            otherThreadException = ex;
        }
        finally
        {
            GuardSignals.OtherThreadDone.Set();
        }

        var result = await run;

        Assert.IsNull( otherThreadException, otherThreadException?.Message );
        CollectionAssert.AreEqual( new[] { "TestBaseline", "WaitsForOtherThread" }, result.AppliedMigrationNames );
        AssertScopeEnded();
    }

    /// <summary>
    /// Asserts that no migration scope is left behind, so a RockContext can
    /// be created again after a run that failed or succeeded.
    /// </summary>
    private static void AssertScopeEnded()
    {
        Assert.IsNull( MigrationExecutionScope.CurrentMigration );

        using var rockContext = new RockContext();
    }

    #endregion Database Access Guard

    #region Database Creation

    [TestMethod]
    public async Task Run_MissingDatabaseInDevelopment_CreatesItAndRunsBaseline()
    {
        using var database = await ScratchDatabase.ReserveAsync();
        var options = Options( database, typeof( TestBaseline ) );
        options.IsDevelopmentEnvironment = true;

        var result = SqlMigrationRunner.Run( options );

        Assert.IsTrue( result.IsBaselineRun );
    }

    [TestMethod]
    public async Task Run_MissingDatabaseOutsideDevelopment_Fails()
    {
        using var database = await ScratchDatabase.ReserveAsync();
        var exception = Assert.ThrowsExactly<SqlMigrationException>( () => SqlMigrationRunner.Run( Options( database, typeof( TestBaseline ) ) ) );

        StringAssert.Contains( exception.Message, $"Cannot open the database '{database.Name}'" );
    }

    #endregion Database Creation

    #region Lock

    [TestMethod]
    public async Task Run_TwoRunnersAtOnce_SecondWaitsThenFindsNothingPending()
    {
        using var database = await ScratchDatabase.CreateAsync();
        var firstLog = new ConcurrentQueue<string>();
        var secondLog = new ConcurrentQueue<string>();
        var firstOptions = Options( database, typeof( TestBaseline ), typeof( SlowMigration ) );
        var secondOptions = Options( database, typeof( TestBaseline ), typeof( SlowMigration ) );
        firstOptions.Log = firstLog.Enqueue;
        secondOptions.Log = secondLog.Enqueue;

        var firstRun = Task.Run( () => SqlMigrationRunner.Run( firstOptions ) );
        await Task.Delay( 1000 );
        var secondRun = Task.Run( () => SqlMigrationRunner.Run( secondOptions ) );

        var results = await Task.WhenAll( firstRun, secondRun );

        CollectionAssert.AreEqual( new[] { "TestBaseline", "SlowMigration" }, results[0].AppliedMigrationNames );
        CollectionAssert.AreEqual( new string[0], results[1].AppliedMigrationNames );
        Assert.IsTrue( secondLog.Any( m => m.StartsWith( "Waiting up to 60 seconds" ) ), string.Join( Environment.NewLine, secondLog ) );
        CollectionAssert.AreEqual( new[] { "20131125173405 TestBaseline 19.7 21.0", "20260512231528 IncludedInBaseline (null) 21.0", "20250401000000 SlowMigration 20.0 21.0" }, ReadHistory( database ) );
    }

    [TestMethod]
    public async Task Run_LockHeldPastTimeout_FailsNamingTheHolder()
    {
        using var database = await ScratchDatabase.CreateAsync();
        using var holder = new SqlConnection( new SqlConnectionStringBuilder( database.ConnectionString ) { ApplicationName = "Lock Holder Test" }.ConnectionString );
        holder.Open();

        using var command = new SqlCommand( "EXEC [sp_getapplock] @Resource = 'Rock Migrations', @LockMode = 'Exclusive', @LockOwner = 'Session';", holder );
        command.ExecuteNonQuery();

        var log = new ConcurrentQueue<string>();
        var options = Options( database, typeof( TestBaseline ) );
        options.LockTimeout = TimeSpan.FromSeconds( 2 );
        options.Log = log.Enqueue;

        var exception = Assert.ThrowsExactly<SqlMigrationException>( () => SqlMigrationRunner.Run( options ) );

        StringAssert.Contains( exception.Message, "Timed out after 2 seconds waiting for the database migration lock" );
        StringAssert.Contains( exception.Message, "program 'Lock Holder Test'" );
        Assert.IsTrue( log.Any( m => m.StartsWith( "Waiting up to 2 seconds" ) && m.Contains( "program 'Lock Holder Test'" ) ), string.Join( Environment.NewLine, log ) );
        Assert.AreEqual( 0, Scalar( database, "SELECT COUNT(*) FROM [sys].[tables] WHERE [name] = 'Person'" ) );
    }

    #endregion Lock

    #region Timing

    [TestMethod]
    public async Task Run_ManyEmptyMigrations_RecordsEachAndReportsOverhead()
    {
        foreach ( var count in new[] { 1, 50, 500 } )
        {
            using var database = await ScratchDatabase.CreateAsync();
            var migrations = new List<SqlMigrationDescriptor> { SqlMigrationDescriptor.FromType( typeof( TestBaseline ) ) };
            migrations.AddRange( Enumerable.Range( 1, count ).Select( i => new SqlMigrationDescriptor
            {
                Id = 20260101000000 + i,
                TargetVersion = "21.0",
                Name = $"Empty{i}",
                FullName = $"Empty{i}",
                Create = () => new EmptyMigration()
            } ) );

            var options = Options( database );
            options.Migrations = migrations;

            var stopwatch = Stopwatch.StartNew();
            var result = SqlMigrationRunner.Run( options );
            stopwatch.Stop();

            Assert.AreEqual( count + 1, result.AppliedMigrationNames.Count );
            TestContext.WriteLine( $"{count} empty migrations: {stopwatch.ElapsedMilliseconds} ms total, {stopwatch.Elapsed.TotalMilliseconds / ( count + 1 ):0.00} ms per migration including the baseline." );
        }
    }

    #endregion Timing

    #region Support

    private static SqlMigrationRunnerOptions Options( ScratchDatabase database, params Type[] migrationTypes )
    {
        return new SqlMigrationRunnerOptions
        {
            ConnectionString = database.ConnectionString,
            RockVersion = new Version( 21, 0 ),
            Migrations = migrationTypes.Select( SqlMigrationDescriptor.FromType ).ToList()
        };
    }

    /// <summary>
    /// Reads the history table in install order as
    /// <c>MigrationId MigrationName TargetVersion AppliedVersion</c> lines.
    /// </summary>
    private static List<string> ReadHistory( ScratchDatabase database )
    {
        var rows = new List<string>();

        using var connection = new SqlConnection( database.ConnectionString );
        using var command = new SqlCommand( "SELECT [MigrationId], [MigrationName], ISNULL( [TargetVersion], '(null)' ), ISNULL( [AppliedVersion], '(null)' ) FROM [dbo].[RockMigration] ORDER BY [Id];", connection );
        connection.Open();

        using var reader = command.ExecuteReader();
        while ( reader.Read() )
        {
            rows.Add( $"{reader.GetInt64( 0 )} {reader.GetString( 1 )} {reader.GetString( 2 )} {reader.GetString( 3 )}" );
        }

        return rows;
    }

    private static int Scalar( ScratchDatabase database, string sql )
    {
        using var connection = new SqlConnection( database.ConnectionString );
        using var command = new SqlCommand( sql, connection );
        connection.Open();

        return ( int ) command.ExecuteScalar();
    }

    private static class TransactionCountRecorder
    {
        public static readonly ConcurrentDictionary<string, int> Counts = new ConcurrentDictionary<string, int>();
    }

    /// <summary>
    /// Lets a test act on another thread while a migration is running.
    /// </summary>
    private static class GuardSignals
    {
        public static readonly ManualResetEventSlim MigrationStarted = new ManualResetEventSlim();

        public static readonly ManualResetEventSlim OtherThreadDone = new ManualResetEventSlim();
    }

    [SqlMigration( 20250501000000, "20.0" )]
    private sealed class CreatesRockContext : Migration
    {
        public override void Up()
        {
            Sql( "CREATE TABLE [dbo].[CreatedBeforeRockContext] ( [Id] INT NOT NULL );" );

            using var rockContext = new RockContext();
        }
    }

    [SqlMigration( 20250502000000, "20.0" )]
    private sealed class CallsCreateRockContext : Migration
    {
        public override void Up()
        {
            using var rockContext = RockApp.Current.CreateRockContext();
        }
    }

    /// <summary>
    /// Reaches the database while it is being created, before Up() runs.
    /// </summary>
    [SqlMigration( 20250502100000, "20.0" )]
    private sealed class CreatesRockContextInFieldInitializer : Migration
    {
        private readonly RockContext _rockContext = new RockContext();

        public override void Up()
        {
            _rockContext.Dispose();
        }
    }

    [SqlMigration( 20250302000000, "20.0" )]
    private sealed class StoresGoLine : Migration
    {
        public override void Up()
        {
            Sql( "CREATE TABLE [dbo].[GoValue] ( [Value] NVARCHAR(100) NOT NULL );\r\nINSERT INTO [dbo].[GoValue] ( [Value] ) VALUES ( N'<button>\r\nGo\r\n</button>' );" );
        }
    }

    [SqlMigration( 20250503000000, "20.0" )]
    private sealed class CallsDbService : Migration
    {
        public override void Up()
        {
            DbService.ExecuteScalar( "SELECT 1" );
        }
    }

    [SqlMigration( 20250504000000, "20.0" )]
    private sealed class StartsTaskThatCreatesRockContext : Migration
    {
        public override void Up()
        {
            Task.Run( () =>
            {
                using var rockContext = new RockContext();
            } ).GetAwaiter().GetResult();
        }
    }

    [SqlMigration( 20250505000000, "20.0" )]
    private sealed class WaitsForOtherThread : Migration
    {
        public override void Up()
        {
            GuardSignals.MigrationStarted.Set();

            if ( !GuardSignals.OtherThreadDone.Wait( TimeSpan.FromSeconds( 60 ) ) )
            {
                throw new TimeoutException( "The test never signaled from its other thread." );
            }
        }
    }

    [SqlMigration( Migration.BaselineMigrationId, "19.7" )]
    private sealed class TestBaseline : Migration
    {
        public override void Up()
        {
            Sql( "CREATE TABLE [dbo].[Person] ( [Id] INT NOT NULL ); CREATE TABLE [dbo].[BaselineMarker] ( [Id] INT NOT NULL );" );
        }
    }

    /// <summary>
    /// A 19.x fix absorbed after the squash: a lower target version than
    /// the baseline and a later ID than the v20 migrations.
    /// </summary>
    [SqlMigration( 20260601000000, "19.1" )]
    private sealed class V19Fix : Migration
    {
        public override void Up()
        {
            Sql( "SELECT 1;" );
        }
    }

    [SqlMigration( 20250601000000, "20.0" )]
    private sealed class V20First : Migration
    {
        public override void Up()
        {
            Sql( "SELECT 1;" );
        }
    }

    [SqlMigration( 20250601000000, "20.0" )]
    private sealed class DuplicateOfV20First : Migration
    {
        public override void Up()
        {
            Sql( "SELECT 1;" );
        }
    }

    [SqlMigration( 20250701000000, "20.0" )]
    private sealed class V20Second : Migration
    {
        public override void Up()
        {
            Sql( "SELECT 1;" );
        }
    }

    [SqlMigration( 20250101000000, "21.0" )]
    private sealed class V21First : Migration
    {
        public override void Up()
        {
            Sql( "SELECT 1;" );
        }
    }

    [SqlMigration( 20250103000000, "21.0" )]
    private sealed class PrintsMessage : Migration
    {
        public override void Up()
        {
            Sql( "PRINT N'Hello from a migration.';" );
        }
    }

    [SqlMigration( 20250102000000, "21.0", MinimumRockVersion = "22.0" )]
    private sealed class NeedsRock22 : Migration
    {
        public override void Up()
        {
            Sql( "SELECT 1;" );
        }
    }

    [SqlMigration( 20250201000000, "20.0" )]
    private sealed class CreatesTableThenFails : Migration
    {
        public override void Up()
        {
            Sql( "CREATE TABLE [dbo].[CreatedBeforeFailure] ( [Id] INT NOT NULL );" );
            Sql( "SELECT * FROM [dbo].[TableThatDoesNotExist];" );
        }
    }

    [SqlMigration( 20250301000000, "20.0" )]
    private sealed class RecordsTransactionCount : Migration
    {
        public override void Up()
        {
            TransactionCountRecorder.Counts[GetType().Name] = ( int ) SqlScalar( "SELECT @@TRANCOUNT;" );
        }
    }

    [SqlMigration( 20250303000000, "20.0" )]
    private sealed class ReadsItsOwnWrite : Migration
    {
        public override void Up()
        {
            Sql( "CREATE TABLE [dbo].[Scalar] ( [Value] INT NOT NULL ); INSERT INTO [dbo].[Scalar] ( [Value] ) VALUES ( 42 );" );
            TransactionCountRecorder.Counts[GetType().Name] = ( int ) SqlScalar( "SELECT [Value] FROM [dbo].[Scalar];" );
        }
    }

    [SqlMigration( 20250401000000, "20.0" )]
    private sealed class SlowMigration : Migration
    {
        public override void Up()
        {
            Sql( "WAITFOR DELAY '00:00:04';" );
        }
    }

    /// <summary>
    /// Runs Scripts\20250901000000.sql, then Scripts\20250901000000-Second_Part.sql.
    /// </summary>
    [SqlMigration( 20250901000000, "20.0" )]
    private sealed class RunsScripts : Migration
    {
        public override void Up()
        {
            SqlResource();
            SqlResource( "Second_Part" );
        }
    }

    /// <summary>
    /// Creates [BulkRow] with Scripts\20250903000000.sql, then loads
    /// Scripts\20250903000000-BulkRow.json into it.
    /// </summary>
    [SqlMigration( 20250903000000, "20.0" )]
    private sealed class RunsBulkLoad : Migration
    {
        public override void Up()
        {
            SqlResource();
            BulkResource( "BulkRow" );
        }
    }

    [SqlMigration( 20250902000000, "20.0" )]
    private sealed class RunsMissingScript : Migration
    {
        public override void Up()
        {
            SqlResource( "NotThere" );
        }
    }

    [SqlMigration( 20260910160347, "21.0", ReplacesHotFixNumber = 320 )]
    private sealed class ReplacesHotFix320 : Migration
    {
        public override void Up()
        {
            Sql( "CREATE TABLE [dbo].[HotFix320Ran] ( [Id] INT NOT NULL );" );
        }
    }

    [SqlMigration( 20260911123830, "21.0", ReplacesHotFixNumber = 321 )]
    private sealed class ReplacesHotFix321 : Migration
    {
        public override void Up()
        {
            Sql( "CREATE TABLE [dbo].[HotFix321Ran] ( [Id] INT NOT NULL );" );
        }
    }

    [SqlMigration( 20260912000000, "21.0", ReplacesHotFixNumber = 320 )]
    private sealed class AlsoReplacesHotFix320 : Migration
    {
        public override void Up()
        {
            Sql( "SELECT 1;" );
        }
    }

    private sealed class EmptyMigration : Migration
    {
        public override void Up()
        {
        }
    }

    #endregion Support
}
