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
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

using Microsoft.Data.SqlClient;

namespace Rock.Data;

/// <summary>
/// Loads a table's rows from a JSON data file with <see cref="SqlBulkCopy"/>.
/// The baseline loads all of its data this way.
/// </summary>
/// <remarks>
/// <para>
/// The format is what the baseline tool's <c>export</c> writes:
/// </para>
/// <code>
/// {
///   "generateGuid": true,
///   "columns": [ "Id", "Name" ],
///   "rows": [
///     [ 1, "One" ],
///     [ 2, null ]
///   ]
/// }
/// </code>
/// <para>
/// Each row is an array of values in column order. Strings, numbers,
/// <c>true</c>/<c>false</c> and <c>null</c> are JSON values; dates are
/// strings in <c>yyyy-MM-ddTHH:mm:ss.fffffff</c> form (with <c>zzz</c> for
/// <c>datetimeoffset</c>, <c>c</c> form for <c>time</c>); GUIDs are
/// strings; binary, including <c>geography</c>, <c>geometry</c> and
/// <c>hierarchyid</c> in SQL Server's own serialized form, is base64.
/// Line breaks in strings are JSON escapes, so each row stays on one line.
/// </para>
/// <para>
/// The file must name every column the table can be given a value for,
/// except <c>Guid</c> when <c>generateGuid</c> is true: then every row gets
/// a new one, so each database loaded from the file has its own values.
/// Identity values are kept, and <c>CHECK</c> constraints are checked; a
/// bulk copy skips them by default, which leaves them untrusted, and the
/// query optimizer ignores untrusted constraints.
/// </para>
/// </remarks>
internal static class MigrationBulkLoader
{
    /// <summary>
    /// Rows sent to the server per bulk copy, so a large table is never
    /// held in memory at once.
    /// </summary>
    private const int RowsPerBatch = 10000;

    /// <summary>
    /// The <c>system_type_id</c> SQL Server gives every CLR type:
    /// <c>geography</c>, <c>geometry</c> and <c>hierarchyid</c>. Their
    /// serialized bytes load into the column as they are.
    /// </summary>
    private const int ClrSystemTypeId = 240;

    /// <summary>
    /// The .NET type each SQL Server base type loads as.
    /// </summary>
    private static readonly Dictionary<string, Type> TypesBySqlName = new Dictionary<string, Type>( StringComparer.OrdinalIgnoreCase )
    {
        ["bit"] = typeof( bool ),
        ["tinyint"] = typeof( byte ),
        ["smallint"] = typeof( short ),
        ["int"] = typeof( int ),
        ["bigint"] = typeof( long ),
        ["decimal"] = typeof( decimal ),
        ["numeric"] = typeof( decimal ),
        ["money"] = typeof( decimal ),
        ["smallmoney"] = typeof( decimal ),
        ["float"] = typeof( double ),
        ["real"] = typeof( float ),
        ["date"] = typeof( DateTime ),
        ["datetime"] = typeof( DateTime ),
        ["datetime2"] = typeof( DateTime ),
        ["smalldatetime"] = typeof( DateTime ),
        ["datetimeoffset"] = typeof( DateTimeOffset ),
        ["time"] = typeof( TimeSpan ),
        ["char"] = typeof( string ),
        ["varchar"] = typeof( string ),
        ["text"] = typeof( string ),
        ["nchar"] = typeof( string ),
        ["nvarchar"] = typeof( string ),
        ["ntext"] = typeof( string ),
        ["xml"] = typeof( string ),
        ["uniqueidentifier"] = typeof( Guid ),
        ["binary"] = typeof( byte[] ),
        ["varbinary"] = typeof( byte[] ),
        ["image"] = typeof( byte[] )
    };

    /// <summary>
    /// Loads every row of a JSON data file into a table.
    /// </summary>
    /// <param name="connection">The open connection.</param>
    /// <param name="transaction">The transaction to load in.</param>
    /// <param name="table">The <c>dbo</c> table to load.</param>
    /// <param name="json">The file's UTF-8 bytes.</param>
    /// <param name="timeoutSeconds">The timeout for each batch.</param>
    /// <returns>The number of rows loaded.</returns>
    internal static int Load( SqlConnection connection, SqlTransaction transaction, string table, byte[] json, int timeoutSeconds )
    {
        // A file saved by an editor may start with a UTF-8 byte order mark, which the reader rejects.
        var hasByteOrderMark = json.Length >= 3 && json[0] == 0xEF && json[1] == 0xBB && json[2] == 0xBF;
        var reader = new Utf8JsonReader( new ReadOnlySpan<byte>( json, hasByteOrderMark ? 3 : 0, json.Length - ( hasByteOrderMark ? 3 : 0 ) ) );
        var isGuidGenerated = false;
        List<string> fileColumns = null;
        var rowCount = 0;

        Expect( ref reader, JsonTokenType.StartObject, table );

        while ( Read( ref reader, table ) && reader.TokenType == JsonTokenType.PropertyName )
        {
            var property = reader.GetString();
            Read( ref reader, table );

            switch ( property )
            {
                case "generateGuid":
                    isGuidGenerated = reader.GetBoolean();
                    break;

                case "columns":
                    fileColumns = ReadColumns( ref reader, table );
                    break;

                case "rows":
                    if ( fileColumns == null )
                    {
                        throw new InvalidDataException( $"The data file for {table} has \"rows\" before \"columns\"." );
                    }

                    rowCount = LoadRows( ref reader, connection, transaction, table, fileColumns, isGuidGenerated, timeoutSeconds );
                    break;

                default:
                    throw new InvalidDataException( $"The data file for {table} has an unknown property \"{property}\"." );
            }
        }

        return rowCount;
    }

