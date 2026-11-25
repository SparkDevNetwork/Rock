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
using System.Text;

namespace Rock.BaselineTool;

/// <summary>
/// Finishes the export: gives each new install its own instance ID, adds
/// the identity reseeds, generates the baseline migration class, and
/// prints the line to paste into <c>Rock.Data.Migration</c>.
/// </summary>
internal static class PackageCommand
{
    /// <summary>
    /// The tables the export leaves out, so they are not reseeded.
    /// </summary>
    private static readonly HashSet<string> ExcludedTables = new HashSet<string>( StringComparer.OrdinalIgnoreCase ) { "[dbo].[__MigrationHistory]", "[dbo].[RockMigration]" };

    /// <summary>
    /// UTF-8 without a byte order mark.
    /// </summary>
    private static readonly Encoding Utf8 = new UTF8Encoding( false );

    /// <summary>
    /// Runs package.
    /// </summary>
    /// <param name="options">The parsed options.</param>
    /// <returns>0 on success.</returns>
    public static int Run( BaselineOptions options )
    {
        var work = BaselineWork.Load( options.WorkPath );

        Package( work, options.OutputPath, Console.WriteLine );

        Console.WriteLine();
        Console.WriteLine( "Paste this into Rock/Data/Migration.cs, replacing the existing line:" );
        Console.WriteLine();
        Console.WriteLine( $"        internal static readonly long BaselineIncludesThroughMigrationId = {work.IncludesThroughMigrationId};" );
        Console.WriteLine();
        Console.WriteLine( "package: DONE." );

        return 0;
    }

    /// <summary>
    /// Edits and adds to the scripts in the output folder.
    /// </summary>
    /// <param name="work">The state cleanup recorded.</param>
    /// <param name="outputPath">The folder export wrote to.</param>
    /// <param name="log">Receives progress lines.</param>
    internal static void Package( BaselineWork work, string outputPath, Action<string> log )
    {
        WriteReseedScript( work.IdentityValues, outputPath, log );
        WriteBaselineClass( work, outputPath, log );
    }

    /// <summary>
    /// Writes the script that sets each table's identity to the value it
    /// had in B's database, so later migrations assign the same IDs as on
    /// a database built through EF6.
    /// </summary>
    /// <remarks>
    /// <c>DBCC CHECKIDENT ... RESEED, n</c> makes the next identity
    /// <c>n + 1</c> on a table that has had rows inserted, but <c>n</c> on
    /// one that never has. The baseline inserts rows into some tables and
    /// not others, so each statement checks which case applies.
    /// </remarks>
    internal static void WriteReseedScript( Dictionary<string, long> identityValues, string outputPath, Action<string> log )
    {
        var builder = new StringBuilder();

        foreach ( var entry in identityValues.Where( e => !ExcludedTables.Contains( e.Key ) ).OrderBy( e => e.Key, StringComparer.Ordinal ) )
        {
            var table = entry.Key.Replace( "'", "''" );

            builder.AppendLine( $"IF EXISTS ( SELECT 1 FROM [sys].[identity_columns] WHERE [object_id] = OBJECT_ID( N'{table}' ) AND [last_value] IS NOT NULL )" );
            builder.AppendLine( $"    DBCC CHECKIDENT ( N'{table}', RESEED, {entry.Value} ) WITH NO_INFOMSGS;" );
            builder.AppendLine( "ELSE" );
            builder.AppendLine( $"    DBCC CHECKIDENT ( N'{table}', RESEED, {entry.Value + 1} ) WITH NO_INFOMSGS;" );
        }

        var path = Path.Combine( outputPath, $"{ExportCommand.BaselineMigrationId}-04_Reseed.sql" );
        File.WriteAllText( path, builder.ToString(), Utf8 );

        log( $"Wrote {identityValues.Count} identity reseeds to {Path.GetFileName( path )}." );
    }

    /// <summary>
    /// Generates the baseline migration class, running every script in name order.
    /// </summary>
    private static void WriteBaselineClass( BaselineWork work, string outputPath, Action<string> log )
    {
        var version = Version.Parse( work.RockVersion );
        var steps = GetBaselineSteps( outputPath );
        var calls = string.Join( Environment.NewLine, steps.Select( s => s.IsBulk ? $"        BulkResource( \"{s.Suffix}\" );" : $"        SqlResource( \"{s.Suffix}\" );" ) );

        var source = $@"// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the ""License"");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an ""AS IS"" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Data.Migrations;

/// <summary>
/// Builds a new database at Rock {version.Major}.{version.Minor}. Generated by the baseline
/// tool's package command from a database built by Rock {work.RockVersion}. Values that
/// must differ on every install are regenerated by the hand-written Baseline.Reseed.cs.
/// </summary>
[SqlMigration( Migration.BaselineMigrationId, ""{version.Major}.{version.Minor}"" )]
internal partial class Baseline : Migration
{{
    /// <inheritdoc/>
    public override void Up()
    {{
{calls}
        RegeneratePerInstallValues();
    }}
}}
";

        var path = Path.Combine( outputPath, "Baseline.cs" );
        File.WriteAllText( path, source.Replace( "\r\n", "\n" ).Replace( "\n", "\r\n" ), Utf8 );

        log( $"Wrote {Path.GetFileName( path )} running {steps.Count( s => !s.IsBulk )} scripts and {steps.Count( s => s.IsBulk )} bulk loads." );
    }

    /// <summary>
    /// The baseline's steps in run order: the schema, then each table's
    /// JSON data file in name order, then the foreign keys and triggers,
    /// which need every row present, then the reseeds.
    /// </summary>
    /// <param name="outputPath">The folder export wrote to.</param>
    /// <returns>Each step's file name suffix, and whether it is a JSON bulk load.</returns>
    internal static List<(string Suffix, bool IsBulk)> GetBaselineSteps( string outputPath )
    {
        var prefix = $"{ExportCommand.BaselineMigrationId}-";
        var scripts = Directory.GetFiles( outputPath, $"{prefix}*.sql" )
            .Select( f => Path.GetFileNameWithoutExtension( f ).Substring( prefix.Length ) )
            .OrderBy( s => s, StringComparer.Ordinal )
            .ToList();
        var bulkLoads = Directory.GetFiles( outputPath, $"{prefix}*.json" )
            .Select( f => Path.GetFileNameWithoutExtension( f ).Substring( prefix.Length ) )
            .OrderBy( s => s, StringComparer.Ordinal )
            .Select( s => (s, true) );
        var steps = scripts.Where( s => string.CompareOrdinal( s, "02_" ) < 0 ).Select( s => (s, false) ).ToList();

        steps.AddRange( bulkLoads );
        steps.AddRange( scripts.Where( s => string.CompareOrdinal( s, "02_" ) >= 0 ).Select( s => (s, false) ) );

        return steps;
    }
}
