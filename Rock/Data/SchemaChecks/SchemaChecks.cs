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

namespace Rock.Data;

/// <summary>
/// The schema checks that gate SQL migrations: the mapping check, the
/// name ratchet and catalog equivalence. Each takes catalogs that were
/// already read and returns findings, so they can run from the schema
/// check CLI or from integration tests.
/// </summary>
internal static class SchemaChecks
{
    /// <summary>
    /// The check name for the mapping check.
    /// </summary>
    public const string MappingCheck = "mapping";

    /// <summary>
    /// The check name for the name ratchet.
    /// </summary>
    public const string NamesCheck = "names";

    /// <summary>
    /// The check name for catalog equivalence (also used by commute).
    /// </summary>
    public const string EquivalenceCheck = "equivalence";

    /// <summary>
    /// The check name for table data equivalence.
    /// </summary>
    public const string DataCheck = "data";

    #region Mapping Check

    /// <summary>
    /// Checks that the database supports everything the entity model maps.
    /// Fails on missing tables and columns, store facet and nullability
    /// differences, primary key differences, and unmapped <c>NOT NULL</c>
    /// columns without a default. Reports missing or different foreign
    /// keys without failing. Extra tables, indexes and constraint names
    /// are ignored.
    /// </summary>
    /// <param name="expected">What the entity model maps.</param>
    /// <param name="actual">The database's catalog.</param>
    /// <returns>The findings.</returns>
    public static List<SchemaFinding> CheckMapping( SchemaExpectedModel expected, SchemaCatalog actual )
    {
        var findings = new List<SchemaFinding>();

        foreach ( var expectedTable in expected.Tables )
        {
            if ( !actual.Tables.TryGetValue( expectedTable.Key, out var actualTable ) )
            {
                if ( actual.Views.TryGetValue( expectedTable.Key, out var actualView ) )
                {
                    // SQL Server infers a view's column types, lengths and nullability,
                    // and EF only reads from it, so only column presence is checked.
                    CheckMappedColumns( expectedTable, actualView, findings, isView: true );
                }
                else
                {
                    findings.Add( SchemaFinding.Failure( MappingCheck, "MissingTable", expectedTable.Key, "The model maps this table but the database has no table or view with that name." ) );
                }

                continue;
            }

            CheckMappedColumns( expectedTable, actualTable, findings, isView: false );
            CheckUnmappedColumns( expectedTable, actualTable, findings );
            CheckMappedPrimaryKey( expectedTable, actualTable, findings );
            CheckMappedForeignKeys( expectedTable, actualTable, findings );
        }

        return findings;
    }

    /// <summary>
    /// Compares each mapped column with the database column. For a view,
    /// only checks that the column exists.
    /// </summary>
    private static void CheckMappedColumns( SchemaExpectedTable expectedTable, SchemaCatalogTable actualTable, List<SchemaFinding> findings, bool isView )
    {
        foreach ( var expectedColumn in expectedTable.Columns )
        {
            var objectKey = $"{expectedTable.Key}.{expectedColumn.Name}";

            if ( !actualTable.Columns.TryGetValue( expectedColumn.Name, out var actualColumn ) )
            {
                findings.Add( SchemaFinding.Failure( MappingCheck, "MissingColumn", objectKey, "The model maps this column but the database does not have it." ) );
                continue;
            }

            if ( isView )
            {
                continue;
            }

            var expectedType = DescribeType( expectedColumn );
            var actualType = DescribeType( actualColumn );
            var isTypeDifferent = !string.Equals( expectedColumn.StoreType, actualColumn.StoreType, StringComparison.OrdinalIgnoreCase );
            var isLengthDifferent = expectedColumn.MaxLength.HasValue && expectedColumn.MaxLength != actualColumn.MaxLength;
            var isPrecisionDifferent = ( expectedColumn.Precision.HasValue && expectedColumn.Precision != actualColumn.Precision )
                || ( expectedColumn.Scale.HasValue && expectedColumn.Scale != actualColumn.Scale );

            if ( isTypeDifferent || isLengthDifferent || isPrecisionDifferent )
            {
                var code = isTypeDifferent ? "TypeMismatch" : isLengthDifferent ? "LengthMismatch" : "PrecisionMismatch";

                findings.Add( SchemaFinding.Failure( MappingCheck, code, objectKey, $"The model maps {expectedType} but the database has {actualType}." ) );
            }

            if ( expectedColumn.IsNullable != actualColumn.IsNullable )
            {
                findings.Add( SchemaFinding.Failure( MappingCheck, "NullabilityMismatch", objectKey, $"The model maps {DescribeNullability( expectedColumn.IsNullable )} but the database has {DescribeNullability( actualColumn.IsNullable )}." ) );
            }
        }
    }

