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
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Tests.Integration.TestFramework.Database;

namespace Rock.Tests.Integration.Data.SqlMigrations;

/// <summary>
/// Loads the real baseline and checks the per-install values that
/// <c>Baseline.Reseed.cs</c> regenerates.
/// </summary>
[TestClass]
public class BaselineReseedTests
{
    public TestContext TestContext { get; set; }

    private static Type BaselineType => typeof( Rock.Data.Migrations.Baseline );

    [TestMethod]
    public async Task Baseline_LoadedTwice_GivesEachInstallItsOwnPerInstallValues()
    {
        var frozenAdminGuid = ReadFrozenGuid( "Person", "Id", 1 );
        var frozenAdminAliasGuid = ReadFrozenGuid( "PersonAlias", "Id", ReadFrozenInt( "Person", "Id", 1, "PrimaryAliasId" ) );
        var frozenInstanceIdGuid = ReadFrozenGuid( "Attribute", "Key", "RockInstanceId" );
        var frozenGuids = new[] { frozenAdminGuid, frozenAdminAliasGuid, frozenInstanceIdGuid };

        using var first = await ScratchDatabase.CreateAsync();
        using var second = await ScratchDatabase.CreateAsync();
        LoadBaseline( first );
        LoadBaseline( second );

        var firstValues = ReadPerInstallValues( first );
        var secondValues = ReadPerInstallValues( second );

        for ( var i = 0; i < firstValues.Length; i++ )
        {
            Assert.AreNotEqual( firstValues[i], secondValues[i], $"Value {i} is the same on both installs." );
            CollectionAssert.DoesNotContain( frozenGuids, firstValues[i], $"Value {i} kept the baseline's frozen value." );
        }

        foreach ( var database in new[] { first, second } )
        {
            CollectionAssert.AreEqual( new string[0], FindColumnsContaining( database, frozenGuids ), "A copy of a frozen per-install Guid was left behind." );
            Assert.AreEqual( 1, Scalar( database, "SELECT COUNT(*) FROM [dbo].[Person] AS [p] INNER JOIN [dbo].[PersonAlias] AS [pa] ON [pa].[Id] = [p].[PrimaryAliasId] WHERE [p].[Id] = 1 AND [pa].[AliasPersonGuid] = [p].[Guid] AND [p].[PrimaryAliasGuid] = [pa].[Guid]" ), "The Admin person's copies match its new Guids." );
        }
    }

    [TestMethod]
    public async Task PerInstallSteps_TargetChangedOrRemoved_FailNamingTheStep()
    {
        using var database = await ScratchDatabase.CreateAsync();
        LoadBaseline( database );

        Execute( database, "UPDATE [dbo].[Person] SET [LastName] = N'Renamed' WHERE [Id] = 1;" );
        var adminException = Assert.ThrowsExactly<TargetInvocationException>( () => RunStep( database, "RegenerateAdminPersonGuids" ) );
        StringAssert.Contains( adminException.InnerException.Message, "RegenerateAdminPersonGuids: Person 1 is not the built-in Admin person" );

        Execute( database, "DELETE FROM [dbo].[Attribute] WHERE [Key] = N'RockInstanceId';" );
        var instanceException = Assert.ThrowsExactly<TargetInvocationException>( () => RunStep( database, "RegenerateRockInstanceId" ) );
        StringAssert.Contains( instanceException.InnerException.Message, "RegenerateRockInstanceId: expected one RockInstanceId system setting" );
    }

    #region Support

    /// <summary>
    /// Runs the baseline alone through the runner.
    /// </summary>
    private void LoadBaseline( ScratchDatabase database )
    {
        var result = TestMigrationRunner.Run( new SqlMigrationRunnerOptions
        {
            ConnectionString = database.ConnectionString,
            RockVersion = new Version( 21, 0 ),
            Migrations = new[] { SqlMigrationDescriptor.FromType( BaselineType ) },
            Log = TestContext.WriteLine
        } );

        Assert.IsTrue( result.IsBaselineRun );
    }

    /// <summary>
    /// Runs one of the baseline's per-install steps on its own.
    /// </summary>
    private static void RunStep( ScratchDatabase database, string stepName )
    {
        using var connection = new SqlConnection( database.ConnectionString );
        connection.Open();

        var baseline = ( Migration ) Activator.CreateInstance( BaselineType, true );
        baseline.Connection = connection;

        BaselineType.GetMethod( stepName, BindingFlags.Instance | BindingFlags.NonPublic ).Invoke( baseline, null );
    }

