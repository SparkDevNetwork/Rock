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
using System.IO;

namespace Rock.BaselineTool;

/// <summary>
/// The parsed command line of the baseline tool.
/// </summary>
internal class BaselineOptions
{
    /// <summary>
    /// The usage text printed for bad arguments.
    /// </summary>
    public const string Usage = @"Usage:
  Rock.BaselineTool verify  --rockweb <B RockWeb folder> [--database <db>] [--sql-major <n>]
  Rock.BaselineTool monitor --source <B repository root> --rockweb <B RockWeb folder> [--database <db>] [--quiet <seconds>] [--poll <seconds>] [--timeout <minutes>]
  Rock.BaselineTool cleanup --source <B repository root> --rockweb <B RockWeb folder> [--database <db>] [--accept-exceptions] [--work <dir>]
  Rock.BaselineTool export  [--rockweb <B RockWeb folder>] [--database <db>] [--output <dir>] [--work <dir>]
  Rock.BaselineTool package [--output <dir>] [--work <dir>]

<db> is a connection string, or a database name on the server in the RockWeb
folder's web.ConnectionStrings.config; it defaults to that file's database.
--sql-major is the minimum SQL Server major version B supports (default 16, SQL Server
2022); verify accepts that version or newer.
monitor waits until nothing is outstanding and the registration tables have not
changed for --quiet seconds (default 60), polling every --poll seconds (default
10), and gives up after --timeout minutes (default 120).
--work holds the state cleanup records for package (default Dev Tools/baseline-tool/work).
--output is where export writes and package edits the scripts (default
Rock.Data.Migrations/Migrations/Baseline in this repository).";

    /// <summary>
    /// The command: <c>verify</c>, <c>monitor</c>, <c>cleanup</c>, <c>export</c> or <c>package</c>.
    /// </summary>
    public string Command { get; set; }

    /// <summary>
    /// The RockWeb folder of B's site, or <c>null</c>.
    /// </summary>
    public string RockWebPath { get; set; }

    /// <summary>
    /// The root of B's source tree, or <c>null</c>.
    /// </summary>
    public string SourcePath { get; set; }

    /// <summary>
    /// The database argument as given, or <c>null</c>.
    /// </summary>
    public string Database { get; set; }

    /// <summary>
    /// The minimum SQL Server major version B supports; <c>verify</c> accepts it or newer.
    /// </summary>
    public int SqlMajorVersion { get; set; } = 16;

    /// <summary>
    /// How long, in seconds, <c>monitor</c> needs the registration tables unchanged.
    /// </summary>
    public int QuietSeconds { get; set; } = 60;

    /// <summary>
    /// The seconds between <c>monitor</c>'s polls.
    /// </summary>
    public int PollSeconds { get; set; } = 10;

    /// <summary>
    /// The minutes <c>monitor</c> watches before giving up.
    /// </summary>
    public int TimeoutMinutes { get; set; } = 120;

    /// <summary>
    /// Whether <c>cleanup</c> continues when the exception log has rows.
    /// </summary>
    public bool IsAcceptExceptions { get; set; }

    /// <summary>
    /// The folder for the state <c>cleanup</c> records.
    /// </summary>
    public string WorkPath { get; set; }

    /// <summary>
    /// The folder <c>export</c> writes to and <c>package</c> edits.
    /// </summary>
    public string OutputPath { get; set; }

    /// <summary>
    /// Parses the command line.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>The options.</returns>
    /// <exception cref="ArgumentException">Thrown when the arguments are not valid.</exception>
    public static BaselineOptions Parse( string[] args )
    {
        if ( args.Length == 0 )
        {
            throw new ArgumentException( "No command given." );
        }

        var options = new BaselineOptions { Command = args[0].ToLowerInvariant() };

        for ( var i = 1; i < args.Length; i++ )
        {
            var arg = args[i];

            switch ( arg )
            {
                case "--rockweb":
                    options.RockWebPath = RequireValue( args, ref i, arg );
                    break;

                case "--source":
                    options.SourcePath = RequireValue( args, ref i, arg );
                    break;

                case "--database":
                    options.Database = RequireValue( args, ref i, arg );
                    break;

                case "--sql-major":
                    options.SqlMajorVersion = int.Parse( RequireValue( args, ref i, arg ) );
                    break;

                case "--quiet":
                    options.QuietSeconds = int.Parse( RequireValue( args, ref i, arg ) );
                    break;

                case "--poll":
                    options.PollSeconds = int.Parse( RequireValue( args, ref i, arg ) );
                    break;

                case "--timeout":
                    options.TimeoutMinutes = int.Parse( RequireValue( args, ref i, arg ) );
                    break;

                case "--accept-exceptions":
                    options.IsAcceptExceptions = true;
                    break;

                case "--work":
                    options.WorkPath = RequireValue( args, ref i, arg );
                    break;

                case "--output":
                    options.OutputPath = RequireValue( args, ref i, arg );
                    break;

                default:
                    throw new ArgumentException( $"Unknown argument '{arg}'." );
            }
        }

        var repositoryRoot = FindRepositoryRoot();

        options.WorkPath = options.WorkPath ?? ( repositoryRoot == null ? null : Path.Combine( repositoryRoot, "Dev Tools", "baseline-tool", "work" ) );
        options.OutputPath = options.OutputPath ?? ( repositoryRoot == null ? null : Path.Combine( repositoryRoot, "Rock.Data.Migrations", "Migrations", "Baseline" ) );

        switch ( options.Command )
        {
            case "verify":
                Require( options.RockWebPath, "--rockweb" );
                break;

            case "monitor":
                Require( options.SourcePath, "--source" );
                Require( options.RockWebPath, "--rockweb" );
                break;

            case "cleanup":
                Require( options.SourcePath, "--source" );
                Require( options.RockWebPath, "--rockweb" );
                Require( options.WorkPath, "--work" );
                break;

            case "export":
                Require( options.OutputPath, "--output" );
                Require( options.WorkPath, "--work" );
                if ( options.RockWebPath == null && ( options.Database == null || !options.Database.Contains( "=" ) ) )
                {
                    throw new ArgumentException( "export needs --rockweb, or --database with a full connection string." );
                }
                break;

            case "package":
                Require( options.OutputPath, "--output" );
                Require( options.WorkPath, "--work" );
                break;

            default:
                throw new ArgumentException( $"Unknown command '{options.Command}'." );
        }

        return options;
    }

    /// <summary>
    /// Fails when a required argument is missing.
    /// </summary>
    private static void Require( string value, string argumentName )
    {
        if ( string.IsNullOrWhiteSpace( value ) )
        {
            throw new ArgumentException( $"{argumentName} is required." );
        }
    }

    /// <summary>
    /// Reads the value that follows an argument.
    /// </summary>
    private static string RequireValue( string[] args, ref int index, string argumentName )
    {
        if ( index + 1 >= args.Length )
        {
            throw new ArgumentException( $"{argumentName} requires a value." );
        }

        index++;

        return args[index];
    }

    /// <summary>
    /// Walks up from the current directory and the tool's directory to
    /// the folder containing <c>Rock.sln</c>.
    /// </summary>
    private static string FindRepositoryRoot()
    {
        foreach ( var start in new[] { Environment.CurrentDirectory, AppDomain.CurrentDomain.BaseDirectory } )
        {
            var directory = new DirectoryInfo( start );

            while ( directory != null )
            {
                if ( File.Exists( Path.Combine( directory.FullName, "Rock.sln" ) ) )
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }
        }

        return null;
    }
}
