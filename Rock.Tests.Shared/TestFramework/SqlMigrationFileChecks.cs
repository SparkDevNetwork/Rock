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
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using Rock.Data;

namespace Rock.Tests.Shared.TestFramework;

/// <summary>
/// Checks of SQL migration files and IDs that need no database, shared by
/// <c>Rock.Tests</c>, which CI runs on every pull request, and
/// <c>Rock.Tests.Integration</c>, which checks its own test scripts.
/// </summary>
internal static class SqlMigrationFileChecks
{
    #region Fields

    /// <summary>
    /// A script or JSON data file name: the 14-digit migration ID, then an
    /// optional suffix of letters, digits and underscores after a <c>-</c>.
    /// </summary>
    private static readonly Regex ScriptName = new Regex( @"^(?<id>\d{14})(-(?<suffix>[A-Za-z0-9_]+))?\.(sql|json)$" );

    /// <summary>
    /// GitHub warns at 50 MB; larger scripts must be split.
    /// </summary>
    private const long MaximumScriptBytes = 50L * 1024 * 1024;

    #endregion Fields

    #region Checks

    /// <summary>
    /// Compares the <c>.sql</c> and <c>.json</c> files under a folder with
    /// the <c>.sql.gz</c> and <c>.json.gz</c> resources embedded in an assembly.
    /// </summary>
    /// <param name="assembly">The assembly the scripts are embedded in.</param>
    /// <param name="folder">The folder holding the scripts on disk.</param>
    /// <returns>One sorted line per problem.</returns>
    internal static List<string> FindScriptProblems( Assembly assembly, string folder )
    {
        var problems = new List<string>();
        var files = Directory.EnumerateFiles( folder, "*", SearchOption.AllDirectories )
            .Where( f => !IsBuildOutput( folder, f ) )
            .ToList();
        var resources = new HashSet<string>( assembly.GetManifestResourceNames().Where( r => r.EndsWith( ".sql.gz", StringComparison.OrdinalIgnoreCase ) || r.EndsWith( ".json.gz", StringComparison.OrdinalIgnoreCase ) ) );
        var migrationIds = new HashSet<long>( assembly.GetTypes()
            .Select( t => t.GetCustomAttribute<SqlMigrationAttribute>() )
            .Where( a => a != null )
            .Select( a => a.Id ) );
        var scriptFileNames = new HashSet<string>( StringComparer.OrdinalIgnoreCase );

        foreach ( var file in files.Where( f => f.EndsWith( ".sql.gz", StringComparison.OrdinalIgnoreCase ) || f.EndsWith( ".json.gz", StringComparison.OrdinalIgnoreCase ) ) )
        {
            problems.Add( $"{Path.GetFileName( file )} is committed. Commit the {Path.GetExtension( Path.GetFileNameWithoutExtension( file ) )}; the build compresses it." );
        }

        foreach ( var file in files.Where( f => f.EndsWith( ".sql", StringComparison.OrdinalIgnoreCase ) || f.EndsWith( ".json", StringComparison.OrdinalIgnoreCase ) ) )
        {
            var fileName = Path.GetFileName( file );
            var match = ScriptName.Match( fileName );
            scriptFileNames.Add( fileName );

            if ( !match.Success )
            {
                problems.Add( $"{fileName}: the name must be the 14-digit migration ID, optionally followed by '-' and a suffix of letters, digits and underscores." );
                continue;
            }

            if ( new FileInfo( file ).Length >= MaximumScriptBytes )
            {
                problems.Add( $"{fileName} is 50 MB or larger. Split it into ordered named scripts." );
            }

            var id = long.Parse( match.Groups["id"].Value );

            if ( !migrationIds.Contains( id ) )
            {
                problems.Add( $"{fileName} has no migration with ID {id} (orphan script)." );
            }

            if ( !resources.Contains( $"{fileName}.gz" ) )
            {
                problems.Add( $"{fileName} is not embedded. Check the project's SqlScript items." );
            }
            else if ( !ReadResource( assembly, $"{fileName}.gz" ).SequenceEqual( File.ReadAllBytes( file ) ) )
            {
                problems.Add( $"{fileName}: the embedded copy does not match the file on disk. Rebuild the project." );
            }
        }

        foreach ( var resource in resources.Where( r => !scriptFileNames.Contains( r.Substring( 0, r.Length - 3 ) ) ) )
        {
            problems.Add( $"{resource} is embedded but has no {Path.GetExtension( resource.Substring( 0, resource.Length - 3 ) )} file on disk." );
        }

        return problems.OrderBy( p => p, StringComparer.Ordinal ).ToList();
    }

