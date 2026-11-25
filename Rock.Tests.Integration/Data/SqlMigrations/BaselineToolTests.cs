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
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.BaselineTool;
using Rock.Data;
using Rock.Tests.Integration.TestFramework.Database;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Data.SqlMigrations;

/// <summary>
/// Runs the baseline tool against scratch databases: export and package
/// a hand-made database, run the scripts into an empty one and prove the
/// two match; and clean up a hand-made settled database.
/// </summary>
[TestClass]
public class BaselineToolTests
{
    public TestContext TestContext { get; set; }

    #region Export and Package

    /// <summary>
    /// Every object kind the export has to order and reproduce, rows with
    /// awkward values, deleted rows that leave identities ahead of the
    /// data, and the history tables the export must leave out.
    /// </summary>
    private const string SourceDatabase = @"
CREATE TYPE [dbo].[IdList] AS TABLE ( [Id] INT NOT NULL );
GO
CREATE FUNCTION [dbo].[ufnDouble]( @Value INT ) RETURNS INT WITH SCHEMABINDING AS BEGIN RETURN @Value * 2; END
GO
CREATE TABLE [dbo].[Parent] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(50) NOT NULL,
    [Amount] DECIMAL(18,2) NULL,
    [Doubled] AS ( [dbo].[ufnDouble]( [Id] ) ),
    [Point] GEOGRAPHY NULL,
    [Data] VARBINARY(MAX) NULL,
    [IsActive] BIT NOT NULL CONSTRAINT [DF_Parent_IsActive] DEFAULT ((1)),
    [Guid] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_dbo.Parent] PRIMARY KEY CLUSTERED ( [Id] ),
    CONSTRAINT [CK_Parent_Amount] CHECK ( [Amount] >= 0 )
);
CREATE UNIQUE INDEX [IX_Guid] ON [dbo].[Parent] ( [Guid] );
CREATE INDEX [IX_Name] ON [dbo].[Parent] ( [Name] ) INCLUDE ( [Amount] ) WHERE [IsActive] = 1;
CREATE SPATIAL INDEX [IX_Point] ON [dbo].[Parent] ( [Point] );
CREATE TABLE [dbo].[Child] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [ParentId] INT NOT NULL,
    [Note] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_dbo.Child] PRIMARY KEY CLUSTERED ( [Id] ),
    CONSTRAINT [FK_dbo.Child_dbo.Parent_ParentId] FOREIGN KEY ( [ParentId] ) REFERENCES [dbo].[Parent] ( [Id] ) ON DELETE CASCADE
);
CREATE TABLE [dbo].[ChildAudit] ( [Id] INT IDENTITY(1,1) NOT NULL, [ChildId] INT NOT NULL, CONSTRAINT [PK_dbo.ChildAudit] PRIMARY KEY ( [Id] ) );
CREATE TABLE [dbo].[Emptied] ( [Id] INT IDENTITY(1,1) NOT NULL, [Value] INT NULL, CONSTRAINT [PK_dbo.Emptied] PRIMARY KEY ( [Id] ) );
CREATE TABLE [dbo].[Attribute] ( [Id] INT IDENTITY(1,1) NOT NULL, [Key] NVARCHAR(100) NOT NULL, [EntityTypeQualifierColumn] NVARCHAR(50) NULL, [Guid] UNIQUEIDENTIFIER NOT NULL, CONSTRAINT [PK_dbo.Attribute] PRIMARY KEY ( [Id] ) );
CREATE TABLE [dbo].[__MigrationHistory] ( [MigrationId] NVARCHAR(150) NOT NULL PRIMARY KEY );
CREATE TABLE [dbo].[RockMigration] ( [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, [MigrationId] BIGINT NOT NULL );
CREATE TABLE [dbo].[MetaLastNameLookup] ( [Id] INT IDENTITY(1,1) NOT NULL, [LastName] NVARCHAR(100) NULL, [CountIn100k] DECIMAL(18,2) NULL, [Guid] UNIQUEIDENTIFIER NOT NULL, CONSTRAINT [PK_dbo.MetaLastNameLookup] PRIMARY KEY ( [Id] ) );
GO
CREATE TRIGGER [dbo].[tgrChild_Insert] ON [dbo].[Child] AFTER INSERT AS
BEGIN
    INSERT INTO [dbo].[ChildAudit] ( [ChildId] ) SELECT [Id] FROM [inserted];
END
GO
CREATE VIEW [dbo].[ParentView] AS SELECT [Id], [Name] FROM [dbo].[Parent];
GO
CREATE PROCEDURE [dbo].[spParentCount] AS SELECT COUNT(*) FROM [dbo].[Parent];
GO
INSERT INTO [dbo].[Parent] ( [Name], [Amount], [Point], [Data], [Guid] ) VALUES
    ( N'One', 1.50, geography::Point( 33.4, -112.0, 4326 ), 0x0102FF, NEWID() ),
    ( N'Ünïcödé ''quote''', NULL, NULL, NULL, NEWID() ),
    ( N'Three', 3, NULL, NULL, NEWID() );
DELETE FROM [dbo].[Parent] WHERE [Name] = N'Three';
INSERT INTO [dbo].[Child] ( [ParentId], [Note] ) VALUES ( 1, N'first' ), ( 2, NULL );
INSERT INTO [dbo].[Emptied] ( [Value] ) VALUES ( 1 ), ( 2 );
DELETE FROM [dbo].[Emptied];
INSERT INTO [dbo].[Attribute] ( [Key], [EntityTypeQualifierColumn], [Guid] ) VALUES ( N'RockInstanceId', N'SystemSetting', 'C4E4A3D1-7D2B-4F43-9A37-6B1F0A1E2C3D' ), ( N'Other', NULL, NEWID() );
INSERT INTO [dbo].[__MigrationHistory] VALUES ( N'202605122315282_Last' );
INSERT INTO [dbo].[RockMigration] ( [MigrationId] ) VALUES ( 20260601000000 );
INSERT INTO [dbo].[MetaLastNameLookup] ( [LastName], [CountIn100k], [Guid] ) VALUES
    ( N'O''Brien, ""Jr"" Ünïcödé', 1.25, NEWID() ), ( N'', NULL, NEWID() ), ( NULL, 0, NEWID() ), ( N'Two' + NCHAR(13) + NCHAR(10) + N'lines' + NCHAR(10), 2, NEWID() ), ( N'Gone', 1, NEWID() );
DELETE FROM [dbo].[MetaLastNameLookup] WHERE [LastName] = N'Gone';";

    [TestMethod]
    public async Task ExportAndPackage_HandMadeDatabase_RebuildsAnEquivalentDatabase()
    {
        var outputPath = Path.Combine( Path.GetTempPath(), $"RockBaselineTest_{Guid.NewGuid():N}" );

        try
        {
            using var source = await ScratchDatabase.CreateAsync( SourceDatabase );
            using var target = await ScratchDatabase.CreateAsync();
            var work = new BaselineWork
            {
                RockVersion = "19.7.0.0"
            };

            using var database = new BaselineDatabase( source.ConnectionString );
            work.IdentityValues = CleanupCommand.ReadIdentityValues( database );

            var files = ExportCommand.Export( source.ConnectionString, outputPath, TestContext.WriteLine );
            PackageCommand.Package( work, outputPath, TestContext.WriteLine );
            RunScripts( target, outputPath );

            var catalogFindings = Rock.Data.SchemaChecks.CompareCatalogs( SchemaCatalogReader.Read( source.ConnectionString ), SchemaCatalogReader.Read( target.ConnectionString ) );
            var dataFindings = SchemaDataComparer.Compare(
                source.ConnectionString, SchemaCatalogReader.Read( source.ConnectionString ),
                target.ConnectionString, SchemaCatalogReader.Read( target.ConnectionString ) );
            var baselineClass = File.ReadAllText( Path.Combine( outputPath, "Baseline.cs" ) );

            CollectionAssert.AreEqual( new[]
            {
                "20131125173405-01_Schema.sql",
                "20131125173405-Attribute.json",
                "20131125173405-Child.json",
                "20131125173405-ChildAudit.json",
                "20131125173405-MetaLastNameLookup.json",
                "20131125173405-Parent.json",
                "20131125173405-03_Constraints.sql"
            }, files.Select( Path.GetFileName ).ToArray(), "One data file per table with rows; Emptied has none." );
            CollectionAssert.AreEqual( new[]
            {
                "FAIL equivalence TableOnlyInLeft dbo.RockMigration",
                "FAIL equivalence TableOnlyInLeft dbo.__MigrationHistory"
            }, SchemaFindings.Describe( catalogFindings ), string.Join( Environment.NewLine, catalogFindings.Select( f => $"{f.Code} {f.Object}: {f.Message}" ) ) );
            CollectionAssert.AreEqual( new string[0], SchemaFindings.Describe( dataFindings ), string.Join( Environment.NewLine, dataFindings.Select( f => $"{f.Code} {f.Object}: {f.Message}" ) ) );
            StringAssert.Contains( baselineClass, "        SqlResource( \"01_Schema\" );\r\n        BulkResource( \"Attribute\" );\r\n        BulkResource( \"Child\" );\r\n        BulkResource( \"ChildAudit\" );\r\n        BulkResource( \"MetaLastNameLookup\" );\r\n        BulkResource( \"Parent\" );\r\n        SqlResource( \"03_Constraints\" );\r\n        SqlResource( \"04_Reseed\" );\r\n        RegeneratePerInstallValues();" );
            StringAssert.Contains( baselineClass, "internal partial class Baseline : Migration", "Baseline.Reseed.cs, written by hand, holds the other half." );
            StringAssert.Contains( baselineClass, "[SqlMigration( Migration.BaselineMigrationId, \"19.7\" )]" );

            // Each row is one line, so a changed row is a one-line diff.
            Assert.AreEqual( 7, File.ReadAllLines( Path.Combine( outputPath, "20131125173405-Parent.json" ) ).Length, "Opening, columns and rows lines, two rows of one line each, then two closing lines." );
            StringAssert.StartsWith( File.ReadAllText( Path.Combine( outputPath, "20131125173405-MetaLastNameLookup.json" ) ), "{\r\n  \"generateGuid\": true,\r\n  \"columns\": [ \"Id\", \"LastName\", \"CountIn100k\" ],", "The Meta lookups leave out Guid and generate it." );
            Assert.AreEqual( 4, Scalar( target, "SELECT COUNT(DISTINCT [Guid]) FROM [dbo].[MetaLastNameLookup]" ) );
            Assert.AreEqual( 0, Scalar( source, "SELECT COUNT(*) FROM [dbo].[MetaLastNameLookup] WHERE [Guid] IN ( SELECT [Guid] FROM [" + target.Name + "].[dbo].[MetaLastNameLookup] )" ), "Each load generates its own Meta Guid values." );
            Assert.AreEqual( 1, Scalar( target, "SELECT COUNT(*) FROM [dbo].[MetaLastNameLookup] WHERE [LastName] = N'Two' + NCHAR(13) + NCHAR(10) + N'lines' + NCHAR(10)" ), "Line breaks in a value load exactly." );

            // The scripts keep every Guid; Baseline.Reseed.cs regenerates per-install values in code.
            Assert.IsFalse( File.ReadAllText( Path.Combine( outputPath, "20131125173405-04_Reseed.sql" ) ).Contains( "NEWID" ), "The reseed script only reseeds identities." );
            Assert.AreEqual( 2, Scalar( target, "SELECT COUNT(*) FROM [dbo].[Attribute] AS [t] JOIN [" + source.Name + "].[dbo].[Attribute] AS [s] ON [s].[Guid] = [t].[Guid]" ), "The scripts keep every Guid, the instance ID's included." );

            Assert.AreEqual( 1, Scalar( target, "SELECT COUNT(*) FROM [dbo].[Parent] AS [t] JOIN [" + source.Name + "].[dbo].[Parent] AS [s] ON [s].[Id] = [t].[Id] WHERE [s].[Point] IS NOT NULL AND CAST( [t].[Point] AS VARBINARY(MAX) ) = CAST( [s].[Point] AS VARBINARY(MAX) )" ), "Geography loads byte for byte." );
            Assert.AreEqual( 2, Scalar( target, "SELECT COUNT(*) FROM [dbo].[ChildAudit]" ), "The trigger is created after the data, so it does not fire again." );
            Assert.AreEqual( 0, Scalar( target, "SELECT COUNT(*) FROM [sys].[check_constraints] WHERE [is_not_trusted] = 1" ), "The load checks CHECK constraints, so they stay trusted." );
            Assert.AreEqual( 0, Scalar( target, "SELECT COUNT(*) FROM [sys].[foreign_keys] WHERE [is_not_trusted] = 1" ) );

            // The next identity in each table matches the source, including tables
            // whose last rows were deleted and a table the baseline loads nothing into.
            foreach ( var insert in new[]
            {
                "INSERT INTO [dbo].[Parent] ( [Name], [Guid] ) VALUES ( N'Next', NEWID() ); SELECT SCOPE_IDENTITY();",
                "INSERT INTO [dbo].[Emptied] ( [Value] ) VALUES ( 3 ); SELECT SCOPE_IDENTITY();",
                "INSERT INTO [dbo].[MetaLastNameLookup] ( [Guid] ) VALUES ( NEWID() ); SELECT SCOPE_IDENTITY();",
                "INSERT INTO [dbo].[Child] ( [ParentId] ) VALUES ( 1 ); SELECT SCOPE_IDENTITY();",
                "INSERT INTO [dbo].[Attribute] ( [Key], [Guid] ) VALUES ( N'Next', NEWID() ); SELECT SCOPE_IDENTITY();"
            } )
            {
                Assert.AreEqual( ScalarDecimal( source, insert ), ScalarDecimal( target, insert ), insert );
            }
        }
        finally
        {
            if ( Directory.Exists( outputPath ) )
            {
                Directory.Delete( outputPath, true );
            }
        }
    }

    #endregion Export and Package

    #region Cleanup

    /// <summary>
    /// A settled database as B's startup leaves it: one run-once job still
    /// present, run data, a startup exception, and job run state. The
    /// post-install job has run and deleted itself; Interaction and the
    /// other newer tables do not exist, so cleanup must skip them.
    /// </summary>
    private const string SettledDatabase = @"
CREATE TABLE [dbo].[ServiceJob] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, [IsSystem] BIT NOT NULL, [IsActive] BIT NULL, [Name] NVARCHAR(100) NOT NULL, [Description] NVARCHAR(MAX) NULL,
    [Class] NVARCHAR(100) NOT NULL, [CronExpression] NVARCHAR(120) NOT NULL, [NotificationStatus] INT NOT NULL, [Guid] UNIQUEIDENTIFIER NOT NULL,
    [EnableHistory] BIT NOT NULL DEFAULT 0, [HistoryCount] INT NOT NULL DEFAULT 0, [LastRunDateTime] DATETIME NULL, [LastStatus] NVARCHAR(50) NULL );
