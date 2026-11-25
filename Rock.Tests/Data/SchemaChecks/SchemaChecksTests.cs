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
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Data.SchemaChecks;

/// <summary>
/// Tests the mapping check, name ratchet, catalog equivalence and allow
/// list on catalogs built in memory.
/// </summary>
[TestClass]
public class SchemaChecksTests
{
    #region Mapping Check

    [TestMethod]
    public void Mapping_DatabaseMatchesModel_HasNoFindings()
    {
        var findings = Rock.Data.SchemaChecks.CheckMapping( ExpectedPerson(), ActualPerson() );

        CollectionAssert.AreEqual( new string[0], SchemaFindings.Describe( findings ) );
    }

    [TestMethod]
    public void Mapping_TableMissing_Fails()
    {
        var actual = new SchemaCatalog();

        var findings = Rock.Data.SchemaChecks.CheckMapping( ExpectedPerson(), actual );

        CollectionAssert.AreEqual( new[] { "FAIL mapping MissingTable dbo.Person" }, SchemaFindings.Describe( findings ) );
    }

    [TestMethod]
    public void Mapping_ColumnMissing_Fails()
    {
        var actual = ActualPerson();
        actual.Tables["dbo.Person"].Columns.Remove( "NickName" );

        var findings = Rock.Data.SchemaChecks.CheckMapping( ExpectedPerson(), actual );

        CollectionAssert.AreEqual( new[] { "FAIL mapping MissingColumn dbo.Person.NickName" }, SchemaFindings.Describe( findings ) );
    }

    [TestMethod]
    public void Mapping_TypeLengthPrecisionAndNullabilityDiffer_FailsForEach()
    {
        var actual = ActualPerson();
        var columns = actual.Tables["dbo.Person"].Columns;
        columns["Age"].StoreType = "bigint";
        columns["NickName"].MaxLength = 100;
        columns["Balance"].Scale = 4;
        columns["Email"].IsNullable = false;

        var findings = Rock.Data.SchemaChecks.CheckMapping( ExpectedPerson(), actual );

        CollectionAssert.AreEqual( new[]
        {
            "FAIL mapping LengthMismatch dbo.Person.NickName",
            "FAIL mapping NullabilityMismatch dbo.Person.Email",
            "FAIL mapping PrecisionMismatch dbo.Person.Balance",
            "FAIL mapping TypeMismatch dbo.Person.Age"
        }, SchemaFindings.Describe( findings ) );
        Assert.AreEqual( "The model maps decimal(18,2) but the database has decimal(18,4).", findings.Single( f => f.Code == "PrecisionMismatch" ).Message );
    }

    [TestMethod]
    public void Mapping_PrimaryKeyDiffers_Fails()
    {
        var actual = ActualPerson();
        actual.Tables["dbo.Person"].PrimaryKey.KeyColumns = new List<string> { "Id", "Age" };

        var findings = Rock.Data.SchemaChecks.CheckMapping( ExpectedPerson(), actual );

        CollectionAssert.AreEqual( new[] { "FAIL mapping PrimaryKeyMismatch dbo.Person" }, SchemaFindings.Describe( findings ) );
        Assert.AreEqual( "The model maps primary key (Id) but the database has (Id, Age).", findings[0].Message );
    }

    [TestMethod]
    public void Mapping_UnmappedNotNullColumnWithoutDefault_Fails()
    {
        var actual = ActualPerson();
        actual.Tables["dbo.Person"]
            .AddColumn( Column( "Required", "int" ) )
            .AddColumn( Column( "Defaulted", "int", defaultDefinition: "((0))" ) )
            .AddColumn( Column( "Optional", "int", isNullable: true ) );

        var findings = Rock.Data.SchemaChecks.CheckMapping( ExpectedPerson(), actual );

        CollectionAssert.AreEqual( new[] { "FAIL mapping UnmappedRequiredColumn dbo.Person.Required" }, SchemaFindings.Describe( findings ) );
    }

    [TestMethod]
    public void Mapping_EntityMappedToView_ChecksOnlyColumnPresence()
    {
        var expected = new SchemaExpectedModel();
        var expectedView = new SchemaExpectedTable { Name = "PersonView", PrimaryKeyColumns = new List<string> { "Id" } };
        expectedView.Columns.Add( Column( "Id", "int" ) );
        expectedView.Columns.Add( Column( "FullName", "nvarchar", maxLength: -1 ) );
        expectedView.Columns.Add( Column( "Missing", "int" ) );
        expected.Tables.Add( expectedView );

        var actual = new SchemaCatalog();
        var actualView = new SchemaCatalogTable { Name = "PersonView" }
            .AddColumn( Column( "Id", "int", isNullable: true ) )
            .AddColumn( Column( "FullName", "varchar", maxLength: 101 ) );
        actual.Views.Add( actualView.Key, actualView );

        var findings = Rock.Data.SchemaChecks.CheckMapping( expected, actual );

        CollectionAssert.AreEqual( new[] { "FAIL mapping MissingColumn dbo.PersonView.Missing" }, SchemaFindings.Describe( findings ) );
    }

