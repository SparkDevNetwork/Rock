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
using System.Diagnostics;
using System.IO;
using System.Linq;

using Rock.Data;

namespace Rock.SchemaCheck;

/// <summary>
/// Console entry point for the schema checks that gate SQL migrations:
/// the mapping check, the name ratchet, equivalence and commute.
/// </summary>
internal class Program
{
    /// <summary>
    /// The exit code when every check passed.
    /// </summary>
    private const int PassedExitCode = 0;

    /// <summary>
    /// The exit code when a check found a difference that is not allowed.
    /// </summary>
    private const int FailedExitCode = 1;

    /// <summary>
    /// The exit code when the tool could not run.
    /// </summary>
    private const int ErrorExitCode = 2;

    /// <summary>
    /// The reason written for names frozen by <c>--freeze</c>.
    /// </summary>
    private const string FrozenNameReason = "Existing name when the name ratchet was introduced. Remove the entry if the object is renamed or dropped.";

    /// <summary>
    /// The console entry point.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>0 when the checks pass, 1 when they fail, 2 when the tool could not run.</returns>
    public static int Main( string[] args )
    {
        SchemaCheckOptions options;

        try
        {
            options = SchemaCheckOptions.Parse( args );
        }
        catch ( ArgumentException ex )
        {
            Console.Error.WriteLine( ex.Message );
            Console.Error.WriteLine();
            Console.Error.WriteLine( SchemaCheckOptions.Usage );
            return ErrorExitCode;
        }

        try
        {
            return Run( options );
        }
        catch ( Exception ex )
        {
            Console.Error.WriteLine( $"Schema check failed to run: {ex}" );
            return ErrorExitCode;
        }
    }

    /// <summary>
    /// Runs the command and prints its report.
    /// </summary>
    /// <param name="options">The parsed options.</param>
    /// <returns>The exit code.</returns>
    private static int Run( SchemaCheckOptions options )
    {
        var totalStopwatch = Stopwatch.StartNew();
        var allowList = File.Exists( options.AllowListPath )
            ? SchemaAllowList.FromJson( File.ReadAllText( options.AllowListPath ) )
            : new SchemaAllowList();
        List<SchemaFinding> findings;
        string[] checks;

        switch ( options.Command )
        {
            case "mapping":
                {
                    var expected = Timed( "Build EF6 model", () => Ef6ModelAdapter.Build( options.DatabaseConnectionString ) );
                    var catalog = Timed( "Read catalog", () => SchemaCatalogReader.Read( options.DatabaseConnectionString ) );

                    findings = Timed( "Check mapping", () => SchemaChecks.CheckMapping( expected, catalog ) );
                    checks = new[] { SchemaChecks.MappingCheck };
                    break;
                }

            case "names":
                {
                    var catalog = Timed( "Read catalog", () => SchemaCatalogReader.Read( options.DatabaseConnectionString ) );

                    findings = Timed( "Check names", () => SchemaChecks.CheckNames( catalog ) );
                    checks = new[] { SchemaChecks.NamesCheck };

                    if ( options.IsFreeze )
                    {
                        Freeze( allowList, findings, options.AllowListPath );
                    }

                    break;
                }

            default:
                {
                    var left = Timed( "Read left catalog", () => SchemaCatalogReader.Read( options.LeftConnectionString ) );
                    var right = Timed( "Read right catalog", () => SchemaCatalogReader.Read( options.RightConnectionString ) );

                    findings = Timed( "Compare catalogs", () => SchemaChecks.CompareCatalogs( left, right ) );
                    checks = new[] { SchemaChecks.EquivalenceCheck };

                    if ( options.Command == "equivalence" && !options.IsCatalogOnly )
                    {
                        findings.AddRange( Timed( "Compare data", () => SchemaDataComparer.Compare( options.LeftConnectionString, left, options.RightConnectionString, right ) ) );
                        checks = new[] { SchemaChecks.EquivalenceCheck, SchemaChecks.DataCheck };
                    }

                    break;
                }
        }

        findings.AddRange( allowList.Apply( findings, checks ) );

        return Report( options, findings, totalStopwatch.Elapsed );
    }

    /// <summary>
    /// Replaces the allow list's names entries with the current findings
    /// and writes the file.
    /// </summary>
    private static void Freeze( SchemaAllowList allowList, List<SchemaFinding> findings, string allowListPath )
    {
        allowList.Entries.RemoveAll( e => e.Check == SchemaChecks.NamesCheck );
        allowList.Entries.AddRange( findings.Select( f => new SchemaAllowListEntry
        {
            Check = f.Check,
            Code = f.Code,
            Object = f.Object,
            Reason = FrozenNameReason
        } ) );

        File.WriteAllText( allowListPath, allowList.ToJson() );

        Console.WriteLine( $"Froze {findings.Count} existing names into {allowListPath}." );
    }

    /// <summary>
    /// Prints every blocking and informational finding (and allowed ones
    /// when verbose) with a summary, and returns the exit code.
    /// </summary>
    private static int Report( SchemaCheckOptions options, List<SchemaFinding> findings, TimeSpan elapsed )
    {
        var blocking = findings.Where( f => f.IsBlocking ).ToList();
        var allowed = findings.Where( f => f.IsFailure && !f.IsBlocking ).ToList();
        var informational = findings.Where( f => !f.IsFailure ).ToList();

        Print( "FAIL", blocking );
        Print( "INFO", informational );

        if ( options.IsVerbose )
        {
            foreach ( var finding in allowed )
            {
                Console.WriteLine( $"ALLOW {finding.Check} {finding.Code} {finding.Object}: {finding.Message} Reason: {finding.AllowedReason}" );
            }
        }

        Console.WriteLine();
        Console.WriteLine( $"{options.Command}: {( blocking.Count == 0 ? "PASSED" : "FAILED" )}. {blocking.Count} failing, {allowed.Count} allowed, {informational.Count} informational. Total {elapsed.TotalSeconds:0.0}s." );

        return blocking.Count == 0 ? PassedExitCode : FailedExitCode;
    }

    /// <summary>
    /// Prints findings one per line, ordered so related findings sit together.
    /// </summary>
    private static void Print( string label, IEnumerable<SchemaFinding> findings )
    {
        foreach ( var finding in findings.OrderBy( f => f.Check ).ThenBy( f => f.Code ).ThenBy( f => f.Object, StringComparer.OrdinalIgnoreCase ) )
        {
            Console.WriteLine( $"{label}  {finding.Check} {finding.Code} {finding.Object}: {finding.Message}" );
        }
    }

    /// <summary>
    /// Runs a step and prints how long it took, for the timing questions
    /// in the SQL migrations spec.
    /// </summary>
    private static T Timed<T>( string step, Func<T> action )
    {
        var stopwatch = Stopwatch.StartNew();
        var result = action();

        Console.WriteLine( $"{step}: {stopwatch.Elapsed.TotalSeconds:0.00}s" );

        return result;
    }
}
