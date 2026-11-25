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
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Tests.Integration.TestFramework.Database;

using Testcontainers.MsSql;

namespace Rock.Tests.Integration.Data.SchemaChecks;

/// <summary>
/// The commute check. Production applies migrations by target version, then
/// ID. A developer's database applies them as they arrive, which is
/// effectively ID order, so a v21 migration can run before a fix with a lower
/// target version that was merged up later. This proves both orders build
/// the same schema, so migrations in concurrently developed versions stay
/// independent of each other.
/// </summary>
[TestClass]
public class MigrationOrderCommuteTests
{
    #region Fields

    /// <summary>
    /// The SQL Server image the two databases are built on: the minimum
    /// version Rock supports.
    /// </summary>
    private const string SqlServerImage = "mcr.microsoft.com/mssql/server:2022-latest";

    #endregion

    #region Properties

    public TestContext TestContext { get; set; }

    #endregion

    #region Tests

    [TestMethod]
    public async Task Commute_ProductionAndDevelopmentOrder_BuildTheSameSchema()
    {
        var migrations = DatabaseContainerImageBuilder.GetMigrations();
        var productionOrder = SqlMigrationRunner.OrderForProduction( migrations );
        var developmentOrder = GetDevelopmentOrder( migrations );

        // The orders differ only when a migration with a lower target
        // version has a later ID than one with a higher target version.
        // When they match there is nothing to prove.
        if ( productionOrder.Select( m => m.Id ).SequenceEqual( developmentOrder.Select( m => m.Id ) ) )
        {
            TestContext.WriteLine( "Production and development order are the same, so there is nothing to compare." );
            return;
        }

        LogOrderDifferences( productionOrder, developmentOrder );

        var container = new MsSqlBuilder().WithImage( SqlServerImage ).Build();
        var startStopwatch = Stopwatch.StartNew();

        await container.StartAsync();

        TestContext.WriteLine( $"SQL Server container start: {startStopwatch.Elapsed.TotalSeconds:0.0}s" );

        try
        {
            var productionDatabase = CreateDatabase( container, "ProductionOrder" );
            var developmentDatabase = CreateDatabase( container, "DevelopmentOrder" );

            // The two builds use separate databases, so they run in parallel.
            // GetAwaiter().GetResult() rethrows the first failure as it is,
            // not wrapped in an AggregateException.
            TestMigrationRunner.RunInOneScope( () => Task.WhenAll(
                    Task.Run( () => Timed( "Production order build", () => BuildInProductionOrder( productionDatabase, productionOrder ) ) ),
                    Task.Run( () => Timed( "Development order build", () => BuildInDevelopmentOrder( developmentDatabase, developmentOrder ) ) ) )
                .GetAwaiter()
                .GetResult() );

            var findings = Rock.Data.SchemaChecks.CompareCatalogs( SchemaCatalogReader.Read( productionDatabase ), SchemaCatalogReader.Read( developmentDatabase ) )
                .Where( f => f.IsBlocking )
                .OrderBy( f => f.Code )
                .ThenBy( f => f.Object, StringComparer.OrdinalIgnoreCase )
                .Select( f => $"{f.Code} {f.Object}: {f.Message}" )
                .ToList();

            Assert.AreEqual( 0, findings.Count, $"Building in production order (target version, then ID) and in development order (ID) gave different schemas. Two migrations that change the same object depend on their order; make the later one independent of the earlier one:{Environment.NewLine}{string.Join( Environment.NewLine, findings )}" );
        }
        finally
        {
            await container.DisposeAsync();
        }
    }

    #endregion

    #region Support Methods

    /// <summary>
    /// Orders the migrations the way a developer's database receives them:
    /// the baseline first, then by ID.
    /// </summary>
    /// <param name="migrations">The migrations.</param>
    /// <returns>The migrations in development order.</returns>
    private static List<SqlMigrationDescriptor> GetDevelopmentOrder( List<SqlMigrationDescriptor> migrations )
    {
        return migrations
            .OrderBy( m => m.Id == Migration.BaselineMigrationId ? 0 : 1 )
            .ThenBy( m => m.Id )
            .ToList();
    }