    /// <summary>
    /// Reads the Admin person, its primary alias and the instance ID's <c>Guid</c> values.
    /// </summary>
    private static Guid[] ReadPerInstallValues( ScratchDatabase database )
    {
        using var connection = new SqlConnection( database.ConnectionString );
        using var command = new SqlCommand( @"
SELECT [p].[Guid], [pa].[Guid], ( SELECT [Guid] FROM [dbo].[Attribute] WHERE [Key] = N'RockInstanceId' AND [EntityTypeQualifierColumn] = N'SystemSetting' )
FROM [dbo].[Person] AS [p]
INNER JOIN [dbo].[PersonAlias] AS [pa] ON [pa].[Id] = [p].[PrimaryAliasId]
WHERE [p].[Id] = 1;", connection );
        connection.Open();

        using var reader = command.ExecuteReader();
        reader.Read();

        return new[] { reader.GetGuid( 0 ), reader.GetGuid( 1 ), reader.GetGuid( 2 ) };
    }

    /// <summary>
    /// Lists every <c>table.column</c> holding one of the values, as a
    /// <c>uniqueidentifier</c> or inside text. The <c>Meta*</c> lookups are
    /// skipped: they are large and generate their own <c>Guid</c> values.
    /// </summary>
    private static string[] FindColumnsContaining( ScratchDatabase database, Guid[] values )
    {
        var columns = new List<(string Table, string Column, bool IsGuid)>();

        using var connection = new SqlConnection( database.ConnectionString );
        connection.Open();

        using ( var command = new SqlCommand( @"
SELECT [t].[name], [c].[name], CAST( CASE WHEN [ty].[name] = 'uniqueidentifier' THEN 1 ELSE 0 END AS BIT )
FROM [sys].[tables] AS [t]
INNER JOIN [sys].[columns] AS [c] ON [c].[object_id] = [t].[object_id]
INNER JOIN [sys].[types] AS [ty] ON [ty].[user_type_id] = [c].[user_type_id]
WHERE [t].[name] NOT LIKE 'Meta%'
    AND [c].[is_computed] = 0
    AND ( [ty].[name] = 'uniqueidentifier' OR ( [ty].[name] IN ( 'nvarchar', 'varchar' ) AND ( [c].[max_length] = -1 OR [c].[max_length] >= 72 ) ) );", connection ) )
        using ( var reader = command.ExecuteReader() )
        {
            while ( reader.Read() )
            {
                columns.Add( (reader.GetString( 0 ), reader.GetString( 1 ), reader.GetBoolean( 2 )) );
            }
        }

        var found = new List<string>();

        foreach ( var column in columns )
        {
            var conditions = values.Select( v => column.IsGuid
                ? $"[{column.Column}] = '{v}'"
                : $"[{column.Column}] LIKE N'%{v}%'" );

            using var command = new SqlCommand( $"SELECT COUNT(*) FROM [dbo].[{column.Table}] WHERE {string.Join( " OR ", conditions )};", connection );
            if ( ( int ) command.ExecuteScalar() > 0 )
            {
                found.Add( $"{column.Table}.{column.Column}" );
            }
        }

        return found.ToArray();
    }

    /// <summary>
    /// Reads a <c>Guid</c> from one of the baseline's data files: the row
    /// whose <paramref name="keyColumn"/> equals <paramref name="keyValue"/>.
    /// </summary>
    private static Guid ReadFrozenGuid( string table, string keyColumn, object keyValue )
    {
        return Guid.Parse( ReadFrozenValue( table, keyColumn, keyValue, "Guid" ).GetString() );
    }

    /// <summary>
    /// Reads an integer column from one of the baseline's data files.
    /// </summary>
    private static int ReadFrozenInt( string table, string keyColumn, object keyValue, string column )
    {
        return ReadFrozenValue( table, keyColumn, keyValue, column ).GetInt32();
    }

    private static JsonElement ReadFrozenValue( string table, string keyColumn, object keyValue, string column )
    {
        var path = Path.Combine( RepositoryRoot(), "Rock.Data.Migrations", "Migrations", "Baseline", $"{Migration.BaselineMigrationId}-{table}.json" );

        using var document = JsonDocument.Parse( File.ReadAllText( path ) );
        var columns = document.RootElement.GetProperty( "columns" ).EnumerateArray().Select( c => c.GetString() ).ToList();
        var keyIndex = columns.IndexOf( keyColumn );
        var valueIndex = columns.IndexOf( column );

        foreach ( var row in document.RootElement.GetProperty( "rows" ).EnumerateArray() )
        {
            var key = row[keyIndex];
            var isMatch = keyValue is int id
                ? key.ValueKind == JsonValueKind.Number && key.GetInt32() == id
                : key.ValueKind == JsonValueKind.String && key.GetString() == ( string ) keyValue;

            if ( isMatch )
            {
                return row[valueIndex].Clone();
            }
        }

        throw new InvalidOperationException( $"No {table} row with {keyColumn} = {keyValue} in the baseline's data files." );
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo( AppDomain.CurrentDomain.BaseDirectory );

        while ( directory != null && !File.Exists( Path.Combine( directory.FullName, "Rock.sln" ) ) )
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException( "Could not find Rock.sln above the test folder." );
    }

    private static void Execute( ScratchDatabase database, string sql )
    {
        using var connection = new SqlConnection( database.ConnectionString );
        using var command = new SqlCommand( sql, connection );
        connection.Open();
        command.ExecuteNonQuery();
    }

    private static int Scalar( ScratchDatabase database, string sql )
    {
        using var connection = new SqlConnection( database.ConnectionString );
        using var command = new SqlCommand( sql, connection );
        connection.Open();

        return ( int ) command.ExecuteScalar();
    }

    #endregion Support
}
