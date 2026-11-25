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

/// <summary>
/// Reads a <see cref="SchemaCatalog"/> from a database through the
/// <c>sys.*</c> catalog views, with plain ADO.NET and no EF.
/// </summary>
internal static class SchemaCatalogReader
{
    #region Catalog Queries

    /// <summary>
    /// Every catalog query in one batch, read as consecutive result sets.
    /// </summary>
    private const string CatalogSql = @"
SELECT [s].[name] AS [SchemaName]
    , [t].[name] AS [TableName]
    , [c].[name] AS [ColumnName]
    , CASE WHEN [ty].[is_user_defined] = 1 AND [ty].[is_assembly_type] = 0 THEN TYPE_NAME( [c].[system_type_id] ) ELSE [ty].[name] END AS [StoreType]
    , [c].[max_length] AS [MaxLength]
    , [c].[precision] AS [Precision]
    , [c].[scale] AS [Scale]
    , [c].[is_nullable] AS [IsNullable]
    , [c].[is_identity] AS [IsIdentity]
    , [cc].[definition] AS [ComputedDefinition]
    , [dc].[definition] AS [DefaultDefinition]
    , CAST( CASE WHEN [t].[type] = 'V' THEN 1 ELSE 0 END AS BIT ) AS [IsView]
FROM [sys].[objects] AS [t]
INNER JOIN [sys].[schemas] AS [s] ON [s].[schema_id] = [t].[schema_id]
INNER JOIN [sys].[columns] AS [c] ON [c].[object_id] = [t].[object_id]
INNER JOIN [sys].[types] AS [ty] ON [ty].[user_type_id] = [c].[user_type_id]
LEFT OUTER JOIN [sys].[computed_columns] AS [cc] ON [cc].[object_id] = [c].[object_id] AND [cc].[column_id] = [c].[column_id]
LEFT OUTER JOIN [sys].[default_constraints] AS [dc] ON [dc].[parent_object_id] = [c].[object_id] AND [dc].[parent_column_id] = [c].[column_id]
WHERE [t].[is_ms_shipped] = 0
    AND [t].[type] IN ( 'U', 'V' )
ORDER BY [s].[name], [t].[name], [c].[column_id];

SELECT [s].[name] AS [SchemaName]
    , [t].[name] AS [TableName]
    , [i].[name] AS [IndexName]
    , [i].[is_primary_key] AS [IsPrimaryKey]
    , [i].[is_unique] AS [IsUnique]
    , [i].[is_unique_constraint] AS [IsUniqueConstraint]
    , CAST( CASE WHEN [i].[type] IN ( 1, 5 ) THEN 1 ELSE 0 END AS BIT ) AS [IsClustered]
    , [i].[filter_definition] AS [FilterDefinition]
    , [c].[name] AS [ColumnName]
    , [ic].[is_descending_key] AS [IsDescending]
    , [ic].[is_included_column] AS [IsIncluded]
FROM [sys].[indexes] AS [i]
INNER JOIN [sys].[tables] AS [t] ON [t].[object_id] = [i].[object_id]
INNER JOIN [sys].[schemas] AS [s] ON [s].[schema_id] = [t].[schema_id]
INNER JOIN [sys].[index_columns] AS [ic] ON [ic].[object_id] = [i].[object_id] AND [ic].[index_id] = [i].[index_id]
INNER JOIN [sys].[columns] AS [c] ON [c].[object_id] = [ic].[object_id] AND [c].[column_id] = [ic].[column_id]
WHERE [t].[is_ms_shipped] = 0
    AND [i].[type] > 0
ORDER BY [s].[name], [t].[name], [i].[name], [ic].[is_included_column], [ic].[key_ordinal], [c].[name];

SELECT [s].[name] AS [SchemaName]
    , [t].[name] AS [TableName]
    , [fk].[name] AS [ForeignKeyName]
    , [fk].[delete_referential_action_desc] AS [DeleteAction]
    , [rs].[name] AS [ReferencedSchemaName]
    , [rt].[name] AS [ReferencedTableName]
    , [pc].[name] AS [ColumnName]
    , [rc].[name] AS [ReferencedColumnName]
FROM [sys].[foreign_keys] AS [fk]
INNER JOIN [sys].[tables] AS [t] ON [t].[object_id] = [fk].[parent_object_id]
INNER JOIN [sys].[schemas] AS [s] ON [s].[schema_id] = [t].[schema_id]
INNER JOIN [sys].[tables] AS [rt] ON [rt].[object_id] = [fk].[referenced_object_id]
INNER JOIN [sys].[schemas] AS [rs] ON [rs].[schema_id] = [rt].[schema_id]
INNER JOIN [sys].[foreign_key_columns] AS [fkc] ON [fkc].[constraint_object_id] = [fk].[object_id]
INNER JOIN [sys].[columns] AS [pc] ON [pc].[object_id] = [fkc].[parent_object_id] AND [pc].[column_id] = [fkc].[parent_column_id]
INNER JOIN [sys].[columns] AS [rc] ON [rc].[object_id] = [fkc].[referenced_object_id] AND [rc].[column_id] = [fkc].[referenced_column_id]
WHERE [t].[is_ms_shipped] = 0
ORDER BY [s].[name], [t].[name], [fk].[name], [fkc].[constraint_column_id];

SELECT [s].[name] AS [SchemaName]
    , [t].[name] AS [TableName]
    , [ck].[definition] AS [Definition]
FROM [sys].[check_constraints] AS [ck]
INNER JOIN [sys].[tables] AS [t] ON [t].[object_id] = [ck].[parent_object_id]
INNER JOIN [sys].[schemas] AS [s] ON [s].[schema_id] = [t].[schema_id]
WHERE [t].[is_ms_shipped] = 0
ORDER BY [s].[name], [t].[name], [ck].[definition];

SELECT [s].[name] AS [SchemaName]
    , [o].[name] AS [ObjectName]
    , [o].[type_desc] AS [ObjectType]
    , [m].[definition] AS [Definition]
FROM [sys].[sql_modules] AS [m]
INNER JOIN [sys].[objects] AS [o] ON [o].[object_id] = [m].[object_id]
INNER JOIN [sys].[schemas] AS [s] ON [s].[schema_id] = [o].[schema_id]
WHERE [o].[is_ms_shipped] = 0
ORDER BY [s].[name], [o].[name];";

