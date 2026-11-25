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
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;

using Rock.Data;

using Testcontainers.MsSql;

namespace Rock.Tests.Integration.TestFramework.Database;

/// <summary>
/// An empty, uniquely named database that is dropped on dispose. For
/// tests that build small hand-made schemas rather than a Rock database.
/// </summary>
/// <remarks>
/// Every scratch database lives in a SQL Server Docker container started
/// on first use and shared by the whole test run, never on a real server.
/// </remarks>
internal sealed class ScratchDatabase : IDisposable
{
    /// <summary>
    /// The shared container, started on first use.
    /// </summary>
    private static readonly Lazy<Task<MsSqlContainer>> SharedContainer = new Lazy<Task<MsSqlContainer>>( StartContainerAsync );

    /// <summary>
    /// The connection string of the server, pointing at <c>master</c>.
    /// </summary>
    private readonly string _serverConnectionString;

    /// <summary>
    /// The database name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The connection string of the scratch database.
    /// </summary>
    public string ConnectionString { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScratchDatabase"/> class.
    /// </summary>
    private ScratchDatabase( string serverConnectionString, string name )
    {
        _serverConnectionString = serverConnectionString;
        Name = name;
        ConnectionString = new SqlConnectionStringBuilder( serverConnectionString ) { InitialCatalog = name }.ConnectionString;
    }

    /// <summary>
    /// Creates an empty scratch database and runs the optional script in it.
    /// </summary>
    /// <param name="script">SQL to run after creating the database; batches are separated by <c>GO</c> lines.</param>
    /// <returns>The scratch database.</returns>
    public static async Task<ScratchDatabase> CreateAsync( string script = null )
    {
        var serverConnectionString = await GetServerConnectionStringAsync();
        var database = new ScratchDatabase( serverConnectionString, $"RockScratch_{Guid.NewGuid():N}" );

        ExecuteBatches( serverConnectionString, $"CREATE DATABASE [{database.Name}];" );

        if ( script != null )
        {
            database.Execute( script );
        }

        return database;
    }

    /// <summary>
    /// Reserves a unique scratch database name without creating the
    /// database, for tests of code that creates it. Dispose still drops it.
    /// </summary>
    /// <returns>The scratch database, not yet created.</returns>
    public static async Task<ScratchDatabase> ReserveAsync()
    {
        return new ScratchDatabase( await GetServerConnectionStringAsync(), $"RockScratch_{Guid.NewGuid():N}" );
    }

    /// <summary>
    /// Runs a script in the scratch database.
    /// </summary>
    /// <param name="script">The SQL; batches are separated by <c>GO</c> lines.</param>
    public void Execute( string script )
    {
        ExecuteBatches( ConnectionString, script );
    }

    /// <summary>
    /// Drops the scratch database.
    /// </summary>
    public void Dispose()
    {
        SqlConnection.ClearAllPools();
        ExecuteBatches( _serverConnectionString, $@"
IF DB_ID( '{Name}' ) IS NOT NULL
BEGIN
    ALTER DATABASE [{Name}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [{Name}];
END" );
    }

    /// <summary>
    /// Runs each <c>GO</c>-separated batch of a script.
    /// </summary>
    private static void ExecuteBatches( string connectionString, string script )
    {
        using var connection = new SqlConnection( connectionString );
        connection.Open();

        // The runner's splitting, so test scripts split exactly like migration scripts.
        foreach ( var batch in Migration.SplitBatches( script ) )
        {
            using var command = new SqlCommand( batch, connection );
            command.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// Gets the shared container's connection string, pointing at <c>master</c>.
    /// </summary>
    private static async Task<string> GetServerConnectionStringAsync()
    {
        var connectionString = ( await SharedContainer.Value ).GetConnectionString();

        return new SqlConnectionStringBuilder( connectionString )
        {
            InitialCatalog = "master",
            TrustServerCertificate = true
        }.ConnectionString;
    }

    /// <summary>
    /// Starts the shared SQL Server container.
    /// </summary>
    private static async Task<MsSqlContainer> StartContainerAsync()
    {
        var container = new MsSqlBuilder().Build();

        await container.StartAsync();

        return container;
    }
}
