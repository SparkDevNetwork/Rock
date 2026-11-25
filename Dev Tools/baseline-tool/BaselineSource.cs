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
using System.Text.RegularExpressions;

namespace Rock.BaselineTool;

/// <summary>
/// What the tool reads from B's source tree: the run-once job GUIDs that
/// <c>DataMigrationsStartup</c> lists and the HotFix numbers in
/// <c>Rock.dll</c>. Reading source rather than compiling against B lets
/// one tool build the baseline for any version.
/// </summary>
internal sealed class BaselineSource
{
    /// <summary>
    /// A <c>SystemGuid.ServiceJob</c> constant.
    /// </summary>
    private static readonly Regex GuidConstant = new Regex( @"public\s+const\s+string\s+(?<name>\w+)\s*=\s*""(?<guid>[0-9A-Fa-f-]{36})""" );

    /// <summary>
    /// An entry of a <c>DataMigrationsStartup</c> list.
    /// </summary>
    private static readonly Regex ListEntry = new Regex( @"SystemGuid\.ServiceJob\.(?<name>\w+)\.AsGuid\(\)" );

    /// <summary>
    /// A HotFix's <c>[MigrationNumber( number, "minimum version" )]</c> attribute.
    /// </summary>
    private static readonly Regex MigrationNumber = new Regex( @"\[MigrationNumber\(\s*(?<number>\d+)\s*,\s*""(?<version>[^""]+)""" );

    /// <summary>
    /// The root of B's source tree.
    /// </summary>
    public string RootPath { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaselineSource"/> class.
    /// </summary>
    /// <param name="rootPath">The root of B's source tree.</param>
    public BaselineSource( string rootPath )
    {
        if ( !File.Exists( Path.Combine( rootPath, "Rock.sln" ) ) )
        {
            throw new ArgumentException( $"'{rootPath}' is not the root of a Rock source tree (no Rock.sln)." );
        }

        RootPath = rootPath;
    }

    /// <summary>
    /// Reads the GUIDs of every job on both <c>DataMigrationsStartup</c>
    /// lists (the startup list and the 2 AM list), with their constant names.
    /// </summary>
    /// <returns>The job GUIDs keyed by constant name.</returns>
    public Dictionary<string, Guid> GetRunOnceJobGuids()
    {
        var startupPath = Directory.EnumerateFiles( RootPath, "DataMigrationsStartup.cs", SearchOption.AllDirectories )
            .FirstOrDefault( f => !f.Contains( $"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}" ) )
            ?? throw new InvalidOperationException( "DataMigrationsStartup.cs was not found in the source tree." );
        var constantsPath = Path.Combine( RootPath, "Rock", "SystemGuid", "ServiceJob.cs" );

        var guidsByName = GuidConstant.Matches( File.ReadAllText( constantsPath ) )
            .Cast<Match>()
            .ToDictionary( m => m.Groups["name"].Value, m => Guid.Parse( m.Groups["guid"].Value ) );

        var jobs = new Dictionary<string, Guid>();

        foreach ( Match match in ListEntry.Matches( File.ReadAllText( startupPath ) ) )
        {
            var name = match.Groups["name"].Value;

            if ( !guidsByName.TryGetValue( name, out var guid ) )
            {
                throw new InvalidOperationException( $"DataMigrationsStartup lists SystemGuid.ServiceJob.{name}, which is not defined in {constantsPath}." );
            }

            jobs[name] = guid;
        }

        return jobs;
    }

    /// <summary>
    /// Reads the numbers of the HotFixes that a Rock of the given version
    /// runs: every <c>Rock/Plugin/HotFixes</c> migration whose minimum
    /// version is at or below it, as <c>RunPluginMigrations</c> decides.
    /// </summary>
    /// <param name="rockVersion">B's Rock version.</param>
    /// <returns>The HotFix numbers.</returns>
    public List<int> GetHotFixNumbers( Version rockVersion )
    {
        var numbers = new List<int>();

        foreach ( var file in Directory.EnumerateFiles( Path.Combine( RootPath, "Rock", "Plugin", "HotFixes" ), "*.cs" ) )
        {
            var match = MigrationNumber.Match( File.ReadAllText( file ) );

            if ( match.Success && Version.Parse( match.Groups["version"].Value ) <= rockVersion )
            {
                numbers.Add( int.Parse( match.Groups["number"].Value ) );
            }
        }

        return numbers.OrderBy( n => n ).ToList();
    }
}