    /// <summary>
    /// Reads the <c>columns</c> array.
    /// </summary>
    private static List<string> ReadColumns( ref Utf8JsonReader reader, string table )
    {
        if ( reader.TokenType != JsonTokenType.StartArray )
        {
            throw new InvalidDataException( $"The data file for {table} must have \"columns\" as an array of names." );
        }

        var columns = new List<string>();

        while ( Read( ref reader, table ) && reader.TokenType != JsonTokenType.EndArray )
        {
            columns.Add( reader.GetString() );
        }

        return columns;
    }

    /// <summary>
    /// Reads the <c>rows</c> array and bulk copies the rows in batches.
    /// </summary>
    private static int LoadRows( ref Utf8JsonReader reader, SqlConnection connection, SqlTransaction transaction, string table, List<string> fileColumns, bool isGuidGenerated, int timeoutSeconds )
    {
        if ( reader.TokenType != JsonTokenType.StartArray )
        {
            throw new InvalidDataException( $"The data file for {table} must have \"rows\" as an array of arrays." );
        }

        var rows = CreateTable( connection, transaction, table, fileColumns, isGuidGenerated );
        var types = rows.Columns.Cast<DataColumn>().Select( c => c.DataType ).ToArray();
        var rowCount = 0;

        using var bulkCopy = new SqlBulkCopy( connection, SqlBulkCopyOptions.KeepIdentity | SqlBulkCopyOptions.KeepNulls | SqlBulkCopyOptions.CheckConstraints, transaction );
        bulkCopy.DestinationTableName = $"[dbo].[{table}]";
        bulkCopy.BulkCopyTimeout = timeoutSeconds;

        foreach ( DataColumn column in rows.Columns )
        {
            bulkCopy.ColumnMappings.Add( column.ColumnName, column.ColumnName );
        }

        while ( Read( ref reader, table ) && reader.TokenType == JsonTokenType.StartArray )
        {
            var values = new object[types.Length];
            var fieldCount = 0;

            while ( Read( ref reader, table ) && reader.TokenType != JsonTokenType.EndArray )
            {
                if ( fieldCount >= fileColumns.Count )
                {
                    throw new InvalidDataException( $"Row {rowCount + 1} of the data file for {table} has more values than the {fileColumns.Count} columns." );
                }

                values[fieldCount] = ReadValue( ref reader, types[fieldCount], table, rowCount + 1, fileColumns[fieldCount] );
                fieldCount++;
            }

            if ( fieldCount != fileColumns.Count )
            {
                throw new InvalidDataException( $"Row {rowCount + 1} of the data file for {table} has {fieldCount} values, but there are {fileColumns.Count} columns." );
            }

            if ( isGuidGenerated )
            {
                values[fieldCount] = Guid.NewGuid();
            }

            rows.Rows.Add( values );
            rowCount++;

            if ( rows.Rows.Count == RowsPerBatch )
            {
                bulkCopy.WriteToServer( rows );
                rows.Clear();
            }
        }

        if ( rows.Rows.Count > 0 )
        {
            bulkCopy.WriteToServer( rows );
        }

        return rowCount;
    }

    /// <summary>
    /// Creates an empty <see cref="DataTable"/> with the file's columns,
    /// then <c>Guid</c> when it is generated, typed from the table. The
    /// file must cover every column the table takes a value for.
    /// </summary>
    private static DataTable CreateTable( SqlConnection connection, SqlTransaction transaction, string table, List<string> fileColumns, bool isGuidGenerated )
    {
        var tableColumns = ReadTableColumns( connection, transaction, table );

        if ( tableColumns.Count == 0 )
        {
            throw new InvalidDataException( $"The data file is for table {table}, which does not exist." );
        }

        var rows = new DataTable( table );

        foreach ( var name in fileColumns )
        {
            if ( !tableColumns.TryGetValue( name, out var type ) )
            {
                throw new InvalidDataException( $"The data file for {table} names column '{name}', which the table does not have or cannot be given a value." );
            }

            if ( isGuidGenerated && name.Equals( "Guid", StringComparison.OrdinalIgnoreCase ) )
            {
                throw new InvalidDataException( $"The data file for {table} sets generateGuid but also has a Guid column." );
            }

            rows.Columns.Add( name, type );
        }

        if ( isGuidGenerated )
        {
            if ( !tableColumns.TryGetValue( "Guid", out var guidType ) || guidType != typeof( Guid ) )
            {
                throw new InvalidDataException( $"The data file for {table} sets generateGuid, but the table has no uniqueidentifier Guid column." );
            }

            rows.Columns.Add( "Guid", typeof( Guid ) );
        }

        var missing = tableColumns.Keys.Where( c => !rows.Columns.Contains( c ) ).ToList();

        if ( missing.Count > 0 )
        {
            throw new InvalidDataException( $"The data file for {table} has no values for {string.Join( ", ", missing )}." );
        }

        return rows;
    }

