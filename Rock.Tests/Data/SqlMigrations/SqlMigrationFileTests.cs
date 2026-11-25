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
using System.IO;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Data.SqlMigrations;

/// <summary>
/// Checks that the scripts in <c>Rock.Data.Migrations</c> are embedded
/// correctly and that migration IDs are valid, so mistakes fail the build
/// rather than startup. These need no database, so they live here, where
/// CI runs them on every pull request.
/// </summary>
[TestClass]
public class SqlMigrationFileTests
{
    private static Assembly MigrationsAssembly => typeof( Rock.Data.Migrations.Baseline ).Assembly;

    [TestMethod]
    public void Scripts_RockDataMigrations_AreEmbeddedAndValid()
    {
        var problems = SqlMigrationFileChecks.FindScriptProblems( MigrationsAssembly, Path.Combine( SqlMigrationFileChecks.RepositoryRoot(), "Rock.Data.Migrations" ) );

        CollectionAssert.AreEqual( new string[0], problems );
    }

    [TestMethod]
    public void MigrationIds_RockDataMigrations_AreValidAndUnique()
    {
        var problems = SqlMigrationFileChecks.FindIdProblems( MigrationsAssembly );

        CollectionAssert.AreEqual( new string[0], problems );
    }

    [TestMethod]
    public void MigrationIds_InvalidIds_ReportsEachOne()
    {
        var problems = SqlMigrationFileChecks.FindIdProblems( new[]
        {
            ( "Valid", 20261027143015L ),
            ( "TooShort", 2026102714301L ),
            ( "NotADate", 20261327143015L ),
            ( "Duplicate", 20261027143015L )
        } );

        CollectionAssert.AreEqual( new[]
        {
            "Migration ID 20261027143015 is used by Valid and Duplicate.",
            "NotADate: migration ID 20261327143015 is not a valid yyyyMMddHHmmss date.",
            "TooShort: migration ID 2026102714301 must be exactly 14 digits."
        }, problems );
    }
}
