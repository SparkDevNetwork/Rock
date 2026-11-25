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
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.Smo;

namespace Rock.BaselineTool;

/// <summary>
/// Exports the cleaned database: the schema with SMO, the library behind
/// SSMS's Generate Scripts, without foreign keys or triggers; each table's
/// rows as a JSON data file the baseline bulk loads; then the foreign keys
/// and triggers. The migration history tables are left out.
/// </summary>
internal static class ExportCommand
{
    /// <summary>
    /// The baseline's migration ID, which every script name starts with.
    /// </summary>
    public const long BaselineMigrationId = 20131125173405;

    /// <summary>
    /// A data file at or over this size is reported, keeping each well
    /// under GitHub's 50 MB warning.
    /// </summary>
    private const long MaximumDataFileBytes = 45L * 1024 * 1024;

    /// <summary>
    /// The tables never scripted: the runner records the baseline itself.
    /// </summary>
    private static readonly HashSet<string> ExcludedTables = new HashSet<string>( StringComparer.OrdinalIgnoreCase ) { "__MigrationHistory", "RockMigration" };

    /// <summary>
    /// Tables whose rows get a new <c>Guid</c> on every install, as
    /// <c>AddMetaData</c> gave them: their data files leave out
    /// <c>Guid</c> and set <c>generateGuid</c>.
    /// </summary>
    internal static readonly HashSet<string> GuidGeneratedTables = new HashSet<string>( StringComparer.OrdinalIgnoreCase )
    {
        "MetaFirstNameGenderLookup", "MetaLastNameLookup", "MetaNickNameLookup"
    };

    /// <summary>
    /// UTF-8 without a byte order mark.
    /// </summary>
    private static readonly Encoding Utf8 = new UTF8Encoding( false );

    /// <summary>
    /// Runs the export.
    /// </summary>
    /// <param name="options">The parsed options.</param>
    /// <returns>0 on success.</returns>
    public static int Run( BaselineOptions options )
    {
        var site = options.RockWebPath == null ? null : new BaselineSite( options.RockWebPath );
        var connectionString = new SqlConnectionStringBuilder( BaselineSite.ResolveDatabase( options.Database, site ) )
        {
            TrustServerCertificate = true,
            ApplicationName = "Rock Baseline Tool",

            // SMO's dependency discovery opens a transaction in one batch and
            // closes it in another, which fails with Rock's MARS setting.
            MultipleActiveResultSets = false
        }.ConnectionString;

        var files = Export( connectionString, options.OutputPath, Console.WriteLine );

        Console.WriteLine( $"export: DONE. Wrote {files.Count} scripts to {options.OutputPath}." );

        return 0;
    }

