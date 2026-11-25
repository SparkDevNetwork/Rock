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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Data.SqlMigrations;

/// <summary>
/// The stray-file guard. Develop runs only SQL migrations, so the EF6
/// <c>Rock.Migrations</c> project and the <c>Rock/Plugin/HotFixes</c> folder
/// were removed. Branches without the runner (release, hotfix and older
/// feature branches) still add files there, and git merges a new file into
/// a removed folder with no conflict. Nothing compiles or runs it, so the
/// migration would be silently lost. These tests fail on any such file so
/// the merge converts it to <c>Rock.Data.Migrations</c> instead.
/// </summary>
/// <remarks>
/// Git's directory rename detection sees the converted migrations as
/// renames of the removed files, so it reports a new EF6 migration or
/// HotFix from such a branch as a "file location" conflict and moves it into
/// <c>Rock.Data.Migrations/Migrations</c>. Accepting that location must
/// still fail here, so these tests also check the file names in that folder
/// and the types in its assembly.
/// </remarks>
[TestClass]
public class StrayMigrationFilesTests
{
    #region Fields

    /// <summary>
    /// The file types that can carry a migration or its scripts. Other files,
    /// such as the git-ignored <c>bin</c> and <c>obj</c> output and
    /// <c>.csproj.user</c> files left on developer machines, are not checked.
    /// </summary>
    private static readonly string[] MigrationFileExtensions = { ".cs", ".sql", ".resx", ".csproj" };

    /// <summary>
    /// The folders that must not exist any more, relative to the folder
    /// containing <c>Rock.sln</c>.
    /// </summary>
    private static readonly string[] RemovedFolders =
    {
        "Rock.Migrations",
        Path.Combine( "Rock", "Plugin", "HotFixes" )
    };

    /// <summary>
    /// A converted or new migration's class file: the 14-digit migration ID,
    /// then the migration name. EF6 IDs have 15 digits and HotFix files start
    /// with their number, so neither matches, and neither do EF6
    /// <c>.Designer.cs</c> files.
    /// </summary>
    private static readonly Regex MigrationClassFileName = new Regex( @"^\d{14}_[A-Za-z0-9_]+\.cs$" );

    #endregion

    #region Tests

    [TestMethod]
    public void RemovedMigrationFolders_HaveNoMigrationFiles()
    {
        var root = SqlMigrationFileChecks.RepositoryRoot();

        var strayFiles = RemovedFolders
            .SelectMany( folder => FindMigrationFiles( Path.Combine( root, folder ) ) )
            .Select( path => path.Substring( root.Length + 1 ) )
            .OrderBy( path => path, StringComparer.OrdinalIgnoreCase )
            .ToList();

        Assert.AreEqual( 0, strayFiles.Count, $"Found {strayFiles.Count} files in folders that develop no longer builds. They most likely came from a merge of a branch that still writes EF6 migrations or HotFixes. Convert each migration or HotFix to Rock.Data.Migrations (see the conversion checklist in specs/261027-sql-migrations-on-net-framework.md), then delete the files:{Environment.NewLine}{string.Join( Environment.NewLine, strayFiles )}" );
    }

    [TestMethod]
    public void MigrationFolder_HasOnlySqlMigrationFileNames()
    {
        var root = SqlMigrationFileChecks.RepositoryRoot();
        var migrationsFolder = Path.Combine( root, "Rock.Data.Migrations", "Migrations" );
        var baselineFolder = Path.Combine( migrationsFolder, "Baseline" ) + Path.DirectorySeparatorChar;

        // Scripts and data files are checked by SqlMigrationFileTests.
        var badFiles = FindMigrationFiles( migrationsFolder )
            .Where( path => !path.StartsWith( baselineFolder, StringComparison.OrdinalIgnoreCase ) )
            .Where( path => !path.EndsWith( ".sql", StringComparison.OrdinalIgnoreCase ) )
            .Where( path => !MigrationClassFileName.IsMatch( Path.GetFileName( path ) ) )
            .Select( path => path.Substring( root.Length + 1 ) )
            .OrderBy( path => path, StringComparer.OrdinalIgnoreCase )
            .ToList();

        Assert.AreEqual( 0, badFiles.Count, $"Found {badFiles.Count} files in Rock.Data.Migrations/Migrations that are not named like a SQL migration ({{14-digit ID}}_{{Name}}.cs). Git moves new EF6 migrations and HotFixes here when it merges a branch that still writes them. Convert each one (see the conversion checklist in specs/261027-sql-migrations-on-net-framework.md), then delete the files:{Environment.NewLine}{string.Join( Environment.NewLine, badFiles )}" );
    }

    [TestMethod]
    public void RockAndMigrationsAssemblies_HaveNoHotFixMigrations()
    {
        var assemblies = new[]
        {
            typeof( Rock.Plugin.Migration ).Assembly,
            typeof( Rock.Data.Migrations.Baseline ).Assembly
        };

        var hotFixTypes = assemblies
            .SelectMany( a => a.GetTypes() )
            .Where( t => !t.IsAbstract && typeof( Rock.Plugin.Migration ).IsAssignableFrom( t ) )
            .Select( t => $"{t.FullName} ({t.Assembly.GetName().Name})" )
            .OrderBy( name => name, StringComparer.Ordinal )
            .ToList();

        Assert.AreEqual( 0, hotFixTypes.Count, $"Found {hotFixTypes.Count} HotFix migrations. Develop does not run HotFixes, and one in Rock.Data.Migrations would run through the plugin runner. Convert each to Rock.Data.Migrations with ReplacesHotFixNumber, then delete it:{Environment.NewLine}{string.Join( Environment.NewLine, hotFixTypes )}" );
    }

    #endregion

    #region Support Methods

    /// <summary>
    /// Finds every file that can carry a migration under the folder,
    /// skipping <c>bin</c> and <c>obj</c> build output.
    /// </summary>
    /// <param name="folder">The folder to search.</param>
    /// <returns>The full paths of the files found.</returns>
    private static IEnumerable<string> FindMigrationFiles( string folder )
    {
        if ( !Directory.Exists( folder ) )
        {
            return Enumerable.Empty<string>();
        }

        return Directory.EnumerateFiles( folder, "*", SearchOption.AllDirectories )
            .Where( path => MigrationFileExtensions.Contains( Path.GetExtension( path ), StringComparer.OrdinalIgnoreCase ) )
            .Where( path => !SqlMigrationFileChecks.IsBuildOutput( folder, path ) );
    }

    #endregion
}
