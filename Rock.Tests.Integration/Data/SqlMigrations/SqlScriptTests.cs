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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Data.SqlMigrations;

/// <summary>
/// Checks the scripts this test project embeds for its own test migrations,
/// and that the script check reports every kind of problem. The checks of
/// <c>Rock.Data.Migrations</c> itself are in <c>Rock.Tests</c>
/// (<c>SqlMigrationFileTests</c>), where CI runs them on every pull request.
/// </summary>
[TestClass]
public class SqlScriptTests
{
    [TestMethod]
    public void Scripts_TestProjectScripts_AreEmbeddedAndValid()
    {
        var problems = SqlMigrationFileChecks.FindScriptProblems( GetType().Assembly, Path.Combine( SqlMigrationFileChecks.RepositoryRoot(), "Rock.Tests.Integration", "Data", "SqlMigrations", "Scripts" ) );

        CollectionAssert.AreEqual( new string[0], problems );
    }

    [TestMethod]
    public void Scripts_FolderWithEveryProblem_ReportsEachOne()
    {
        var folder = Path.Combine( Path.GetTempPath(), $"RockScriptTest_{Guid.NewGuid():N}" );
        Directory.CreateDirectory( folder );

        try
        {
            // Embedded in this assembly, but the content on disk differs.
            File.WriteAllText( Path.Combine( folder, "20250901000000.sql" ), "SELECT 'changed';" );
            // Not embedded, and no migration has this ID.
            File.WriteAllText( Path.Combine( folder, "20991231235959.sql" ), "SELECT 1;" );
            // Invalid suffix character.
            File.WriteAllText( Path.Combine( folder, "20250901000000-Bad-Suffix.sql" ), "SELECT 1;" );
            // Compressed files must never be committed.
            File.WriteAllText( Path.Combine( folder, "20250901000000.sql.gz" ), string.Empty );
            File.WriteAllText( Path.Combine( folder, "20250901000000-Data.json.gz" ), string.Empty );

            var problems = SqlMigrationFileChecks.FindScriptProblems( GetType().Assembly, folder );

            CollectionAssert.AreEqual( new[]
            {
                "20250901000000-Bad-Suffix.sql: the name must be the 14-digit migration ID, optionally followed by '-' and a suffix of letters, digits and underscores.",
                "20250901000000-Data.json.gz is committed. Commit the .json; the build compresses it.",
                "20250901000000-Second_Part.sql.gz is embedded but has no .sql file on disk.",
                "20250901000000.sql.gz is committed. Commit the .sql; the build compresses it.",
                "20250901000000.sql: the embedded copy does not match the file on disk. Rebuild the project.",
                "20250903000000-BulkRow.json.gz is embedded but has no .json file on disk.",
                "20250903000000.sql.gz is embedded but has no .sql file on disk.",
                "20991231235959.sql has no migration with ID 20991231235959 (orphan script).",
                "20991231235959.sql is not embedded. Check the project's SqlScript items."
            }, problems );
        }
        finally
        {
            Directory.Delete( folder, true );
        }
    }
}