    [TestMethod]
    public void Mapping_ForeignKeyMissingOrCascadeDiffers_IsInformational()
    {
        var expected = ExpectedPerson();
        expected.Tables[0].ForeignKeys.Add( new SchemaCatalogForeignKey { Columns = new List<string> { "Age" }, ReferencedTable = "dbo.Age", IsCascadeDelete = true } );
        expected.Tables[0].ForeignKeys.Add( new SchemaCatalogForeignKey { Columns = new List<string> { "NickName" }, ReferencedTable = "dbo.Nick" } );

        var actual = ActualPerson();
        actual.Tables["dbo.Person"].ForeignKeys.Add( new SchemaCatalogForeignKey { Name = "FK_dbo.Person_dbo.Age_Age", Columns = new List<string> { "Age" }, ReferencedTable = "dbo.Age", ReferencedColumns = new List<string> { "Id" } } );

        var findings = Rock.Data.SchemaChecks.CheckMapping( expected, actual );

        CollectionAssert.AreEqual( new[]
        {
            "INFO mapping CascadeMismatch dbo.Person(Age)",
            "INFO mapping MissingForeignKey dbo.Person(NickName)"
        }, SchemaFindings.Describe( findings ) );
    }

    #endregion Mapping Check

    #region Name Ratchet

    [TestMethod]
    public void Names_EfConventionNames_Pass()
    {
        var catalog = new SchemaCatalog();
        var table = catalog.AddTable( new SchemaCatalogTable { Name = "GroupMember" } );
        table.PrimaryKey = new SchemaCatalogIndex { Name = "PK_dbo.GroupMember", KeyColumns = new List<string> { "Id" } };
        table.ForeignKeys.Add( new SchemaCatalogForeignKey { Name = "FK_dbo.GroupMember_dbo.Group_GroupId", Columns = new List<string> { "GroupId" }, ReferencedTable = "dbo.Group" } );
        table.Indexes.Add( new SchemaCatalogIndex { Name = "IX_GroupId_PersonId", KeyColumns = new List<string> { "GroupId", "PersonId DESC" } } );

        var findings = Rock.Data.SchemaChecks.CheckNames( catalog );

        CollectionAssert.AreEqual( new string[0], SchemaFindings.Describe( findings ) );
    }

    [TestMethod]
    public void Names_NonconformingNames_Fail()
    {
        var catalog = new SchemaCatalog();
        var table = catalog.AddTable( new SchemaCatalogTable { Name = "GroupMember" } );
        table.PrimaryKey = new SchemaCatalogIndex { Name = "PK_GroupMember", KeyColumns = new List<string> { "Id" } };
        table.ForeignKeys.Add( new SchemaCatalogForeignKey { Name = "FK_GroupMember_Group", Columns = new List<string> { "GroupId" }, ReferencedTable = "dbo.Group" } );
        table.Indexes.Add( new SchemaCatalogIndex { Name = "IX_GroupIdPersonId", KeyColumns = new List<string> { "GroupId", "PersonId" } } );

        var findings = Rock.Data.SchemaChecks.CheckNames( catalog );

        CollectionAssert.AreEqual( new[]
        {
            "FAIL names ForeignKeyName dbo.GroupMember/FK_GroupMember_Group",
            "FAIL names IndexName dbo.GroupMember/IX_GroupIdPersonId",
            "FAIL names PrimaryKeyName dbo.GroupMember/PK_GroupMember"
        }, SchemaFindings.Describe( findings ) );
        Assert.AreEqual( "Expected the EF6 name 'IX_GroupId_PersonId'.", findings.Single( f => f.Code == "IndexName" ).Message );
    }