    /// <summary>
    /// Builds the database the way production does: one run with every migration.
    /// </summary>
    /// <param name="connectionString">The empty database.</param>
    /// <param name="migrations">The migrations.</param>
    private void BuildInProductionOrder( string connectionString, List<SqlMigrationDescriptor> migrations )
    {
        var result = Run( connectionString, migrations );

        Assert.AreEqual( migrations.Count, result.AppliedMigrationNames.Count, "Every migration ran in production order." );
    }

    /// <summary>
    /// Builds the database in ID order.
    /// </summary>
    /// <param name="connectionString">The empty database.</param>
    /// <param name="migrations">The migrations in development order.</param>
    private void BuildInDevelopmentOrder( string connectionString, List<SqlMigrationDescriptor> migrations )
    {
        /*
            11/17/26 - CLAUDE

            The runner always applies what is pending by target version, then
            ID, so it cannot be asked for ID order directly. Instead it runs
            once per migration, each time given the migrations a developer
            would have so far. Only the newest one is pending, so each run
            applies exactly that migration, which is how a developer's
            database receives them as they arrive.

            Reason: Reproduce arrival order with the real runner.
        */
        var appliedCount = 0;

        for ( var count = 1; count <= migrations.Count; count++ )
        {
            appliedCount += Run( connectionString, migrations.Take( count ).ToList() ).AppliedMigrationNames.Count;
        }

        Assert.AreEqual( migrations.Count, appliedCount, "Every migration ran in development order." );
    }

    /// <summary>
    /// Runs the given migrations against the database.
    /// </summary>
    /// <param name="connectionString">The database.</param>
    /// <param name="migrations">The migrations the runner knows about.</param>
    /// <returns>What the run did.</returns>
    private SqlMigrationRunResult Run( string connectionString, List<SqlMigrationDescriptor> migrations )
    {
        // Called inside TestMigrationRunner.RunInOneScope, which makes
        // RockApp's database unusable for every run at once.
        return SqlMigrationRunner.Run( new SqlMigrationRunnerOptions
        {
            ConnectionString = connectionString,
            RockVersion = new Version( Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber() ),
            Migrations = migrations
        } );
    }

    /// <summary>
    /// Runs the action and logs how long it took.
    /// </summary>
    /// <param name="name">What the action does.</param>
    /// <param name="action">The action.</param>
    private void Timed( string name, Action action )
    {
        var stopwatch = Stopwatch.StartNew();

        action();

        TestContext.WriteLine( $"{name}: {stopwatch.Elapsed.TotalSeconds:0.0}s" );
    }

    /// <summary>
    /// Logs each migration that production applies earlier than its ID
    /// suggests, so a failure shows which migrations the orders disagree on.
    /// </summary>
    /// <param name="productionOrder">The migrations in production order.</param>
    /// <param name="developmentOrder">The migrations in development order.</param>
    private void LogOrderDifferences( List<SqlMigrationDescriptor> productionOrder, List<SqlMigrationDescriptor> developmentOrder )
    {
        var developmentPositions = developmentOrder
            .Select( ( m, index ) => (m.Id, index) )
            .ToDictionary( p => p.Id, p => p.index );

        var moved = productionOrder
            .Select( ( m, index ) => (Migration: m, Index: index) )
            .Where( p => developmentPositions[p.Migration.Id] != p.Index )
            .Select( p => $"{p.Migration.Id} {p.Migration.Name} (target {p.Migration.TargetVersion})" )
            .ToList();

        TestContext.WriteLine( $"Production and development order differ for {moved.Count} migrations:{Environment.NewLine}{string.Join( Environment.NewLine, moved )}" );
    }

    /// <summary>
    /// Creates an empty database in the container and returns its connection string.
    /// </summary>
    /// <param name="container">The SQL Server container.</param>
    /// <param name="name">The database name.</param>
    /// <returns>The connection string.</returns>
    private static string CreateDatabase( MsSqlContainer container, string name )
    {
        var builder = new SqlConnectionStringBuilder( container.GetConnectionString() ) { TrustServerCertificate = true };

        using var connection = new SqlConnection( builder.ConnectionString );
        using var command = new SqlCommand( $"CREATE DATABASE [{name}];", connection );
        connection.Open();
        command.ExecuteNonQuery();

        builder.InitialCatalog = name;

        return builder.ConnectionString;
    }

    #endregion
}
