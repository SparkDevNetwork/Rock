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

using Microsoft.Data.SqlClient;

namespace Rock.Data;

/*
    11/18/26 - CLAUDE

    GUID and date/time columns are left out by type, not listed one by one
    in the allow list as the spec first said. Measured on 11/18/26: 65 of
    300 GUID columns differed between an EF6-built and a baseline-built
    test database, among them Attribute, Auth, EntityType, Block, BlockType
    and DefinedValue. Those columns are only partly random (migrations
    write fixed GUIDs and NEWID() into the same column), and Id values move
    between builds, so allow-listing them would skip the very tables a GUID
    check is meant to cover. As a result this check cannot catch a wrong
    GUID; conversions are straight copies, which keeps that risk low.

    Reason: Per-column GUID exclusion cannot work for Rock's system tables.
*/
/// <summary>
/// Compares the data in two databases table by table. Each row is hashed
/// in SQL Server, so only keys and hashes cross the network. Every GUID,
/// date and time column is left out of the hash, because many of them
/// hold <c>NEWID()</c> and <c>GETDATE()</c> values that differ on every
/// build, so a wrong GUID is not detected. Types that cannot be
/// serialized to JSON are left out too.
/// </summary>
internal static class SchemaDataComparer
{
    /// <summary>
    /// The store types left out of the row hash.
    /// </summary>
    private static readonly HashSet<string> ExcludedStoreTypes = new HashSet<string>( StringComparer.OrdinalIgnoreCase )
    {
        "date", "datetime", "datetime2", "smalldatetime", "datetimeoffset", "time",
        "uniqueidentifier", "timestamp",
        "geography", "geometry", "hierarchyid", "sql_variant", "xml"
    };

    /// <summary>
    /// The most row keys listed in one finding.
    /// </summary>
    private const int MaximumExampleCount = 5;

    /// <summary>
    /// Compares the rows of every table present in both catalogs. Rows
    /// are matched by primary key when both tables have the same one,
    /// otherwise the tables are compared as multisets of row hashes.
    /// </summary>
    /// <param name="leftConnectionString">The first database's connection string.</param>
    /// <param name="left">The first database's catalog.</param>
    /// <param name="rightConnectionString">The second database's connection string.</param>
    /// <param name="right">The second database's catalog.</param>
    /// <returns>One finding per table and kind of difference.</returns>
    public static List<SchemaFinding> Compare( string leftConnectionString, SchemaCatalog left, string rightConnectionString, SchemaCatalog right )
    {
        var findings = new List<SchemaFinding>();

        using var leftConnection = new SqlConnection( SchemaCatalogReader.CreateConnectionString( leftConnectionString ) );
        using var rightConnection = new SqlConnection( SchemaCatalogReader.CreateConnectionString( rightConnectionString ) );
        leftConnection.Open();
        rightConnection.Open();

        foreach ( var leftTable in left.Tables.Values.OrderBy( t => t.Key, StringComparer.OrdinalIgnoreCase ) )
        {
            if ( !right.Tables.TryGetValue( leftTable.Key, out var rightTable ) )
            {
                continue;
            }

            var sql = BuildRowHashSql( leftTable, rightTable );
            var leftRows = ReadRowHashes( leftConnection, sql );
            var rightRows = ReadRowHashes( rightConnection, sql );

            CompareRows( findings, leftTable.Key, leftRows, rightRows );
        }

        return findings;
    }

    /// <summary>
    /// Builds the query that returns a key and a hash for each row. The
    /// key is the primary key as JSON, or empty when the two tables do
    /// not share a primary key.
    /// </summary>
    internal static string BuildRowHashSql( SchemaCatalogTable left, SchemaCatalogTable right )
    {
        var comparedColumns = left.Columns.Values
            .Where( c => !ExcludedStoreTypes.Contains( c.StoreType )
                && right.Columns.TryGetValue( c.Name, out var other )
                && other.StoreType == c.StoreType )
            .Select( c => c.Name )
            .OrderBy( n => n, StringComparer.OrdinalIgnoreCase )
            .ToList();

        var leftKey = left.PrimaryKey?.KeyColumns ?? new List<string>();
        var rightKey = right.PrimaryKey?.KeyColumns ?? new List<string>();
        var keyColumns = leftKey.SequenceEqual( rightKey, StringComparer.OrdinalIgnoreCase )
            ? leftKey.Select( c => c.EndsWith( " DESC" ) ? c.Substring( 0, c.Length - 5 ) : c ).ToList()
            : new List<string>();

        var keyExpression = keyColumns.Count == 0
            ? "N''"
            : $"( SELECT {SelectList( keyColumns )} FOR JSON PATH, WITHOUT_ARRAY_WRAPPER, INCLUDE_NULL_VALUES )";
        var hashExpression = comparedColumns.Count == 0
            ? "''"
            : $"CONVERT( VARCHAR(64), HASHBYTES( 'SHA2_256', ( SELECT {SelectList( comparedColumns )} FOR JSON PATH, WITHOUT_ARRAY_WRAPPER, INCLUDE_NULL_VALUES ) ), 2 )";

        return $"SELECT {keyExpression} AS [RowKey], {hashExpression} AS [RowHash] FROM [{Escape( left.Schema )}].[{Escape( left.Name )}] AS [t]";
    }