    [TestMethod]
    public void Names_AllowListedNonconformingName_IsAllowed()
    {
        var catalog = new SchemaCatalog();
        var table = catalog.AddTable( new SchemaCatalogTable { Name = "GroupMember" } );
        table.Indexes.Add( new SchemaCatalogIndex { Name = "IX_Old", KeyColumns = new List<string> { "GroupId" } } );
        table.Indexes.Add( new SchemaCatalogIndex { Name = "IX_New", KeyColumns = new List<string> { "PersonId" } } );
        var allowList = new SchemaAllowList
        {
            Entries = new List<SchemaAllowListEntry>
            {
                new SchemaAllowListEntry { Check = "names", Code = "IndexName", Object = "dbo.GroupMember/IX_Old", Reason = "Frozen." }
            }
        };

        var findings = Rock.Data.SchemaChecks.CheckNames( catalog );
        findings.AddRange( allowList.Apply( findings, "names" ) );

        CollectionAssert.AreEqual( new[]
        {
            "ALLOW names IndexName dbo.GroupMember/IX_Old",
            "FAIL names IndexName dbo.GroupMember/IX_New"
        }, SchemaFindings.Describe( findings ) );
    }

    #endregion Name Ratchet

    #region Allow List

    [TestMethod]
    public void AllowList_EntryMatchingNothing_FailsAsStale()
    {
        var allowList = SchemaAllowList.FromJson( @"{ ""Entries"": [
                { ""Check"": ""mapping"", ""Code"": ""MissingColumn"", ""Object"": ""dbo.Person.Gone"", ""Reason"": ""Dropped later."" },
                { ""Check"": ""names"", ""Code"": ""IndexName"", ""Object"": ""dbo.Person/IX_Other"", ""Reason"": ""Another check."" } ] }" );

        var stale = allowList.Apply( new List<SchemaFinding>(), "mapping" );

