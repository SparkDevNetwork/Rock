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
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Tests.Integration.TestFramework.Database;

using Testcontainers.MsSql;

namespace Rock.Tests.Integration.Data.SqlMigrations;

/// <summary>
/// Runs the baseline and every migration on each supported SQL Server
/// version and on Azure SQL Database. The baseline is exported for the minimum version from a
/// database that may have been built on a newer one, so this is what
/// proves the scripts run on every supported version.
/// </summary>
[TestClass]
public class BaselineSqlServerVersionTests
{
    /// <summary>
    /// The SQL Server images the migrations must run on. Adding or
    /// dropping a supported version is one line.
    /// </summary>
    private static readonly string[] SupportedImages =
    {
        "mcr.microsoft.com/mssql/server:2022-latest",
        "mcr.microsoft.com/mssql/server:2025-latest",
        // Azure SQL Database container. Microsoft publishes only "latest", and
        // Azure SQL Database is upgraded in place with no version to choose,
        // so this floats on purpose to match what Azure runs.
        "mcr.microsoft.com/mssql/azure-sql-db-server:latest",
    };

    public TestContext TestContext { get; set; }

    public static IEnumerable<object[]> Images => SupportedImages.Select( i => new object[] { i } );

    [TestMethod]
    [DynamicData( nameof( Images ) )]
    public async Task AllMigrations_OnSupportedSqlServer_RunWithoutErrors( string image )
    {
        TestContext.WriteLine( $"Starting {image}." );

        var container = new MsSqlBuilder().WithImage( image ).Build();

        await container.StartAsync();

        try
        {
            var migrations = DatabaseContainerImageBuilder.GetMigrations();

            var result = TestMigrationRunner.Run( new SqlMigrationRunnerOptions
            {
                ConnectionString = CreateDatabase( container ),
                RockVersion = new Version( Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber() ),
                Migrations = migrations,
                Log = TestContext.WriteLine
            } );

            Assert.IsTrue( result.IsBaselineRun );
            Assert.AreEqual( migrations.Count, result.AppliedMigrationNames.Count, "Every migration ran." );
        }
        finally
        {
            await container.DisposeAsync();
        }
    }

    /// <summary>
    /// Creates an empty database in the container and returns its connection string.
    /// </summary>
    private static string CreateDatabase( MsSqlContainer container )
    {
        var builder = new SqlConnectionStringBuilder( container.GetConnectionString() ) { TrustServerCertificate = true };

        using var connection = new SqlConnection( builder.ConnectionString );
        using var command = new SqlCommand( "CREATE DATABASE [Rock];", connection );
        connection.Open();
        command.ExecuteNonQuery();

        builder.InitialCatalog = "Rock";

        return builder.ConnectionString;
    }
}