    /// <summary>
    /// Scripts the database into the output folder, replacing any baseline
    /// scripts already there.
    /// </summary>
    /// <param name="connectionString">The database to script.</param>
    /// <param name="outputPath">The folder to write to.</param>
    /// <param name="log">Receives progress and timing lines.</param>
    /// <returns>The files written, in run order.</returns>
    internal static List<string> Export( string connectionString, string outputPath, Action<string> log )
    {
        Directory.CreateDirectory( outputPath );

        foreach ( var oldFile in Directory.GetFiles( outputPath, $"{BaselineMigrationId}-*.*" ).Where( f => f.EndsWith( ".sql" ) || f.EndsWith( ".csv" ) || f.EndsWith( ".json" ) ) )
        {
            File.Delete( oldFile );
        }

        using var sqlConnection = new SqlConnection( connectionString );
        var server = new Server( new ServerConnection( sqlConnection ) );

        // Loads these properties with each collection. Without this SMO
        // reads them one query per object, which took most of the export.
        server.SetDefaultInitFields( typeof( Table ), "IsSystemObject" );
        server.SetDefaultInitFields( typeof( View ), "IsSystemObject" );
        server.SetDefaultInitFields( typeof( UserDefinedFunction ), "IsSystemObject" );
        server.SetDefaultInitFields( typeof( StoredProcedure ), "IsSystemObject" );
        server.SetDefaultInitFields( typeof( Schema ), "IsSystemObject" );
        server.SetDefaultInitFields( typeof( Column ), "Identity" );
        var database = server.Databases[sqlConnection.Database]
            ?? throw new InvalidOperationException( $"Database '{sqlConnection.Database}' was not found." );
        var tables = database.Tables.Cast<Table>()
            .Where( t => !t.IsSystemObject && !ExcludedTables.Contains( t.Name ) )
            .OrderBy( t => t.Schema, StringComparer.Ordinal )
            .ThenBy( t => t.Name, StringComparer.Ordinal )
            .ToList();
        var files = new List<string>();
        var totalStopwatch = Stopwatch.StartNew();
        var rowCounts = ReadRowCounts( database );
        var tablesWithRows = tables.Where( t => rowCounts.TryGetValue( $"{t.Schema}.{t.Name}", out var count ) && count > 0 ).ToList();

        files.Add( Timed( log, "Schema", () => WriteSchema( server, database, tables, outputPath, log ) ) );
        files.AddRange( Timed( log, "Data", () => WriteJsonData( connectionString, tablesWithRows, outputPath, log ) ) );
        files.Add( Timed( log, "Foreign keys and triggers", () => WriteConstraints( server, database, tables, outputPath ) ) );

        log( $"Export took {totalStopwatch.Elapsed.TotalSeconds:0.0}s; {files.Sum( f => new FileInfo( f ).Length ) / 1024.0 / 1024.0:0.0} MB in {files.Count} files." );

        return files;
    }

    #region Phases

    /// <summary>
    /// Writes types, tables (with keys, defaults, checks and indexes, but
    /// no foreign keys or triggers), views, functions and procedures, in
    /// the dependency order SMO works out.
    /// </summary>
    private static string WriteSchema( Server server, Database database, List<Table> tables, string outputPath, Action<string> log )
    {
        var stopwatch = Stopwatch.StartNew();
        var urns = new List<Urn>();

        // Each kind is sorted by schema and name here rather than left in
        // SMO's collection order, which follows the database collation, so
        // the next squash writes unchanged objects in the same place.
        urns.AddRange( Sorted( database.Schemas.Cast<Schema>().Where( s => !s.IsSystemObject && s.Name != "dbo" ), s => string.Empty, s => s.Name ).Select( s => s.Urn ) );
        urns.AddRange( Sorted( database.UserDefinedDataTypes.Cast<UserDefinedDataType>(), t => t.Schema, t => t.Name ).Select( t => t.Urn ) );
        urns.AddRange( Sorted( database.UserDefinedTableTypes.Cast<UserDefinedTableType>(), t => t.Schema, t => t.Name ).Select( t => t.Urn ) );
        urns.AddRange( Sorted( database.XmlSchemaCollections.Cast<XmlSchemaCollection>(), x => x.Schema, x => x.Name ).Select( x => x.Urn ) );
        urns.AddRange( Sorted( database.Sequences.Cast<Sequence>(), q => q.Schema, q => q.Name ).Select( q => q.Urn ) );
        urns.AddRange( tables.Select( t => t.Urn ) );
        urns.AddRange( Sorted( database.Views.Cast<View>().Where( v => !v.IsSystemObject ), v => v.Schema, v => v.Name ).Select( v => v.Urn ) );
        urns.AddRange( Sorted( database.UserDefinedFunctions.Cast<UserDefinedFunction>().Where( f => !f.IsSystemObject ), f => f.Schema, f => f.Name ).Select( f => f.Urn ) );
        urns.AddRange( Sorted( database.StoredProcedures.Cast<StoredProcedure>().Where( p => !p.IsSystemObject ), p => p.Schema, p => p.Name ).Select( p => p.Urn ) );
        urns.AddRange( Sorted( database.Synonyms.Cast<Synonym>(), y => y.Schema, y => y.Name ).Select( y => y.Urn ) );

        log( $"  listing {urns.Count} objects: {stopwatch.Elapsed.TotalSeconds:0.0}s" );
        stopwatch.Restart();

        var ordered = OrderByDependencies( database, urns );

        log( $"  dependency order: {stopwatch.Elapsed.TotalSeconds:0.0}s" );
        stopwatch.Restart();

        var options = CreateSchemaOptions();
        options.DriForeignKeys = false;
        options.Triggers = false;

        var scripter = new Scripter( server ) { Options = options, PrefetchObjects = true };
        var path = ScriptPath( outputPath, "01_Schema" );

        using var writer = new StreamWriter( path, false, Utf8 );
        WriteBatches( writer, scripter.EnumScript( ordered.ToArray() ) );

        log( $"  scripting: {stopwatch.Elapsed.TotalSeconds:0.0}s" );

        return path;
    }

