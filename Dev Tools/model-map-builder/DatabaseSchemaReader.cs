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
using System.Data.SqlClient;
using System.Linq;

namespace Rock.ModelMapBuilder
{
    /// <summary>
    /// The physical database schema for a single table: its columns (keyed by
    /// column name), indexes, and foreign keys.
    /// </summary>
    internal class TableSchema
    {
        public Dictionary<string, ColumnSchema> Columns { get; } = new Dictionary<string, ColumnSchema>( StringComparer.OrdinalIgnoreCase );

        public List<ModelMapIndexInfo> Indexes { get; } = new List<ModelMapIndexInfo>();

        public List<ModelMapForeignKeyInfo> ForeignKeys { get; } = new List<ModelMapForeignKeyInfo>();
    }

    /// <summary>
    /// The physical schema of a single database column.
    /// </summary>
    internal class ColumnSchema
    {
        public string DataType { get; set; }

        public int? Length { get; set; }

        public int? Scale { get; set; }

        public bool IsNullable { get; set; }

        public bool IsPrimaryKey { get; set; }
    }

    /// <summary>
    /// Reads the physical database schema (columns, primary keys, indexes, and
    /// foreign keys) for every base table in a single set of catalog queries.
    /// These facts (SQL types, lengths, indexes, FK targets) do not exist in the
    /// C# model and are only available from the database itself.
    /// </summary>
    internal static class DatabaseSchemaReader
    {
        /// <summary>
        /// Loads the schema for all base tables, keyed by table name.
        /// </summary>
        /// <param name="connectionString">The Rock database connection string.</param>
        /// <returns>A dictionary of table name to <see cref="TableSchema"/>.</returns>
        public static Dictionary<string, TableSchema> Load( string connectionString )
        {
            var tables = new Dictionary<string, TableSchema>( StringComparer.OrdinalIgnoreCase );

            using ( var connection = new SqlConnection( connectionString ) )
            {
                connection.Open();

                LoadColumns( connection, tables );
                LoadPrimaryKeys( connection, tables );
                LoadIndexes( connection, tables );
                LoadForeignKeys( connection, tables );
            }

            return tables;
        }

        /// <summary>
        /// Loads every registered entity type (an <c>[EntityType]</c> row where
        /// <c>IsEntity</c> is true) from the database. Reading the table directly
        /// avoids <c>EntityTypeCache.All()</c>, which eagerly loads every CLR type
        /// and throws in a headless process.
        /// </summary>
        /// <param name="connectionString">The Rock database connection string.</param>
        /// <returns>The entity types, ordered by name.</returns>
        public static List<ModelMapEntityType> LoadEntityTypes( string connectionString )
        {
            var entityTypes = new List<ModelMapEntityType>();

            using ( var connection = new SqlConnection( connectionString ) )
            {
                connection.Open();

                const string sql = @"
SELECT [Name], [Guid]
FROM   [EntityType]
WHERE  [IsEntity] = 1 AND [Name] IS NOT NULL
ORDER BY [Name]";

                using ( var reader = ExecuteReader( connection, sql ) )
                {
                    while ( reader.Read() )
                    {
                        var name = reader["Name"].ToString();

                        entityTypes.Add( new ModelMapEntityType
                        {
                            Name = name,
                            Model = name.Replace( "Rock.Model.", string.Empty ),
                            Guid = ( Guid ) reader["Guid"]
                        } );
                    }
                }
            }

            return entityTypes;
        }

        /// <summary>
        /// Gets (creating if necessary) the schema entry for a table.
        /// </summary>
        /// <param name="tables">The table dictionary.</param>
        /// <param name="tableName">The table name.</param>
        private static TableSchema GetOrAdd( Dictionary<string, TableSchema> tables, string tableName )
        {
            if ( !tables.TryGetValue( tableName, out var table ) )
            {
                table = new TableSchema();
                tables[tableName] = table;
            }

            return table;
        }