    #endregion Catalog Queries

    /// <summary>
    /// Builds the connection string the schema checks use: trusts the
    /// server certificate only where Rock's EF6 workload, which uses
    /// <c>System.Data.SqlClient</c>, would not check it (see
    /// <see cref="SqlConnectionStringDefaults"/>), and names the application
    /// so the session is recognizable on the server.
    /// </summary>
    /// <param name="connectionString">The connection string to start from.</param>
    /// <returns>The connection string to open.</returns>
    public static string CreateConnectionString( string connectionString )
    {
        var builder = new SqlConnectionStringBuilder( connectionString )
        {
            ApplicationName = "Rock Schema Check"
        };

        SqlConnectionStringDefaults.TrustServerCertificateUnlessEncryptionRequested( builder );

        return builder.ConnectionString;
    }

    /// <summary>
    /// Reads the catalog of the database the connection string points to.
    /// </summary>
    /// <param name="connectionString">The connection string of the database to read.</param>
    /// <returns>The database's catalog.</returns>
    public static SchemaCatalog Read( string connectionString )
    {
        var catalog = new SchemaCatalog();

        using var connection = new SqlConnection( CreateConnectionString( connectionString ) );
        connection.Open();

        using var command = new SqlCommand( CatalogSql, connection );
        command.CommandTimeout = 300;

        using var reader = command.ExecuteReader();
        ReadColumns( reader, catalog );
        reader.NextResult();
        ReadIndexes( reader, catalog );
        reader.NextResult();
        ReadForeignKeys( reader, catalog );
        reader.NextResult();
        ReadCheckConstraints( reader, catalog );
        reader.NextResult();
        ReadModules( reader, catalog );

        return catalog;
    }

