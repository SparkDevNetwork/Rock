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
using System.Xml;

using System.Data.SqlClient;

namespace Rock.SchemaCheck;

/// <summary>
/// The parsed command line of the schema check tool.
/// </summary>
internal class SchemaCheckOptions
{
    /// <summary>
    /// The usage text printed for bad arguments.
    /// </summary>
    public const string Usage = @"Usage:
  Rock.SchemaCheck mapping     [--database <db>] [--allow-list <file>] [--verbose]
  Rock.SchemaCheck names       [--database <db>] [--allow-list <file>] [--freeze] [--verbose]
  Rock.SchemaCheck equivalence --left <db> --right <db> [--catalog-only] [--allow-list <file>] [--verbose]
  Rock.SchemaCheck commute     --left <db> --right <db> [--allow-list <file>] [--verbose]

<db> is a connection string, or a database name on the server in
RockWeb/web.ConnectionStrings.config. --database defaults to that file's database.
--freeze rewrites the names entries of the allow list from the current database.
--verbose also lists allowed findings.";

    /// <summary>
    /// The command to run: <c>mapping</c>, <c>names</c>, <c>equivalence</c> or <c>commute</c>.
    /// </summary>
    public string Command { get; set; }

    /// <summary>
    /// The connection string of the database checked by <c>mapping</c> and <c>names</c>.
    /// </summary>
    public string DatabaseConnectionString { get; set; }

    /// <summary>
    /// The connection string of the left database for comparisons.
    /// </summary>
    public string LeftConnectionString { get; set; }

    /// <summary>
    /// The connection string of the right database for comparisons.
    /// </summary>
    public string RightConnectionString { get; set; }

    /// <summary>
    /// The allow list file.
    /// </summary>
    public string AllowListPath { get; set; }

    /// <summary>
    /// Whether <c>names</c> rewrites its allow list entries from the current database.
    /// </summary>
    public bool IsFreeze { get; set; }

    /// <summary>
    /// Whether <c>equivalence</c> skips the table data comparison.
    /// </summary>
    public bool IsCatalogOnly { get; set; }

    /// <summary>
    /// Whether allowed findings are listed too.
    /// </summary>
    public bool IsVerbose { get; set; }

    /// <summary>
    /// Parses the command line.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>The options.</returns>
    /// <exception cref="ArgumentException">Thrown when the arguments are not valid.</exception>
    public static SchemaCheckOptions Parse( string[] args )
    {
        if ( args.Length == 0 )
        {
            throw new ArgumentException( "No command given." );
        }

        var options = new SchemaCheckOptions { Command = args[0].ToLowerInvariant() };
        string database = null;
        string left = null;
        string right = null;

        for ( var i = 1; i < args.Length; i++ )
        {
            var arg = args[i];

            switch ( arg )
            {
                case "--database":
                    database = RequireValue( args, ref i, arg );
                    break;

                case "--left":
                    left = RequireValue( args, ref i, arg );
                    break;

                case "--right":
                    right = RequireValue( args, ref i, arg );
                    break;

                case "--allow-list":
                    options.AllowListPath = RequireValue( args, ref i, arg );
                    break;

                case "--freeze":
                    options.IsFreeze = true;
                    break;

                case "--catalog-only":
                    options.IsCatalogOnly = true;
                    break;

                case "--verbose":
                    options.IsVerbose = true;
                    break;

                default:
                    throw new ArgumentException( $"Unknown argument '{arg}'." );
            }
        }

        var repositoryRoot = FindRepositoryRoot();
        var defaultConnectionString = repositoryRoot == null ? null : ReadRockWebConnectionString( repositoryRoot );

        if ( options.AllowListPath == null )
        {
            if ( repositoryRoot == null )
            {
                throw new ArgumentException( "Could not locate the repository root (a folder containing Rock.sln). Pass --allow-list explicitly." );
            }

            options.AllowListPath = Path.Combine( repositoryRoot, "Dev Tools", "schema-check", "schema-check-allow-list.json" );
        }

        switch ( options.Command )
        {
            case "mapping":
            case "names":
                options.DatabaseConnectionString = ResolveDatabase( database, defaultConnectionString, "--database" );
                break;

            case "equivalence":
            case "commute":
                options.LeftConnectionString = ResolveDatabase( left ?? throw new ArgumentException( "--left is required." ), defaultConnectionString, "--left" );
                options.RightConnectionString = ResolveDatabase( right ?? throw new ArgumentException( "--right is required." ), defaultConnectionString, "--right" );
                break;

            default:
                throw new ArgumentException( $"Unknown command '{options.Command}'." );
        }

        return options;
    }

    /// <summary>
    /// Turns a database argument into a connection string. A value
    /// containing <c>=</c> is already a connection string; any other value
    /// is a database name on the RockWeb server.
    /// </summary>
    private static string ResolveDatabase( string value, string defaultConnectionString, string argumentName )
    {
        if ( value != null && value.Contains( "=" ) )
        {
            return value;
        }

        if ( defaultConnectionString == null )
        {
            throw new ArgumentException( $"{argumentName} needs a connection string because RockWeb/web.ConnectionStrings.config was not found." );
        }

        if ( value == null )
        {
            return defaultConnectionString;
        }

        return new SqlConnectionStringBuilder( defaultConnectionString ) { InitialCatalog = value }.ConnectionString;
    }

    /// <summary>
    /// Reads the <c>RockContext</c> connection string from
    /// <c>RockWeb/web.ConnectionStrings.config</c>, or returns <c>null</c>
    /// when the file is missing.
    /// </summary>
    private static string ReadRockWebConnectionString( string repositoryRoot )
    {
        var configPath = Path.Combine( repositoryRoot, "RockWeb", "web.ConnectionStrings.config" );

        if ( !File.Exists( configPath ) )
        {
            return null;
        }

        var document = new XmlDocument();
        document.Load( configPath );

        var connectionString = document.DocumentElement?.SelectSingleNode( "add[@name = \"RockContext\"]" )?.Attributes?["connectionString"]?.Value;

        // The config file spreads the connection string across lines.
        return connectionString == null ? null : new SqlConnectionStringBuilder( connectionString.Trim() ).ConnectionString;
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
