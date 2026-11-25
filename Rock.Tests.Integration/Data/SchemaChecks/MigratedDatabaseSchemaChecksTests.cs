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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Data;
using Rock.Model;
using Rock.SchemaCheck;
using Rock.Tests.Integration.TestFramework.Database;
using Rock.WebStartup;

namespace Rock.Tests.Integration.Data.SchemaChecks;

/// <summary>
/// Runs the name ratchet and the mapping check against the test database,
/// which is built from the baseline and every migration in
/// <c>Rock.Data.Migrations</c>. These are the required schema checks that
/// gate SQL migrations; findings are accepted only through the allow list
/// beside the schema check tool.
/// </summary>
[TestClass]
public class MigratedDatabaseSchemaChecksTests : DatabaseTestsBase
{
    #region Tests

    [TestMethod]
    public void Names_MigratedDatabase_EveryNewNameFollowsEf6Conventions()
    {
        var catalog = SchemaCatalogReader.Read( GetConnectionString() );

        var findings = Rock.Data.SchemaChecks.CheckNames( catalog );

        AssertNoBlockingFindings( findings, Rock.Data.SchemaChecks.NamesCheck );
    }

    [TestMethod]
    [IsolatedTestDatabase]
    public void Mapping_MigratedDatabase_SupportsEverythingTheModelMaps()
    {
        /*
            11/16/26 - CLAUDE

            Rock's startup, not its migrations, registers entity types and
            builds the AttributeValue_* views from the model, so a database
            built only from migrations lacks the views for newer entities.
            Run those two startup steps first, as every real start does, so
            the check reports only what migrations are responsible for.

            Reason: Check the schema a running Rock sees, not a half-started one.
        */
        EntityTypeService.RegisterEntityTypes();
        RockApplicationStartupHelper.InitializeQueryableAttributeValues();

        var connectionString = GetConnectionString();
        var expected = Ef6ModelAdapter.Build( connectionString );
        var catalog = SchemaCatalogReader.Read( connectionString );

        var findings = Rock.Data.SchemaChecks.CheckMapping( expected, catalog );

        AssertNoBlockingFindings( findings, Rock.Data.SchemaChecks.MappingCheck );
    }

    #endregion

    #region Support Methods

    /// <summary>
    /// Gets the connection string of the test database the container set up.
    /// </summary>
    private static string GetConnectionString()
    {
        return RockApp.Current.GetRequiredService<IConnectionStringProvider>().ConnectionString;
    }

    /// <summary>
    /// Applies the allow list for the check and fails with every finding
    /// that is not allowed, including allow list entries that no longer
    /// match anything.
    /// </summary>
    /// <param name="findings">The findings from the check.</param>
    /// <param name="check">The check that ran.</param>
    private static void AssertNoBlockingFindings( List<SchemaFinding> findings, string check )
    {
        var allowListPath = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "TestData", "schema-check-allow-list.json" );
        var allowList = SchemaAllowList.FromJson( File.ReadAllText( allowListPath ) );

        findings.AddRange( allowList.Apply( findings, check ) );

        var blocking = findings
            .Where( f => f.IsBlocking )
            .OrderBy( f => f.Code )
            .ThenBy( f => f.Object, StringComparer.OrdinalIgnoreCase )
            .Select( f => $"{f.Code} {f.Object}: {f.Message}" )
            .ToList();

        Assert.AreEqual( 0, blocking.Count, $"The {check} check found {blocking.Count} problems. Fix them with a migration, or add them to Dev Tools/schema-check/schema-check-allow-list.json with a reason:{Environment.NewLine}{string.Join( Environment.NewLine, blocking )}" );
    }

    #endregion
}