    /// <summary>
    /// Writes each table's rows to a JSON data file of its own,
    /// <c>{Table}.json</c>, in the format <c>Rock.Data.MigrationBulkLoader</c>
    /// reads: one row per line, in primary key order, so a squash writes
    /// unchanged rows in the same place and growth in one table never
    /// moves another's. Computed and <c>rowversion</c> columns are left
    /// out; the server fills them. For the <c>Meta*</c> lookups the
    /// <c>Guid</c> column is left out and the file sets
    /// <c>generateGuid</c>, so each install has its own, as
    /// <c>AddMetaData</c> did; every other table keeps its <c>Guid</c>
    /// values, which code refers to.
    /// </summary>
    private static List<string> WriteJsonData( string connectionString, List<Table> tables, string outputPath, Action<string> log )
    {
        var files = new List<string>();
        var rowOptions = new JsonWriterOptions
        {
            // Leaves non-ASCII text and HTML characters readable; quotes,
            // backslashes and control characters are still escaped.
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        using var connection = new SqlConnection( connectionString );
        connection.Open();

        foreach ( var table in tables )
        {
            var isGuidGenerated = GuidGeneratedTables.Contains( table.Name );
            var columns = table.Columns.Cast<Column>()
                .Where( c => !c.Computed && c.DataType.SqlDataType != SqlDataType.Timestamp )
                .Where( c => !( isGuidGenerated && c.Name.Equals( "Guid", StringComparison.OrdinalIgnoreCase ) ) )
                .ToList();
            var unsupported = columns.FirstOrDefault( c => c.DataType.SqlDataType == SqlDataType.Variant );

            if ( unsupported != null )
            {
                throw new InvalidOperationException( $"{table.Name}.{unsupported.Name} is sql_variant, which the JSON data files do not support." );
            }

            // CLR types are read as SQL Server's own serialized bytes,
            // which load back into the column unchanged.
            var selectList = columns.Select( c => IsClrType( c ) ? $"CAST( [{c.Name}] AS VARBINARY(MAX) ) AS [{c.Name}]" : $"[{c.Name}]" );
            var keyColumns = table.Indexes.Cast<Index>()
                .Where( i => i.IndexKeyType == IndexKeyType.DriPrimaryKey )
                .SelectMany( i => i.IndexedColumns.Cast<IndexedColumn>().Select( c => $"[{c.Name}]" ) )
                .ToList();
            var orderBy = keyColumns.Count > 0 ? string.Join( ", ", keyColumns ) : string.Join( ", ", columns.Where( c => !IsClrType( c ) ).Select( c => $"[{c.Name}]" ) );
            var path = Path.Combine( outputPath, $"{BaselineMigrationId}-{table.Name}.json" );

            using var command = new SqlCommand( $"SELECT {string.Join( ", ", selectList )} FROM [{table.Schema}].[{table.Name}] ORDER BY {orderBy};", connection ) { CommandTimeout = 600 };
            using var reader = command.ExecuteReader();
            using var writer = new StreamWriter( path, false, Utf8 ) { NewLine = "\r\n" };
            using var rowBuffer = new MemoryStream();
            writer.WriteLine( "{" );

            if ( isGuidGenerated )
            {
                writer.WriteLine( "  \"generateGuid\": true," );
            }

            writer.WriteLine( $"  \"columns\": [ {string.Join( ", ", columns.Select( c => JsonSerializer.Serialize( c.Name ) ) )} ]," );
            writer.Write( "  \"rows\": [" );

            var isFirstRow = true;

            while ( reader.Read() )
            {
                rowBuffer.SetLength( 0 );

                using ( var json = new Utf8JsonWriter( rowBuffer, rowOptions ) )
                {
                    json.WriteStartArray();

                    for ( var i = 0; i < reader.FieldCount; i++ )
                    {
                        WriteJsonValue( json, reader.IsDBNull( i ) ? null : reader.GetValue( i ), table.Name, columns[i].Name );
                    }

                    json.WriteEndArray();
                }

                writer.WriteLine( isFirstRow ? string.Empty : "," );
                writer.Write( "    " );
                writer.Write( Utf8.GetString( rowBuffer.GetBuffer(), 0, ( int ) rowBuffer.Length ) );
                isFirstRow = false;
            }

            writer.WriteLine();
            writer.WriteLine( "  ]" );
            writer.WriteLine( "}" );

            if ( new FileInfo( path ).Length >= MaximumDataFileBytes )
            {
                log( $"  WARNING: {Path.GetFileName( path )} is {new FileInfo( path ).Length / 1024.0 / 1024.0:0.0} MB, over the {MaximumDataFileBytes / 1024 / 1024} MB file size limit." );
            }

            files.Add( path );
        }

        return files;
    }

    /// <summary>
    /// Writes one value in the form <c>Rock.Data.MigrationBulkLoader</c> reads.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown for a value type the format does not support.</exception>
    internal static void WriteJsonValue( Utf8JsonWriter json, object value, string table, string column )
    {
        switch ( value )
        {
            case null:
                json.WriteNullValue();
                break;

            case string text:
                json.WriteStringValue( text );
                break;

            case bool flag:
                json.WriteBooleanValue( flag );
                break;

            case byte number:
                json.WriteNumberValue( number );
                break;

            case short number:
                json.WriteNumberValue( number );
                break;

            case int number:
                json.WriteNumberValue( number );
                break;

            case long number:
                json.WriteNumberValue( number );
                break;

            case decimal number:
                json.WriteNumberValue( number );
                break;

            case double number:
                json.WriteNumberValue( number );
                break;

            case float number:
                json.WriteNumberValue( number );
                break;

            case Guid guid:
                json.WriteStringValue( guid );
                break;

            case DateTime dateTime:
                json.WriteStringValue( dateTime.ToString( "yyyy-MM-ddTHH:mm:ss.fffffff", CultureInfo.InvariantCulture ) );
                break;

            case DateTimeOffset dateTimeOffset:
                json.WriteStringValue( dateTimeOffset.ToString( "yyyy-MM-ddTHH:mm:ss.fffffffzzz", CultureInfo.InvariantCulture ) );
                break;

            case TimeSpan timeSpan:
                json.WriteStringValue( timeSpan.ToString( "c", CultureInfo.InvariantCulture ) );
                break;

            case byte[] bytes:
                json.WriteBase64StringValue( bytes );
                break;

            default:
                throw new InvalidOperationException( $"{table}.{column} has a {value.GetType().FullName} value, which the JSON data files do not support." );
        }
    }

    /// <summary>
    /// Whether a column is a CLR type (<c>geography</c>, <c>geometry</c>,
    /// <c>hierarchyid</c>), read and loaded as its serialized bytes.
    /// </summary>
    private static bool IsClrType( Column column )
    {
        var type = column.DataType.SqlDataType;

        return type == SqlDataType.Geography || type == SqlDataType.Geometry || type == SqlDataType.HierarchyId || type == SqlDataType.UserDefinedType;
    }

    /// <summary>
    /// Reads every table's row count in one query.
    /// </summary>
    /// <returns>Row counts keyed by <c>schema.table</c>.</returns>
    private static Dictionary<string, long> ReadRowCounts( Database database )
    {
        return database.ExecuteWithResults( @"
SELECT OBJECT_SCHEMA_NAME( [object_id] ) + '.' + OBJECT_NAME( [object_id] ), SUM( [rows] )
FROM [sys].[partitions]
WHERE [index_id] IN ( 0, 1 )
GROUP BY [object_id];" ).Tables[0].Rows.Cast<System.Data.DataRow>()
            .ToDictionary( r => ( string ) r[0], r => Convert.ToInt64( r[1] ), StringComparer.OrdinalIgnoreCase );
    }

    /// <summary>
    /// Writes every foreign key, then every table, view and database
    /// trigger, after the data so inserts neither fail on order nor fire triggers.
    /// </summary>
    private static string WriteConstraints( Server server, Database database, List<Table> tables, string outputPath )
    {
        var options = CreateSchemaOptions();
        var path = ScriptPath( outputPath, "03_Constraints" );

        // One scripter call for every foreign key, so SMO reads their
        // columns together instead of once per key.
        var foreignKeyUrns = tables
            .SelectMany( t => t.ForeignKeys.Cast<ForeignKey>().OrderBy( f => f.Name, StringComparer.Ordinal ) )
            .Select( f => f.Urn )
            .ToArray();
        var scripter = new Scripter( server ) { Options = options, PrefetchObjects = true };

        using var writer = new StreamWriter( path, false, Utf8 );
        if ( foreignKeyUrns.Length > 0 )
        {
            WriteBatches( writer, scripter.EnumScript( foreignKeyUrns ) );
        }

        foreach ( var table in tables )
        {
            foreach ( var trigger in table.Triggers.Cast<Trigger>().OrderBy( t => t.Name, StringComparer.Ordinal ) )
            {
                WriteBatches( writer, trigger.Script( options ) );
            }
        }

        foreach ( var view in database.Views.Cast<View>().Where( v => !v.IsSystemObject ) )
        {
            foreach ( var trigger in view.Triggers.Cast<Trigger>().OrderBy( t => t.Name, StringComparer.Ordinal ) )
            {
                WriteBatches( writer, trigger.Script( options ) );
            }
        }

        foreach ( var trigger in database.Triggers.Cast<DatabaseDdlTrigger>().Where( t => !t.IsSystemObject ) )
        {
            WriteBatches( writer, trigger.Script( options ) );
        }

        return path;
    }

    #endregion Phases

    #region Support

    /// <summary>
    /// The schema options, matching the SSMS settings in the 2022 squash
    /// guide: no USE statements, no filegroups, triggers on, extended
    /// properties on, no collation, and no headers so two runs compare.
    /// </summary>
    private static ScriptingOptions CreateSchemaOptions()
    {
        return new ScriptingOptions
        {
            ScriptSchema = true,
            ScriptData = false,
            IncludeHeaders = false,
            IncludeDatabaseContext = false,
            NoFileGroup = true,
            NoCollation = true,
            SchemaQualify = true,
            AnsiPadding = true,
            ExtendedProperties = true,
            Permissions = false,
            ScriptDataCompression = false,
            ScriptOwner = false,
            Statistics = false,
            Indexes = true,
            ClusteredIndexes = true,
            NonClusteredIndexes = true,
            XmlIndexes = true,
            SpatialIndexes = true,
            ColumnStoreIndexes = true,
            FullTextIndexes = true,
            DriPrimaryKey = true,
            DriUniqueKeys = true,
            DriChecks = true,
            DriDefaults = true,
            DriIndexes = true,
            DriForeignKeys = true,
            Triggers = true,
            TargetServerVersion = SqlServerVersion.Version160
        };
    }

    /// <summary>
    /// Sorts objects by schema, then name, ordinally.
    /// </summary>
    private static IEnumerable<T> Sorted<T>( IEnumerable<T> items, Func<T, string> schema, Func<T, string> name )
    {
        return items.OrderBy( schema, StringComparer.Ordinal ).ThenBy( name, StringComparer.Ordinal );
    }

    /// <summary>
    /// Orders the objects so each comes after the objects it references,
    /// keeping the given order wherever the references allow it.
    /// </summary>
    /// <remarks>
    /// SMO's dependency walk (<c>WithDependencies</c>) asked the server
    /// about each object and took about 30 seconds on a new Rock
    /// database; <c>sys.sql_expression_dependencies</c> gives the same
    /// references in one query. It covers what creation order depends on:
    /// views, functions and procedures on what they reference, and tables
    /// on functions used by computed columns, defaults and checks. Types,
    /// schemas and sequences come first in the given order, and foreign
    /// keys are scripted after the data, so neither needs it.
    /// </remarks>
    /// <param name="database">The database being scripted.</param>
    /// <param name="urns">The objects in their default order.</param>
    /// <returns>The objects in creation order.</returns>
    private static List<Urn> OrderByDependencies( Database database, List<Urn> urns )
    {
        var indexByName = new Dictionary<string, int>( StringComparer.OrdinalIgnoreCase );

        for ( var i = 0; i < urns.Count; i++ )
        {
            var schema = urns[i].GetAttribute( "Schema" );
            if ( schema != null )
            {
                indexByName[$"{schema}.{urns[i].GetAttribute( "Name" )}"] = i;
            }
        }

        // referencedBy[i] lists the objects that must come after object i.
        var referencedBy = urns.Select( _ => new List<int>() ).ToList();
        var waitingOn = new int[urns.Count];
        var references = database.ExecuteWithResults( @"
SELECT DISTINCT
    OBJECT_SCHEMA_NAME( [d].[referencing_id] ) + '.' + OBJECT_NAME( [d].[referencing_id] ),
    OBJECT_SCHEMA_NAME( [d].[referenced_id] ) + '.' + OBJECT_NAME( [d].[referenced_id] )
FROM [sys].[sql_expression_dependencies] AS [d]
WHERE [d].[referenced_id] IS NOT NULL
    AND [d].[referencing_class] = 1
    AND [d].[referenced_class] = 1
    AND [d].[referencing_id] <> [d].[referenced_id];" ).Tables[0];

        foreach ( System.Data.DataRow row in references.Rows )
        {
            if ( row[0] is string referencing && row[1] is string referenced
                && indexByName.TryGetValue( referencing, out var referencingIndex )
                && indexByName.TryGetValue( referenced, out var referencedIndex ) )
            {
                referencedBy[referencedIndex].Add( referencingIndex );
                waitingOn[referencingIndex]++;
            }
        }

        var ready = new SortedSet<int>( Enumerable.Range( 0, urns.Count ).Where( i => waitingOn[i] == 0 ) );
        var remaining = new SortedSet<int>( Enumerable.Range( 0, urns.Count ) );
        var ordered = new List<Urn>( urns.Count );

        while ( remaining.Count > 0 )
        {
            // Procedures can call each other in a cycle; SQL Server
            // resolves those names when they run, so any order works.
            var next = ready.Count > 0 ? ready.Min : remaining.Min;

            ready.Remove( next );
            remaining.Remove( next );
            ordered.Add( urns[next] );

            foreach ( var dependent in referencedBy[next] )
            {
                if ( --waitingOn[dependent] == 0 && remaining.Contains( dependent ) )
                {
                    ready.Add( dependent );
                }
            }
        }

        return ordered;
    }

    /// <summary>
    /// Writes each statement followed by <c>GO</c>.
    /// </summary>
    private static void WriteBatches( StreamWriter writer, IEnumerable<string> statements )
    {
        foreach ( var statement in statements )
        {
            writer.WriteLine( statement.TrimEnd() );
            writer.WriteLine( "GO" );
        }
    }

    /// <summary>
    /// Writes each statement of a <see cref="StringCollection"/> followed by <c>GO</c>.
    /// </summary>
    private static void WriteBatches( StreamWriter writer, StringCollection statements )
    {
        WriteBatches( writer, statements.Cast<string>() );
    }

    /// <summary>
    /// The path of a named baseline script.
    /// </summary>
    private static string ScriptPath( string outputPath, string suffix )
    {
        return Path.Combine( outputPath, $"{BaselineMigrationId}-{suffix}.sql" );
    }

    /// <summary>
    /// Runs a phase and logs how long it took.
    /// </summary>
    private static T Timed<T>( Action<string> log, string phase, Func<T> action )
    {
        var stopwatch = Stopwatch.StartNew();
        var result = action();

        log( $"{phase}: {stopwatch.Elapsed.TotalSeconds:0.0}s" );

        return result;
    }

    #endregion Support
}