        /// <summary>
        /// Loads column types, lengths, scales, and nullability for all base tables.
        /// </summary>
        private static void LoadColumns( SqlConnection connection, Dictionary<string, TableSchema> tables )
        {
            const string sql = @"
SELECT c.[TABLE_NAME], c.[COLUMN_NAME], c.[DATA_TYPE],
       c.[CHARACTER_MAXIMUM_LENGTH], c.[NUMERIC_PRECISION], c.[NUMERIC_SCALE], c.[IS_NULLABLE]
FROM   [INFORMATION_SCHEMA].[COLUMNS] AS [c]
JOIN   [INFORMATION_SCHEMA].[TABLES] AS [t]
    ON [t].[TABLE_NAME] = [c].[TABLE_NAME] AND [t].[TABLE_SCHEMA] = [c].[TABLE_SCHEMA]
WHERE  [t].[TABLE_TYPE] = 'BASE TABLE'";

            using ( var reader = ExecuteReader( connection, sql ) )
            {
                while ( reader.Read() )
                {
                    var tableName = reader["TABLE_NAME"].ToString();
                    var columnName = reader["COLUMN_NAME"].ToString();
                    var dataType = reader["DATA_TYPE"].ToString();

                    var column = new ColumnSchema
                    {
                        DataType = dataType,
                        IsNullable = string.Equals( reader["IS_NULLABLE"].ToString(), "YES", StringComparison.OrdinalIgnoreCase )
                    };

                    // Character and binary types carry a length; decimal/numeric
                    // types carry precision and scale.
                    if ( IsLengthType( dataType ) )
                    {
                        column.Length = GetNullableInt( reader["CHARACTER_MAXIMUM_LENGTH"] );
                    }
                    else if ( dataType.Equals( "decimal", StringComparison.OrdinalIgnoreCase ) || dataType.Equals( "numeric", StringComparison.OrdinalIgnoreCase ) )
                    {
                        column.Length = GetNullableInt( reader["NUMERIC_PRECISION"] );
                        column.Scale = GetNullableInt( reader["NUMERIC_SCALE"] );
                    }

                    GetOrAdd( tables, tableName ).Columns[columnName] = column;
                }
            }
        }

        /// <summary>
        /// Marks primary key columns on the previously-loaded columns.
        /// </summary>
        private static void LoadPrimaryKeys( SqlConnection connection, Dictionary<string, TableSchema> tables )
        {
            const string sql = @"
SELECT [tc].[TABLE_NAME], [kcu].[COLUMN_NAME]
FROM   [INFORMATION_SCHEMA].[TABLE_CONSTRAINTS] AS [tc]
JOIN   [INFORMATION_SCHEMA].[KEY_COLUMN_USAGE] AS [kcu]
    ON [kcu].[CONSTRAINT_NAME] = [tc].[CONSTRAINT_NAME] AND [kcu].[TABLE_NAME] = [tc].[TABLE_NAME]
WHERE  [tc].[CONSTRAINT_TYPE] = 'PRIMARY KEY'";

            using ( var reader = ExecuteReader( connection, sql ) )
            {
                while ( reader.Read() )
                {
                    var tableName = reader["TABLE_NAME"].ToString();
                    var columnName = reader["COLUMN_NAME"].ToString();

                    if ( tables.TryGetValue( tableName, out var table ) && table.Columns.TryGetValue( columnName, out var column ) )
                    {
                        column.IsPrimaryKey = true;
                    }
                }
            }
        }