    /// <summary>
    /// Fails on database columns the model does not map that an insert
    /// from EF would leave without a value.
    /// </summary>
    private static void CheckUnmappedColumns( SchemaExpectedTable expectedTable, SchemaCatalogTable actualTable, List<SchemaFinding> findings )
    {
        var mappedColumnNames = new HashSet<string>( expectedTable.Columns.Select( c => c.Name ), StringComparer.OrdinalIgnoreCase );

        foreach ( var actualColumn in actualTable.Columns.Values )
        {
            if ( !mappedColumnNames.Contains( actualColumn.Name ) && !actualColumn.IsValueOptionalOnInsert )
            {
                findings.Add( SchemaFinding.Failure( MappingCheck, "UnmappedRequiredColumn", $"{expectedTable.Key}.{actualColumn.Name}", "The column is NOT NULL with no default and the model does not map it, so every insert from EF fails." ) );
            }
        }
    }

    /// <summary>
    /// Compares the mapped primary key columns with the database's primary key.
    /// </summary>
    private static void CheckMappedPrimaryKey( SchemaExpectedTable expectedTable, SchemaCatalogTable actualTable, List<SchemaFinding> findings )
    {
        var actualColumns = actualTable.PrimaryKey?.KeyColumns ?? new List<string>();

        if ( !expectedTable.PrimaryKeyColumns.SequenceEqual( actualColumns, StringComparer.OrdinalIgnoreCase ) )
        {
            var actualDescription = actualColumns.Count == 0 ? "no primary key" : $"({string.Join( ", ", actualColumns )})";

            findings.Add( SchemaFinding.Failure( MappingCheck, "PrimaryKeyMismatch", expectedTable.Key, $"The model maps primary key ({string.Join( ", ", expectedTable.PrimaryKeyColumns )}) but the database has {actualDescription}." ) );
        }
    }

    /// <summary>
    /// Reports mapped relationships whose foreign key is missing or has
    /// different cascade behavior. Never a failure.
    /// </summary>
    private static void CheckMappedForeignKeys( SchemaExpectedTable expectedTable, SchemaCatalogTable actualTable, List<SchemaFinding> findings )
    {
        foreach ( var expectedForeignKey in expectedTable.ForeignKeys )
        {
            var objectKey = $"{expectedTable.Key}({string.Join( ", ", expectedForeignKey.Columns )})";
            var actualForeignKey = actualTable.ForeignKeys.FirstOrDefault( fk => fk.Columns.SequenceEqual( expectedForeignKey.Columns, StringComparer.OrdinalIgnoreCase )
                && string.Equals( fk.ReferencedTable, expectedForeignKey.ReferencedTable, StringComparison.OrdinalIgnoreCase ) );

            if ( actualForeignKey == null )
            {
                findings.Add( SchemaFinding.Informational( MappingCheck, "MissingForeignKey", objectKey, $"The model maps a relationship to {expectedForeignKey.ReferencedTable} but the database has no foreign key for it." ) );
            }
            else if ( actualForeignKey.IsCascadeDelete != expectedForeignKey.IsCascadeDelete )
            {
                findings.Add( SchemaFinding.Informational( MappingCheck, "CascadeMismatch", objectKey, $"The model maps cascade delete {( expectedForeignKey.IsCascadeDelete ? "on" : "off" )} but {actualForeignKey.Name} is ON DELETE {actualForeignKey.DeleteAction}." ) );
            }
        }
    }

    #endregion Mapping Check

    #region Name Ratchet