    /// <summary>
    /// Reads the tables, views and their columns from the first result set.
    /// </summary>
    private static void ReadColumns( SqlDataReader reader, SchemaCatalog catalog )
    {
        while ( reader.Read() )
        {
            var table = reader.GetBoolean( 11 )
                ? GetOrAddView( catalog, reader.GetString( 0 ), reader.GetString( 1 ) )
                : GetOrAddTable( catalog, reader.GetString( 0 ), reader.GetString( 1 ) );
            var storeType = reader.GetString( 3 ).ToLowerInvariant();
            int byteLength = reader.GetInt16( 4 );
            int precision = reader.GetByte( 5 );
            int scale = reader.GetByte( 6 );

            table.AddColumn( new SchemaCatalogColumn
            {
                Name = reader.GetString( 2 ),
                StoreType = storeType,
                MaxLength = ToCharacterLength( storeType, byteLength ),
                Precision = ToPrecision( storeType, precision, scale ),
                Scale = storeType == "decimal" || storeType == "numeric" ? scale : ( int? ) null,
                IsNullable = reader.GetBoolean( 7 ),
                IsIdentity = reader.GetBoolean( 8 ),
                ComputedDefinition = reader.IsDBNull( 9 ) ? null : reader.GetString( 9 ),
                DefaultDefinition = reader.IsDBNull( 10 ) ? null : reader.GetString( 10 )
            } );
        }
    }

    /// <summary>
    /// Reads the indexes, unique constraints and primary keys. Rows are
    /// ordered so each index's key columns arrive in key order, followed
    /// by its included columns.
    /// </summary>
    private static void ReadIndexes( SqlDataReader reader, SchemaCatalog catalog )
    {
        SchemaCatalogIndex current = null;
        string currentKey = null;

        while ( reader.Read() )
        {
            var table = GetOrAddTable( catalog, reader.GetString( 0 ), reader.GetString( 1 ) );
            var indexName = reader.GetString( 2 );
            var rowKey = $"{table.Key}|{indexName}";

            if ( rowKey != currentKey )
            {
                currentKey = rowKey;
                current = new SchemaCatalogIndex
                {
                    Name = indexName,
                    IsUnique = reader.GetBoolean( 4 ),
                    IsUniqueConstraint = reader.GetBoolean( 5 ),
                    IsClustered = reader.GetBoolean( 6 ),
                    FilterDefinition = reader.IsDBNull( 7 ) ? null : reader.GetString( 7 )
                };

                if ( reader.GetBoolean( 3 ) )
                {
                    table.PrimaryKey = current;
                }
                else
                {
                    table.Indexes.Add( current );
                }
            }

            var columnName = reader.GetString( 8 );

            if ( reader.GetBoolean( 10 ) )
            {
                current.IncludedColumns.Add( columnName );
            }
            else
            {
                current.KeyColumns.Add( reader.GetBoolean( 9 ) ? $"{columnName} DESC" : columnName );
            }
        }
    }

    /// <summary>
    /// Reads the foreign keys, one row per column pair in constraint order.
    /// </summary>
    private static void ReadForeignKeys( SqlDataReader reader, SchemaCatalog catalog )
    {
        SchemaCatalogForeignKey current = null;
        string currentKey = null;

        while ( reader.Read() )
        {
            var table = GetOrAddTable( catalog, reader.GetString( 0 ), reader.GetString( 1 ) );
            var foreignKeyName = reader.GetString( 2 );
            var rowKey = $"{table.Key}|{foreignKeyName}";

            if ( rowKey != currentKey )
            {
                currentKey = rowKey;

                var deleteAction = reader.GetString( 3 );
                current = new SchemaCatalogForeignKey
                {
                    Name = foreignKeyName,
                    DeleteAction = deleteAction,
                    IsCascadeDelete = deleteAction == "CASCADE",
                    ReferencedTable = $"{reader.GetString( 4 )}.{reader.GetString( 5 )}"
                };

                table.ForeignKeys.Add( current );
            }

            current.Columns.Add( reader.GetString( 6 ) );
            current.ReferencedColumns.Add( reader.GetString( 7 ) );
        }
    }