CREATE TABLE [dbo].[PluginMigration] ( [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, [PluginAssemblyName] NVARCHAR(512) NOT NULL, [MigrationNumber] INT NOT NULL );
CREATE TABLE [dbo].[Attribute] ( [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, [Key] NVARCHAR(100) NOT NULL, [EntityTypeQualifierColumn] NVARCHAR(50) NULL, [Guid] UNIQUEIDENTIFIER NOT NULL, [CreatedDateTime] DATETIME NULL );
CREATE TABLE [dbo].[ExceptionLog] ( [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, [ExceptionType] NVARCHAR(150) NULL, [Description] NVARCHAR(MAX) NULL );
CREATE TABLE [dbo].[History] ( [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, [Verb] NVARCHAR(40) NULL );
CREATE TABLE [dbo].[ScheduleDate] ( [ScheduleId] INT NOT NULL, [StartDateTime] DATETIME NOT NULL, CONSTRAINT [PK_dbo.ScheduleDate] PRIMARY KEY ( [ScheduleId], [StartDateTime] ) );
CREATE TABLE [dbo].[AnalyticsSourceDate] ( [DateKey] INT NOT NULL PRIMARY KEY );
CREATE TABLE [dbo].[AnalyticsSourcePostalCode] ( [PostalCode] NVARCHAR(50) NOT NULL PRIMARY KEY );
CREATE TABLE [dbo].[IdentityVerificationCode] ( [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, [Code] NVARCHAR(10) NOT NULL );
CREATE TABLE [dbo].[UserLogin] ( [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, [IsOnLine] BIT NULL );
CREATE TABLE [dbo].[__MigrationHistory] ( [MigrationId] NVARCHAR(150) NOT NULL PRIMARY KEY );
GO
SET IDENTITY_INSERT [dbo].[ServiceJob] ON;
INSERT INTO [dbo].[ServiceJob] ( [Id], [IsSystem], [Name], [Class], [CronExpression], [NotificationStatus], [Guid], [LastRunDateTime], [LastStatus] ) VALUES
    ( 1, 1, N'Job Pulse', N'Rock.Jobs.JobPulse', N'0 * * * * ?', 1, NEWID(), GETDATE(), N'Success' ),
    ( 70, 1, N'Rock Update Helper v19.0', N'Rock.Jobs.PostV19Thing', N'0 0 2 * * ?', 1, 'AAAAAAAA-0000-0000-0000-000000000001', NULL, NULL );
SET IDENTITY_INSERT [dbo].[ServiceJob] OFF;
INSERT INTO [dbo].[PluginMigration] ( [PluginAssemblyName], [MigrationNumber] ) VALUES ( N'Rock', 1 );
INSERT INTO [dbo].[Attribute] ( [Key], [EntityTypeQualifierColumn], [Guid], [CreatedDateTime] ) VALUES ( N'RockInstanceId', N'SystemSetting', 'C4E4A3D1-7D2B-4F43-9A37-6B1F0A1E2C3D', GETDATE() ), ( N'OpenIdConnectRsaKeys', N'SystemSetting', NEWID(), NULL ), ( N'core_RockSecuritySettings', N'SystemSetting', NEWID(), NULL );
INSERT INTO [dbo].[ExceptionLog] ( [ExceptionType], [Description] ) VALUES ( N'System.Exception', N'Block conversion failed once' );
INSERT INTO [dbo].[History] ( [Verb] ) VALUES ( N'ADD' );
INSERT INTO [dbo].[ScheduleDate] VALUES ( 1, GETDATE() );
INSERT INTO [dbo].[AnalyticsSourceDate] VALUES ( 20260101 );
INSERT INTO [dbo].[AnalyticsSourcePostalCode] VALUES ( N'85001' );
INSERT INTO [dbo].[IdentityVerificationCode] ( [Code] ) VALUES ( N'001001' );
INSERT INTO [dbo].[UserLogin] ( [IsOnLine] ) VALUES ( 1 );
INSERT INTO [dbo].[__MigrationHistory] VALUES ( N'201311251734059_CreateDatabase' ), ( N'202605122315282_Last' );";

    [TestMethod]
    public async Task Cleanup_SettledDatabase_StopsUntilSettledAndAcceptedThenResets()
    {
        using var database = await ScratchDatabase.CreateAsync( SettledDatabase );
        using var fixture = new BaselineFixture();
        var options = new BaselineOptions
        {
            Command = "cleanup",
            SourcePath = fixture.SourcePath,
            RockWebPath = fixture.RockWebPath,
            Database = database.ConnectionString,
            WorkPath = fixture.WorkPath
        };

        var notSettled = CleanupCommand.Run( options );

        database.Execute( "DELETE FROM [dbo].[ServiceJob] WHERE [Id] = 70;" );
        var exceptionsNotAccepted = CleanupCommand.Run( options );

        options.IsAcceptExceptions = true;
        var accepted = CleanupCommand.Run( options );

        var work = BaselineWork.Load( fixture.WorkPath );

        Assert.AreEqual( 1, notSettled, "A run-once job from DataMigrationsStartup is still present." );
        Assert.AreEqual( 1, exceptionsNotAccepted, "Startup logged an exception." );
        Assert.AreEqual( 0, accepted );
        Assert.AreEqual( "64|Rock.Jobs.PostInstallDataMigrations|0 0 2 1/1 * ? *", ScalarString( database, "SELECT CONCAT( [Id], '|', [Class], '|', [CronExpression] ) FROM [dbo].[ServiceJob] WHERE [Guid] = '322984F1-A7A0-4D1B-AE6F-D7F043F66EB3'" ) );
        Assert.AreEqual( 0, Scalar( database, "SELECT COUNT(*) FROM [dbo].[ServiceJob] WHERE [LastRunDateTime] IS NOT NULL OR [LastStatus] IS NOT NULL" ) );
        Assert.AreEqual( 0, Scalar( database, "SELECT COUNT(*) FROM [dbo].[Attribute] WHERE [CreatedDateTime] IS NOT NULL" ) );
        Assert.AreEqual( 0, Scalar( database, "SELECT ( SELECT COUNT(*) FROM [dbo].[ExceptionLog] ) + ( SELECT COUNT(*) FROM [dbo].[History] ) + ( SELECT COUNT(*) FROM [dbo].[ScheduleDate] ) + ( SELECT COUNT(*) FROM [dbo].[AnalyticsSourceDate] ) + ( SELECT COUNT(*) FROM [dbo].[AnalyticsSourcePostalCode] ) + ( SELECT COUNT(*) FROM [dbo].[IdentityVerificationCode] )" ) );
        Assert.AreEqual( 0, Scalar( database, "SELECT COUNT(*) FROM [dbo].[UserLogin] WHERE [IsOnLine] = 1" ) );
        Assert.AreEqual( 0, Scalar( database, "SELECT COUNT(*) FROM [dbo].[Attribute] WHERE [Key] IN ( N'OpenIdConnectRsaKeys', N'core_RockSecuritySettings' )" ), "Settings holding the build site's values are not kept." );
        Assert.AreEqual( 1, Scalar( database, "SELECT COUNT(*) FROM [dbo].[Attribute] WHERE [Key] = N'RockInstanceId'" ), "The instance ID is kept; the baseline regenerates its Guid." );
        Assert.AreEqual( 20260512231528, work.IncludesThroughMigrationId );
        Assert.AreEqual( 70L, work.IdentityValues["[dbo].[ServiceJob]"], "Deleted job rows leave the identity ahead of the highest Id." );
    }

    #endregion Cleanup

    #region Monitor

    /// <summary>
    /// Startup still running: one run-once job left and a registration
    /// table the block type thread is writing.
    /// </summary>
    private const string StartingDatabase = @"
CREATE TABLE [dbo].[ServiceJob] ( [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, [Name] NVARCHAR(100) NOT NULL, [Guid] UNIQUEIDENTIFIER NOT NULL );
CREATE TABLE [dbo].[PluginMigration] ( [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, [PluginAssemblyName] NVARCHAR(512) NOT NULL, [MigrationNumber] INT NOT NULL );
CREATE TABLE [dbo].[BlockType] ( [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, [Name] NVARCHAR(100) NOT NULL, [ModifiedDateTime] DATETIME NULL );
GO
INSERT INTO [dbo].[ServiceJob] ( [Name], [Guid] ) VALUES ( N'Rock Update Helper v19.0', 'AAAAAAAA-0000-0000-0000-000000000001' );
INSERT INTO [dbo].[PluginMigration] ( [PluginAssemblyName], [MigrationNumber] ) VALUES ( N'Rock', 1 );
INSERT INTO [dbo].[BlockType] ( [Name], [ModifiedDateTime] ) VALUES ( N'First', GETDATE() );";

    [TestMethod]
    public async Task Monitor_StartupFinishesThenGoesQuiet_SettlesAfterTheQuietPeriod()
    {
        using var database = await ScratchDatabase.CreateAsync( StartingDatabase );
        using var fixture = new BaselineFixture();
        var timing = new MonitorTiming { QuietPeriod = TimeSpan.FromSeconds( 3 ), PollInterval = TimeSpan.FromMilliseconds( 250 ), Timeout = TimeSpan.FromMinutes( 1 ) };
        var monitor = Task.Run( () => MonitorCommand.Monitor( database.ConnectionString, new BaselineSource( fixture.SourcePath ), new Version( 19, 7 ), timing, TestContext.WriteLine ) );

        // The job finishes, then the block type thread inserts a row and updates one,
        // stamping ModifiedDateTime with the current time as Rock's SaveChanges does.
        await Task.Delay( 1000 );
        database.Execute( "DELETE FROM [dbo].[ServiceJob];" );
        await Task.Delay( 1000 );
        database.Execute( "INSERT INTO [dbo].[BlockType] ( [Name], [ModifiedDateTime] ) VALUES ( N'Second', GETDATE() );" );
        await Task.Delay( 1000 );
        database.Execute( "UPDATE [dbo].[BlockType] SET [ModifiedDateTime] = GETDATE() WHERE [Name] = N'First';" );
        var lastWrite = DateTime.Now;

        Assert.IsTrue( await monitor );
        Assert.IsTrue( DateTime.Now - lastWrite >= timing.QuietPeriod, "The update restarted the quiet period." );
    }

    [TestMethod]
    public async Task Monitor_JobNeverRuns_StopsAtTheTimeout()
    {
        using var database = await ScratchDatabase.CreateAsync( StartingDatabase );
        using var fixture = new BaselineFixture();
        var timing = new MonitorTiming { QuietPeriod = TimeSpan.FromMilliseconds( 500 ), PollInterval = TimeSpan.FromMilliseconds( 250 ), Timeout = TimeSpan.FromSeconds( 2 ) };
        var lines = new List<string>();

        var isSettled = MonitorCommand.Monitor( database.ConnectionString, new BaselineSource( fixture.SourcePath ), new Version( 19, 7 ), timing, lines.Add );

        Assert.IsFalse( isSettled, "The quiet period passed, but a run-once job is still present." );
        Assert.AreEqual( 1, lines.Count( l => l.StartsWith( "OUTSTANDING  job 'Rock Update Helper v19.0'" ) ), "Outstanding items are listed once, not on every poll." );
    }

    #endregion Monitor

    #region Fixtures

    /// <summary>
    /// A minimal B source tree and RockWeb folder: one job on the startup
    /// list, the post-install job, and two HotFixes of which B runs one.
    /// </summary>
    private sealed class BaselineFixture : IDisposable
    {
        private readonly string _rootPath = Path.Combine( Path.GetTempPath(), $"RockBaselineFixture_{Guid.NewGuid():N}" );

        public string SourcePath => Path.Combine( _rootPath, "source" );

        public string RockWebPath => Path.Combine( _rootPath, "RockWeb" );

        public string WorkPath => Path.Combine( _rootPath, "work" );

        public BaselineFixture()
        {
            Write( "source/Rock.sln", string.Empty );
            Write( "source/Rock/SystemGuid/ServiceJob.cs", @"
public const string POST_V19_THING = ""AAAAAAAA-0000-0000-0000-000000000001"";
public const string POST_INSTALL_DATA_MIGRATIONS = ""322984F1-A7A0-4D1B-AE6F-D7F043F66EB3"";" );
            Write( "source/Rock.Migrations/RockStartup/DataMigrationsStartup.cs", @"
startupRunOnceJobGuids = { SystemGuid.ServiceJob.POST_V19_THING.AsGuid(), SystemGuid.ServiceJob.POST_INSTALL_DATA_MIGRATIONS.AsGuid() };" );
            Write( "source/Rock/Plugin/HotFixes/001_Ran.cs", @"[MigrationNumber( 1, ""1.16.0"" )]" );
            Write( "source/Rock/Plugin/HotFixes/002_Later.cs", @"[MigrationNumber( 2, ""99.0"" )]" );
            Write( "RockWeb/web.config", @"<configuration><appSettings><add key=""RunJobsInIISContext"" value=""False"" /></appSettings></configuration>" );

            Directory.CreateDirectory( Path.Combine( RockWebPath, "Bin" ) );
            File.Copy( typeof( Rock.VersionInfo.VersionInfo ).Assembly.Location, Path.Combine( RockWebPath, "Bin", "Rock.Version.dll" ) );
        }

        public void Dispose()
        {
            Directory.Delete( _rootPath, true );
        }

        private void Write( string relativePath, string content )
        {
            var path = Path.Combine( _rootPath, relativePath );
            Directory.CreateDirectory( Path.GetDirectoryName( path ) );
            File.WriteAllText( path, content );
        }
    }

    #endregion Fixtures

    #region Support

    /// <summary>
    /// Runs the baseline's steps in the order the generated class does, on
    /// one connection: each script batch by batch, as
    /// <see cref="Migration.SqlResource(string)"/> would, and each JSON data
    /// file through <see cref="MigrationBulkLoader"/>, as <c>BulkResource</c> would.
    /// </summary>
    private static void RunScripts( ScratchDatabase target, string outputPath )
    {
        using var connection = new SqlConnection( target.ConnectionString );
        connection.Open();

        foreach ( var (suffix, isBulk) in PackageCommand.GetBaselineSteps( outputPath ) )
        {
            var file = Path.Combine( outputPath, $"{ExportCommand.BaselineMigrationId}-{suffix}.{( isBulk ? "json" : "sql" )}" );

            if ( isBulk )
            {
                MigrationBulkLoader.Load( connection, null, suffix, File.ReadAllBytes( file ), 300 );
                continue;
            }

            foreach ( var batch in Migration.SplitBatches( File.ReadAllText( file ) ) )
            {
                using var command = new SqlCommand( batch, connection );
                command.ExecuteNonQuery();
            }
        }
    }

    private static int Scalar( ScratchDatabase database, string sql )
    {
        return Convert.ToInt32( ExecuteScalar( database, sql ) );
    }

    private static decimal ScalarDecimal( ScratchDatabase database, string sql )
    {
        return Convert.ToDecimal( ExecuteScalar( database, sql ) );
    }

    private static string ScalarString( ScratchDatabase database, string sql )
    {
        return ExecuteScalar( database, sql ) as string;
    }

    private static object ExecuteScalar( ScratchDatabase database, string sql )
    {
        using var connection = new SqlConnection( database.ConnectionString );
        using var command = new SqlCommand( sql, connection );
        connection.Open();

        return command.ExecuteScalar();
    }

    #endregion Support
}