    /// <summary>
    /// Checks that every primary key, foreign key, index and unique
    /// constraint follows EF6 naming: <c>PK_dbo.Table</c>,
    /// <c>FK_dbo.Table_dbo.Target_Column</c> and <c>IX_Column</c> (key
    /// columns joined by <c>_</c>). Default and check constraints are not
    /// checked, since EF6 let SQL Server name them.
    /// </summary>
    /// <param name="catalog">The database's catalog.</param>
    /// <returns>The findings, one per nonconforming name.</returns>
    public static List<SchemaFinding> CheckNames( SchemaCatalog catalog )
    {
        var findings = new List<SchemaFinding>();

        foreach ( var table in catalog.Tables.Values.OrderBy( t => t.Key, StringComparer.OrdinalIgnoreCase ) )
        {
            if ( table.PrimaryKey != null )
            {
                CheckName( findings, "PrimaryKeyName", table, table.PrimaryKey.Name, $"PK_{table.Key}" );
            }

            foreach ( var foreignKey in table.ForeignKeys )
            {
                CheckName( findings, "ForeignKeyName", table, foreignKey.Name, $"FK_{table.Key}_{foreignKey.ReferencedTable}_{string.Join( "_", foreignKey.Columns )}" );
            }

            foreach ( var index in table.Indexes )
            {
                var keyColumnNames = index.KeyColumns.Select( c => c.EndsWith( " DESC" ) ? c.Substring( 0, c.Length - 5 ) : c );

                CheckName( findings, "IndexName", table, index.Name, $"IX_{string.Join( "_", keyColumnNames )}" );
            }
        }

        return findings;
    }

    /// <summary>
    /// Adds a finding when a name differs from the EF6 name.
    /// </summary>
    private static void CheckName( List<SchemaFinding> findings, string code, SchemaCatalogTable table, string actualName, string expectedName )
    {
        if ( !string.Equals( actualName, expectedName, StringComparison.Ordinal ) )
        {
            findings.Add( SchemaFinding.Failure( NamesCheck, code, $"{table.Key}/{actualName}", $"Expected the EF6 name '{expectedName}'." ) );
        }
    }

    #endregion Name Ratchet

    #region Catalog Equivalence

    /// <summary>
    /// Compares two databases catalog to catalog: tables, columns and
    /// their types, nullability, defaults and computed definitions, keys,
    /// foreign keys, indexes and their names, check constraints, and the
    /// definitions of views, procedures, functions and triggers. Column
    /// order is not compared. Every difference fails.
    /// </summary>
    /// <param name="left">The first database's catalog.</param>
    /// <param name="right">The second database's catalog.</param>
    /// <returns>The findings.</returns>
    public static List<SchemaFinding> CompareCatalogs( SchemaCatalog left, SchemaCatalog right )
    {
        var findings = new List<SchemaFinding>();

        CompareKeyed( findings, "Table", left.Tables, right.Tables, ( l, r, objectKey ) => CompareTables( findings, l, r ) );
        CompareKeyed( findings, "Module", left.Modules, right.Modules, ( l, r, objectKey ) =>
        {
            if ( l.ObjectType != r.ObjectType || l.Definition != r.Definition )
            {
                findings.Add( SchemaFinding.Failure( EquivalenceCheck, "ModuleDiffers", objectKey, $"The {l.ObjectType} definitions differ{DescribeFirstDifference( l.Definition, r.Definition )}." ) );
            }
        } );

        return findings;
    }

    /// <summary>
    /// Compares the parts of a table present in both databases.
    /// </summary>
    private static void CompareTables( List<SchemaFinding> findings, SchemaCatalogTable left, SchemaCatalogTable right )
    {
        CompareKeyed( findings, "Column", Prefix( left, left.Columns ), Prefix( right, right.Columns ), ( l, r, objectKey ) =>
        {
            CompareDescriptions( findings, "ColumnDiffers", objectKey, DescribeColumn( l ), DescribeColumn( r ) );
        } );

        CompareDescriptions( findings, "PrimaryKeyDiffers", left.Key,
            left.PrimaryKey == null ? "no primary key" : $"{left.PrimaryKey.Name} {left.PrimaryKey.Describe()}",
            right.PrimaryKey == null ? "no primary key" : $"{right.PrimaryKey.Name} {right.PrimaryKey.Describe()}" );

        CompareKeyed( findings, "Index", Prefix( left, left.Indexes.ToDictionary( i => i.Name ) ), Prefix( right, right.Indexes.ToDictionary( i => i.Name ) ), ( l, r, objectKey ) =>
        {
            CompareDescriptions( findings, "IndexDiffers", objectKey, l.Describe(), r.Describe() );
        } );

        CompareKeyed( findings, "ForeignKey", Prefix( left, left.ForeignKeys.ToDictionary( fk => fk.Name ) ), Prefix( right, right.ForeignKeys.ToDictionary( fk => fk.Name ) ), ( l, r, objectKey ) =>
        {
            CompareDescriptions( findings, "ForeignKeyDiffers", objectKey, l.Describe(), r.Describe() );
        } );

        foreach ( var definition in left.CheckConstraints.Except( right.CheckConstraints ) )
        {
            findings.Add( SchemaFinding.Failure( EquivalenceCheck, "CheckConstraintOnlyInLeft", left.Key, $"CHECK {definition}" ) );
        }

        foreach ( var definition in right.CheckConstraints.Except( left.CheckConstraints ) )
        {
            findings.Add( SchemaFinding.Failure( EquivalenceCheck, "CheckConstraintOnlyInRight", left.Key, $"CHECK {definition}" ) );
        }
    }