    /// <summary>
    /// Reads the check constraint definitions.
    /// </summary>
    private static void ReadCheckConstraints( SqlDataReader reader, SchemaCatalog catalog )
    {
        while ( reader.Read() )
        {
            GetOrAddTable( catalog, reader.GetString( 0 ), reader.GetString( 1 ) ).CheckConstraints.Add( reader.GetString( 2 ) );
        }
    }

    /// <summary>
    /// Reads the views, procedures, functions and triggers.
    /// </summary>
    private static void ReadModules( SqlDataReader reader, SchemaCatalog catalog )
    {
        while ( reader.Read() )
        {
            var key = $"{reader.GetString( 0 )}.{reader.GetString( 1 )}";

            catalog.Modules[key] = new SchemaCatalogModule
            {
                Key = key,
                ObjectType = reader.GetString( 2 ),
                Definition = reader.IsDBNull( 3 ) ? string.Empty : NormalizeDefinition( reader.GetString( 3 ) )
            };
        }
    }

    /// <summary>
    /// Normalizes line endings and trailing whitespace so a module
    /// scripted on a different machine or by SSMS compares equal.
    /// </summary>
    /// <param name="definition">The definition as stored by SQL Server.</param>
    /// <returns>The normalized definition.</returns>
    internal static string NormalizeDefinition( string definition )
    {
        var lines = definition.Replace( "\r\n", "\n" ).Replace( '\r', '\n' ).Split( '\n' );

        return string.Join( "\n", lines.Select( l => l.TrimEnd() ) ).Trim();
    }

    /// <summary>
    /// Converts <c>sys.columns.max_length</c> (bytes) to the length EF
    /// and DDL use: characters for Unicode types, <c>-1</c> for <c>max</c>,
    /// <c>null</c> for types without a length.
    /// </summary>
    private static int? ToCharacterLength( string storeType, int byteLength )
    {
        switch ( storeType )
        {
            case "nchar":
            case "nvarchar":
                return byteLength == -1 ? -1 : byteLength / 2;

            case "char":
            case "varchar":
            case "binary":
            case "varbinary":
                return byteLength;

            default:
                return null;
        }
    }

    /// <summary>
    /// Returns the precision that matters for the type: numeric precision
    /// for <c>decimal</c> and <c>numeric</c>, fractional seconds for the
    /// newer date and time types, otherwise <c>null</c>.
    /// </summary>
    private static int? ToPrecision( string storeType, int precision, int scale )
    {
        switch ( storeType )
        {
            case "decimal":
            case "numeric":
                return precision;

            case "datetime2":
            case "time":
            case "datetimeoffset":
                return scale;

            default:
                return null;
        }
    }

    /// <summary>
    /// Finds a view in the catalog, adding it when it is not there yet.
    /// </summary>
    private static SchemaCatalogTable GetOrAddView( SchemaCatalog catalog, string schema, string name )
    {
        var key = $"{schema}.{name}";

        if ( !catalog.Views.TryGetValue( key, out var view ) )
        {
            view = new SchemaCatalogTable { Schema = schema, Name = name };
            catalog.Views.Add( key, view );
        }

        return view;
    }

    /// <summary>
    /// Finds a table in the catalog, adding it when it is not there yet.
    /// </summary>
    private static SchemaCatalogTable GetOrAddTable( SchemaCatalog catalog, string schema, string name )
    {
        if ( catalog.Tables.TryGetValue( $"{schema}.{name}", out var table ) )
        {
            return table;
        }

        return catalog.AddTable( new SchemaCatalogTable { Schema = schema, Name = name } );
    }
}
