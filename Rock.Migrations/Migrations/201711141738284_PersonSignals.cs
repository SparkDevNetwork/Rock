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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class PersonSignals : Rock.Migrations.RockMigration
    {
        const string ENTITY_TYPE_BADGE_TOP_PERSON_SIGNAL = "1BC1335A-A37E-4C02-83C1-AD2883FD954E";
        const string ENTITY_TYPE_SIGNAL_TYPE_CACHE = "0FCFAEE7-F945-4FC7-AD46-6F3CADB28C9B";
        const string ENTITY_TYPE_REPORTING_SIGNAL_SELECT = "63A79A4D-A3B0-4B97-B5AB-CE93FC3C03C7";
        const string ENTITY_TYPE_REPORTING_HAS_SIGNAL_FILTER = "5DC0EEB7-2B9E-4828-883B-0E7090C992AA";

        const string BADGE_TOP_PERSON_SIGNAL = "B4B336CE-137E-44BE-9123-27740D0064C2";

        const string BLOCK_TYPE_PERSON_SIGNAL_TYPE_LIST = "250FF1C3-B2AE-4AFD-BEFA-29C45BEB30D2";
        const string BLOCK_TYPE_PERSON_SIGNAL_TYPE_DETAIL = "E9AB79D9-429F-410D-B4A8-327829FC7C63";
        const string BLOCK_TYPE_SIGNAL_LIST = "813CFCCF-30BF-4A2F-BB55-F240A3B7809F";

        const string PAGE_SIGNAL_TYPE_LIST = "EA6B3CF2-9DE2-4CF0-8EFA-01B76B51C329";
        const string PAGE_SIGNAL_TYPE_DETAIL = "67AF60BC-D814-4DBC-BA64-D12128CCF52C";

        const string BLOCK_SIGNAL_TYPE_LIST = "1134FF9C-8539-462F-95B4-65B89178B8EA";
        const string BLOCK_SIGNAL_TYPE_DETAIL = "72D04752-0024-4ED9-BBD0-0F12714ABB31";
        const string BLOCK_SIGNAL_LIST = "C5DA2773-E12B-4E63-A392-7A775C09BCE5";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            //
            // Add the SignalType table.
            //
            CreateTable(
                "dbo.SignalType",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    Name = c.String( nullable: false, maxLength: 100 ),
                    Description = c.String(),
                    SignalColor = c.String( nullable: false, maxLength: 100 ),
                    SignalIconCssClass = c.String( maxLength: 100 ),
                    Order = c.Int( nullable: false ),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 )
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            //
            // Add the PersonSignal table.
            //
            CreateTable(
                "dbo.PersonSignal",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    PersonId = c.Int( nullable: false ),
                    SignalTypeId = c.Int( nullable: false ),
                    OwnerPersonAliasId = c.Int( nullable: false ),
                    Note = c.String(),
                    ExpirationDate = c.DateTime(),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 )
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.Person", t => t.PersonId, cascadeDelete: true )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.OwnerPersonAliasId )
                .ForeignKey( "dbo.SignalType", t => t.SignalTypeId, cascadeDelete: true )
                .Index( t => t.PersonId )
                .Index( t => t.SignalTypeId )
                .Index( t => t.OwnerPersonAliasId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            //
            // Update the Person table with new columns and references.
            //
            AddColumn( "dbo.Person", "TopSignalColor", c => c.String( nullable: true, maxLength: 100 ) );
            AddColumn( "dbo.Person", "TopSignalIconCssClass", c => c.String( nullable: true, maxLength: 100 ) );
            AddColumn( "dbo.Person", "TopSignalId", c => c.Int( nullable: true ) );
            AddForeignKey( "dbo.Person", "TopSignalId", "dbo.PersonSignal", "Id" );

            //
            // Ensure all the entity types are registered with known Guids.
            //
            RockMigrationHelper.UpdateEntityType( "Rock.Model.SignalType", "Signal Type", "Rock.Model.SignalType, Rock, Version=1.8.0.0, Culture=neutral, PublicKeyToken=null", true, true, Rock.SystemGuid.EntityType.SIGNAL_TYPE );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.PersonSignal", "Person Signal", "Rock.Model.PersonSignal, Rock, Version=1.8.0.0, Culture=neutral, PublicKeyToken=null", true, true, Rock.SystemGuid.EntityType.PERSON_SIGNAL );
            RockMigrationHelper.UpdateEntityType( "Rock.Web.Cache.SignalTypeCache", "Signal Type Cache", "Rock.Web.Cache.SignalTypeCache, Rock, Version=1.8.0.0, Culture=neutral, PublicKeyToken=null", false, true, ENTITY_TYPE_SIGNAL_TYPE_CACHE );
            RockMigrationHelper.UpdateEntityType( "Rock.PersonProfile.Badge.TopPersonSignal", "Top Person Signal", "Rock.PersonProfile.Badge.TopPersonSignal, Rock, Version=1.8.0.0, Culture=neutral, PublicKeyToken=null", false, true, ENTITY_TYPE_BADGE_TOP_PERSON_SIGNAL );
            RockMigrationHelper.UpdateEntityType( "Rock.Reporting.DataSelect.Person.SignalSelect", "Signal Select", "Rock.Reporting.DataSelect.Person.SignalSelect, Rock, Version=1.8.0.0, Culture=neutral, PublicKeyToken=null", false, true, ENTITY_TYPE_REPORTING_SIGNAL_SELECT );
            RockMigrationHelper.UpdateEntityType( "Rock.Reporting.DataFilter.Person.HasSignalFilter", "Has Signal Filter", "Rock.Reporting.DataFilter.Person.HasSignalFilter, Rock, Version=1.8.0.0, Culture=neutral, PublicKeyToken=null", false, true, ENTITY_TYPE_REPORTING_HAS_SIGNAL_FILTER );

            //
            // Set security on the reporting entity types so only Rock Administrators can
            // use them by default.
            //
            RockMigrationHelper.AddSecurityAuthForEntityType(
                "Rock.Reporting.DataSelect.Person.SignalSelect",
                0,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_ADMINISTRATORS,
                ( int ) Rock.Model.SpecialRole.AllUsers,
                "F466F3C8-BDAF-439B-B43D-EBE38D3E23B1" );
            RockMigrationHelper.AddSecurityAuthForEntityType(
                "Rock.Reporting.DataSelect.Person.SignalSelect",
                1,
                Rock.Security.Authorization.VIEW,
                false,
                null,
                ( int ) Rock.Model.SpecialRole.AllUsers,
                "A3B5206D-E52A-4545-987F-B2FF39AA4818" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                "Rock.Reporting.DataFilter.Person.HasSignalFilter",
                0,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_ADMINISTRATORS,
                ( int ) Rock.Model.SpecialRole.AllUsers,
                "6B7790A1-68C0-4453-B3D5-A58BE3928935" );
            RockMigrationHelper.AddSecurityAuthForEntityType(
                "Rock.Reporting.DataFilter.Person.HasSignalFilter",
                1,
                Rock.Security.Authorization.VIEW,
                false,
                null,
                ( int ) Rock.Model.SpecialRole.AllUsers,
                "E8A3332F-8D63-43DC-B13E-B3D947D9C9AD" );

            //
            // Register the Top Person Signal badge in the database.
            //
            RockMigrationHelper.UpdatePersonBadge(
                "Top Person Signal",
                "Shows the top person badge and the number of signals.",
                "Rock.PersonProfile.Badge.TopPersonSignal",
                0,
                BADGE_TOP_PERSON_SIGNAL );

            //
            // Add the signal badge to the left badge bar on the person details page.
            //
            Sql( string.Format( @"
    UPDATE AV
    	SET AV.[Value] = AV.[Value] + CASE WHEN AV.[Value] != '' THEN ',' END + '{0}'
        FROM AttributeValue AS AV
        LEFT JOIN [Block] AS B ON B.Id = AV.EntityId
        LEFT JOIN [Attribute] AS A ON A.Id = AV.[AttributeId]
        WHERE B.[Guid] = '98A30DD7-8665-4C6D-B1BB-A8380E862A04'
          AND A.[Guid] = 'F5AB231E-3836-4D52-BD03-BF79773C237A'
    	  AND AV.[Value] NOT LIKE '%{0}%'
", BADGE_TOP_PERSON_SIGNAL ) );

            //
            // Add the Job for calculating person signals.
            //
            Sql( @"
    INSERT INTO [ServiceJob] (
         [IsSystem]
        ,[IsActive]
        ,[Name]
        ,[Description]
        ,[Class]
        ,[CronExpression]
        ,[NotificationStatus]
        ,[Guid] )
    VALUES (
         0
        ,1
        ,'Calculate Person Signals'
        ,'Re-calculates all person signals to ensure that the top-most signal is still the current one.'
        ,'Rock.Jobs.CalculatePersonSignals'
        ,'0 15 3 1/1 * ? *'
        ,1
        ,'82B6315E-53D0-43C6-8F09-C5B0E8890B8D');
" );

            //
            // Update all the new Block Types.
            //
            RockMigrationHelper.UpdateBlockType(
                "Person Signal Type List",
                "Shows a list of all signal types.",
                "~/Blocks/CRM/PersonSignalTypeList.ascx",
                "CRM",
                BLOCK_TYPE_PERSON_SIGNAL_TYPE_LIST );
            RockMigrationHelper.AddBlockTypeAttribute(
                BLOCK_TYPE_PERSON_SIGNAL_TYPE_LIST,
                SystemGuid.FieldType.PAGE_REFERENCE,
                "Detail Page",
                "DetailPage",
                string.Empty,
                string.Empty,
                0,
                string.Empty,
                "5ECE9E10-0C47-4CC1-8491-E73873E6BD66",
                true );

            RockMigrationHelper.UpdateBlockType(
                "Person Signal Type Detail",
                "Shows the details of a particular person signal type.",
                "~/Blocks/CRM/PersonSignalTypeDetail.ascx",
                "CRM",
                BLOCK_TYPE_PERSON_SIGNAL_TYPE_DETAIL );

            RockMigrationHelper.UpdateBlockType(
                "Signal List",
                "Lists all the signals on a person.",
                "~/Blocks/CRM/PersonDetail/SignalList.ascx",
                "CRM > Person Detail",
                BLOCK_TYPE_SIGNAL_LIST );

            //
            // Create the Security > Person Signal Types page.
            //
            RockMigrationHelper.AddPage(
                "91CCB1C9-5F9F-44F5-8BE2-9EC3A3CFD46F", // Security
                "D65F783D-87A9-4CC9-8110-E83466A0EADB", // Full Width
                "Person Signal Types",
                string.Empty,
                PAGE_SIGNAL_TYPE_LIST,
                "fa fa-flag" );
            RockMigrationHelper.AddBlock(
                PAGE_SIGNAL_TYPE_LIST,
                string.Empty,
                BLOCK_TYPE_PERSON_SIGNAL_TYPE_LIST,
                "Person Signal Type List",
                "Main",
                string.Empty,
                string.Empty,
                0,
                BLOCK_SIGNAL_TYPE_LIST );
            RockMigrationHelper.AddBlockAttributeValue(
                BLOCK_SIGNAL_TYPE_LIST,
                "5ECE9E10-0C47-4CC1-8491-E73873E6BD66",
                PAGE_SIGNAL_TYPE_DETAIL );

            //
            // Create the Security > Person Signal Types > Person Signal Type Detail page.
            //
            RockMigrationHelper.AddPage(
                PAGE_SIGNAL_TYPE_LIST,
                "D65F783D-87A9-4CC9-8110-E83466A0EADB", // Full Width
                "Person Signal Type Detail",
                string.Empty,
                PAGE_SIGNAL_TYPE_DETAIL,
                "fa fa-flag" );
            RockMigrationHelper.AddBlock(
                PAGE_SIGNAL_TYPE_DETAIL,
                string.Empty,
                BLOCK_TYPE_PERSON_SIGNAL_TYPE_DETAIL,
                "Person Signal Type Detail",
                "Main",
                string.Empty,
                string.Empty,
                0,
                BLOCK_SIGNAL_TYPE_DETAIL );

            //
            // Insert the Signal List block at top of the Person Detail > Security page.
            //
            RockMigrationHelper.AddBlock(
                "0E56F56E-FB32-4827-A69A-B90D43CB47F5",
                "",
                BLOCK_TYPE_SIGNAL_LIST,
                "Signal List",
                "SectionC1",
                string.Empty,
                string.Empty,
                0,
                BLOCK_SIGNAL_LIST );
            Sql( string.Format( @"
    UPDATE [B]
    	SET [B].[Order] = [B].[Order] + 1
    	FROM [Block] AS [B]
    	LEFT JOIN [Page] AS [P] ON [P].[Id] = [B].[PageId]
    	WHERE [P].[Guid] = '0E56F56E-FB32-4827-A69A-B90D43CB47F5'
	      AND [B].[Zone] = 'SectionC1'
    	  AND [B].[Guid] != '{0}'
", BLOCK_SIGNAL_LIST ) );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            //
            // Delete the Signal List block from the Person Detail > Security page.
            //
            RockMigrationHelper.DeleteBlock( BLOCK_SIGNAL_LIST );

            //
            // Delete the Security > Person Signal Types > Person Signal Type Detail page.
            //
            RockMigrationHelper.DeleteBlock( BLOCK_SIGNAL_TYPE_DETAIL );
            RockMigrationHelper.DeletePage( PAGE_SIGNAL_TYPE_DETAIL );

            //
            // Delete the Security > Person Signal Types page.
            //
            RockMigrationHelper.DeleteBlock( BLOCK_SIGNAL_TYPE_LIST );
            RockMigrationHelper.DeletePage( PAGE_SIGNAL_TYPE_LIST );

            //
            // Delete block types.
            //
            RockMigrationHelper.DeleteBlockType( BLOCK_TYPE_SIGNAL_LIST );
            RockMigrationHelper.DeleteBlockType( BLOCK_TYPE_PERSON_SIGNAL_TYPE_DETAIL );
            RockMigrationHelper.DeleteAttribute( "5ECE9E10-0C47-4CC1-8491-E73873E6BD66" );
            RockMigrationHelper.DeleteBlockType( BLOCK_TYPE_PERSON_SIGNAL_TYPE_LIST );

            //
            // Remove the job to calculate signals.
            //
            Sql( "DELETE [ServiceJob] WHERE [Guid] = '82B6315E-53D0-43C6-8F09-C5B0E8890B8D'" );

            //
            // Delete the signal badge to the left badge bar on the person details page.
            // Remove in order of ",guid", "guid,", "guid" to keep formatting correct.
            //
            Sql( string.Format( @"
    UPDATE AV
    	SET AV.[Value] = REPLACE(REPLACE(REPLACE(AV.[Value], ',{0}', ''), '{0},', ''), '{0}', '')
        FROM AttributeValue AS AV
        LEFT JOIN [Block] AS B ON B.Id = AV.EntityId
        LEFT JOIN [Attribute] AS A ON A.Id = AV.[AttributeId]
        WHERE B.[Guid] = '98A30DD7-8665-4C6D-B1BB-A8380E862A04'
          AND A.[Guid] = 'F5AB231E-3836-4D52-BD03-BF79773C237A'
", BADGE_TOP_PERSON_SIGNAL ) );

            //
            // Undo changes to the Person table.
            //
            DropForeignKey( "dbo.Person", "TopSignalId", "dbo.PersonSignal" );
            DropColumn( "dbo.Person", "TopSignalId" );
            DropColumn( "dbo.Person", "TopSignalIconCssClass" );
            DropColumn( "dbo.Person", "TopSignalColor" );

            //
            // Remove the PersonSignal table.
            //
            DropForeignKey( "dbo.PersonSignal", "SignalTypeId", "dbo.SignalType" );
            DropForeignKey( "dbo.PersonSignal", "OwnerPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.PersonSignal", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.PersonSignal", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.PersonSignal", "PersonId", "dbo.Person" );
            DropIndex( "dbo.PersonSignal", new[] { "PersonId" } );
            DropIndex( "dbo.PersonSignal", new[] { "SignalTypeId" } );
            DropIndex( "dbo.PersonSignal", new[] { "OwnerPersonAliasId" } );
            DropIndex( "dbo.PersonSignal", new string[] { "Guid" } );
            DropIndex( "dbo.PersonSignal", new string[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.PersonSignal", new string[] { "CreatedByPersonAliasId" } );
            DropTable( "dbo.PersonSignal" );

            //
            // Remove the SignalType table.
            //
            DropForeignKey( "dbo.SignalType", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.SignalType", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropIndex( "dbo.SignalType", new string[] { "Guid" } );
            DropIndex( "dbo.SignalType", new string[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.SignalType", new string[] { "CreatedByPersonAliasId" } );
            DropTable( "dbo.SignalType" );
        }
    }
}