        /// <summary>
        /// Loads indexes (name, uniqueness, primary-key flag, and ordered key columns).
        /// </summary>
        private static void LoadIndexes( SqlConnection connection, Dictionary<string, TableSchema> tables )
        {
            const string sql = @"
SELECT [t].[name] AS [TableName], [i].[name] AS [IndexName],
       [i].[is_unique] AS [IsUnique], [i].[is_primary_key] AS [IsPrimaryKey],
       [c].[name] AS [ColumnName], [ic].[key_ordinal] AS [KeyOrdinal]
FROM   [sys].[indexes] AS [i]
JOIN   [sys].[tables] AS [t] ON [t].[object_id] = [i].[object_id]
JOIN   [sys].[index_columns] AS [ic]
    ON [ic].[object_id] = [i].[object_id] AND [ic].[index_id] = [i].[index_id]
JOIN   [sys].[columns] AS [c]
    ON [c].[object_id] = [ic].[object_id] AND [c].[column_id] = [ic].[column_id]
WHERE  [i].[type] > 0 AND [ic].[is_included_column] = 0 AND [i].[name] IS NOT NULL
ORDER BY [t].[name], [i].[name], [ic].[key_ordinal]";

            // Group the flat rows into one index per (table, index name).
            var indexColumns = new Dictionary<string, ModelMapIndexInfo>();

            using ( var reader = ExecuteReader( connection, sql ) )
            {
                while ( reader.Read() )
                {
                    var tableName = reader["TableName"].ToString();
                    var indexName = reader["IndexName"].ToString();
                    var columnName = reader["ColumnName"].ToString();
                    var key = $"{tableName}|{indexName}";

                    if ( !indexColumns.TryGetValue( key, out var index ) )
                    {
                        index = new ModelMapIndexInfo
                        {
                            Name = indexName,
                            IsUnique = Convert.ToBoolean( reader["IsUnique"] ),
                            IsPrimaryKey = Convert.ToBoolean( reader["IsPrimaryKey"] ),
                            Columns = new List<string>()
                        };

                        indexColumns[key] = index;
                        GetOrAdd( tables, tableName ).Indexes.Add( index );
                    }

                    index.Columns.Add( columnName );
                }
            }
        }

        /// <summary>
        /// Loads foreign key relationships (column, referenced table, referenced column).
        /// </summary>
        private static void LoadForeignKeys( SqlConnection connection, Dictionary<string, TableSchema> tables )
        {
            const string sql = @"
SELECT [pt].[name] AS [TableName], [pc].[name] AS [ColumnName],
       [rt].[name] AS [ReferenceTableName], [rc].[name] AS [ReferenceColumnName]
FROM   [sys].[foreign_key_columns] AS [fkc]
JOIN   [sys].[tables] AS [pt] ON [pt].[object_id] = [fkc].[parent_object_id]
JOIN   [sys].[columns] AS [pc]
    ON [pc].[object_id] = [fkc].[parent_object_id] AND [pc].[column_id] = [fkc].[parent_column_id]
JOIN   [sys].[tables] AS [rt] ON [rt].[object_id] = [fkc].[referenced_object_id]
JOIN   [sys].[columns] AS [rc]
    ON [rc].[object_id] = [fkc].[referenced_object_id] AND [rc].[column_id] = [fkc].[referenced_column_id]
ORDER BY [pt].[name], [pc].[name]";

            using ( var reader = ExecuteReader( connection, sql ) )
            {
                while ( reader.Read() )
                {
                    var tableName = reader["TableName"].ToString();

                    GetOrAdd( tables, tableName ).ForeignKeys.Add( new ModelMapForeignKeyInfo
                    {
                        ColumnName = reader["ColumnName"].ToString(),
                        ReferenceTableName = reader["ReferenceTableName"].ToString(),
                        ReferenceColumnName = reader["ReferenceColumnName"].ToString()
                    } );
                }
            }
        }

        /// <summary>
        /// Executes a read-only query and returns the data reader.
        /// </summary>
        private static IDataReader ExecuteReader( SqlConnection connection, string sql )
        {
            var command = new SqlCommand( sql, connection )
            {
                CommandType = CommandType.Text
            };

            return command.ExecuteReader();
        }

        /// <summary>
        /// Returns whether the SQL data type carries a character/binary length.
        /// </summary>
        private static bool IsLengthType( string dataType )
        {
            var lengthTypes = new[] { "char", "varchar", "nchar", "nvarchar", "binary", "varbinary" };
            return lengthTypes.Contains( dataType, StringComparer.OrdinalIgnoreCase );
        }

        /// <summary>
        /// Converts a nullable database value to a nullable integer.
        /// </summary>
        private static int? GetNullableInt( object value )
        {
            if ( value == null || value == DBNull.Value )
            {
                return null;
            }

            return Convert.ToInt32( value );
        }
    }
}
