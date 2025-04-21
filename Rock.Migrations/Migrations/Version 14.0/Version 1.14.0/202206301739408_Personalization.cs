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
    public partial class Personalization : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.PersonAliasPersonalization",
                c => new
                {
                    PersonAliasId = c.Int( nullable: false ),
                    PersonalizationType = c.Int( nullable: false ),
                    PersonalizationTypeId = c.Int( nullable: false ),
                } )
                .PrimaryKey( t => new { t.PersonAliasId, t.PersonalizationType, t.PersonalizationTypeId } )
                .ForeignKey( "dbo.PersonAlias", t => t.PersonAliasId )
                .Index( t => t.PersonAliasId );

            CreateTable(
                "dbo.PersonalizedEntity",
                c => new
                {
                    EntityTypeId = c.Int( nullable: false ),
                    EntityId = c.Int( nullable: false ),
                    PersonalizationType = c.Int( nullable: false ),
                    PersonalizationTypeId = c.Int( nullable: false ),
                } )
                .PrimaryKey( t => new { t.EntityTypeId, t.EntityId, t.PersonalizationType, t.PersonalizationTypeId } )
                .ForeignKey( "dbo.EntityType", t => t.EntityTypeId )
                .Index( t => t.EntityTypeId );

            CreateTable(
                "dbo.RequestFilter",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    Name = c.String( maxLength: 100 ),
                    RequestFilterKey = c.String(),
                    SiteId = c.Int(),
                    IsActive = c.Boolean( nullable: false ),
                    FilterJson = c.String(),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .ForeignKey( "dbo.Site", t => t.SiteId )
                .Index( t => t.SiteId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.PersonalizationSegment",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    Name = c.String( maxLength: 100 ),
                    SegmentKey = c.String(),
                    IsActive = c.Boolean( nullable: false ),
                    FilterDataViewId = c.Int(),
                    AdditionalFilterJson = c.String(),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.DataView", t => t.FilterDataViewId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .Index( t => t.FilterDataViewId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            AddColumn( "dbo.PersonAlias", "AliasedDateTime", c => c.DateTime() );
            AddColumn( "dbo.PersonAlias", "LastVisitDateTime", c => c.DateTime() );
            AddColumn( "dbo.ContentChannel", "EnablePersonalization", c => c.Boolean( nullable: false ) );
            AddColumn( "dbo.Site", "EnableVisitorTracking", c => c.Boolean( nullable: false ) );
            AddColumn( "dbo.Site", "EnablePersonalization", c => c.Boolean( nullable: false ) );

            RockMigrationHelper.UpdateDefinedValue( "26BE73A6-A9C5-4E94-AE00-3AFDCF8C9275", "Anonymous Visitor", "An Anonymous Visitor", "80007453-30A7-453C-BF0B-C82AAFE2BA12", true );

            AddAnonymousVisitor_Up();

            UpdatePersonAliasAliasPersonIdIndex_Up();

            BlocksPagesUp();

            AddUpdatePersonalizationDataJob_Up();

            PageRoutes_Up();
        }

        private void PageRoutes_Up()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            // Add Page Route
            //   Page:Request Filter Detail
            //   Route:admin/cms/request-filters/{RequestFilterId}
            RockMigrationHelper.AddPageRoute( "435496F7-6C3F-4007-97F0-0B439F5D910E", "admin/cms/request-filters/{RequestFilterId}", "1009E2CB-7D77-4A94-B927-B38FB099F871" );

            // Add Page Route
            //   Page:Personalization Segment Detail
            //   Route:admin/cms/personalization-segments/{PersonalizationSegmentId}
            RockMigrationHelper.AddPageRoute( "28B26424-5EC3-498B-8407-620FA763EE09", "admin/cms/personalization-segments/{PersonalizationSegmentId}", "2B7EDBA9-B1B6-48F1-A541-7B78002E1EAE" );

            // Add Page Route
            //   Page:Request Filters
            //   Route:admin/cms/request-filters
            RockMigrationHelper.AddPageRoute( "511FC29A-EAF2-4AC0-B9B3-8613739A9ACF", "admin/cms/request-filters", "4463198E-1187-4CCC-88A2-26484E1565B8" );

            // Add Page Route
            //   Page:Personalization Segments
            //   Route:admin/cms/personalization-segments
            RockMigrationHelper.AddPageRoute( "905F6132-AE1C-4C85-9752-18D22E604C3A", "admin/cms/personalization-segments", "55D0D8A0-D534-467E-9C85-12B4C79EC5B1" );
#pragma warning restore CS0618 // Type or member is obsolete
        }


        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            BlocksPagesDown();

            DropForeignKey( "dbo.PersonalizationSegment", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.PersonalizationSegment", "FilterDataViewId", "dbo.DataView" );
            DropForeignKey( "dbo.PersonalizationSegment", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.RequestFilter", "SiteId", "dbo.Site" );
            DropForeignKey( "dbo.RequestFilter", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.RequestFilter", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.PersonalizedEntity", "EntityTypeId", "dbo.EntityType" );
            DropForeignKey( "dbo.PersonAliasPersonalization", "PersonAliasId", "dbo.PersonAlias" );
            DropIndex( "dbo.PersonalizationSegment", new[] { "Guid" } );
            DropIndex( "dbo.PersonalizationSegment", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.PersonalizationSegment", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.PersonalizationSegment", new[] { "FilterDataViewId" } );
            DropIndex( "dbo.RequestFilter", new[] { "Guid" } );
            DropIndex( "dbo.RequestFilter", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.RequestFilter", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.RequestFilter", new[] { "SiteId" } );
            DropIndex( "dbo.PersonalizedEntity", new[] { "EntityTypeId" } );
            DropIndex( "dbo.PersonAliasPersonalization", new[] { "PersonAliasId" } );
            DropColumn( "dbo.Site", "EnablePersonalization" );
            DropColumn( "dbo.Site", "EnableVisitorTracking" );
            DropColumn( "dbo.ContentChannel", "EnablePersonalization" );
            DropColumn( "dbo.PersonAlias", "LastVisitDateTime" );
            DropColumn( "dbo.PersonAlias", "AliasedDateTime" );
            DropTable( "dbo.PersonalizationSegment" );
            DropTable( "dbo.RequestFilter" );
            DropTable( "dbo.PersonalizedEntity" );
            DropTable( "dbo.PersonAliasPersonalization" );
            UpdatePersonAliasAliasPersonIdIndex_Down();
            AddAnonymousVisitor_Down();
            AddUpdatePersonalizationDataJob_Down();
        }

        private void UpdatePersonAliasAliasPersonIdIndex_Up()
        {
            // Delete this index because it a unique constraint that includes the NULL value, so only one NULL allowed */
            RockMigrationHelper.DropIndexIfExists( "PersonAlias", "IX_AliasPersonId" );
            Sql( @"
/* This is a 'filtered' unique constraint that excludes NULL value, so we can have as many nulls as we want.*/
CREATE UNIQUE NONCLUSTERED INDEX[IX_AliasPersonId] ON[dbo].[PersonAlias]
(

    [AliasPersonId] ASC
) WHERE[AliasPersonId] IS NOT NULL" );
        }
        private void UpdatePersonAliasAliasPersonIdIndex_Down()
        {
            // Recreate index as it was before this migration
            RockMigrationHelper.DropIndexIfExists( "PersonAlias", "IX_AliasPersonId" );
            Sql( @"
/* This is a 'filtered' unique constraint that excludes NULL value, so we can have as many nulls as we want.*/
CREATE UNIQUE NONCLUSTERED INDEX[IX_AliasPersonId] ON[dbo].[PersonAlias]
(

    [AliasPersonId] ASC
)" );
        }


        private void AddUpdatePersonalizationDataJob_Down()
        {
            Sql( $"DELETE FROM ServiceJob WHERE Guid = '{Rock.SystemGuid.ServiceJob.UPDATE_PERSONALIZATION_DATA}';" );
        }

        private void AddUpdatePersonalizationDataJob_Up()
        {
            Sql( $@"
            IF NOT EXISTS (
                SELECT 1
                FROM [ServiceJob]
                WHERE [Class] = 'Rock.Jobs.UpdatePersonalizationData'
                                AND [Guid] = '{SystemGuid.ServiceJob.UPDATE_PERSONALIZATION_DATA}'
            )
            BEGIN
                INSERT INTO [ServiceJob] (
                    [IsSystem]
                    ,[IsActive]
                    ,[Name]
                    ,[Description]
                    ,[Class]
                    ,[CronExpression]
                    ,[NotificationStatus]
                    ,[HistoryCount]
                    ,[Guid]
                ) VALUES (
                    0
                    ,1
                    ,'Update Personalization Data'
                    ,'Job that updates Personalization Data.'
                    ,'Rock.Jobs.UpdatePersonalizationData'
                    ,'0 20 1 1/1 * ? *'
                    ,1
                    ,500 
                    ,'{SystemGuid.ServiceJob.UPDATE_PERSONALIZATION_DATA}'
                );
            END" );

        }

        private void AddAnonymousVisitor_Up()
        {
            Sql( $@"
DECLARE @personRecordType INT = ( SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '{SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON}' ),
		@connectionStatusValueId INT = ( SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '{SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PARTICIPANT}'),
		@recordStatusValueId INT = (select [Id] from [DefinedValue] where [Guid] = '{SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE}'),
		@personId INT,
		@groupId INT,
		@personGuid UNIQUEIDENTIFIER = '{SystemGuid.Person.ANONYMOUS_VISITOR}',
		@familyGroupType INT = ( SELECT [Id] FROM [GroupType] WHERE [Guid] = '790E3215-3B10-442B-AF69-616C0DCB998E' ),
		@adultRole INT = ( SELECT [Id] FROM [GroupTypeRole] WHERE [Guid] = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42' )

INSERT INTO [Person] (
    [IsSystem]
    ,[FirstName]
    ,[NickName]
    ,[LastName]
    ,[Gender]
    ,[AgeClassification]
    ,[IsEmailActive]
    ,[EmailPreference]
    ,[Guid]
    ,[RecordTypeValueId]
    ,[RecordStatusValueId]
    ,[ConnectionStatusValueId]
	,[IsDeceased]
    ,[CreatedDateTime]
    )
VALUES (
    1
    ,'Anonymous'
    ,'Anonymous'
    ,'Visitor'
    ,0
    ,1
    ,1
    ,0
    ,@personGuid
    ,@personRecordType
    ,@recordStatusValueId
    ,@connectionStatusValueId
	,0
    ,SYSDATETIME()
    )

SET @personId = SCOPE_IDENTITY()

INSERT INTO [PersonAlias] (
    PersonId
    ,AliasPersonId
    ,AliasPersonGuid
    ,[Guid]
    )
VALUES (
    @personId
    ,@personId
    ,@personGuid
    ,NEWID()
    );

declare @randomCampusId int = (select top 1 Id from Campus order by newid())

-- create family
INSERT INTO [Group] (
    IsSystem
    ,GroupTypeId
    ,NAME
    ,IsSecurityRole
    ,IsActive
    ,CampusId
    ,[Guid]
    ,[Order]
    )
VALUES (
    0
    ,@familyGroupType
    ,'Anonymous Visitor Family'
    ,0
    ,1
    ,@randomCampusId
    ,NEWID()
    ,0
    )

SET @groupId = SCOPE_IDENTITY()

INSERT INTO [GroupMember] (
    IsSystem
    ,GroupId
    ,PersonId
    ,GroupRoleId
    ,[Guid]
    ,GroupMemberStatus
	,DateTimeAdded
    ,[GroupTypeId]
    )
VALUES (
    0
    ,@groupId
    ,@personId
    ,@adultRole
    ,newid()
    ,1
	,SYSDATETIME()
    ,@familyGroupType
    )

	UPDATE Person
        SET GivingId = (
		        CASE 
			        WHEN [GivingGroupId] IS NOT NULL
				        THEN 'G' + CONVERT([varchar], [GivingGroupId])
			        ELSE 'P' + CONVERT([varchar], [Id])
			        END
		        )
        WHERE GivingId IS NULL AND Id = @personId

        UPDATE x
        SET x.PrimaryFamilyId = x.CalculatedPrimaryFamilyId
            ,x.PrimaryCampusId = x.CalculatedPrimaryCampusId
        FROM (
            SELECT p.Id
                ,p.NickName
                ,p.LastName
                ,p.PrimaryFamilyId
                ,p.PrimaryCampusId
                ,pf.CalculatedPrimaryFamilyId
                ,pf.CalculatedPrimaryCampusId
            FROM Person p
            OUTER APPLY (
                SELECT TOP 1
                    g.Id [CalculatedPrimaryFamilyId]
                    ,g.CampusId [CalculatedPrimaryCampusId]
                FROM GroupMember gm
                JOIN [Group] g ON g.Id = gm.GroupId
                WHERE g.GroupTypeId = @familyGroupType
                    AND gm.PersonId = p.Id
                ORDER BY gm.GroupOrder
                    ,gm.GroupId
                ) pf
            WHERE (
                    (ISNULL(p.PrimaryFamilyId, 0) != ISNULL(pf.CalculatedPrimaryFamilyId, 0))
                    OR (ISNULL(p.PrimaryCampusId, 0) != ISNULL(pf.CalculatedPrimaryCampusId, 0))
                    ) AND ( p.Id = @personId) ) x

            UPDATE x
            SET x.GivingLeaderId = x.CalculatedGivingLeaderId
            FROM (
	            SELECT p.Id
		            ,p.NickName
		            ,p.LastName
		            ,p.GivingLeaderId
		            ,isnull(pf.CalculatedGivingLeaderId, p.Id) CalculatedGivingLeaderId
	            FROM Person p
	            OUTER APPLY (
		            SELECT TOP 1 p2.[Id] CalculatedGivingLeaderId
		            FROM [GroupMember] gm
		            INNER JOIN [GroupTypeRole] r ON r.[Id] = gm.[GroupRoleId]
		            INNER JOIN [Person] p2 ON p2.[Id] = gm.[PersonId]
		            WHERE gm.[GroupId] = p.GivingGroupId
			            AND p2.[IsDeceased] = 0
			            AND p2.[GivingGroupId] = p.GivingGroupId
		            ORDER BY r.[Order]
			            ,p2.[Gender]
			            ,p2.[BirthYear]
			            ,p2.[BirthMonth]
			            ,p2.[BirthDay]
		            ) pf
	            WHERE (
			            p.GivingLeaderId IS NULL
			            OR (p.GivingLeaderId != pf.CalculatedGivingLeaderId)
			            ) AND ( p.GivingId in (select GivingId from Person where Id = @personId ) )) x
" );
        }

        private void AddAnonymousVisitor_Down()
        {
            Sql( $@"UPDATE Person
SET PrimaryFamilyId = NULL
WHERE [Guid] = '{SystemGuid.Person.ANONYMOUS_VISITOR}'

DELETE
FROM [Group]
WHERE Id IN (
        SELECT GroupId
        FROM GroupMember
        WHERE Personid IN (
                SELECT Id
                FROM Person
                WHERE Guid = '{SystemGuid.Person.ANONYMOUS_VISITOR}'
                )
        )

DELETE
FROM PersonAlias
WHERE PersonId IN (
        SELECT Id
        FROM Person
        WHERE Guid = '{SystemGuid.Person.ANONYMOUS_VISITOR}'
        )

DELETE
FROM Person
WHERE [Guid] = '{SystemGuid.Person.ANONYMOUS_VISITOR}'
" );
        }

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        private void BlocksPagesUp()
        {
            // Add Page 
            //  Internal Name: Personalization Segments
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "B4A24AB7-9369-4055-883F-4F4892C39AE3", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Personalization Segments", "", "905F6132-AE1C-4C85-9752-18D22E604C3A", "fa fa-user-tag" );

            // Add Page 
            //  Internal Name: Request Filters
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "B4A24AB7-9369-4055-883F-4F4892C39AE3", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Request Filters", "", "511FC29A-EAF2-4AC0-B9B3-8613739A9ACF", "fa fa-filter" );

            // Add Page 
            //  Internal Name: Personalization Segment Detail
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "905F6132-AE1C-4C85-9752-18D22E604C3A", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Personalization Segment Detail", "", "28B26424-5EC3-498B-8407-620FA763EE09", "" );

            // Add Page 
            //  Internal Name: Request Filter Detail
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "511FC29A-EAF2-4AC0-B9B3-8613739A9ACF", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Request Filter Detail", "", "435496F7-6C3F-4007-97F0-0B439F5D910E", "" );

            // Add/Update BlockType 
            //   Name: Personalization Segment Detail
            //   Category: Cms
            //   Path: ~/Blocks/Cms/PersonalizationSegmentDetail.ascx
            //   EntityType: -
            RockMigrationHelper.UpdateBlockType( "Personalization Segment Detail", "Displays the details of a personalization segment.", "~/Blocks/Cms/PersonalizationSegmentDetail.ascx", "Cms", "1F0A0A57-952D-4774-8760-52C6D56B9DB5" );

            // Add/Update BlockType 
            //   Name: Personalization Segment List
            //   Category: Cms
            //   Path: ~/Blocks/Cms/PersonalizationSegmentList.ascx
            //   EntityType: -
            RockMigrationHelper.UpdateBlockType( "Personalization Segment List", "Block that lists existing Personalization Segments.", "~/Blocks/Cms/PersonalizationSegmentList.ascx", "Cms", "06EC24B2-0B2A-47E0-9A1F-44587BC46099" );

            // Add/Update BlockType 
            //   Name: Request Filter Detail
            //   Category: Cms
            //   Path: ~/Blocks/Cms/RequestFilterDetails.ascx
            //   EntityType: -
            RockMigrationHelper.UpdateBlockType( "Request Filter Detail", "Displays the details of a request filter.", "~/Blocks/Cms/RequestFilterDetails.ascx", "Cms", "0CE221F6-EECE-46F9-A703-FCD09DEBC653" );

            // Add/Update BlockType 
            //   Name: Request Filter List
            //   Category: Cms
            //   Path: ~/Blocks/Cms/RequestFilterList.ascx
            //   EntityType: -
            RockMigrationHelper.UpdateBlockType( "Request Filter List", "Block that lists existing Request Filters.", "~/Blocks/Cms/RequestFilterList.ascx", "Cms", "650E16B0-8B97-4336-9CE0-EAF8AAC20BDF" );

            // Add Block 
            //  Block Name: Personalization Segment List
            //  Page Name: Personalization Segments
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "905F6132-AE1C-4C85-9752-18D22E604C3A".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "06EC24B2-0B2A-47E0-9A1F-44587BC46099".AsGuid(), "Personalization Segment List", "Main", @"", @"", 0, "C021A093-7AAB-4BA2-A88C-A934555D4754" );

            // Add Block 
            //  Block Name: Request Filter List
            //  Page Name: Request Filters
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "511FC29A-EAF2-4AC0-B9B3-8613739A9ACF".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "650E16B0-8B97-4336-9CE0-EAF8AAC20BDF".AsGuid(), "Request Filter List", "Main", @"", @"", 0, "EC64C9A2-9BCC-4B3C-8EA1-8AB996CA19A9" );

            // Add Block 
            //  Block Name: Personalization Segment Detail
            //  Page Name: Personalization Segment Detail
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "28B26424-5EC3-498B-8407-620FA763EE09".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "1F0A0A57-952D-4774-8760-52C6D56B9DB5".AsGuid(), "Personalization Segment Detail", "Main", @"", @"", 0, "4A59C082-FD9B-4B27-A5C7-5FC2DCB7075D" );

            // Add Block 
            //  Block Name: Request Filter Detail
            //  Page Name: Request Filter Detail
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "435496F7-6C3F-4007-97F0-0B439F5D910E".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "0CE221F6-EECE-46F9-A703-FCD09DEBC653".AsGuid(), "Request Filter Detail", "Main", @"", @"", 0, "25D0BE0D-B2BF-4CD8-8D0B-2A1E01E32C02" );

            // Attribute for BlockType
            //   BlockType: Personalization Segment List
            //   Category: Cms
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "06EC24B2-0B2A-47E0-9A1F-44587BC46099", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 0, @"", "FFA53235-9001-4536-8FF3-D23710F70C66" );

            // Attribute for BlockType
            //   BlockType: Personalization Segment List
            //   Category: Cms
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "06EC24B2-0B2A-47E0-9A1F-44587BC46099", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "7BF5D1B5-9CF6-4DE1-9FD5-50AAD1DE1378" );

            // Attribute for BlockType
            //   BlockType: Personalization Segment List
            //   Category: Cms
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "06EC24B2-0B2A-47E0-9A1F-44587BC46099", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "BC1EC043-58D7-4F6F-88BA-FAEF7D8A8FBC" );

            // Attribute for BlockType
            //   BlockType: Request Filter List
            //   Category: Cms
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "650E16B0-8B97-4336-9CE0-EAF8AAC20BDF", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 0, @"", "97E7084A-31BD-4989-B6E2-A9151AF9208C" );

            // Attribute for BlockType
            //   BlockType: Request Filter List
            //   Category: Cms
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "650E16B0-8B97-4336-9CE0-EAF8AAC20BDF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "0D017A46-155B-40D6-83E1-C2BD8C350C12" );

            // Attribute for BlockType
            //   BlockType: Request Filter List
            //   Category: Cms
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "650E16B0-8B97-4336-9CE0-EAF8AAC20BDF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "4CF5D41B-5E5B-4493-9FF8-9F71BE03042A" );

            // Add Block Attribute Value
            //   Block: Personalization Segment List
            //   BlockType: Personalization Segment List
            //   Category: Cms
            //   Block Location: Page=Personalization Segments, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: 28b26424-5ec3-498b-8407-620fa763ee09 */
            RockMigrationHelper.AddBlockAttributeValue( "C021A093-7AAB-4BA2-A88C-A934555D4754", "FFA53235-9001-4536-8FF3-D23710F70C66", @"28b26424-5ec3-498b-8407-620fa763ee09" );

            // Add Block Attribute Value
            //   Block: Personalization Segment List
            //   BlockType: Personalization Segment List
            //   Category: Cms
            //   Block Location: Page=Personalization Segments, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "C021A093-7AAB-4BA2-A88C-A934555D4754", "4B9E1BE4-1074-43E5-AA46-190CBE64688D", @"False" );

            // Add Block Attribute Value
            //   Block: Personalization Segment List
            //   BlockType: Personalization Segment List
            //   Category: Cms
            //   Block Location: Page=Personalization Segments, Site=Rock RMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "C021A093-7AAB-4BA2-A88C-A934555D4754", "BC1EC043-58D7-4F6F-88BA-FAEF7D8A8FBC", @"True" );

            // Add Block Attribute Value
            //   Block: Request Filter List
            //   BlockType: Request Filter List
            //   Category: Cms
            //   Block Location: Page=Request Filters, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: 435496f7-6c3f-4007-97f0-0b439f5d910e */
            RockMigrationHelper.AddBlockAttributeValue( "EC64C9A2-9BCC-4B3C-8EA1-8AB996CA19A9", "97E7084A-31BD-4989-B6E2-A9151AF9208C", @"435496f7-6c3f-4007-97f0-0b439f5d910e" );

            // Add Block Attribute Value
            //   Block: Request Filter List
            //   BlockType: Request Filter List
            //   Category: Cms
            //   Block Location: Page=Request Filters, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "EC64C9A2-9BCC-4B3C-8EA1-8AB996CA19A9", "4D958B29-38DE-4833-93DD-4046BD663404", @"False" );

            // Add Block Attribute Value
            //   Block: Request Filter List
            //   BlockType: Request Filter List
            //   Category: Cms
            //   Block Location: Page=Request Filters, Site=Rock RMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "EC64C9A2-9BCC-4B3C-8EA1-8AB996CA19A9", "4CF5D41B-5E5B-4493-9FF8-9F71BE03042A", @"True" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        private void BlocksPagesDown()
        {

            // Attribute for BlockType
            //   BlockType: Request Filter List
            //   Category: Cms
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "4CF5D41B-5E5B-4493-9FF8-9F71BE03042A" );

            // Attribute for BlockType
            //   BlockType: Request Filter List
            //   Category: Cms
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "0D017A46-155B-40D6-83E1-C2BD8C350C12" );

            // Attribute for BlockType
            //   BlockType: Request Filter List
            //   Category: Cms
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "97E7084A-31BD-4989-B6E2-A9151AF9208C" );

            // Attribute for BlockType
            //   BlockType: Personalization Segment List
            //   Category: Cms
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "BC1EC043-58D7-4F6F-88BA-FAEF7D8A8FBC" );

            // Attribute for BlockType
            //   BlockType: Personalization Segment List
            //   Category: Cms
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "7BF5D1B5-9CF6-4DE1-9FD5-50AAD1DE1378" );

            // Attribute for BlockType
            //   BlockType: Personalization Segment List
            //   Category: Cms
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "FFA53235-9001-4536-8FF3-D23710F70C66" );

            // Remove Block
            //  Name: Request Filter Detail, from Page: Request Filter Detail, Site: Rock RMS
            //  from Page: Request Filter Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "25D0BE0D-B2BF-4CD8-8D0B-2A1E01E32C02" );

            // Remove Block
            //  Name: Request Filter List, from Page: Request Filters, Site: Rock RMS
            //  from Page: Request Filters, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "EC64C9A2-9BCC-4B3C-8EA1-8AB996CA19A9" );

            // Remove Block
            //  Name: Personalization Segment Detail, from Page: Personalization Segment Detail, Site: Rock RMS
            //  from Page: Personalization Segment Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "4A59C082-FD9B-4B27-A5C7-5FC2DCB7075D" );

            // Remove Block
            //  Name: Personalization Segment List, from Page: Personalization Segments, Site: Rock RMS
            //  from Page: Personalization Segments, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "C021A093-7AAB-4BA2-A88C-A934555D4754" );

            // Delete BlockType 
            //   Name: Request Filter List
            //   Category: Cms
            //   Path: ~/Blocks/Cms/RequestFilterList.ascx
            //   EntityType: -
            RockMigrationHelper.DeleteBlockType( "650E16B0-8B97-4336-9CE0-EAF8AAC20BDF" );

            // Delete BlockType 
            //   Name: Request Filter Detail
            //   Category: Cms
            //   Path: ~/Blocks/Cms/RequestFilterDetails.ascx
            //   EntityType: -
            RockMigrationHelper.DeleteBlockType( "0CE221F6-EECE-46F9-A703-FCD09DEBC653" );

            // Delete BlockType 
            //   Name: Personalization Segment List
            //   Category: Cms
            //   Path: ~/Blocks/Cms/PersonalizationSegmentList.ascx
            //   EntityType: -
            RockMigrationHelper.DeleteBlockType( "06EC24B2-0B2A-47E0-9A1F-44587BC46099" );

            // Delete BlockType 
            //   Name: Personalization Segment Detail
            //   Category: Cms
            //   Path: ~/Blocks/Cms/PersonalizationSegmentDetail.ascx
            //   EntityType: -
            RockMigrationHelper.DeleteBlockType( "1F0A0A57-952D-4774-8760-52C6D56B9DB5" );

            // Delete Page 
            //  Internal Name: Request Filter Detail
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "435496F7-6C3F-4007-97F0-0B439F5D910E" );

            // Delete Page 
            //  Internal Name: Personalization Segment Detail
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "28B26424-5EC3-498B-8407-620FA763EE09" );

            // Delete Page 
            //  Internal Name: Request Filters
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "511FC29A-EAF2-4AC0-B9B3-8613739A9ACF" );

            // Delete Page 
            //  Internal Name: Personalization Segments
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "905F6132-AE1C-4C85-9752-18D22E604C3A" );
        }
    }
}