        CollectionAssert.AreEqual( new[] { "FAIL mapping StaleAllowListEntry dbo.Person.Gone" }, SchemaFindings.Describe( stale ) );
    }

    [TestMethod]
    public void AllowList_EntryListedTwice_FailsAsDuplicateNotStale()
    {
        var allowList = SchemaAllowList.FromJson( @"{ ""Entries"": [
                { ""Check"": ""mapping"", ""Code"": ""MissingColumn"", ""Object"": ""dbo.Person.Old"", ""Reason"": ""Kept for plugins."" },
                { ""Check"": ""mapping"", ""Code"": ""MissingColumn"", ""Object"": ""dbo.Person.Old"", ""Reason"": ""Kept for plugins."" } ] }" );
        var findings = new List<SchemaFinding> { SchemaFinding.Failure( "mapping", "MissingColumn", "dbo.Person.Old", "Missing." ) };

        var problems = allowList.Apply( findings, "mapping" );

        CollectionAssert.AreEqual( new[] { "ALLOW mapping MissingColumn dbo.Person.Old" }, SchemaFindings.Describe( findings ) );
        CollectionAssert.AreEqual( new[] { "FAIL mapping DuplicateAllowListEntry dbo.Person.Old" }, SchemaFindings.Describe( problems ) );
    }

    #endregion Allow List

    #region Equivalence

    [TestMethod]
    public void Equivalence_IdenticalCatalogs_HasNoFindings()
    {
        var findings = Rock.Data.SchemaChecks.CompareCatalogs( ActualPerson(), ActualPerson() );

        CollectionAssert.AreEqual( new string[0], SchemaFindings.Describe( findings ) );
    }

    [TestMethod]
    public void Equivalence_DifferentCatalogs_ReportsEachDifference()
    {
        var left = ActualPerson();
        left.Tables["dbo.Person"].Indexes.Add( new SchemaCatalogIndex { Name = "IX_Age", KeyColumns = new List<string> { "Age" } } );
        left.Modules["dbo.spGetPerson"] = new SchemaCatalogModule { Key = "dbo.spGetPerson", ObjectType = "SQL_STORED_PROCEDURE", Definition = "CREATE PROCEDURE x\nAS\nSELECT 1" };

        var right = ActualPerson();
        right.Tables["dbo.Person"].Columns["Email"].DefaultDefinition = "('')";
        right.Tables["dbo.Person"].Indexes.Add( new SchemaCatalogIndex { Name = "IX_Age_Renamed", KeyColumns = new List<string> { "Age" } } );
        right.Modules["dbo.spGetPerson"] = new SchemaCatalogModule { Key = "dbo.spGetPerson", ObjectType = "SQL_STORED_PROCEDURE", Definition = "CREATE PROCEDURE x\nAS\nSELECT 2" };
        right.AddTable( new SchemaCatalogTable { Name = "Extra" } );

        var findings = Rock.Data.SchemaChecks.CompareCatalogs( left, right );

        CollectionAssert.AreEqual( new[]
        {
            "FAIL equivalence ColumnDiffers dbo.Person.Email",
            "FAIL equivalence IndexOnlyInLeft dbo.Person.IX_Age",
            "FAIL equivalence IndexOnlyInRight dbo.Person.IX_Age_Renamed",
            "FAIL equivalence ModuleDiffers dbo.spGetPerson",
            "FAIL equivalence TableOnlyInRight dbo.Extra"
        }, SchemaFindings.Describe( findings ) );
        Assert.AreEqual( "Left: nvarchar(250) NULL. Right: nvarchar(250) NULL DEFAULT ('').", findings.Single( f => f.Code == "ColumnDiffers" ).Message );
        Assert.AreEqual( "The SQL_STORED_PROCEDURE definitions differ at line 3. Left: 'SELECT 1'. Right: 'SELECT 2'.", findings.Single( f => f.Code == "ModuleDiffers" ).Message );
    }

    [TestMethod]
    public void Data_KeyedRows_ReportsMissingAndDifferentRows()
    {
        var findings = new List<SchemaFinding>();
        var left = new List<KeyValuePair<string, string>> { Row( "{\"Id\":1}", "A" ), Row( "{\"Id\":2}", "B" ), Row( "{\"Id\":3}", "C" ) };
        var right = new List<KeyValuePair<string, string>> { Row( "{\"Id\":1}", "A" ), Row( "{\"Id\":2}", "X" ), Row( "{\"Id\":4}", "D" ) };

        SchemaDataComparer.CompareRows( findings, "dbo.Person", left, right );

        CollectionAssert.AreEqual( new[]
        {
            "FAIL data RowsDiffer dbo.Person",
            "FAIL data RowsOnlyInLeft dbo.Person",
            "FAIL data RowsOnlyInRight dbo.Person"
        }, SchemaFindings.Describe( findings ) );
        Assert.AreEqual( "1 rows have different values. For example: {\"Id\":2}", findings.Single( f => f.Code == "RowsDiffer" ).Message );
    }

    [TestMethod]
    public void Data_UnkeyedRows_ComparesAsMultisets()
    {
        var findings = new List<SchemaFinding>();
        var left = new List<KeyValuePair<string, string>> { Row( "", "A" ), Row( "", "A" ), Row( "", "B" ) };
        var right = new List<KeyValuePair<string, string>> { Row( "", "A" ), Row( "", "B" ) };

        SchemaDataComparer.CompareRows( findings, "dbo.Log", left, right );

        CollectionAssert.AreEqual( new[] { "FAIL data RowsOnlyInLeft dbo.Log" }, SchemaFindings.Describe( findings ) );
    }

    #endregion Equivalence

    #region Support

    /// <summary>
    /// The Person table as the model maps it.
    /// </summary>
    private static SchemaExpectedModel ExpectedPerson()
    {
        var table = new SchemaExpectedTable { Name = "Person", PrimaryKeyColumns = new List<string> { "Id" } };
        table.Columns.Add( Column( "Id", "int" ) );
        table.Columns.Add( Column( "NickName", "nvarchar", maxLength: 50 ) );
        table.Columns.Add( Column( "Email", "nvarchar", maxLength: 250, isNullable: true ) );
        table.Columns.Add( Column( "Age", "int", isNullable: true ) );
        table.Columns.Add( Column( "Balance", "decimal", precision: 18, scale: 2 ) );

        var model = new SchemaExpectedModel();
        model.Tables.Add( table );

        return model;
    }

    /// <summary>
    /// A Person table that matches <see cref="ExpectedPerson"/>.
    /// </summary>
    private static SchemaCatalog ActualPerson()
    {
        var catalog = new SchemaCatalog();
        var table = catalog.AddTable( new SchemaCatalogTable { Name = "Person" } )
            .AddColumn( new SchemaCatalogColumn { Name = "Id", StoreType = "int", IsIdentity = true } )
            .AddColumn( Column( "NickName", "nvarchar", maxLength: 50 ) )
            .AddColumn( Column( "Email", "nvarchar", maxLength: 250, isNullable: true ) )
            .AddColumn( Column( "Age", "int", isNullable: true ) )
            .AddColumn( Column( "Balance", "decimal", precision: 18, scale: 2 ) );
        table.PrimaryKey = new SchemaCatalogIndex { Name = "PK_dbo.Person", IsClustered = true, IsUnique = true, KeyColumns = new List<string> { "Id" } };

        return catalog;
    }

    private static SchemaCatalogColumn Column( string name, string storeType, int? maxLength = null, bool isNullable = false, int? precision = null, int? scale = null, string defaultDefinition = null )
    {
        return new SchemaCatalogColumn
        {
            Name = name,
            StoreType = storeType,
            MaxLength = maxLength,
            IsNullable = isNullable,
            Precision = precision,
            Scale = scale,
            DefaultDefinition = defaultDefinition
        };
    }

    private static KeyValuePair<string, string> Row( string key, string hash )
    {
        return new KeyValuePair<string, string>( key, hash );
    }

    #endregion Support
}