    /// <summary>
    /// Builds a select list of <c>[t].[Column] AS [Column]</c> entries.
    /// </summary>
    private static string SelectList( IEnumerable<string> columnNames )
    {
        return string.Join( ", ", columnNames.Select( n => $"[t].[{Escape( n )}] AS [{Escape( n )}]" ) );
    }

    /// <summary>
    /// Escapes a closing bracket in an identifier.
    /// </summary>
    private static string Escape( string identifier )
    {
        return identifier.Replace( "]", "]]" );
    }

    /// <summary>
    /// Runs the row hash query and returns each row's key and hash.
    /// </summary>
    private static List<KeyValuePair<string, string>> ReadRowHashes( SqlConnection connection, string sql )
    {
        var rows = new List<KeyValuePair<string, string>>();

        using var command = new SqlCommand( sql, connection );
        command.CommandTimeout = 600;

        using var reader = command.ExecuteReader();
        while ( reader.Read() )
        {
            rows.Add( new KeyValuePair<string, string>( reader.GetString( 0 ), reader.GetString( 1 ) ) );
        }

        return rows;
    }

    /// <summary>
    /// Compares the rows of one table and adds a finding for each kind of difference.
    /// </summary>
    internal static void CompareRows( List<SchemaFinding> findings, string tableKey, List<KeyValuePair<string, string>> leftRows, List<KeyValuePair<string, string>> rightRows )
    {
        var isKeyed = leftRows.Concat( rightRows ).Any( r => r.Key.Length > 0 );

        if ( !isKeyed )
        {
            var onlyInLeft = SubtractMultiset( leftRows.Select( r => r.Value ), rightRows.Select( r => r.Value ) );
            var onlyInRight = SubtractMultiset( rightRows.Select( r => r.Value ), leftRows.Select( r => r.Value ) );

            AddRowFinding( findings, "RowsOnlyInLeft", tableKey, onlyInLeft.Count, "rows (table has no shared primary key) are only in the left database", onlyInLeft );
            AddRowFinding( findings, "RowsOnlyInRight", tableKey, onlyInRight.Count, "rows (table has no shared primary key) are only in the right database", onlyInRight );

            return;
        }

        var leftByKey = leftRows.ToDictionary( r => r.Key, r => r.Value );
        var rightByKey = rightRows.ToDictionary( r => r.Key, r => r.Value );
        var leftOnlyKeys = leftByKey.Keys.Where( k => !rightByKey.ContainsKey( k ) ).OrderBy( k => k ).ToList();
        var rightOnlyKeys = rightByKey.Keys.Where( k => !leftByKey.ContainsKey( k ) ).OrderBy( k => k ).ToList();
        var differentKeys = leftByKey.Where( kv => rightByKey.TryGetValue( kv.Key, out var rightHash ) && rightHash != kv.Value ).Select( kv => kv.Key ).OrderBy( k => k ).ToList();

        AddRowFinding( findings, "RowsOnlyInLeft", tableKey, leftOnlyKeys.Count, "rows are only in the left database", leftOnlyKeys );
        AddRowFinding( findings, "RowsOnlyInRight", tableKey, rightOnlyKeys.Count, "rows are only in the right database", rightOnlyKeys );
        AddRowFinding( findings, "RowsDiffer", tableKey, differentKeys.Count, "rows have different values", differentKeys );
    }

    /// <summary>
    /// Returns the items of <paramref name="items"/> left after removing
    /// one occurrence for each item of <paramref name="remove"/>.
    /// </summary>
    private static List<string> SubtractMultiset( IEnumerable<string> items, IEnumerable<string> remove )
    {
        var remaining = remove.GroupBy( r => r ).ToDictionary( g => g.Key, g => g.Count() );
        var result = new List<string>();

        foreach ( var item in items )
        {
            if ( remaining.TryGetValue( item, out var count ) && count > 0 )
            {
                remaining[item] = count - 1;
            }
            else
            {
                result.Add( item );
            }
        }

        return result;
    }

    /// <summary>
    /// Adds a failure listing up to <see cref="MaximumExampleCount"/> examples, when there is a difference.
    /// </summary>
    private static void AddRowFinding( List<SchemaFinding> findings, string code, string tableKey, int count, string description, List<string> examples )
    {
        if ( count == 0 )
        {
            return;
        }

        var exampleText = string.Join( "; ", examples.Take( MaximumExampleCount ) );

        findings.Add( SchemaFinding.Failure( SchemaChecks.DataCheck, code, tableKey, $"{count} {description}. For example: {exampleText}" ) );
    }
}