    /// <summary>
    /// Reports items present on only one side and calls
    /// <paramref name="compareBoth"/> for items present on both.
    /// </summary>
    private static void CompareKeyed<T>( List<SchemaFinding> findings, string kind, IDictionary<string, T> left, IDictionary<string, T> right, Action<T, T, string> compareBoth )
    {
        foreach ( var key in left.Keys.Union( right.Keys, StringComparer.OrdinalIgnoreCase ).OrderBy( k => k, StringComparer.OrdinalIgnoreCase ) )
        {
            var isInLeft = left.TryGetValue( key, out var leftItem );
            var isInRight = right.TryGetValue( key, out var rightItem );

            if ( isInLeft && isInRight )
            {
                compareBoth( leftItem, rightItem, key );
            }
            else
            {
                var side = isInLeft ? "Left" : "Right";

                findings.Add( SchemaFinding.Failure( EquivalenceCheck, $"{kind}OnlyIn{side}", key, $"Only the {side.ToLowerInvariant()} database has this {kind.ToLowerInvariant()}." ) );
            }
        }
    }

    /// <summary>
    /// Adds a finding when two descriptions differ.
    /// </summary>
    private static void CompareDescriptions( List<SchemaFinding> findings, string code, string objectKey, string left, string right )
    {
        if ( left != right )
        {
            findings.Add( SchemaFinding.Failure( EquivalenceCheck, code, objectKey, $"Left: {left}. Right: {right}." ) );
        }
    }

    /// <summary>
    /// Keys a table's items by <c>schema.table.name</c> so findings name the table.
    /// </summary>
    private static Dictionary<string, T> Prefix<T>( SchemaCatalogTable table, IDictionary<string, T> items )
    {
        return items.ToDictionary( kv => $"{table.Key}.{kv.Key}", kv => kv.Value, StringComparer.OrdinalIgnoreCase );
    }

    /// <summary>
    /// Describes where two module definitions first differ.
    /// </summary>
    private static string DescribeFirstDifference( string left, string right )
    {
        var leftLines = left.Split( '\n' );
        var rightLines = right.Split( '\n' );

        for ( var i = 0; i < Math.Max( leftLines.Length, rightLines.Length ); i++ )
        {
            var leftLine = i < leftLines.Length ? leftLines[i] : "(end)";
            var rightLine = i < rightLines.Length ? rightLines[i] : "(end)";

            if ( leftLine != rightLine )
            {
                return $" at line {i + 1}. Left: '{leftLine.Trim()}'. Right: '{rightLine.Trim()}'";
            }
        }

        return string.Empty;
    }

    #endregion Catalog Equivalence

    #region Descriptions

    /// <summary>
    /// Describes a column's store type with its length, precision and scale.
    /// </summary>
    internal static string DescribeType( SchemaCatalogColumn column )
    {
        if ( column.MaxLength.HasValue )
        {
            return $"{column.StoreType}({( column.MaxLength == -1 ? "max" : column.MaxLength.ToString() )})";
        }

        if ( column.Scale.HasValue )
        {
            return $"{column.StoreType}({column.Precision},{column.Scale})";
        }

        if ( column.Precision.HasValue )
        {
            return $"{column.StoreType}({column.Precision})";
        }

        return column.StoreType;
    }

    /// <summary>
    /// Describes everything about a column that equivalence compares.
    /// </summary>
    private static string DescribeColumn( SchemaCatalogColumn column )
    {
        var description = $"{DescribeType( column )} {DescribeNullability( column.IsNullable )}";

        if ( column.IsIdentity )
        {
            description += " IDENTITY";
        }

        if ( column.ComputedDefinition != null )
        {
            description += $" AS {column.ComputedDefinition}";
        }

        if ( column.DefaultDefinition != null )
        {
            description += $" DEFAULT {column.DefaultDefinition}";
        }

        return description;
    }

    /// <summary>
    /// Describes nullability as it appears in DDL.
    /// </summary>
    private static string DescribeNullability( bool isNullable )
    {
        return isNullable ? "NULL" : "NOT NULL";
    }

    #endregion Descriptions
}