    /// <summary>
    /// Reads the columns of a table that take a value (not computed and
    /// not <c>rowversion</c>), with the .NET type each loads as.
    /// </summary>
    private static Dictionary<string, Type> ReadTableColumns( SqlConnection connection, SqlTransaction transaction, string table )
    {
        var columns = new Dictionary<string, Type>( StringComparer.OrdinalIgnoreCase );
        // 189 is timestamp (rowversion). CLR types have no TYPE_NAME for their system_type_id.
        var sql = @"
SELECT [c].[name], [c].[system_type_id], ISNULL( TYPE_NAME( [c].[system_type_id] ), '' )
FROM [sys].[columns] AS [c]
WHERE [c].[object_id] = OBJECT_ID( @Table )
    AND [c].[is_computed] = 0
    AND [c].[system_type_id] <> 189
ORDER BY [c].[column_id];";

        using var command = new SqlCommand( sql, connection, transaction );
        command.Parameters.AddWithValue( "@Table", $"[dbo].[{table}]" );

        using var reader = command.ExecuteReader();
        while ( reader.Read() )
        {
            var name = reader.GetString( 0 );
            var typeName = reader.GetString( 2 );

            if ( Convert.ToInt32( reader.GetValue( 1 ) ) == ClrSystemTypeId )
            {
                columns[name] = typeof( byte[] );
            }
            else if ( TypesBySqlName.TryGetValue( typeName, out var type ) )
            {
                columns[name] = type;
            }
            else
            {
                throw new NotSupportedException( $"Bulk loading does not support {table}.{name}, which is {typeName}." );
            }
        }

        return columns;
    }

    /// <summary>
    /// Reads one value as the column's type.
    /// </summary>
    private static object ReadValue( ref Utf8JsonReader reader, Type type, string table, int row, string column )
    {
        if ( reader.TokenType == JsonTokenType.Null )
        {
            return DBNull.Value;
        }

        try
        {
            if ( type == typeof( string ) )
            {
                return reader.GetString();
            }

            if ( type == typeof( int ) )
            {
                return reader.GetInt32();
            }

            if ( type == typeof( long ) )
            {
                return reader.GetInt64();
            }

            if ( type == typeof( short ) )
            {
                return reader.GetInt16();
            }

            if ( type == typeof( byte ) )
            {
                return reader.GetByte();
            }

            if ( type == typeof( decimal ) )
            {
                return reader.GetDecimal();
            }

            if ( type == typeof( double ) )
            {
                return reader.GetDouble();
            }

            if ( type == typeof( float ) )
            {
                return reader.GetSingle();
            }

            if ( type == typeof( bool ) )
            {
                return reader.GetBoolean();
            }

            if ( type == typeof( Guid ) )
            {
                return reader.GetGuid();
            }

            if ( type == typeof( DateTime ) )
            {
                return DateTime.ParseExact( reader.GetString(), "yyyy-MM-ddTHH:mm:ss.fffffff", CultureInfo.InvariantCulture );
            }

            if ( type == typeof( DateTimeOffset ) )
            {
                return DateTimeOffset.ParseExact( reader.GetString(), "yyyy-MM-ddTHH:mm:ss.fffffffzzz", CultureInfo.InvariantCulture );
            }

            if ( type == typeof( TimeSpan ) )
            {
                return TimeSpan.ParseExact( reader.GetString(), "c", CultureInfo.InvariantCulture );
            }

            return reader.GetBytesFromBase64();
        }
        catch ( Exception ex ) when ( ex is InvalidOperationException || ex is FormatException )
        {
            throw new InvalidDataException( $"Row {row} of the data file for {table} has a value for {column} that is not a valid {type.Name}.", ex );
        }
    }

    /// <summary>
    /// Reads the next token, failing at the end of the data.
    /// </summary>
    private static bool Read( ref Utf8JsonReader reader, string table )
    {
        if ( !reader.Read() )
        {
            throw new InvalidDataException( $"The data file for {table} ends early." );
        }

        return true;
    }

    /// <summary>
    /// Reads the next token and fails unless it is the expected kind.
    /// </summary>
    private static void Expect( ref Utf8JsonReader reader, JsonTokenType tokenType, string table )
    {
        Read( ref reader, table );

        if ( reader.TokenType != tokenType )
        {
            throw new InvalidDataException( $"The data file for {table} must start with a JSON object." );
        }
    }
}