    /// <summary>
    /// Checks every <see cref="Migration"/> type in an assembly has a
    /// valid, unique ID.
    /// </summary>
    /// <param name="assembly">The assembly holding the migrations.</param>
    /// <returns>One sorted line per problem.</returns>
    internal static List<string> FindIdProblems( Assembly assembly )
    {
        var problems = new List<string>();
        var migrationTypes = assembly.GetTypes().Where( t => typeof( Migration ).IsAssignableFrom( t ) && !t.IsAbstract ).ToList();
        var ids = new List<(string Name, long Id)>();

        foreach ( var type in migrationTypes )
        {
            var attribute = type.GetCustomAttribute<SqlMigrationAttribute>();

            if ( attribute == null )
            {
                problems.Add( $"{type.FullName} has no SqlMigration attribute." );
            }
            else
            {
                ids.Add( (type.FullName, attribute.Id) );
            }
        }

        problems.AddRange( FindIdProblems( ids ) );

        return problems.OrderBy( p => p, StringComparer.Ordinal ).ToList();
    }

    /// <summary>
    /// Checks that each ID is 14 digits, a valid <c>yyyyMMddHHmmss</c> date, and unique.
    /// </summary>
    /// <param name="ids">The name and ID of each migration.</param>
    /// <returns>One sorted line per problem.</returns>
    internal static List<string> FindIdProblems( IEnumerable<(string Name, long Id)> ids )
    {
        var problems = new List<string>();
        var idList = ids.ToList();

        foreach ( var (name, id) in idList )
        {
            var text = id.ToString( CultureInfo.InvariantCulture );

            if ( text.Length != 14 )
            {
                problems.Add( $"{name}: migration ID {id} must be exactly 14 digits." );
            }
            else if ( !DateTime.TryParseExact( text, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out _ ) )
            {
                problems.Add( $"{name}: migration ID {id} is not a valid yyyyMMddHHmmss date." );
            }
        }

        foreach ( var group in idList.GroupBy( i => i.Id ).Where( g => g.Count() > 1 ) )
        {
            problems.Add( $"Migration ID {group.Key} is used by {string.Join( " and ", group.Select( i => i.Name ) )}." );
        }

        return problems.OrderBy( p => p, StringComparer.Ordinal ).ToList();
    }

    #endregion Checks

    #region Support

    /// <summary>
    /// Finds the folder containing <c>Rock.sln</c>, walking up from the test assembly.
    /// </summary>
    /// <returns>The full path of the repository root.</returns>
    internal static string RepositoryRoot()
    {
        var directory = new DirectoryInfo( AppDomain.CurrentDomain.BaseDirectory );

        while ( directory != null && !File.Exists( Path.Combine( directory.FullName, "Rock.sln" ) ) )
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException( "Could not find Rock.sln above the test folder." );
    }

    /// <summary>
    /// Whether a file is under a <c>bin</c> or <c>obj</c> folder, at any
    /// depth, below the scanned folder. Shared by every migration file
    /// check, so they all skip the same build output.
    /// </summary>
    /// <param name="folder">The scanned folder.</param>
    /// <param name="file">The full path of the file.</param>
    /// <returns><c>true</c> if the file is build output.</returns>
    internal static bool IsBuildOutput( string folder, string file )
    {
        var relativeFolders = Path.GetDirectoryName( file ).Substring( folder.TrimEnd( Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar ).Length )
            .Split( new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries );

        return relativeFolders.Any( name => name.Equals( "bin", StringComparison.OrdinalIgnoreCase ) || name.Equals( "obj", StringComparison.OrdinalIgnoreCase ) );
    }

    /// <summary>
    /// Reads and decompresses an embedded <c>.sql.gz</c> or <c>.json.gz</c> resource.
    /// </summary>
    /// <param name="assembly">The assembly holding the resource.</param>
    /// <param name="name">The resource name.</param>
    /// <returns>The decompressed bytes.</returns>
    private static byte[] ReadResource( Assembly assembly, string name )
    {
        using var stream = assembly.GetManifestResourceStream( name );
        using var gzip = new GZipStream( stream, CompressionMode.Decompress );
        using var memory = new MemoryStream();
        gzip.CopyTo( memory );

        return memory.ToArray();
    }

    #endregion Support
}
